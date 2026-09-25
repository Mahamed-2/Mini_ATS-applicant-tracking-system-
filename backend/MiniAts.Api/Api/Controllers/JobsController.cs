// JobsController handles CRUD for job postings – customer scoped.
// Related: Application/Interfaces/Interfaces.cs (IJobService)
//          Application/Services/JobService.cs
//          Application/Dtos/Dtos.cs (JobDto, CreateJobRequest, UpdateJobRequest)
//          Api/Middleware/CurrentUserMiddleware.cs (RequireCurrentProfile)
//          frontend/src/stores/ats.ts (calls all /api/jobs endpoints)
//          frontend/src/views/DashboardView.vue

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAts.Api.Middleware;
using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;

namespace MiniAts.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobs;

    public JobsController(IJobService jobs) => _jobs = jobs;

    // GET /api/jobs?customerId=uuid – list jobs for a customer.
    // Customer uses own id; admin supplies any customer id.
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid customerId, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();
        var result = await _jobs.ListAsync(caller.Id, role, customerId, ct);
        return Ok(result);
    }

    // POST /api/jobs – create a job. Body includes customerId.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest req, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();

        try
        {
            var job = await _jobs.CreateAsync(req, caller.Id, role, ct);
            return CreatedAtAction(nameof(Get), new { id = job.Id, customerId = job.CustomerId }, job);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/jobs/{id}?customerId=uuid – get a single job.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromQuery] Guid customerId, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();
        var job    = await _jobs.GetAsync(id, caller.Id, role, customerId, ct);
        return job is null ? NotFound() : Ok(job);
    }

    // PATCH /api/jobs/{id}?customerId=uuid – update title/description/status.
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromQuery] Guid customerId, [FromBody] UpdateJobRequest req, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();

        try
        {
            var job = await _jobs.UpdateAsync(id, req, caller.Id, role, customerId, ct);
            return Ok(job);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE /api/jobs/{id}?customerId=uuid – delete a job.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid customerId, CancellationToken ct)
    {
        var caller = HttpContext.RequireCurrentProfile();
        var role   = caller.Role.ToString().ToLowerInvariant();
        await _jobs.DeleteAsync(id, caller.Id, role, customerId, ct);
        return NoContent();
    }
}
