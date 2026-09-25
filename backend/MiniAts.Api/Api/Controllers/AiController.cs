// AiController proxies the AI assessment request through to the Python AI service.
// Related: Application/Interfaces/Interfaces.cs (ICandidateService)
//          Application/Services/CandidateService.cs (AssessAsync loads candidate/job, calls AiClient)
//          Infrastructure/AiClient.cs (HTTP call to Python FastAPI)
//          backend/ai-service/main.py (POST /assess)
//          frontend/src/components/AiAssessmentPanel.vue (displays result)
//          frontend/src/stores/ats.ts (calls POST /api/ai/candidates/{id}/assess)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAts.Api.Middleware;
using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;

namespace MiniAts.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly ICandidateService _candidates;
    private readonly IPipelineReportService _reports;

    public AiController(ICandidateService candidates, IPipelineReportService reports)
    {
        _candidates = candidates;
        _reports    = reports;
    }

    // POST /api/ai/candidates/{id}/assess?customerId=uuid
    // Fetches candidate and job, calls Python AI service, stores result, returns JSON.
    [HttpPost("candidates/{id:guid}/assess")]
    public async Task<IActionResult> Assess(Guid id, [FromQuery] Guid customerId, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();

        try
        {
            var result = await _candidates.AssessAsync(id, caller.Id, role, customerId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Candidate not found." });
        }
        catch (InvalidOperationException ex)
        {
            // AI service returned an error or was unreachable.
            return StatusCode(502, new { message = ex.Message });
        }
    }

    // POST /api/ai/reports/pipeline
    // Gathers server-side aggregates and calls Python service for pipeline intelligence narrative.
    [HttpPost("reports/pipeline")]
    public async Task<IActionResult> GeneratePipelineReport(
        [FromBody] PipelineReportRequest? body,
        [FromQuery] Guid? customerId,
        CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();

        var targetCustomerId = body?.CustomerId ?? customerId;

        if (role == "admin" && (targetCustomerId is null || targetCustomerId == Guid.Empty))
        {
            return BadRequest(new { message = "Admin must supply a customerId for pipeline report generation." });
        }

        var effectiveCustomerId = targetCustomerId ?? caller.Id;

        try
        {
            var report = await _reports.GenerateReportAsync(caller.Id, role, effectiveCustomerId, ct);
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = $"Failed to generate pipeline report: {ex.Message}" });
        }
    }
}
