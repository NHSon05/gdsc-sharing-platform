using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

[Route("api/admin/roadmaps/{roadmapId:guid}/nodes")]
[Authorize(Policy = AuthPolicies.AdminPolicy)]
public sealed class AdminRoadmapNodesController(IRoadmapNodeService nodes) : RoadmapApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NodeResponse>>> List(Guid roadmapId, CancellationToken ct)
        => Ok(await nodes.ListAsync(roadmapId, ct));

    [HttpGet("{nodeId:guid}")]
    public async Task<ActionResult<NodeDetailResponse>> Get(Guid roadmapId, Guid nodeId, CancellationToken ct)
        => Ok(await nodes.GetAsync(roadmapId, nodeId, ct));

    [HttpPost]
    [ProducesResponseType(typeof(NodeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<NodeResponse>> Create(Guid roadmapId, NodeRequest request, CancellationToken ct)
    {
        var response = await nodes.CreateAsync(roadmapId, request, ct);
        return CreatedAtAction(nameof(Get), new { roadmapId, nodeId = response.Id }, response);
    }

    [HttpPatch("{nodeId:guid}")]
    public async Task<ActionResult<NodeResponse>> Update(Guid roadmapId, Guid nodeId, NodeRequest request, CancellationToken ct)
        => Ok(await nodes.UpdateAsync(roadmapId, nodeId, request, ct));

    [HttpPatch("{nodeId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Status(Guid roadmapId, Guid nodeId, ActiveStatusRequest request, CancellationToken ct)
    {
        await nodes.SetStatusAsync(roadmapId, nodeId, request, ct);
        return NoContent();
    }

    [HttpPatch("positions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Positions(Guid roadmapId, NodePositionsRequest request, CancellationToken ct)
    {
        await nodes.SavePositionsAsync(roadmapId, request, ct);
        return NoContent();
    }
}
