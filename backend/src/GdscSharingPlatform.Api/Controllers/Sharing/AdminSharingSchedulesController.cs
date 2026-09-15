using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[Route("api/admin/sharing/schedules")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class AdminSharingSchedulesController(ISharingScheduleService service) : SharingApiController
{
    [HttpGet]
    public async Task<ActionResult<SharingPage<ScheduleResponse>>> List([FromQuery] ScheduleQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, false, true, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ScheduleResponse>> Get(Guid id, CancellationToken ct)
    { var result = await service.GetAsync(id, true, ct); ETag(result.Version); return Ok(result); }
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleResponse), 201)]
    public async Task<ActionResult<ScheduleResponse>> Create(ScheduleRequest request, CancellationToken ct)
    { var result = await service.CreateAsync(request, ct); ETag(result.Version); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ScheduleResponse>> Update(Guid id, ScheduleRequest request, CancellationToken ct)
    { var result = await service.UpdateAsync(id, request, Version(), ct); ETag(result.Version); return Ok(result); }
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<ScheduleResponse>> Publish(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ScheduleAction.Publish, Version(), null, ct); ETag(result.Version); return Ok(result); }
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<ScheduleResponse>> Start(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ScheduleAction.Start, Version(), null, ct); ETag(result.Version); return Ok(result); }
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ScheduleResponse>> Complete(Guid id, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ScheduleAction.Complete, Version(), null, ct); ETag(result.Version); return Ok(result); }
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ScheduleResponse>> Cancel(Guid id, CancelScheduleRequest request, CancellationToken ct)
    { var result = await service.TransitionAsync(id, ScheduleAction.Cancel, Version(), request.Reason, ct); ETag(result.Version); return Ok(result); }
    [HttpPatch("{id:guid}/presenters")]
    public async Task<ActionResult<ScheduleResponse>> SetPresenters(Guid id, PresentersRequest request, CancellationToken ct)
    { var result = await service.SetPresentersAsync(id, request, Version(), ct); ETag(result.Version); return Ok(result); }
    [HttpPatch("{id:guid}/contents")]
    public async Task<ActionResult<ScheduleResponse>> SetContents(Guid id, ScheduleContentsRequest request, CancellationToken ct)
    { var result = await service.SetContentsAsync(id, request, Version(), ct); ETag(result.Version); return Ok(result); }
    [HttpPatch("{id:guid}/audience")]
    public async Task<ActionResult<ScheduleResponse>> SetAudience(Guid id, AudienceRequest request, CancellationToken ct)
    { var result = await service.SetAudienceAsync(id, request, Version(), ct); ETag(result.Version); return Ok(result); }
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    { await service.DeleteAsync(id, Version(), ct); return NoContent(); }
}
