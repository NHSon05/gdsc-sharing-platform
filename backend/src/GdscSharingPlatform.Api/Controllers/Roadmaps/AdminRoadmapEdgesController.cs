using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

[Route("api/admin/roadmaps/{roadmapId:guid}/edges")]
[Authorize(Policy = AuthPolicies.AdminPolicy)]
public sealed class AdminRoadmapEdgesController(IRoadmapEdgeService edges) : RoadmapApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EdgeResponse>>> List(Guid roadmapId, CancellationToken ct)
        => Ok(await edges.ListAsync(roadmapId, ct));

    [HttpPost]
    [ProducesResponseType(typeof(EdgeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<EdgeResponse>> Create(Guid roadmapId, EdgeRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await edges.CreateAsync(roadmapId, request, ct));

    [HttpPatch("{edgeId:guid}")]
    public async Task<ActionResult<EdgeResponse>> Update(Guid roadmapId, Guid edgeId, EdgeRequest request, CancellationToken ct)
        => Ok(await edges.UpdateAsync(roadmapId, edgeId, request, ct));

    [HttpDelete("{edgeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid roadmapId, Guid edgeId, CancellationToken ct)
    {
        await edges.DeleteAsync(roadmapId, edgeId, ct);
        return NoContent();
    }
}
