// CandidateService orchestrates candidate CRUD, Kanban stage changes, and AI assessment.
// Related: Application/Interfaces/Interfaces.cs (ICandidateService, ICandidateRepository, IAiClient)
//          Infrastructure/Repositories/CandidateRepository.cs
//          Infrastructure/AiClient.cs
//          Api/Controllers/CandidatesController.cs
//          Api/Controllers/AiController.cs
//          Domain/Candidate.cs, Domain/Job.cs

using System.Text.Json;
using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;
using MiniAts.Domain;

namespace MiniAts.Application.Services;

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _candidates;
    private readonly IJobRepository _jobs;    // needed to load job context for AI assessment
    private readonly IAiClient _aiClient;

    // DI injects all three dependencies – registered in Program.cs.
    public CandidateService(
        ICandidateRepository candidates,
        IJobRepository jobs,
        IAiClient aiClient)
    {
        _candidates = candidates;
        _jobs       = jobs;
        _aiClient   = aiClient;
    }

    // Customer callers use their own id; admin can scope to any customer.
    private static Guid ResolveCustomerId(Guid callerId, string callerRole, Guid customerId)
        => callerRole == "admin" ? customerId : callerId;

    public async Task<IReadOnlyList<CandidateDto>> ListAsync(
        Guid callerId, string callerRole, Guid customerId, Guid? jobId, string? search, CancellationToken ct = default)
    {
        var effectiveId = ResolveCustomerId(callerId, callerRole, customerId);
        var list = await _candidates.ListAsync(effectiveId, jobId, search, ct);
        return list.Select(MapToDto).ToList();
    }

    public async Task<CandidateDto?> GetAsync(
        Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveId = ResolveCustomerId(callerId, callerRole, customerId);
        var candidate = await _candidates.GetByIdAsync(id, effectiveId, ct);
        return candidate is null ? null : MapToDto(candidate);
    }

    public async Task<CandidateDto> CreateAsync(
        CreateCandidateRequest req, Guid callerId, string callerRole, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.FullName))
            throw new ArgumentException("Full name is required.");

        var effectiveId = ResolveCustomerId(callerId, callerRole, req.CustomerId);

        var candidate = new Candidate
        {
            CustomerId   = effectiveId,
            JobId        = req.JobId,
            FullName     = req.FullName.Trim(),
            Email        = req.Email?.Trim(),
            LinkedinUrl  = req.LinkedinUrl?.Trim(),
            CvText       = req.CvText?.Trim(),
            Summary      = req.Summary?.Trim(),
            Stage        = ParseStage(req.Stage),
            CreatedBy    = callerId,
            UpdatedBy    = callerId
        };

        var created = await _candidates.CreateAsync(candidate, ct);
        return MapToDto(created);
    }

    public async Task<CandidateDto> UpdateAsync(
        Guid id, UpdateCandidateRequest req, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveId = ResolveCustomerId(callerId, callerRole, customerId);
        var existing = await _candidates.GetByIdAsync(id, effectiveId, ct)
            ?? throw new KeyNotFoundException($"Candidate {id} not found.");

        // Apply partial update – only overwrite fields that are non-null in the request.
        if (req.JobId.HasValue) existing.JobId = req.JobId;
        if (!string.IsNullOrWhiteSpace(req.FullName)) existing.FullName = req.FullName.Trim();
        if (req.Email is not null) existing.Email = req.Email.Trim();
        if (req.LinkedinUrl is not null) existing.LinkedinUrl = req.LinkedinUrl.Trim();
        if (req.CvText is not null) existing.CvText = req.CvText.Trim();
        if (req.Summary is not null) existing.Summary = req.Summary.Trim();
        if (!string.IsNullOrWhiteSpace(req.Stage)) existing.Stage = ParseStage(req.Stage);
        existing.UpdatedBy = callerId;

        await _candidates.UpdateAsync(existing, ct);
        return MapToDto(existing);
    }

    public async Task MoveStageAsync(
        Guid id, string stage, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveId = ResolveCustomerId(callerId, callerRole, customerId);
        // Validate stage string is a known value before hitting the DB.
        _ = ParseStage(stage);
        await _candidates.UpdateStageAsync(id, effectiveId, stage.ToLowerInvariant(), callerId, ct);
    }

    public async Task<AiAssessmentResult> AssessAsync(
        Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveId = ResolveCustomerId(callerId, callerRole, customerId);

        // Load candidate to get CV text and LinkedIn URL.
        var candidate = await _candidates.GetByIdAsync(id, effectiveId, ct)
            ?? throw new KeyNotFoundException($"Candidate {id} not found.");

        // Load related job to provide job context to the AI service.
        var job = candidate.JobId.HasValue
            ? await _jobs.GetByIdAsync(candidate.JobId.Value, effectiveId, ct)
            : null;

        // Call Python AI service with all available context.
        var result = await _aiClient.AssessAsync(
            candidate.FullName,
            job?.Title ?? string.Empty,
            job?.Description ?? string.Empty,
            candidate.CvText ?? string.Empty,
            candidate.LinkedinUrl ?? string.Empty,
            candidate.Summary ?? string.Empty,
            ct);

        // Persist AI result on the candidate row for future display.
        var feedbackJson = JsonSerializer.Serialize(result);
        await _candidates.SaveAiResultAsync(id, effectiveId, result.Score, feedbackJson, callerId, ct);

        return result;
    }

    public async Task DeleteAsync(
        Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveId = ResolveCustomerId(callerId, callerRole, customerId);
        await _candidates.DeleteAsync(id, effectiveId, ct);
    }

    // Parse a stage string into the enum; throws if unrecognized.
    private static CandidateStage ParseStage(string stage)
    {
        return stage.ToLowerInvariant() switch
        {
            "new"        => CandidateStage.New,
            "screening"  => CandidateStage.Screening,
            "interview"  => CandidateStage.Interview,
            "offer"      => CandidateStage.Offer,
            "hired"      => CandidateStage.Hired,
            "rejected"   => CandidateStage.Rejected,
            _            => throw new ArgumentException($"Unknown stage '{stage}'.")
        };
    }

    // Map domain Candidate to CandidateDto; deserializes AiFeedback JSON if present.
    private static CandidateDto MapToDto(Candidate c)
    {
        object? feedbackObj = null;
        if (!string.IsNullOrEmpty(c.AiFeedback))
        {
            try { feedbackObj = JsonSerializer.Deserialize<object>(c.AiFeedback); }
            catch { /* Return null if stored feedback is malformed. */ }
        }

        return new CandidateDto(
            c.Id, c.CustomerId, c.JobId, c.FullName, c.Email,
            c.LinkedinUrl, c.CvText, c.Summary,
            c.Stage.ToString().ToLowerInvariant(),
            c.AiScore, feedbackObj, c.CreatedAt, c.UpdatedAt);
    }
}
