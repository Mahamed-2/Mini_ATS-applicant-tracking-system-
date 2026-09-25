// AccountController exposes the current user's profile via GET /api/account/me.
// Related: Application/Interfaces/Interfaces.cs (IProfileRepository)
//          Application/Dtos/Dtos.cs (ProfileDto)
//          Api/Middleware/CurrentUserMiddleware.cs (loads profile into context)
//          frontend/src/stores/auth.ts (calls /api/account/me after login)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAts.Api.Middleware;

namespace MiniAts.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]  // requires valid Supabase JWT
public class AccountController : ControllerBase
{
    // GET /api/account/me – returns the authenticated user's profile.
    // Called by the frontend auth store immediately after login to load role and id.
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        // Profile is loaded by CurrentUserMiddleware from public.profiles.
        var profile = HttpContext.RequireCurrentProfile();

        return Ok(new
        {
            id          = profile.Id,
            email       = profile.Email,
            role        = profile.Role.ToString().ToLowerInvariant(),
            displayName = profile.DisplayName,
            companyName = profile.CompanyName
        });
    }
}
