using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[Route("api/sharing/tags")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class SharingTagsController(ISharingTagService service) : SharingApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagResponse>>> List(CancellationToken ct) => Ok(await service.ListAsync(false, ct));
}

[Route("api/admin/sharing/tags")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class AdminSharingTagsController(ISharingTagService service) : SharingApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagResponse>>> List(CancellationToken ct) => Ok(await service.ListAsync(true, ct));
    [HttpPost]
    [ProducesResponseType(typeof(TagResponse), 201)]
    public async Task<ActionResult<TagResponse>> Create(TagRequest request, CancellationToken ct)
    { var result = await service.SaveAsync(null, request, ct); return CreatedAtAction(nameof(List), result); }
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TagResponse>> Update(Guid id, TagRequest request, CancellationToken ct)
        => Ok(await service.SaveAsync(id, request, ct));
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TagResponse>> SetStatus(Guid id, TagStatusRequest request, CancellationToken ct)
        => Ok(await service.SetStatusAsync(id, request, ct));
}
