using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Controllers.Sharing;

[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Route("api/sharing/contents")]
public sealed class ContentInteractionsController(IContentInteractionService service) : SharingApiController
{
    [HttpPut("{contentId:guid}/like"), EnableRateLimiting("social-like")]
    public async Task<ActionResult<InteractionResponse>> Like(Guid contentId, CancellationToken ct) => Ok(await service.LikeAsync(contentId, ct));
    [HttpDelete("{contentId:guid}/like"), EnableRateLimiting("social-like")]
    public async Task<ActionResult<InteractionResponse>> Unlike(Guid contentId, CancellationToken ct) => Ok(await service.UnlikeAsync(contentId, ct));
    [HttpPut("{contentId:guid}/saved"), EnableRateLimiting("social-save")]
    public async Task<ActionResult<SavedContentResponse>> Save(Guid contentId, CancellationToken ct) => Ok(await service.SaveAsync(contentId, ct));
    [HttpDelete("{contentId:guid}/saved"), EnableRateLimiting("social-save")]
    public async Task<ActionResult<SavedContentResponse>> Unsave(Guid contentId, CancellationToken ct) => Ok(await service.UnsaveAsync(contentId, ct));
    [HttpGet("saved")]
    public async Task<ActionResult<SharingPage<SavedContentItem>>> Saved([FromQuery] ContentQuery query, CancellationToken ct) => Ok(await service.ListSavedAsync(query, ct));
}

[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Route("api/sharing")]
public sealed class ContentCommentsController(IContentCommentService service) : SharingApiController
{
    [HttpGet("contents/{contentId:guid}/comments")]
    public async Task<ActionResult<CursorPage<CommentResponse>>> List(Guid contentId, [FromQuery] CommentQuery query, CancellationToken ct) => Ok(await service.ListAsync(contentId, query, ct));
    [HttpPost("contents/{contentId:guid}/comments"), EnableRateLimiting("social-comment")]
    [ProducesResponseType(typeof(CommentResponse), 201)]
    public async Task<ActionResult<CommentResponse>> Create(Guid contentId, CreateCommentRequest request, CancellationToken ct)
    { var result = await service.CreateAsync(contentId, request, ct); ETag(result.Version); return StatusCode(201, result); }
    [HttpPost("comments/{commentId:guid}/replies"), EnableRateLimiting("social-comment")]
    [ProducesResponseType(typeof(CommentResponse), 201)]
    public async Task<ActionResult<CommentResponse>> Reply(Guid commentId, CreateCommentRequest request, CancellationToken ct)
    { var result = await service.ReplyAsync(commentId, request, ct); ETag(result.Version); return StatusCode(201, result); }
    [HttpPut("comments/{commentId:guid}"), EnableRateLimiting("social-edit")]
    public async Task<ActionResult<CommentResponse>> Update(Guid commentId, UpdateCommentRequest request, CancellationToken ct)
    { var result = await service.UpdateAsync(commentId, request, ct); ETag(result.Version); return Ok(result); }
    [HttpDelete("comments/{commentId:guid}"), EnableRateLimiting("social-edit")]
    public async Task<IActionResult> Delete(Guid commentId, CancellationToken ct)
    { await service.DeleteAsync(commentId, ct); return NoContent(); }
}

[Authorize(Roles = RoleNames.Admin, Policy = AuthPolicies.RequireActiveUser)]
[Route("api/admin/sharing/comments")]
public sealed class CommentModerationController(IContentCommentService service) : SharingApiController
{
    [HttpPost("{commentId:guid}/hide"), EnableRateLimiting("social-edit")]
    public async Task<ActionResult<CommentResponse>> Hide(Guid commentId, HideCommentRequest request, CancellationToken ct)
    { var result = await service.HideAsync(commentId, request, ct); ETag(result.Version); return Ok(result); }
    [HttpPost("{commentId:guid}/restore"), EnableRateLimiting("social-edit")]
    public async Task<ActionResult<CommentResponse>> Restore(Guid commentId, CancellationToken ct)
    { var result = await service.RestoreAsync(commentId, ct); ETag(result.Version); return Ok(result); }
}

[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Route("api/sharing/schedules/{scheduleId:guid}/rsvp")]
public sealed class ScheduleRsvpsController(IScheduleRsvpService service) : SharingApiController
{
    [HttpPut, EnableRateLimiting("social-rsvp")]
    public async Task<ActionResult<RsvpResponse>> Upsert(Guid scheduleId, RsvpRequest request, CancellationToken ct)
    { var result = await service.UpsertAsync(scheduleId, request, ct); ETag(result.Version); return Ok(result); }
    [HttpDelete, EnableRateLimiting("social-rsvp")]
    public async Task<IActionResult> Withdraw(Guid scheduleId, CancellationToken ct)
    { await service.WithdrawAsync(scheduleId, Version(), ct); return NoContent(); }
    [HttpGet("me")]
    public async Task<ActionResult<RsvpResponse>> Mine(Guid scheduleId, CancellationToken ct)
    { var result = await service.GetMineAsync(scheduleId, ct); if (result is null) return NoContent(); ETag(result.Version); return Ok(result); }
}

[Authorize(Roles = RoleNames.Admin, Policy = AuthPolicies.RequireActiveUser)]
[Route("api/admin/sharing/schedules/{scheduleId:guid}")]
public sealed class AdminScheduleRsvpsController(IScheduleRsvpService service) : SharingApiController
{
    [HttpGet("rsvps")]
    public async Task<ActionResult<CursorPage<ScheduleRsvpResponse>>> List(Guid scheduleId, CancellationToken ct, [FromQuery] string? cursor = null, [FromQuery] int pageSize = 20)
        => Ok(await service.ListForAdminAsync(scheduleId, cursor, pageSize, ct));
    [HttpGet("rsvp-summary")]
    public async Task<ActionResult<RsvpSummary>> Summary(Guid scheduleId, CancellationToken ct) => Ok(await service.GetSummaryAsync(scheduleId, ct));
}

[Authorize(Policy = AuthPolicies.RequireActiveUser)]
[Route("api/notifications")]
public sealed class NotificationsController(INotificationService service) : SharingApiController
{
    [HttpGet]
    public async Task<ActionResult<NotificationPage>> List([FromQuery] NotificationQuery query, CancellationToken ct) => Ok(await service.ListAsync(query, ct));
    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadNotificationCount>> Count(CancellationToken ct) => Ok(await service.GetUnreadCountAsync(ct));
    [HttpPatch("{notificationId:guid}/read"), EnableRateLimiting("social-read")]
    public async Task<ActionResult<UnreadNotificationCount>> Read(Guid notificationId, CancellationToken ct)
    { await service.MarkReadAsync(notificationId, ct); return Ok(await service.GetUnreadCountAsync(ct)); }
    [HttpPatch("read-all"), EnableRateLimiting("social-read")]
    public async Task<ActionResult<UnreadNotificationCount>> ReadAll(CancellationToken ct)
    { await service.MarkAllReadAsync(ct); return Ok(await service.GetUnreadCountAsync(ct)); }
}
