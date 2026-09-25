// JobService enforces authorization and delegates SQL to IJobRepository.
// Related: Application/Interfaces/Interfaces.cs (IJobService, IJobRepository)
//          Infrastructure/Repositories/JobRepository.cs
//          Api/Controllers/JobsController.cs
//          Domain/Job.cs, Application/Dtos/Dtos.cs

using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;
using MiniAts.Domain;

namespace MiniAts.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobs;

    // JobRepository is injected by DI – registered in Program.cs.
    public JobService(IJobRepository jobs) => _jobs = jobs;

    // Resolve the effective customer id, enforcing customer-can-only-use-own-id.
    private static Guid ResolveCustomerId(Guid callerId, string callerRole, Guid customerId)
    {
        // Admin may supply any customer id; customer must use their own id.
        if (callerRole == "admin") return customerId;
        return callerId; // ignore supplied customerId for non-admins
    }

    public async Task<IReadOnlyList<JobDto>> ListAsync(
        Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveCustomerId = ResolveCustomerId(callerId, callerRole, customerId);
        var jobs = await _jobs.ListByCustomerAsync(effectiveCustomerId, ct);
        return jobs.Select(MapToDto).ToList();
    }

    public async Task<JobDto?> GetAsync(
        Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveCustomerId = ResolveCustomerId(callerId, callerRole, customerId);
        var job = await _jobs.GetByIdAsync(id, effectiveCustomerId, ct);
        return job is null ? null : MapToDto(job);
    }

    public async Task<JobDto> CreateAsync(
        CreateJobRequest req, Guid callerId, string callerRole, CancellationToken ct = default)
    {
        // Validate title is not empty.
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new ArgumentException("Job title is required.");

        var effectiveCustomerId = ResolveCustomerId(callerId, callerRole, req.CustomerId);

        var job = new Job
        {
            CustomerId  = effectiveCustomerId,
            Title       = req.Title.Trim(),
            Description = req.Description?.Trim(),
            Status      = "active",
            CreatedBy   = callerId,
            UpdatedBy   = callerId
        };

        var created = await _jobs.CreateAsync(job, ct);
        return MapToDto(created);
    }

    public async Task<JobDto> UpdateAsync(
        Guid id, UpdateJobRequest req, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveCustomerId = ResolveCustomerId(callerId, callerRole, customerId);
        var existing = await _jobs.GetByIdAsync(id, effectiveCustomerId, ct)
            ?? throw new KeyNotFoundException($"Job {id} not found.");

        // Apply only the fields that were provided in the PATCH request.
        if (!string.IsNullOrWhiteSpace(req.Title)) existing.Title = req.Title.Trim();
        if (req.Description is not null) existing.Description = req.Description.Trim();
        if (!string.IsNullOrWhiteSpace(req.Status)) existing.Status = req.Status;
        existing.UpdatedBy = callerId;

        await _jobs.UpdateAsync(existing, ct);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(
        Guid id, Guid callerId, string callerRole, Guid customerId, CancellationToken ct = default)
    {
        var effectiveCustomerId = ResolveCustomerId(callerId, callerRole, customerId);
        await _jobs.DeleteAsync(id, effectiveCustomerId, ct);
    }

    // Map domain entity to DTO; keeps controllers ignorant of domain internals.
    private static JobDto MapToDto(Job j) => new(
        j.Id, j.CustomerId, j.Title, j.Description, j.Status, j.CreatedAt, j.UpdatedAt);
}
