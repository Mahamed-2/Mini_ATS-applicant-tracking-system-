// Domain entity representing a job candidate owned by a customer.
// Related: public.candidates table (supabase/migrations/0001_init.sql)
//          Application/DTOs/CandidateDto.cs
//          Infrastructure/Repositories/CandidateRepository.cs
//          Api/Controllers/CandidatesController.cs
//          Api/Controllers/AiController.cs (stores ai_score, ai_feedback)

namespace MiniAts.Domain;

/// <summary>Kanban stages – mirrors public.candidate_stage enum in Postgres.</summary>
public enum CandidateStage
{
    New,
    Screening,
    Interview,
    Offer,
    Hired,
    Rejected
}

/// <summary>
/// A Candidate belongs to a customer and optionally links to a Job.
/// Stage determines which Kanban column the card appears in.
/// AiScore and AiFeedback are populated by the AI assessment endpoint.
/// </summary>
public class Candidate
{
    public Guid Id { get; set; }

    // CustomerId scopes this candidate to one customer.
    public Guid CustomerId { get; set; }

    // JobId is optional; null means candidate is not yet linked to a position.
    public Guid? JobId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }

    // LinkedIn URL stored for manual verification and AI context.
    public string? LinkedinUrl { get; set; }

    // Future: Supabase Storage object path for uploaded CV file.
    public string? CvStoragePath { get; set; }

    // Free-form CV text used by Python AI service for assessment.
    public string? CvText { get; set; }

    // Short profile summary displayed on the Kanban card.
    public string? Summary { get; set; }

    // Stage drives Kanban column placement; default is New.
    public CandidateStage Stage { get; set; } = CandidateStage.New;

    // AI score 0–100 stored after POST /api/ai/candidates/{id}/assess.
    public decimal? AiScore { get; set; }

    // Full AI response JSON stored as a string; parsed on demand by frontend.
    public string? AiFeedback { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
