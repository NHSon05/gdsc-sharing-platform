using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class NotificationService(SharingOperations op, SocialOperations social) : INotificationService
{
    public async Task<NotificationPage> ListAsync(NotificationQuery query, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(query, ct);
        var scope = $"notifications:{op.UserId}:{query.IsRead}:{query.Type}";
        var cursor = SocialCursor.Decode(query.Cursor, scope);
        var source = op.Db.Notifications.AsNoTracking().Where(x => x.RecipientUserId == op.UserId);
        if (query.IsRead.HasValue) source = source.Where(x => x.IsRead == query.IsRead);
        if (query.Type.HasValue) source = source.Where(x => x.Type == query.Type);
        if (cursor is not null) source = source.Where(x => x.CreatedAtUtc < cursor.At || x.CreatedAtUtc == cursor.At && x.Id.CompareTo(cursor.Id) < 0);
        var rows = await source.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id).Take(query.PageSize + 1).ToListAsync(ct);
        var page = rows.Take(query.PageSize).ToArray();
        return new(page.Select(SocialOperations.NotificationDto).ToArray(), rows.Count > query.PageSize
            ? new SocialCursor(page[^1].CreatedAtUtc, page[^1].Id, scope).Encode() : null);
    }
    public async Task<UnreadNotificationCount> GetUnreadCountAsync(CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        return new(await op.Db.Notifications.CountAsync(x => x.RecipientUserId == op.UserId && !x.IsRead, ct));
    }
    public async Task MarkReadAsync(Guid notificationId, CancellationToken ct) => await social.WriteAsync(async () =>
    {
        var item = await op.Db.Notifications.SingleOrDefaultAsync(x => x.Id == notificationId && x.RecipientUserId == op.UserId, ct)
            ?? throw new NotFoundException("Notification", notificationId);
        item.MarkRead();
        return true;
    }, ct);
    public async Task MarkAllReadAsync(CancellationToken ct) => await social.WriteAsync(async () =>
    {
        var items = await op.Db.Notifications.Where(x => x.RecipientUserId == op.UserId && !x.IsRead).ToListAsync(ct);
        foreach (var item in items) item.MarkRead();
        return true;
    }, ct);
}
