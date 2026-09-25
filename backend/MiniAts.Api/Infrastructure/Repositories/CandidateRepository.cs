// CandidateRepository executes parameterized SQL against public.candidates.
// Related: Application/Interfaces/Interfaces.cs (ICandidateRepository)
//          Domain/Candidate.cs
//          supabase/migrations/0001_init.sql (candidates table, indexes)
//          Application/Services/CandidateService.cs

using MiniAts.Application.Interfaces;
using MiniAts.Domain;
using Npgsql;

namespace MiniAts.Infrastructure.Repositories;

public class CandidateRepository : ICandidateRepository
{
    private readonly NpgsqlDataSource _db;

    public CandidateRepository(NpgsqlDataSource db) => _db = db;

    public async Task<IReadOnlyList<Candidate>> ListAsync(
        Guid customerId, Guid? jobId, string? search, CancellationToken ct = default)
    {
        // Uses idx_candidates_customer_job_stage and idx_candidates_customer_full_name.
        // ILIKE pattern enables case-insensitive partial name search.
        await using var cmd = _db.CreateCommand("""
            SELECT id, customer_id, job_id, full_name, email, linkedin_url,
                   cv_storage_path, cv_text, summary, stage::text,
                   ai_score, ai_feedback::text, created_by, updated_by, created_at, updated_at
            FROM public.candidates
            WHERE customer_id = @customerId
              AND (@jobId::uuid IS NULL OR job_id = @jobId)
              AND (@search IS NULL OR full_name ILIKE @searchPattern)
            ORDER BY created_at DESC
            """);

        cmd.Parameters.AddWithValue("customerId",    customerId);
        cmd.Parameters.AddWithValue("jobId",         (object?)jobId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("search",        (object?)search ?? DBNull.Value);
        cmd.Parameters.AddWithValue("searchPattern", (object?)(search is null ? null : $"%{search}%") ?? DBNull.Value);

        return await ReadCandidatesAsync(cmd, ct);
    }

    public async Task<Candidate?> GetByIdAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        // Always scope by customer_id – prevents cross-customer reads.
        await using var cmd = _db.CreateCommand("""
            SELECT id, customer_id, job_id, full_name, email, linkedin_url,
                   cv_storage_path, cv_text, summary, stage::text,
                   ai_score, ai_feedback::text, created_by, updated_by, created_at, updated_at
            FROM public.candidates
            WHERE id = @id AND customer_id = @customerId
            """);

        cmd.Parameters.AddWithValue("id",         id);
        cmd.Parameters.AddWithValue("customerId", customerId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return ReadCandidate(reader);
    }

    public async Task<Candidate> CreateAsync(Candidate candidate, CancellationToken ct = default)
    {
        await using var cmd = _db.CreateCommand("""
            INSERT INTO public.candidates
              (customer_id, job_id, full_name, email, linkedin_url,
               cv_storage_path, cv_text, summary, stage, created_by, updated_by)
            VALUES
              (@customerId, @jobId, @fullName, @email, @linkedinUrl,
               @cvStoragePath, @cvText, @summary, @stage::public.candidate_stage, @createdBy, @updatedBy)
            RETURNING id, customer_id, job_id, full_name, email, linkedin_url,
                      cv_storage_path, cv_text, summary, stage::text,
                      ai_score, ai_feedback::text, created_by, updated_by, created_at, updated_at
            """);

        AddCandidateParams(cmd, candidate);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return ReadCandidate(reader);
    }

    public async Task UpdateAsync(Candidate candidate, CancellationToken ct = default)
    {
        await using var cmd = _db.CreateCommand("""
            UPDATE public.candidates SET
              job_id          = @jobId,
              full_name       = @fullName,
              email           = @email,
              linkedin_url    = @linkedinUrl,
              cv_storage_path = @cvStoragePath,
              cv_text         = @cvText,
              summary         = @summary,
              stage           = @stage::public.candidate_stage,
              updated_by      = @updatedBy
            WHERE id = @id AND customer_id = @customerId
            """);

        cmd.Parameters.AddWithValue("id",           candidate.Id);
        cmd.Parameters.AddWithValue("customerId",   candidate.CustomerId);
        AddCandidateParams(cmd, candidate);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateStageAsync(
        Guid id, Guid customerId, string stage, Guid updatedBy, CancellationToken ct = default)
    {
        // Targeted update of only the stage column – called by PATCH /stage endpoint.
        await using var cmd = _db.CreateCommand("""
            UPDATE public.candidates
            SET stage      = @stage::public.candidate_stage,
                updated_by = @updatedBy
            WHERE id = @id AND customer_id = @customerId
            """);

        cmd.Parameters.AddWithValue("id",         id);
        cmd.Parameters.AddWithValue("customerId", customerId);
        cmd.Parameters.AddWithValue("stage",      stage);
        cmd.Parameters.AddWithValue("updatedBy",  updatedBy);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task SaveAiResultAsync(
        Guid id, Guid customerId, decimal score, string feedbackJson, Guid updatedBy, CancellationToken ct = default)
    {
        // Store AI assessment results in ai_score and ai_feedback (jsonb) columns.
        await using var cmd = _db.CreateCommand("""
            UPDATE public.candidates
            SET ai_score    = @score,
                ai_feedback = @feedback::jsonb,
                updated_by  = @updatedBy
            WHERE id = @id AND customer_id = @customerId
            """);

        cmd.Parameters.AddWithValue("id",         id);
        cmd.Parameters.AddWithValue("customerId", customerId);
        cmd.Parameters.AddWithValue("score",      score);
        cmd.Parameters.AddWithValue("feedback",   feedbackJson);
        cmd.Parameters.AddWithValue("updatedBy",  updatedBy);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        await using var cmd = _db.CreateCommand(
            "DELETE FROM public.candidates WHERE id = @id AND customer_id = @customerId");

        cmd.Parameters.AddWithValue("id",         id);
        cmd.Parameters.AddWithValue("customerId", customerId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // Add parameters shared between INSERT and UPDATE operations.
    private static void AddCandidateParams(NpgsqlCommand cmd, Candidate c)
    {
        cmd.Parameters.AddWithValue("customerId",    c.CustomerId);
        cmd.Parameters.AddWithValue("jobId",         (object?)c.JobId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("fullName",      c.FullName);
        cmd.Parameters.AddWithValue("email",         (object?)c.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("linkedinUrl",   (object?)c.LinkedinUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("cvStoragePath", (object?)c.CvStoragePath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("cvText",        (object?)c.CvText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("summary",       (object?)c.Summary ?? DBNull.Value);
        cmd.Parameters.AddWithValue("stage",         c.Stage.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("createdBy",     (object?)c.CreatedBy ?? DBNull.Value);
        cmd.Parameters.AddWithValue("updatedBy",     (object?)c.UpdatedBy ?? DBNull.Value);
    }

    private static async Task<List<Candidate>> ReadCandidatesAsync(NpgsqlCommand cmd, CancellationToken ct)
    {
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Candidate>();
        while (await reader.ReadAsync(ct))
            list.Add(ReadCandidate(reader));
        return list;
    }

    // Map reader columns to Candidate entity; columns must match SELECT order above.
    private static Candidate ReadCandidate(NpgsqlDataReader r) => new()
    {
        Id             = r.GetGuid(0),
        CustomerId     = r.GetGuid(1),
        JobId          = r.IsDBNull(2) ? null : r.GetGuid(2),
        FullName       = r.GetString(3),
        Email          = r.IsDBNull(4) ? null : r.GetString(4),
        LinkedinUrl    = r.IsDBNull(5) ? null : r.GetString(5),
        CvStoragePath  = r.IsDBNull(6) ? null : r.GetString(6),
        CvText         = r.IsDBNull(7) ? null : r.GetString(7),
        Summary        = r.IsDBNull(8) ? null : r.GetString(8),
        Stage          = Enum.Parse<CandidateStage>(r.GetString(9), ignoreCase: true),
        AiScore        = r.IsDBNull(10) ? null : r.GetDecimal(10),
        AiFeedback     = r.IsDBNull(11) ? null : r.GetString(11),
        CreatedBy      = r.IsDBNull(12) ? null : r.GetGuid(12),
        UpdatedBy      = r.IsDBNull(13) ? null : r.GetGuid(13),
        CreatedAt      = r.GetDateTime(14),
        UpdatedAt      = r.GetDateTime(15)
    };
}
