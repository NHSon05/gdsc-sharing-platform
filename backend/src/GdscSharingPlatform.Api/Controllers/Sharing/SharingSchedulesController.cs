using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[Route("api/sharing/schedules")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class SharingSchedulesController(ISharingScheduleService service) : SharingApiController
{
    [HttpGet]
    public async Task<ActionResult<SharingPage<ScheduleResponse>>> List([FromQuery] ScheduleQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, false, false, ct));
    [HttpGet("mine")]
    public async Task<ActionResult<SharingPage<ScheduleResponse>>> Mine([FromQuery] ScheduleQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(query, true, false, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ScheduleResponse>> Get(Guid id, CancellationToken ct)
    { var result = await service.GetAsync(id, false, ct); ETag(result.Version); return Ok(result); }
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleResponse), 201)]
    public async Task<ActionResult<ScheduleResponse>> Create(ScheduleRequest request, CancellationToken ct)
    { var result = await service.CreateAsync(request, ct); ETag(result.Version); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ScheduleResponse>> Update(Guid id, ScheduleRequest request, CancellationToken ct)
    { var result = await service.UpdateAsync(id, request, Version(), ct); ETag(result.Version); return Ok(result); }



}
