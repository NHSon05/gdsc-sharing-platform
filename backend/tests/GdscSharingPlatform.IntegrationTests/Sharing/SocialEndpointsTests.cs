using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.IntegrationTests.Sharing;

public sealed partial class SharingEndpointsTests
{
    private async Task<Guid> PublishedSocialContent()
    {
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", Content() with { ContributorUserIds = [] }, Json), HttpStatusCode.Created);
        await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{content.Content.Id}/submit", 0));
        await Read<ContentResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{content.Content.Id}/approve", 1));
        return content.Content.Id;
    }

    [Fact]
    public async Task Social_LikeCooldown_TransactionalOutbox_PrivateSavedAndNotifications()
    {
        var id = await PublishedSocialContent();
        var path = $"/api/sharing/contents/{id}/like";
        var liked = await Read<InteractionResponse>(await _other.PutAsJsonAsync(path, new { }));
        Assert.True(liked.IsLikedByCurrentUser); Assert.Equal(1, liked.LikeCount);
        await Read<InteractionResponse>(await _other.PutAsJsonAsync(path, new { }));
        var notifications = await Read<NotificationPage>(await _owner.GetAsync("/api/notifications"));
        var first = Assert.Single(notifications.Items);
        Assert.Equal(NotificationType.ContentLiked, first.Type);
        Assert.Equal(HttpStatusCode.NotFound, (await _other.PatchAsJsonAsync($"/api/notifications/{first.Id}/read", new { })).StatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var n = await db.Notifications.SingleAsync();
            var events = await db.OutboxMessages.ToListAsync();
            Assert.Equal(2, events.Count);
            Assert.Contains(events, x => x.Id == n.EventId && x.Type == "notification.created");
            var data = JsonDocument.Parse(events.Single(x => x.Id == n.EventId).PayloadJson).RootElement;
            Assert.Equal($"user:{_ownerId}", data.GetProperty("room").GetString());
            Assert.Equal(1, data.GetProperty("envelope").GetProperty("data").GetProperty("unreadCount").GetInt32());
        }
        await Read<InteractionResponse>(await _other.DeleteAsync(path));
        await Read<InteractionResponse>(await _other.PutAsJsonAsync(path, new { }));
        Assert.Single((await Read<NotificationPage>(await _owner.GetAsync("/api/notifications"))).Items);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Notifications.SingleAsync()).CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-6);
            await db.SaveChangesAsync();
        }
        await Read<InteractionResponse>(await _other.DeleteAsync(path));
        await Read<InteractionResponse>(await _other.PutAsJsonAsync(path, new { }));
        Assert.Equal(2, (await Read<NotificationPage>(await _owner.GetAsync("/api/notifications"))).Items.Count);
        Assert.Equal(0, (await Read<UnreadNotificationCount>(await _owner.PatchAsJsonAsync("/api/notifications/read-all", new { }))).Count);
        await Read<SavedContentResponse>(await _other.PutAsJsonAsync($"/api/sharing/contents/{id}/saved", new { }));
        Assert.Single((await Read<SharingPage<SavedContentItem>>(await _other.GetAsync("/api/sharing/contents/saved"))).Items);
        Assert.Empty((await Read<SharingPage<SavedContentItem>>(await _owner.GetAsync("/api/sharing/contents/saved"))).Items);
        var detail = await Read<ContentResponse>(await _other.GetAsync("/api/sharing/contents/sharing-test"));
        Assert.True(detail.Content.IsSavedByCurrentUser); Assert.True(detail.Content.IsLikedByCurrentUser);
    }

    [Fact]
    public async Task Social_Comments_AuthorOnly_ReplyModeration_Pagination()
    {
        var id = await PublishedSocialContent();
        var path = $"/api/sharing/contents/{id}/comments";
        var root = await Read<CommentResponse>(await _other.PostAsJsonAsync(path, new CreateCommentRequest("  root  "), Json), HttpStatusCode.Created);
        Assert.Equal("root", root.BodyMarkdown);
        Assert.Equal(HttpStatusCode.BadRequest, (await _other.PutAsJsonAsync($"/api/sharing/comments/{root.Id}", new { bodyMarkdown = "missing version" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _other.PostAsJsonAsync(path, new CreateCommentRequest("<script>alert(1)</script>"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _owner.PutAsJsonAsync($"/api/sharing/comments/{root.Id}", new UpdateCommentRequest("steal", 0), Json)).StatusCode);
        var edited = await Read<CommentResponse>(await _other.PutAsJsonAsync($"/api/sharing/comments/{root.Id}", new UpdateCommentRequest("edited", 0), Json));
        Assert.Equal(1, edited.Version);
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await _other.PutAsJsonAsync($"/api/sharing/comments/{root.Id}", new UpdateCommentRequest("stale", 0), Json)).StatusCode);
        var reply = await Read<CommentResponse>(await _owner.PostAsJsonAsync($"/api/sharing/comments/{root.Id}/replies", new CreateCommentRequest("reply"), Json), HttpStatusCode.Created);
        Assert.Equal(HttpStatusCode.BadRequest, (await _owner.PostAsJsonAsync($"/api/sharing/comments/{reply.Id}/replies", new CreateCommentRequest("nested"), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _other.PostAsJsonAsync($"/api/admin/sharing/comments/{root.Id}/hide", new HideCommentRequest("reason"), Json)).StatusCode);
        await Read<CommentResponse>(await _admin.PostAsJsonAsync($"/api/admin/sharing/comments/{root.Id}/hide", new HideCommentRequest("reason"), Json));
        var hidden = (await Read<CursorPage<CommentResponse>>(await _owner.GetAsync(path))).Items.Single(x => x.Id == root.Id);
        Assert.Null(hidden.BodyMarkdown); Assert.Null(hidden.ModerationReason);
        await Read<CommentResponse>(await _admin.PostAsJsonAsync($"/api/admin/sharing/comments/{root.Id}/restore", new { }));
        Assert.Equal(HttpStatusCode.NoContent, (await _other.DeleteAsync($"/api/sharing/comments/{root.Id}")).StatusCode);
        var first = await Read<CursorPage<CommentResponse>>(await _owner.GetAsync(path + "?pageSize=1"));
        Assert.NotNull(first.NextCursor);
        var second = await Read<CursorPage<CommentResponse>>(await _owner.GetAsync(path + "?pageSize=1&cursor=" + Uri.EscapeDataString(first.NextCursor)));
        Assert.NotEqual(first.Items[0].Id, second.Items[0].Id);
        Assert.Contains(first.Items.Concat(second.Items), x => x.Id == reply.Id && x.ParentCommentId == root.Id);
        Assert.Equal(HttpStatusCode.BadRequest, (await _owner.GetAsync(path + "?cursor=invalid")).StatusCode);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(2, await db.Set<SharingAuditEntry>().CountAsync(x => x.Entity == "ContentComment"));
    }

    [Fact]
    public async Task Social_Rsvp_Version_Audience_LockAndScheduleNotifications()
    {
        var req = Schedule([new(_ownerId, PresenterRole.Speaker)]) with
        { StartsAtLocal = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(10), DateTimeKind.Unspecified), EndsAtLocal = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(10).AddHours(1), DateTimeKind.Unspecified), TimeZoneId = "UTC" };
        var schedule = await Read<ScheduleResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", req, Json), HttpStatusCode.Created);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", 0));
        var path = $"/api/sharing/schedules/{schedule.Id}/rsvp";
        Assert.Equal(HttpStatusCode.BadRequest, (await _other.PutAsJsonAsync(path, new { })).StatusCode);
        var rsvp = await Read<RsvpResponse>(await _other.PutAsJsonAsync(path, new RsvpRequest(ScheduleRsvpStatus.Going), Json));
        rsvp = await Read<RsvpResponse>(await _other.PutAsJsonAsync(path, new RsvpRequest(ScheduleRsvpStatus.Maybe, rsvp.Version), Json));
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await _other.PutAsJsonAsync(path, new RsvpRequest(ScheduleRsvpStatus.NotGoing, 0), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _other.GetAsync($"/api/admin/sharing/schedules/{schedule.Id}/rsvps")).StatusCode);
        Assert.Equal(1, (await Read<RsvpSummary>(await _admin.GetAsync($"/api/admin/sharing/schedules/{schedule.Id}/rsvp-summary"))).Maybe);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}", schedule.Version, req with { MeetingUrl = "https://meet.example.com/new" }));
        Assert.Contains((await Read<NotificationPage>(await _other.GetAsync("/api/notifications"))).Items, x => x.Type == NotificationType.ScheduleUpdated);
        await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/cancel", schedule.Version, new CancelScheduleRequest("cancelled")));
        Assert.Equal(HttpStatusCode.Conflict, (await _other.PutAsJsonAsync(path, new RsvpRequest(ScheduleRsvpStatus.Going, rsvp.Version), Json)).StatusCode);
        Assert.Contains((await Read<NotificationPage>(await _other.GetAsync("/api/notifications"))).Items, x => x.Type == NotificationType.ScheduleCancelled);
    }

    [Fact]
    public async Task Social_SwaggerAndRateLimit()
    {
        var id = await PublishedSocialContent();
        var doc = await Read<JsonElement>(await _admin.GetAsync("/swagger/v1/swagger.json"));
        var paths = doc.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/notifications", out _));
        var like = paths.GetProperty("/api/sharing/contents/{contentId}/like").GetProperty("delete");
        Assert.DoesNotContain(like.GetProperty("parameters").EnumerateArray(), x => x.GetProperty("name").GetString() == "If-Match");
        for (var i = 0; i < 10; i++) await _other.PostAsJsonAsync($"/api/sharing/contents/{id}/comments", new CreateCommentRequest("x"));
        var limited = await _other.PostAsJsonAsync($"/api/sharing/contents/{id}/comments", new CreateCommentRequest("x"));
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.NotNull(limited.Headers.RetryAfter);
    }

    [Fact]
    public async Task Social_MultipleAuthorsHaveDistinctEventIds_SelfLikeIsSilent_AndDraftRejectsInteraction()
    {
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", Content(), Json), HttpStatusCode.Created);
        var path = $"/api/sharing/contents/{content.Content.Id}/like";
        Assert.Equal(HttpStatusCode.NotFound, (await _other.PutAsJsonAsync(path, new { })).StatusCode);
        await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{content.Content.Id}/submit", 0);
        await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{content.Content.Id}/approve", 1);
        await Read<InteractionResponse>(await _admin.PutAsJsonAsync(path, new { }));
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notifications = await db.Notifications.ToListAsync();
            Assert.Equal(2, notifications.Count);
            Assert.Equal(2, notifications.Select(x => x.EventId).Distinct().Count());
            Assert.Equal(2, await db.OutboxMessages.CountAsync(x => x.Type == "notification.created"));
        }
        await Read<InteractionResponse>(await _owner.PutAsJsonAsync(path, new { }));
        Assert.Single((await Read<NotificationPage>(await _owner.GetAsync("/api/notifications"))).Items);
        Assert.Equal(2, (await Read<NotificationPage>(await _other.GetAsync("/api/notifications"))).Items.Count);
    }

    [Fact]
    public async Task Social_RsvpOutsideAudienceFailsAndWithdrawIsIdempotent()
    {
        Guid generationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var generation = new GdscSharingPlatform.Domain.Memberships.ClubGeneration(100);
            db.Add(generation); await db.SaveChangesAsync(); generationId = generation.Id;
        }
        var request = Schedule([new(_ownerId, PresenterRole.Speaker)]) with
        {
            StartsAtLocal = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(20), DateTimeKind.Unspecified),
            EndsAtLocal = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(20).AddHours(1), DateTimeKind.Unspecified),
            TimeZoneId = "UTC", AudienceScope = AudienceScope.SelectedAudience, GenerationIds = [generationId]
        };
        var schedule = await Read<ScheduleResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", request, Json), HttpStatusCode.Created);
        await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", 0);
        var path = $"/api/sharing/schedules/{schedule.Id}/rsvp";
        Assert.Equal(HttpStatusCode.Forbidden, (await _other.PutAsJsonAsync(path, new RsvpRequest(ScheduleRsvpStatus.Going), Json)).StatusCode);
        var rsvp = await Read<RsvpResponse>(await _owner.PutAsJsonAsync(path, new RsvpRequest(ScheduleRsvpStatus.Going), Json));
        Assert.Equal(HttpStatusCode.NoContent, (await Mutate(_owner, HttpMethod.Delete, path, rsvp.Version)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Mutate(_owner, HttpMethod.Delete, path, rsvp.Version)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _owner.GetAsync(path + "/me")).StatusCode);
    }
}
