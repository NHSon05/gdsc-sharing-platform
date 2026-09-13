using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

[Route("api/admin/roadmaps")]
[Authorize(Policy = AuthPolicies.AdminPolicy)]
public sealed class AdminRoadmapsController(IRoadmapService roadmaps) : RoadmapApiController
{
    [HttpGet]
    public async Task<ActionResult<PageResponse<RoadmapSummary>>> List([FromQuery] RoadmapQuery query, CancellationToken ct)
        => Ok(await roadmaps.ListAsync(query, true, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoadmapResponse>> Get(Guid id, CancellationToken ct)
        => Ok(await roadmaps.GetAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(RoadmapResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RoadmapResponse>> Create(RoadmapRequest request, CancellationToken ct)
    {
        var response = await roadmaps.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RoadmapResponse>> Update(Guid id, RoadmapRequest request, CancellationToken ct)
        => Ok(await roadmaps.UpdateAsync(id, request, ct));

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Status(Guid id, RoadmapStatusRequest request, CancellationToken ct)
    {
        await roadmaps.SetStatusAsync(id, request, ct);
        return NoContent();
    }

    [HttpPatch("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reorder(ReorderRequest request, CancellationToken ct)
    {
        await roadmaps.ReorderAsync(request, ct);
        return NoContent();
    }
}
