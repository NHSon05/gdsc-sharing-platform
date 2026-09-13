using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers.Roadmaps;

[Route("api/roadmap-resources")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Member)]
public sealed class RoadmapResourcesController(ILearningResourceService resources) : RoadmapApiController
{
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var download = await resources.DownloadAsync(id, ct);
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(download.Content, download.ContentType, download.FileName, enableRangeProcessing: true);
    }
}
