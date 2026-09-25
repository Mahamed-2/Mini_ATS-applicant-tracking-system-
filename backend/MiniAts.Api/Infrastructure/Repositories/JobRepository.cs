// JobRepository executes parameterized SQL against public.jobs.
// Related: Application/Interfaces/Interfaces.cs (IJobRepository)
//          Domain/Job.cs
//          supabase/migrations/0001_init.sql (jobs table, idx_jobs_customer_created_at)
//          Application/Services/JobService.cs

using MiniAts.Application.Interfaces;
using MiniAts.Domain;
using Npgsql;

namespace MiniAts.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly NpgsqlDataSource _db;

    public JobRepository(NpgsqlDataSource db) => _db = db;

    public async Task<IReadOnlyList<Job>> ListByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        // Uses idx_jobs_customer_created_at index for fast dashboard loading.
        await using var cmd = _db.CreateCommand("""
            SELECT id, customer_id, title, description, status,
                   created_by, updated_by, created_at, updated_at
            FROM public.jobs
            WHERE customer_id = @customerId
            ORDER BY created_at DESC
            """);

        cmd.Parameters.AddWithValue("customerId", customerId);
        return await ReadJobsAsync(cmd, ct);
    }

    public async Task<Job?> GetByIdAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        // Always scope by customer_id to prevent cross-customer reads.
        await using var cmd = _db.CreateCommand("""
            SELECT id, customer_id, title, description, status,
                   created_by, updated_by, created_at, updated_at
            FROM public.jobs
            WHERE id = @id AND customer_id = @customerId
            """);

        cmd.Parameters.AddWithValue("id",         id);
        cmd.Parameters.AddWithValue("customerId", customerId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return ReadJob(reader);
    }

    public async Task<Job> CreateAsync(Job job, CancellationToken ct = default)
    {
        // Insert and return the new row (including DB-generated id and timestamps).
        await using var cmd = _db.CreateCommand("""
            INSERT INTO public.jobs (customer_id, title, description, status, created_by, updated_by)
            VALUES (@customerId, @title, @description, @status, @createdBy, @updatedBy)
            RETURNING id, customer_id, title, description, status,
                      created_by, updated_by, created_at, updated_at
            """);

        cmd.Parameters.AddWithValue("customerId",  job.CustomerId);
        cmd.Parameters.AddWithValue("title",       job.Title);
        cmd.Parameters.AddWithValue("description", (object?)job.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("status",      job.Status);
        cmd.Parameters.AddWithValue("createdBy",   (object?)job.CreatedBy ?? DBNull.Value);
        cmd.Parameters.AddWithValue("updatedBy",   (object?)job.UpdatedBy ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return ReadJob(reader);
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        // Partial update; updated_at is refreshed by the DB trigger.
        await using var cmd = _db.CreateCommand("""
            UPDATE public.jobs
            SET title       = @title,
                description = @description,
                status      = @status,
                updated_by  = @updatedBy
            WHERE id = @id AND customer_id = @customerId
            """);

        cmd.Parameters.AddWithValue("id",          job.Id);
        cmd.Parameters.AddWithValue("customerId",  job.CustomerId);
        cmd.Parameters.AddWithValue("title",       job.Title);
        cmd.Parameters.AddWithValue("description", (object?)job.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("status",      job.Status);
        cmd.Parameters.AddWithValue("updatedBy",   (object?)job.UpdatedBy ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        // Scoped delete; DB cascade sets candidate.job_id to null.
        await using var cmd = _db.CreateCommand(
            "DELETE FROM public.jobs WHERE id = @id AND customer_id = @customerId");

        cmd.Parameters.AddWithValue("id",         id);
        cmd.Parameters.AddWithValue("customerId", customerId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // Read all rows from a command into a Job list.
    private static async Task<List<Job>> ReadJobsAsync(NpgsqlCommand cmd, CancellationToken ct)
    {
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Job>();
        while (await reader.ReadAsync(ct))
            list.Add(ReadJob(reader));
        return list;
    }

    // Map reader columns to Job entity; columns must match SELECT order above.
    private static Job ReadJob(NpgsqlDataReader r) => new()
    {
        Id          = r.GetGuid(0),
        CustomerId  = r.GetGuid(1),
        Title       = r.GetString(2),
        Description = r.IsDBNull(3) ? null : r.GetString(3),
        Status      = r.GetString(4),
        CreatedBy   = r.IsDBNull(5) ? null : r.GetGuid(5),
        UpdatedBy   = r.IsDBNull(6) ? null : r.GetGuid(6),
        CreatedAt   = r.GetDateTime(7),
        UpdatedAt   = r.GetDateTime(8)
    };
}
