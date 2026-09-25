// Program.cs – application entry point and DI composition root.
// Related: appsettings.json (all config keys read here)
//          Api/Middleware/CurrentUserMiddleware.cs (registered in pipeline)
//          Application/Services/ (registered below)
//          Infrastructure/Repositories/ (registered below)
//          Infrastructure/SupabaseAdminClient.cs
//          Infrastructure/AiClient.cs

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MiniAts.Api.Middleware;
using MiniAts.Application.Interfaces;
using MiniAts.Application.Services;
using MiniAts.Infrastructure;
using MiniAts.Infrastructure.Repositories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ─────────────────────────────────────────────────────────────

// Read Supabase values needed for JWT validation and admin API calls.
var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase:Url is required in configuration.");
var supabaseJwtSecret = builder.Configuration["Supabase:JwtSecret"]
    ?? throw new InvalidOperationException("Supabase:JwtSecret is required in configuration.");

// ── JWT Authentication ────────────────────────────────────────────────────────

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Supabase signs tokens with ES256 (JWKS) or HS256 (symmetric).
        // Setting Authority automatically discovers keys via .well-known/openid-configuration and jwks.json.
        options.Authority = $"{supabaseUrl.TrimEnd('/')}/auth/v1";

        // Prevent ASP.NET from mapping JWT claims like 'sub' to legacy XML schema URIs.
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidAudience = "authenticated";
        options.TokenValidationParameters.ValidIssuers = new[]
        {
            $"{supabaseUrl.TrimEnd('/')}/auth/v1",
            $"{supabaseUrl.TrimEnd('/')}/auth/v1/",
            "supabase"
        };

        // Allow 1-minute clock skew for client/server time differences.
        options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(1);
    });

// Admin policy enforced in controllers via Forbid() + manual role check.
builder.Services.AddAuthorization();

// ── Postgres / Npgsql ────────────────────────────────────────────────────────

builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    // Build a pooled data source for async parameterized SQL.
    var cs = builder.Configuration.GetConnectionString("Supabase")
        ?? throw new InvalidOperationException("ConnectionStrings:Supabase is required.");
    return NpgsqlDataSource.Create(cs);
});

// ── CORS ─────────────────────────────────────────────────────────────────────

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opts =>
{
    // Named "Frontend" policy allows the Vercel app and local dev server.
    opts.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// ── Application services ─────────────────────────────────────────────────────

// Repositories – infrastructure layer; contain all SQL.
builder.Services.AddScoped<IProfileRepository,   ProfileRepository>();
builder.Services.AddScoped<IJobRepository,        JobRepository>();
builder.Services.AddScoped<ICandidateRepository,  CandidateRepository>();

// Services – application layer; contain business rules and authorization logic.
builder.Services.AddScoped<IJobService,            JobService>();
builder.Services.AddScoped<ICandidateService,      CandidateService>();
builder.Services.AddScoped<IAdminUserService,      AdminUserService>();
builder.Services.AddScoped<IPipelineReportService,  PipelineReportService>();

// External clients – infrastructure layer; call Supabase Auth Admin API and Python AI service.
builder.Services.AddHttpClient<ISupabaseAdminClient, SupabaseAdminClient>();
builder.Services.AddHttpClient<IAiClient,             AiClient>();

// ── ASP.NET Core ──────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Mini ATS API", Version = "v1" });
    // Add JWT bearer scheme to Swagger UI for easy prototype testing.
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// ── Build app ─────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger UI available at /swagger during local development.
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS must run before auth so preflight OPTIONS requests succeed.
app.UseCors("Frontend");

// Validate Supabase JWT; sets HttpContext.User claims.
app.UseAuthentication();

// Load profile from public.profiles and attach to HttpContext.Items.
app.UseMiddleware<CurrentUserMiddleware>();

// Enforce [Authorize] attributes on controllers.
app.UseAuthorization();

// Map REST endpoints.
app.MapControllers();

// Health check for Railway deployment probe.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
