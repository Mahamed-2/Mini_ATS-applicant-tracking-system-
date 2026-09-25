// Domain entity representing a job posting owned by a customer.
// Related: public.jobs table (supabase/migrations/0001_init.sql)
//          Application/DTOs/JobDto.cs
//          Infrastructure/Repositories/JobRepository.cs
//          Api/Controllers/JobsController.cs

namespace MiniAts.Domain;

/// <summary>
/// A Job is a hiring position created by a customer.
/// All queries must scope by CustomerId to enforce customer isolation.
/// </summary>
public class Job
{
    public Guid Id { get; set; }

    // CustomerId is the owning customer's profile id – always filter by this.
    public Guid CustomerId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Status defaults to 'active'; can be 'closed' or 'archived'.
    public string Status { get; set; } = "active";

    // Audit columns set server-side by services, not by the client.
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
