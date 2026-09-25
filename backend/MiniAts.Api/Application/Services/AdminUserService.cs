// AdminUserService creates and lists users via Supabase Auth Admin API + local profiles table.
// Related: Application/Interfaces/Interfaces.cs (IAdminUserService, ISupabaseAdminClient)
//          Infrastructure/SupabaseAdminClient.cs
//          Infrastructure/Repositories/ProfileRepository.cs
//          Api/Controllers/AdminController.cs

using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;
using MiniAts.Domain;

namespace MiniAts.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly ISupabaseAdminClient _supabaseAdmin;  // calls Supabase Auth Admin API
    private readonly IProfileRepository _profiles;          // writes to public.profiles

    public AdminUserService(ISupabaseAdminClient supabaseAdmin, IProfileRepository profiles)
    {
        _supabaseAdmin = supabaseAdmin;
        _profiles      = profiles;
    }

    public async Task<ProfileDto> CreateUserAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        // Validate role value before touching Supabase Auth.
        var role = req.Role.ToLowerInvariant() switch
        {
            "admin"    => UserRole.Admin,
            "customer" => UserRole.Customer,
            _          => throw new ArgumentException($"Unknown role '{req.Role}'. Use admin or customer.")
        };

        // Step 1: Create the user in Supabase Auth using the service role key (never exposed to frontend).
        var authUserId = await _supabaseAdmin.CreateAuthUserAsync(req.Email, req.Password, ct);

        // Step 2: Insert the matching profile row with the role and metadata.
        var profile = new Profile
        {
            Id          = authUserId,          // must match auth.users.id
            Email       = req.Email.ToLowerInvariant().Trim(),
            Role        = role,
            DisplayName = req.DisplayName?.Trim(),
            CompanyName = req.CompanyName?.Trim()
        };

        await _profiles.CreateAsync(profile, ct);

        // Return the profile DTO to the caller (AdminController).
        return new ProfileDto(
            profile.Id,
            profile.Email,
            profile.Role.ToString().ToLowerInvariant(),
            profile.DisplayName,
            profile.CompanyName);
    }

    public async Task<IReadOnlyList<ProfileDto>> ListUsersAsync(CancellationToken ct = default)
    {
        var profiles = await _profiles.ListAsync(ct);
        return profiles.Select(p => new ProfileDto(
            p.Id, p.Email,
            p.Role.ToString().ToLowerInvariant(),
            p.DisplayName, p.CompanyName)).ToList();
    }
}
