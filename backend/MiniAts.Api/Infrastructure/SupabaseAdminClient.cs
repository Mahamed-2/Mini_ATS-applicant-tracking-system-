// SupabaseAdminClient calls the Supabase Auth Admin API to create users server-side.
// Related: Application/Interfaces/Interfaces.cs (ISupabaseAdminClient)
//          Application/Services/AdminUserService.cs
//          appsettings.json Supabase:Url, Supabase:ServiceRoleKey
//          NEVER expose ServiceRoleKey to the frontend or any client-accessible code.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MiniAts.Application.Interfaces;

namespace MiniAts.Infrastructure;

public class SupabaseAdminClient : ISupabaseAdminClient
{
    private readonly HttpClient _http;
    private readonly string _adminApiBase;  // {Supabase:Url}/auth/v1/admin
    private readonly string _serviceRoleKey; // only used server-side

    public SupabaseAdminClient(HttpClient http, IConfiguration config)
    {
        _http = http;

        // Build the Admin API base URL from the Supabase project URL.
        var supabaseUrl = config["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase:Url is required.");
        _adminApiBase = $"{supabaseUrl.TrimEnd('/')}/auth/v1/admin";

        // Service role key grants admin access to Supabase Auth – never expose to clients.
        _serviceRoleKey = config["Supabase:ServiceRoleKey"]
            ?? throw new InvalidOperationException("Supabase:ServiceRoleKey is required.");
    }

    public async Task<Guid> CreateAuthUserAsync(string email, string password, CancellationToken ct = default)
    {
        // Build the Supabase Admin API request to create a confirmed user.
        var payload = new
        {
            email,
            password,
            email_confirm = true  // skip email verification for prototype
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_adminApiBase}/users");

        // Authenticate with the service role key – server-side only.
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
        request.Headers.Add("apikey", _serviceRoleKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Supabase Admin API error: {response.StatusCode} – {error}");
        }

        // Parse the response to extract the new auth user UUID.
        var body = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);

        var idString = doc.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Supabase did not return a user id.");

        return Guid.Parse(idString);
    }
}
