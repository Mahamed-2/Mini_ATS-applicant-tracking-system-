// ProfileRepository executes parameterized SQL against public.profiles.
// Related: Application/Interfaces/Interfaces.cs (IProfileRepository)
//          Domain/Profile.cs
//          supabase/migrations/0001_init.sql (profiles table)
//          Api/Middleware/CurrentUserMiddleware.cs (calls GetByIdAsync on every request)

using MiniAts.Application.Interfaces;
using MiniAts.Domain;
using Npgsql;

namespace MiniAts.Infrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    // NpgsqlDataSource is a singleton registered in Program.cs; provides connection pooling.
    private readonly NpgsqlDataSource _db;

    public ProfileRepository(NpgsqlDataSource db) => _db = db;

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Load a profile by its auth UUID – called on every authenticated request.
        await using var cmd = _db.CreateCommand("""
            SELECT id, email, role, display_name, company_name, created_at, updated_at
            FROM public.profiles
            WHERE id = @id
            """);

        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return ReadProfile(reader);
    }

    public async Task<IReadOnlyList<Profile>> ListAsync(CancellationToken ct = default)
    {
        // List all profiles – admin only, used by GET /api/admin/users.
        await using var cmd = _db.CreateCommand("""
            SELECT id, email, role, display_name, company_name, created_at, updated_at
            FROM public.profiles
            ORDER BY created_at DESC
            """);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Profile>();
        while (await reader.ReadAsync(ct))
            list.Add(ReadProfile(reader));

        return list;
    }

    public async Task CreateAsync(Profile profile, CancellationToken ct = default)
    {
        // Insert profile row created after Supabase Auth user is created.
        // ON CONFLICT allows re-running without duplicate key errors.
        await using var cmd = _db.CreateCommand("""
            INSERT INTO public.profiles (id, email, role, display_name, company_name)
            VALUES (@id, @email, @role::public.user_role, @displayName, @companyName)
            ON CONFLICT (id) DO UPDATE SET
              email        = EXCLUDED.email,
              role         = EXCLUDED.role,
              display_name = EXCLUDED.display_name,
              company_name = EXCLUDED.company_name
            """);

        cmd.Parameters.AddWithValue("id",          profile.Id);
        cmd.Parameters.AddWithValue("email",       profile.Email);
        cmd.Parameters.AddWithValue("role",        profile.Role.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("displayName", (object?)profile.DisplayName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("companyName", (object?)profile.CompanyName ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    // Map a reader row to a Profile entity; centralizes column-to-property mapping.
    private static Profile ReadProfile(NpgsqlDataReader r) => new()
    {
        Id          = r.GetGuid(0),
        Email       = r.GetString(1),
        Role        = Enum.Parse<UserRole>(r.GetString(2), ignoreCase: true),
        DisplayName = r.IsDBNull(3) ? null : r.GetString(3),
        CompanyName = r.IsDBNull(4) ? null : r.GetString(4),
        CreatedAt   = r.GetDateTime(5),
        UpdatedAt   = r.GetDateTime(6)
    };
}
