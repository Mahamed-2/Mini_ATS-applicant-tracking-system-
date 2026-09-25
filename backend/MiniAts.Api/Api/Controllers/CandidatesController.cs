// CandidatesController handles CRUD, stage moves, and AI assessment for candidates.
// Related: Application/Interfaces/Interfaces.cs (ICandidateService)
//          Application/Services/CandidateService.cs
//          Application/Dtos/Dtos.cs (CandidateDto, CreateCandidateRequest, UpdateStageRequest)
//          Api/Middleware/CurrentUserMiddleware.cs
//          frontend/src/stores/ats.ts (all candidate API calls)
//          frontend/src/views/KanbanView.vue (stage moves, filters)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAts.Api.Middleware;
using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;

namespace MiniAts.Api.Controllers;

[ApiController]
[Route("api/candidates")]
[Authorize]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidates;

    public CandidatesController(ICandidateService candidates) => _candidates = candidates;

    // GET /api/candidates?customerId=&jobId=&search= – drives Kanban data loading.
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid customerId,
        [FromQuery] Guid? jobId,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();
        var result = await _candidates.ListAsync(caller.Id, role, customerId, jobId, search, ct);
        return Ok(result);
    }

    // POST /api/candidates – create a candidate with profile data.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCandidateRequest req, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();

        try
        {
            var candidate = await _candidates.CreateAsync(req, caller.Id, role, ct);
            return CreatedAtAction(nameof(Get),
                new { id = candidate.Id, customerId = candidate.CustomerId }, candidate);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/candidates/{id}?customerId= – get candidate detail.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromQuery] Guid customerId, CancellationToken ct)
    {
        var caller    = HttpContext.RequireCurrentProfile();
        var role      = caller.Role.ToString().ToLowerInvariant();
        var candidate = await _candidates.GetAsync(id, caller.Id, role, customerId, ct);
        return candidate is null ? NotFound() : Ok(candidate);
    }

    // PATCH /api/candidates/{id}?customerId= – update candidate fields.
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromQuery] Guid customerId, [FromBody] UpdateCandidateRequest req, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();

        try
        {
            var candidate = await _candidates.UpdateAsync(id, req, caller.Id, role, customerId, ct);
            return Ok(candidate);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // PATCH /api/candidates/{id}/stage – Kanban stage move.
    // Body: { "stage": "screening" }
    [HttpPatch("{id:guid}/stage")]
    public async Task<IActionResult> MoveStage(
        Guid id, [FromQuery] Guid customerId, [FromBody] UpdateStageRequest req, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();

        try
        {
            await _candidates.MoveStageAsync(id, req.Stage, caller.Id, role, customerId, ct);
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE /api/candidates/{id}?customerId= – remove a candidate.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid customerId, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();
        await _candidates.DeleteAsync(id, caller.Id, role, customerId, ct);
        return NoContent();
    }
}
