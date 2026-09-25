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
            // The "sub" claim in the Supabase JWT is the auth.users UUID.
            var sub = context.User.FindFirst("sub")?.Value;

            if (Guid.TryParse(sub, out var userId))
            {
                // Load the profile to get role and customer metadata.
                var profile = await profiles.GetByIdAsync(userId);

                if (profile is not null)
                {
                    // Attach profile to the request context for controllers to read.
                    context.Items["Profile"] = profile;
                }
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
