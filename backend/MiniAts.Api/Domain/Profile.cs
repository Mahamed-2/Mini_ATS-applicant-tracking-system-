// Domain entity representing a user profile in the ATS system.
// Related: public.profiles table (supabase/migrations/0001_init.sql)
//          Application/DTOs/ProfileDto.cs
//          Infrastructure/Repositories/ProfileRepository.cs
//          Api/Middleware/CurrentUserMiddleware.cs

namespace MiniAts.Domain;

/// <summary>Application roles – mirrors public.user_role enum in Postgres.</summary>
public enum UserRole
{
    Admin,
    Customer
}

/// <summary>
/// Profile is the ATS representation of a Supabase Auth user.
/// Every auth.users row that can log into the ATS has a matching Profile.
/// </summary>
public class Profile
{
    // UUID matches auth.users.id – set by Supabase, not generated here.
    public Guid Id { get; set; }

    // Email must match auth.users.email for consistency.
    public string Email { get; set; } = string.Empty;

    // Role controls which API endpoints the user can call.
    public UserRole Role { get; set; } = UserRole.Customer;

    // Optional display name shown in the UI header.
    public string? DisplayName { get; set; }

    // Optional company name used for customer context.
    public string? CompanyName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
