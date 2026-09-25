// AdminController exposes user creation and listing – admin role only.
// Related: Application/Interfaces/Interfaces.cs (IAdminUserService)
//          Application/Services/AdminUserService.cs
//          Application/Dtos/Dtos.cs (CreateUserRequest, ProfileDto)
//          Api/Middleware/CurrentUserMiddleware.cs (RequireCurrentProfile)
//          frontend/src/views/AdminView.vue (calls POST /api/admin/users)
//          Infrastructure/SupabaseAdminClient.cs (creates Supabase Auth user)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAts.Api.Middleware;
using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;

namespace MiniAts.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]  // JWT validation, role check done in methods
public class AdminController : ControllerBase
{
    private readonly IAdminUserService _adminUsers;

    public AdminController(IAdminUserService adminUsers) => _adminUsers = adminUsers;

    // POST /api/admin/users – create a new admin or customer account.
    // Body: { email, password, role, displayName, companyName }
    // Called by AdminView.vue; uses Supabase Auth Admin API server-side.
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest req,
        CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();

        // Enforce: only admin role may create accounts.
        if (caller.Role.ToString().ToLowerInvariant() != "admin")
            return Forbid();

        try
        {
            var profile = await _adminUsers.CreateUserAsync(req, ct);
            // Return 201 Created with location header and new profile.
            return CreatedAtAction(nameof(ListUsers), null, profile);
        }
        catch (ArgumentException ex)
        {
            // Invalid role or missing fields.
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Supabase Auth API returned an error.
            return StatusCode(502, new { message = ex.Message });
        }
    }

    // GET /api/admin/users – list all profiles.
    // Called by AdminView.vue to populate the user table and act-as selector.
    [HttpGet("users")]
    public async Task<IActionResult> ListUsers(CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();

        if (caller.Role.ToString().ToLowerInvariant() != "admin")
            return Forbid();

        var users = await _adminUsers.ListUsersAsync(ct);
        return Ok(users);
    }
}
