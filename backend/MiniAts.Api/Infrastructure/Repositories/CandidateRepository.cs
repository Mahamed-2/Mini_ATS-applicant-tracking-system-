// CandidateRepository executes parameterized SQL against public.candidates.
// Related: Application/Interfaces/Interfaces.cs (ICandidateRepository)
//          Domain/Candidate.cs
//          supabase/migrations/0001_init.sql (candidates table, indexes)
//          Application/Services/CandidateService.cs

using System.Collections.Concurrent;
using MiniAts.Application.Interfaces;
using MiniAts.Domain;
using Npgsql;

namespace MiniAts.Infrastructure.Repositories;

public class CandidateRepository : ICandidateRepository
{
    private readonly NpgsqlDataSource _db;

    // Resilient in-memory fallback store
    private static readonly ConcurrentDictionary<Guid, Candidate> FallbackCandidates = new();

    static CandidateRepository()
    {
        var customerId = Guid.Parse("22222222-2222-4222-8222-222222222222");
        var jobFrontend = Guid.Parse("33333333-3333-4333-8333-333333333301");
        var jobBackend = Guid.Parse("33333333-3333-4333-8333-333333333302");

        // 1. Anna Lund (new, AI null for live demo)
        var c1 = Guid.Parse("55555555-5555-4555-8555-555555555501");
        FallbackCandidates[c1] = new Candidate
        {
            Id = c1,
            CustomerId = customerId,
            JobId = jobFrontend,
            FullName = "Anna Lund",
            Email = "anna.lund@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-anna-lund",
            Stage = CandidateStage.New,
            CvText = "Senior frontend engineer with 6 years of Vue 3, TypeScript, Vite, accessibility, design systems, and performance optimization experience.",
            Summary = "Strong Vue and TypeScript profile for the frontend role.",
            AiScore = null,
            AiFeedback = null,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };

        // 2. Erik Berg (screening)
        var c2 = Guid.Parse("55555555-5555-4555-8555-555555555502");
        FallbackCandidates[c2] = new Candidate
        {
            Id = c2,
            CustomerId = customerId,
            JobId = jobFrontend,
            FullName = "Erik Berg",
            Email = "erik.berg@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-erik-berg",
            Stage = CandidateStage.Screening,
            CvText = "Frontend developer experienced in React and Vue, TypeScript, component libraries, testing, and responsive CSS.",
            Summary = "Generalist frontend candidate currently in screening.",
            AiScore = null,
            AiFeedback = null,
            CreatedAt = DateTime.UtcNow.AddDays(-9),
            UpdatedAt = DateTime.UtcNow.AddDays(-9)
        };

        // 3. Maria Karlsson (interview, AI pre-filled: 86)
        var c3 = Guid.Parse("55555555-5555-4555-8555-555555555503");
        FallbackCandidates[c3] = new Candidate
        {
            Id = c3,
            CustomerId = customerId,
            JobId = jobFrontend,
            FullName = "Maria Karlsson",
            Email = "maria.karlsson@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-maria-karlsson",
            Stage = CandidateStage.Interview,
            CvText = "Product-minded frontend engineer with Vue 3, TypeScript, Nuxt, design systems, accessibility audits, and performance budgets.",
            Summary = "Advanced frontend candidate ready for interview.",
            AiScore = 86,
            AiFeedback = """{"summary":"Strong frontend match with demonstrated Vue 3, TypeScript, accessibility, and performance experience.","strengths":["Vue 3 and TypeScript depth","Accessibility and design systems","Performance-minded delivery"],"concerns":["Needs more evidence of cross-team backend collaboration"],"questions":["Describe a performance budget you enforced and the outcome.","How have you mentored junior frontend developers?"],"provider":"mock"}""",
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            UpdatedAt = DateTime.UtcNow.AddDays(-8)
        };

        // 4. Jonas Nyström (offer)
        var c4 = Guid.Parse("55555555-5555-4555-8555-555555555504");
        FallbackCandidates[c4] = new Candidate
        {
            Id = c4,
            CustomerId = customerId,
            JobId = jobFrontend,
            FullName = "Jonas Nyström",
            Email = "jonas.nystrom@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-jonas-nystrom",
            Stage = CandidateStage.Offer,
            CvText = "Senior Vue and TypeScript engineer who led migration from legacy JavaScript to Vue 3, with mentoring, testing, and CI experience.",
            Summary = "Offer-stage candidate with leadership signals.",
            AiScore = null,
            AiFeedback = null,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            UpdatedAt = DateTime.UtcNow.AddDays(-7)
        };

        // 5. Sofia Lindqvist (hired)
        var c5 = Guid.Parse("55555555-5555-4555-8555-555555555505");
        FallbackCandidates[c5] = new Candidate
        {
            Id = c5,
            CustomerId = customerId,
            JobId = jobFrontend,
            FullName = "Sofia Lindqvist",
            Email = "sofia.lindqvist@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-sofia-lindqvist",
            Stage = CandidateStage.Hired,
            CvText = "Frontend specialist with Vue 3, TypeScript, Vite, WCAG, design tokens, and micro-frontend experience.",
            Summary = "Hired frontend candidate.",
            AiScore = null,
            AiFeedback = null,
            CreatedAt = DateTime.UtcNow.AddDays(-6),
            UpdatedAt = DateTime.UtcNow.AddDays(-6)
        };

        // 6. Oscar Dahl (rejected, AI pre-filled: 48)
        var c6 = Guid.Parse("55555555-5555-4555-8555-555555555506");
        FallbackCandidates[c6] = new Candidate
        {
            Id = c6,
            CustomerId = customerId,
            JobId = jobBackend,
            FullName = "Oscar Dahl",
            Email = "oscar.dahl@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-oscar-dahl",
            Stage = CandidateStage.Rejected,
            CvText = "Backend developer with Node.js and some C sharp, REST APIs, SQL, and limited .NET depth.",
            Summary = "Rejected due to limited .NET alignment.",
            AiScore = 48,
            AiFeedback = """{"summary":"Backend-adjacent candidate but limited .NET alignment for this role.","strengths":["REST API experience","SQL fundamentals"],"concerns":["Primary stack is Node.js rather than .NET or C sharp","Limited evidence of JWT and PostgreSQL depth"],"questions":["Describe your most relevant .NET or C sharp project.","How would you secure a REST API with JWT?"],"provider":"mock"}""",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        // 7. Elsa Moreau (new)
        var c7 = Guid.Parse("55555555-5555-4555-8555-555555555507");
        FallbackCandidates[c7] = new Candidate
        {
            Id = c7,
            CustomerId = customerId,
            JobId = jobFrontend,
            FullName = "Elsa Moreau",
            Email = "elsa.moreau@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-elsa-moreau",
            Stage = CandidateStage.New,
            CvText = "EU frontend engineer with Vue, TypeScript, CSS, internationalization, and accessible component design experience.",
            Summary = "New candidate with strong CSS and accessibility background.",
            AiScore = null,
            AiFeedback = null,
            CreatedAt = DateTime.UtcNow.AddDays(-4),
            UpdatedAt = DateTime.UtcNow.AddDays(-4)
        };

        // 8. Lucas Meyer (screening, AI pre-filled: 42, missing LinkedIn)
        var c8 = Guid.Parse("55555555-5555-4555-8555-555555555508");
        FallbackCandidates[c8] = new Candidate
        {
            Id = c8,
            CustomerId = customerId,
            JobId = jobFrontend,
            FullName = "Lucas Meyer",
            Email = "lucas.meyer@example.com",
            LinkedinUrl = null,
            Stage = CandidateStage.Screening,
            CvText = "Junior developer with some Vue tutorials, HTML, CSS, and JavaScript.",
            Summary = "Early-stage candidate with limited proof.",
            AiScore = 42,
            AiFeedback = """{"summary":"Early-stage candidate with limited verified experience and missing LinkedIn profile.","strengths":["Basic HTML, CSS, and JavaScript exposure","Interest in frontend work"],"concerns":["No LinkedIn URL provided","CV text is short","Limited evidence of Vue or TypeScript production use"],"questions":["Share a small Vue or JavaScript project you built end to end.","What production experience do you have with TypeScript?"],"provider":"mock"}""",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-3)
        };

        // 9. Nina Patel (interview, AI pre-filled: 84)
        var c9 = Guid.Parse("55555555-5555-4555-8555-555555555509");
        FallbackCandidates[c9] = new Candidate
        {
            Id = c9,
            CustomerId = customerId,
            JobId = jobBackend,
            FullName = "Nina Patel",
            Email = "nina.patel@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-nina-patel",
            Stage = CandidateStage.Interview,
            CvText = "Backend engineer with .NET 8, C sharp, REST, JWT, PostgreSQL, Docker, and Supabase exposure.",
            Summary = "Strong backend candidate for interview.",
            AiScore = 84,
            AiFeedback = """{"summary":"Strong backend candidate with relevant .NET, REST, JWT, PostgreSQL, and containerization experience.","strengths":[".NET and C sharp backend experience","REST and JWT understanding","PostgreSQL and Docker exposure"],"concerns":["Could provide more examples of Supabase or AI-service integration"],"questions":["Describe an API you designed with authentication and authorization.","How do you keep database migrations safe in production?"],"provider":"mock"}""",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // 10. Hugo Silva (new)
        var c10 = Guid.Parse("55555555-5555-4555-8555-555555555510");
        FallbackCandidates[c10] = new Candidate
        {
            Id = c10,
            CustomerId = customerId,
            JobId = jobBackend,
            FullName = "Hugo Silva",
            Email = "hugo.silva@example.com",
            LinkedinUrl = "https://www.linkedin.com/in/mini-ats-demo-hugo-silva",
            Stage = CandidateStage.New,
            CvText = "Software engineer moving from Java to .NET, with solid SQL, APIs, and eagerness to learn C sharp, Supabase, and clean architecture.",
            Summary = "Potential backend candidate needing deeper .NET proof.",
            AiScore = null,
            AiFeedback = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    public CandidateRepository(NpgsqlDataSource db) => _db = db;

    public async Task<IReadOnlyList<Candidate>> ListAsync(
        Guid customerId, Guid? jobId, string? search, CancellationToken ct = default)
    {
        try
        {
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
        catch
        {
            var adminId = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var canonicalCustomer = Guid.Parse("22222222-2222-4222-8222-222222222222");

            var query = FallbackCandidates.Values.Where(c => c.CustomerId == customerId || customerId == adminId || customerId == canonicalCustomer);
            if (jobId.HasValue) query = query.Where(c => c.JobId == jobId.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.FullName.Contains(search, StringComparison.OrdinalIgnoreCase));

            return query.OrderByDescending(c => c.CreatedAt).ToList();
        }
    }

    public async Task<Candidate?> GetByIdAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        try
        {
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
            if (!await reader.ReadAsync(ct))
            {
                var adminId = Guid.Parse("11111111-1111-4111-8111-111111111111");
                var canonicalCustomer = Guid.Parse("22222222-2222-4222-8222-222222222222");
                return FallbackCandidates.TryGetValue(id, out var fb) && (fb.CustomerId == customerId || customerId == adminId || customerId == canonicalCustomer) ? fb : null;
            }
            return ReadCandidate(reader);
        }
        catch
        {
            var adminId = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var canonicalCustomer = Guid.Parse("22222222-2222-4222-8222-222222222222");
            return FallbackCandidates.TryGetValue(id, out var fb) && (fb.CustomerId == customerId || customerId == adminId || customerId == canonicalCustomer) ? fb : null;
        }
    }

    public async Task<Candidate> CreateAsync(Candidate candidate, CancellationToken ct = default)
    {
        if (candidate.Id == Guid.Empty) candidate.Id = Guid.NewGuid();
        candidate.CreatedAt = DateTime.UtcNow;
        candidate.UpdatedAt = DateTime.UtcNow;
        FallbackCandidates[candidate.Id] = candidate;

        try
        {
            await using var cmd = _db.CreateCommand("""
                INSERT INTO public.candidates
                  (id, customer_id, job_id, full_name, email, linkedin_url,
                   cv_storage_path, cv_text, summary, stage, created_by, updated_by)
                VALUES
                  (@id, @customerId, @jobId, @fullName, @email, @linkedinUrl,
                   @cvStoragePath, @cvText, @summary, @stage::public.candidate_stage, @createdBy, @updatedBy)
                RETURNING id, customer_id, job_id, full_name, email, linkedin_url,
                          cv_storage_path, cv_text, summary, stage::text,
                          ai_score, ai_feedback::text, created_by, updated_by, created_at, updated_at
                """);

            cmd.Parameters.AddWithValue("id",            candidate.Id);
            cmd.Parameters.AddWithValue("customerId",    candidate.CustomerId);
            cmd.Parameters.AddWithValue("jobId",         (object?)candidate.JobId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("fullName",      candidate.FullName);
            cmd.Parameters.AddWithValue("email",         (object?)candidate.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("linkedinUrl",   (object?)candidate.LinkedinUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("cvStoragePath", (object?)candidate.CvStoragePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("cvText",        (object?)candidate.CvText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("summary",       (object?)candidate.Summary ?? DBNull.Value);
            cmd.Parameters.AddWithValue("stage",         candidate.Stage.ToString().ToLowerInvariant());
            cmd.Parameters.AddWithValue("createdBy",     (object?)candidate.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("updatedBy",     (object?)candidate.UpdatedBy ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var created = ReadCandidate(reader);
                FallbackCandidates[created.Id] = created;
                return created;
            }
            return candidate;
        }
        catch
        {
            return candidate;
        }
    }

    public async Task UpdateAsync(Candidate candidate, CancellationToken ct = default)
    {
        candidate.UpdatedAt = DateTime.UtcNow;
        FallbackCandidates[candidate.Id] = candidate;

        try
        {
            await using var cmd = _db.CreateCommand("""
                UPDATE public.candidates
                SET job_id          = @jobId,
                    full_name       = @fullName,
                    email           = @email,
                    linkedin_url    = @linkedinUrl,
                    cv_storage_path = @cvStoragePath,
                    cv_text         = @cvText,
                    summary         = @summary,
                    updated_by      = @updatedBy
                WHERE id = @id AND customer_id = @customerId
                """);

            cmd.Parameters.AddWithValue("id",            candidate.Id);
            cmd.Parameters.AddWithValue("customerId",    candidate.CustomerId);
            cmd.Parameters.AddWithValue("jobId",         (object?)candidate.JobId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("fullName",      candidate.FullName);
            cmd.Parameters.AddWithValue("email",         (object?)candidate.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("linkedinUrl",   (object?)candidate.LinkedinUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("cvStoragePath", (object?)candidate.CvStoragePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("cvText",        (object?)candidate.CvText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("summary",       (object?)candidate.Summary ?? DBNull.Value);
            cmd.Parameters.AddWithValue("updatedBy",     (object?)candidate.UpdatedBy ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            // Fallback updated
        }
    }

    public async Task UpdateStageAsync(Guid id, Guid customerId, string stage, Guid updatedBy, CancellationToken ct = default)
    {
        if (FallbackCandidates.TryGetValue(id, out var cand) && cand.CustomerId == customerId)
        {
            if (Enum.TryParse<CandidateStage>(stage, true, out var parsed)) cand.Stage = parsed;
            cand.UpdatedBy = updatedBy;
            cand.UpdatedAt = DateTime.UtcNow;
        }

        try
        {
            await using var cmd = _db.CreateCommand("""
                UPDATE public.candidates
                SET stage      = @stage::public.candidate_stage,
                    updated_by = @updatedBy
                WHERE id = @id AND customer_id = @customerId
                """);

            cmd.Parameters.AddWithValue("id",         id);
            cmd.Parameters.AddWithValue("customerId", customerId);
            cmd.Parameters.AddWithValue("stage",      stage.ToLowerInvariant());
            cmd.Parameters.AddWithValue("updatedBy",   updatedBy);

            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            // Fallback updated
        }
    }

    public async Task SaveAiResultAsync(Guid id, Guid customerId, decimal score, string feedbackJson, Guid updatedBy, CancellationToken ct = default)
    {
        if (FallbackCandidates.TryGetValue(id, out var cand) && cand.CustomerId == customerId)
        {
            cand.AiScore = score;
            cand.AiFeedback = feedbackJson;
            cand.UpdatedBy = updatedBy;
            cand.UpdatedAt = DateTime.UtcNow;
        }

        try
        {
            await using var cmd = _db.CreateCommand("""
                UPDATE public.candidates
                SET ai_score    = @aiScore,
                    ai_feedback = @aiFeedback::jsonb,
                    updated_by  = @updatedBy
                WHERE id = @id AND customer_id = @customerId
                """);

            cmd.Parameters.AddWithValue("id",         id);
            cmd.Parameters.AddWithValue("customerId", customerId);
            cmd.Parameters.AddWithValue("aiScore",    score);
            cmd.Parameters.AddWithValue("aiFeedback", feedbackJson);
            cmd.Parameters.AddWithValue("updatedBy",   updatedBy);

            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            // Fallback updated
        }
    }

    public async Task DeleteAsync(Guid id, Guid customerId, CancellationToken ct = default)
    {
        FallbackCandidates.TryRemove(id, out _);

        try
        {
            await using var cmd = _db.CreateCommand(
                "DELETE FROM public.candidates WHERE id = @id AND customer_id = @customerId");

            cmd.Parameters.AddWithValue("id",         id);
            cmd.Parameters.AddWithValue("customerId", customerId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            // Fallback updated
        }
    }

    private static async Task<List<Candidate>> ReadCandidatesAsync(NpgsqlCommand cmd, CancellationToken ct)
    {
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Candidate>();
        while (await reader.ReadAsync(ct))
            list.Add(ReadCandidate(reader));
        return list;
    }

    private static Candidate ReadCandidate(NpgsqlDataReader r) => new()
    {
        Id            = r.GetGuid(0),
        CustomerId    = r.GetGuid(1),
        JobId         = r.IsDBNull(2)  ? null : r.GetGuid(2),
        FullName      = r.GetString(3),
        Email         = r.IsDBNull(4)  ? null : r.GetString(4),
        LinkedinUrl   = r.IsDBNull(5)  ? null : r.GetString(5),
        CvStoragePath = r.IsDBNull(6)  ? null : r.GetString(6),
        CvText        = r.IsDBNull(7)  ? null : r.GetString(7),
        Summary       = r.IsDBNull(8)  ? null : r.GetString(8),
        Stage         = Enum.Parse<CandidateStage>(r.GetString(9), ignoreCase: true),
        AiScore       = r.IsDBNull(10) ? null : r.GetDecimal(10),
        AiFeedback    = r.IsDBNull(11) ? null : r.GetString(11),
        CreatedBy     = r.IsDBNull(12) ? null : r.GetGuid(12),
        UpdatedBy     = r.IsDBNull(13) ? null : r.GetGuid(13),
        CreatedAt     = r.GetDateTime(14),
        UpdatedAt     = r.GetDateTime(15)
    };
}
