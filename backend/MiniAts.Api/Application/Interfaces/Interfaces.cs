// Application-layer interfaces for all repositories and services.
// Related: Infrastructure/Repositories/ (implementations)
//          Application/Services/ (service implementations)
//          Program.cs (DI registrations)

using MiniAts.Application.Dtos;
using MiniAts.Domain;

namespace MiniAts.Application.Interfaces;

// ── Repository interfaces ─────────────────────────────────────────────────────

/// <summary>Data access contract for the public.profiles table.</summary>
public interface IProfileRepository
{
    // Load a profile by its auth UUID – used by CurrentUserMiddleware.
    Task<Profile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // List all profiles – admin only, used by GET /api/admin/users.
    Task<IReadOnlyList<Profile>> ListAsync(CancellationToken ct = default);

    // Insert a new profile row after Supabase Auth user is created.
    Task CreateAsync(Profile profile, CancellationToken ct = default);
}

/// <summary>Data access contract for the public.jobs table.</summary>
public interface IJobRepository
{
    // Fetch all jobs belonging to one customer, newest first.
    Task<IReadOnlyList<Job>> ListByCustomerAsync(Guid customerId, CancellationToken ct = default);

    // Fetch a single job scoped to a customer (prevents cross-customer read).
    Task<Job?> GetByIdAsync(Guid id, Guid customerId, CancellationToken ct = default);

    // Insert a new job row.
    Task<Job> CreateAsync(Job job, CancellationToken ct = default);

    // Update mutable fields (title, description, status, updated_by).
    Task UpdateAsync(Job job, CancellationToken ct = default);

    // Delete a job; cascades to candidates via DB foreign key set-null.
    Task DeleteAsync(Guid id, Guid customerId, CancellationToken ct = default);
}

/// <summary>Data access contract for the public.candidates table.</summary>
public interface ICandidateRepository
{
    // List candidates with optional job and name filters – drives Kanban view.
    Task<IReadOnlyList<Candidate>> ListAsync(
        Guid customerId, Guid? jobId, string? search, CancellationToken ct = default);

    // Fetch single candidate scoped to a customer.
    Task<Candidate?> GetByIdAsync(Guid id, Guid customerId, CancellationToken ct = default);

    // Insert new candidate row.
    Task<Candidate> CreateAsync(Candidate candidate, CancellationToken ct = default);

    // Update mutable fields on a candidate.
    Task UpdateAsync(Candidate candidate, CancellationToken ct = default);

    // Update only the stage column – called by PATCH /api/candidates/{id}/stage.
    Task UpdateStageAsync(Guid id, Guid customerId, string stage, Guid updatedBy, CancellationToken ct = default);

    // Store AI assessment results after Python service responds.
    Task SaveAiResultAsync(Guid id, Guid customerId, decimal score, string feedbackJson, Guid updatedBy, CancellationToken ct = default);

    // Delete candidate – customer-scoped for safety.
    Task DeleteAsync(Guid id, Guid customerId, CancellationToken ct = default);
}

// ── Service interfaces ────────────────────────────────────────────────────────

/// <summary>Business logic for job CRUD with authorization enforcement.</summary>
public interface IJobService
{
    Task<IReadOnlyList<JobDto>> ListAsync(Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
    Task<JobDto?> GetAsync(Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
    Task<JobDto> CreateAsync(CreateJobRequest req, Guid callerId, string callerRole, CancellationToken ct = default);
    Task<JobDto> UpdateAsync(Guid id, UpdateJobRequest req, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
}

/// <summary>Business logic for candidate CRUD, stage moves, and AI assessment.</summary>
public interface ICandidateService
{
    Task<IReadOnlyList<CandidateDto>> ListAsync(Guid callerId, string callerRole, Guid customerId, Guid? jobId, string? search, CancellationToken ct = default);
    Task<CandidateDto?> GetAsync(Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
    Task<CandidateDto> CreateAsync(CreateCandidateRequest req, Guid callerId, string callerRole, CancellationToken ct = default);
    Task<CandidateDto> UpdateAsync(Guid id, UpdateCandidateRequest req, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
    Task MoveStageAsync(Guid id, string stage, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
    Task<AiAssessmentResult> AssessAsync(Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
}

/// <summary>Business logic for AI-driven pipeline report aggregation and narrative analysis.</summary>
public interface IPipelineReportService
{
    Task<PipelineReportDto> GenerateReportAsync(Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default);
}

/// <summary>Business logic for admin user creation via Supabase Auth Admin API.</summary>
public interface IAdminUserService
{
    Task<ProfileDto> CreateUserAsync(CreateUserRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ProfileDto>> ListUsersAsync(CancellationToken ct = default);
}

// ── External client interfaces ────────────────────────────────────────────────

/// <summary>Client for Supabase Auth Admin API (user creation/lookup).</summary>
public interface ISupabaseAdminClient
{
    // Create a user in Supabase Auth; returns the new auth user UUID.
    Task<Guid> CreateAuthUserAsync(string email, string password, CancellationToken ct = default);
}

/// <summary>Client for the Python FastAPI AI assessment service.</summary>
public interface IAiClient
{
    // Call POST /assess on the Python service; returns structured result.
    Task<AiAssessmentResult> AssessAsync(
        string candidateName,
        string jobTitle,
        string jobDescription,
        string cvText,
        string linkedinUrl,
        string summary,
        CancellationToken ct = default);

    // Call POST /analyze on the Python service for executive pipeline intelligence.
    Task<PipelineAnalyzeResponse> AnalyzePipelineAsync(
        object payload,
        CancellationToken ct = default);
}

