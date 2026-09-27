using System.Text.Json;
using System.Text.Json.Serialization;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

// All callers enqueue inside the business transaction; only the worker contacts the gateway.
public sealed class SocialOperations(SharingOperations op, TimeProvider clock)
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    { Converters = { new JsonStringEnumConverter() } };

    public async Task<T> WriteAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            try { return await op.WriteAsync(action, ct); }
            catch (ConflictException) when (attempt < 3)
            { await Task.Delay(TimeSpan.FromMilliseconds(20 * (attempt + 1)), ct); }
        }
    }

    public async Task<SharingContent> PublishedAsync(Guid id, CancellationToken ct)
    {
        var content = await op.ContentAsync(id, ct);
        if (content.Status != SharingContentStatus.Published) throw new NotFoundException("Published content", id);
        return content;
    }

    public void Emit<T>(string name, string room, T data, Guid? eventId = null)
    {
        var id = eventId ?? Guid.NewGuid();
        var now = clock.GetUtcNow();
        var envelope = new { eventId = id, eventName = name, occurredAtUtc = now, version = 1, data };
        op.Db.OutboxMessages.Add(new(id, name, JsonSerializer.Serialize(new { room, envelope }, Json), now));
    }

    public async Task InteractionAsync(Guid contentId, CancellationToken ct)
    {
        Emit(SocialEventNames.ContentInteractionUpdated, $"content:{contentId}", new
        {
            contentId,
            likeCount = await op.Db.ContentLikes.CountAsync(x => x.SharingContentId == contentId, ct),
            commentCount = await op.Db.ContentComments.CountAsync(x => x.SharingContentId == contentId && x.Status == ContentCommentStatus.Active, ct)
        });
    }

    public async Task NotifyAsync(IEnumerable<Guid> recipients, NotificationType type, string entityType,
        Guid entityId, string title, string message, string route, CancellationToken ct)
    {
        var ids = recipients.Distinct().Where(x => x != op.UserId).ToArray();
        var active = await op.Db.Users.Where(x => ids.Contains(x.Id) && !x.IsDeleted && x.Status == UserStatus.Active)
            .Select(x => x.Id).ToListAsync(ct);
        var now = clock.GetUtcNow();
        foreach (var recipient in active)
        {
            // Persisted history survives unlike, restarts, mark-read and gateway outages.
            if (type == NotificationType.ContentLiked && await op.Db.Notifications.AnyAsync(x =>
                x.Type == type && x.ActorUserId == op.UserId && x.EntityId == entityId && x.RecipientUserId == recipient
                && x.CreatedAtUtc > now.AddMinutes(-5), ct)) continue;
            var notification = new Notification(recipient, op.UserId, type, entityType, entityId,
                title, message, Guid.NewGuid(), route) { CreatedAtUtc = now };
            op.Db.Notifications.Add(notification);
            // Include pending additions; later notifications to this recipient see their own count.
            var count = await op.Db.Notifications.CountAsync(x => x.RecipientUserId == recipient && !x.IsRead, ct)
                + op.Db.ChangeTracker.Entries<Notification>().Count(x => x.State == EntityState.Added && x.Entity.RecipientUserId == recipient);
            Emit(SocialEventNames.NotificationCreated, $"user:{recipient}", new
            { notification = NotificationDto(notification), unreadCount = count }, notification.EventId);
        }
    }

    public Task NotifyScheduleAsync(SharingSchedule schedule, bool cancelled, CancellationToken ct) => NotifyScheduleCoreAsync(schedule, cancelled, ct);
    private async Task NotifyScheduleCoreAsync(SharingSchedule schedule, bool cancelled, CancellationToken ct)
    {
        var recipients = await op.Db.ScheduleRsvps.Where(x => x.SharingScheduleId == schedule.Id
            && (x.Status == ScheduleRsvpStatus.Going || x.Status == ScheduleRsvpStatus.Maybe)).Select(x => x.UserId).ToListAsync(ct);
        recipients.AddRange(schedule.Presenters.Select(x => x.UserId));
        await NotifyAsync(recipients, cancelled ? NotificationType.ScheduleCancelled : NotificationType.ScheduleUpdated,
            "SharingSchedule", schedule.Id, cancelled ? "Lịch sharing đã bị hủy" : "Lịch sharing đã thay đổi",
            cancelled ? "Một buổi sharing bạn quan tâm đã bị hủy." : "Thời gian hoặc địa điểm của buổi sharing đã thay đổi.",
            $"/schedule?id={schedule.Id}", ct);
    }

    public static NotificationResponse NotificationDto(Notification n) => new(n.Id, n.Type, n.ActorUserId,
        n.EntityType, n.EntityId, n.Title, n.Message, n.Route, n.IsRead, n.CreatedAtUtc);
    public static CommentResponse CommentDto(ContentComment c, bool admin) => new(c.Id, c.SharingContentId,
        c.AuthorUserId, c.ParentCommentId, c.Status == ContentCommentStatus.Active || admin ? c.BodyMarkdown : null,
        c.Status, admin ? c.ModerationReason : null, admin ? c.ModeratedByUserId : null,
        c.CreatedAtUtc, c.UpdatedAtUtc, c.DeletedAtUtc, c.Version);
}
