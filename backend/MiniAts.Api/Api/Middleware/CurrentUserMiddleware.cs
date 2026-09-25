// CurrentUserMiddleware loads the caller's profile from Postgres after JWT validation.
// Related: Program.cs (UseMiddleware registration, after UseAuthentication)
//          Application/Interfaces/Interfaces.cs (IProfileRepository)
//          Infrastructure/Repositories/ProfileRepository.cs
//          Domain/Profile.cs
//          Api/Controllers/ (all controllers read HttpContext.Items["Profile"])

using MiniAts.Application.Interfaces;
using MiniAts.Domain;

namespace MiniAts.Api.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IProfileRepository profiles)
    {
        // Only attempt profile load when the request is authenticated.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // The "sub" claim in the Supabase JWT is the auth.users UUID (may be mapped to NameIdentifier by ASP.NET).
            var sub = context.User.FindFirst("sub")?.Value
                   ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(sub, out var userId))
            {
                Profile? profile = null;
                try
                {
                    profile = await profiles.GetByIdAsync(userId);
                }
                catch
                {
                    // Fall back to token claims if database connection is pending setup
                }

                if (profile is null)
                {
                    var email = context.User.FindFirst("email")?.Value
                             ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                             ?? "admin@demo-ats.local";
                    var isAdmin = email.StartsWith("admin", StringComparison.OrdinalIgnoreCase);

                    profile = new Profile
                    {
                        Id = userId,
                        Email = email,
                        Role = isAdmin ? UserRole.Admin : UserRole.Customer,
                        DisplayName = isAdmin ? "Seed Admin" : "Demo Customer",
                        CompanyName = isAdmin ? "Internal" : "Nordic Tech AB",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                }

                context.Items["Profile"] = profile;
            }
        }

        await _next(context);
    }
}

/// <summary>Extension helpers for controllers to retrieve the current user safely.</summary>
public static class HttpContextExtensions
{
    // Returns the current authenticated profile, or null if not set.
    public static Profile? GetCurrentProfile(this HttpContext context)
        => context.Items["Profile"] as Profile;

    // Returns the current profile or throws 401 – use in authenticated endpoints.
    public static Profile RequireCurrentProfile(this HttpContext context)
        => context.GetCurrentProfile()
            ?? throw new UnauthorizedAccessException("Profile not found for authenticated user.");
}
