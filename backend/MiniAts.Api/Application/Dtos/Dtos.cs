// Application layer DTOs – shapes data crossing the API boundary.
// Related: Domain/Profile.cs, Domain/Job.cs, Domain/Candidate.cs
//          Api/Controllers/ (all controllers use these DTOs)
//          frontend/src/lib/api.ts (TypeScript mirror of these shapes)

namespace MiniAts.Application.Dtos;

// ── Profile ───────────────────────────────────────────────────────────────────

/// <summary>Response shape for GET /api/account/me and admin user list.</summary>
public record ProfileDto(
    Guid Id,
    string Email,
    string Role,           // lowercase "admin" or "customer" for frontend
    string? DisplayName,
    string? CompanyName
);

// ── User creation ─────────────────────────────────────────────────────────────

/// <summary>Request body for POST /api/admin/users.</summary>
public record CreateUserRequest(
    string Email,
    string Password,
    string Role,           // "admin" or "customer"
    string? DisplayName,
    string? CompanyName
);

// ── Job ───────────────────────────────────────────────────────────────────────

/// <summary>Response shape for job list and detail endpoints.</summary>
public record JobDto(
    Guid Id,
    Guid CustomerId,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>Request body for POST /api/jobs.</summary>
public record CreateJobRequest(
    Guid CustomerId,
    string Title,
    string? Description
);

/// <summary>Request body for PATCH /api/jobs/{id}.</summary>
public record UpdateJobRequest(
    string? Title,
    string? Description,
    string? Status
);

// ── Candidate ─────────────────────────────────────────────────────────────────

/// <summary>Response shape for candidate list and detail endpoints.</summary>
public record CandidateDto(
    Guid Id,
    Guid CustomerId,
    Guid? JobId,
    string FullName,
    string? Email,
    string? LinkedinUrl,
    string? CvText,
    string? Summary,
    string Stage,           // lowercase stage name for Kanban frontend
    decimal? AiScore,
    object? AiFeedback,    // deserialized JSON or null
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>Request body for POST /api/candidates.</summary>
public record CreateCandidateRequest(
    Guid CustomerId,
    Guid? JobId,
    string FullName,
    string? Email,
    string? LinkedinUrl,
    string? CvText,
    string? Summary,
    string Stage = "new"
);

/// <summary>Request body for PATCH /api/candidates/{id}.</summary>
public record UpdateCandidateRequest(
    Guid? JobId,
    string? FullName,
    string? Email,
    string? LinkedinUrl,
    string? CvText,
    string? Summary,
    string? Stage
);

/// <summary>Request body for PATCH /api/candidates/{id}/stage.</summary>
public record UpdateStageRequest(string Stage);

// ── AI ────────────────────────────────────────────────────────────────────────

/// <summary>Response returned to frontend after AI assessment completes.</summary>
public record AiAssessmentResult(
    int Score,
    string Summary,
    List<string> Strengths,
    List<string> Concerns,
    List<string> Questions,
    string Provider          // "mock" or "llm"
);
