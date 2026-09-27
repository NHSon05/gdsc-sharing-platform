using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class ContentInteractionService(SharingOperations op, SocialOperations social) : IContentInteractionService
{
    public Task<InteractionResponse> LikeAsync(Guid contentId, CancellationToken ct) => LikeAsync(contentId, true, ct);
    public Task<InteractionResponse> UnlikeAsync(Guid contentId, CancellationToken ct) => LikeAsync(contentId, false, ct);
    private Task<InteractionResponse> LikeAsync(Guid contentId, bool liked, CancellationToken ct) => social.WriteAsync(async () =>
    {
        var content = await social.PublishedAsync(contentId, ct);
        var row = await op.Db.ContentLikes.SingleOrDefaultAsync(x => x.SharingContentId == contentId && x.UserId == op.UserId, ct);
        var changed = liked != (row is not null);
        if (changed)
        {
            if (liked) op.Db.ContentLikes.Add(new(contentId, op.UserId)); else op.Db.ContentLikes.Remove(row!);
            await op.Db.SaveChangesAsync(ct);
            if (liked) await social.NotifyAsync(content.Authors.Select(x => x.UserId), NotificationType.ContentLiked,
                "SharingContent", contentId, "Bài viết có lượt thích mới", "Một thành viên đã thích bài viết của bạn.",
                $"/sharing/{content.Slug}", ct);
            await social.InteractionAsync(contentId, ct);
        }
        return new InteractionResponse(contentId, liked, await op.Db.ContentLikes.CountAsync(x => x.SharingContentId == contentId, ct));
    }, ct);

    public Task<SavedContentResponse> SaveAsync(Guid contentId, CancellationToken ct) => SaveAsync(contentId, true, ct);
    public Task<SavedContentResponse> UnsaveAsync(Guid contentId, CancellationToken ct) => SaveAsync(contentId, false, ct);
    private Task<SavedContentResponse> SaveAsync(Guid contentId, bool saved, CancellationToken ct) => social.WriteAsync(async () =>
    {
        if (saved) await social.PublishedAsync(contentId, ct);
        var row = await op.Db.SavedContents.SingleOrDefaultAsync(x => x.SharingContentId == contentId && x.UserId == op.UserId, ct);
        if (saved && row is null) op.Db.SavedContents.Add(new(contentId, op.UserId));
        if (!saved && row is not null) op.Db.SavedContents.Remove(row);
        return new SavedContentResponse(contentId, saved);
    }, ct);

    public async Task<SharingPage<SavedContentItem>> ListSavedAsync(ContentQuery query, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(query, ct);
        var content = op.VisibleContents();
        if (!op.IsAdmin) content = content.Where(x => x.Status == SharingContentStatus.Published);
        if (query.TagId.HasValue) content = content.Where(x => x.Tags.Any(t => t.SharingTagId == query.TagId));
        if (query.AuthorId.HasValue) content = content.Where(x => x.Authors.Any(a => a.UserId == query.AuthorId));
        if (query.Status.HasValue) content = content.Where(x => x.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        { var term = query.Search.Trim().ToLowerInvariant(); content = content.Where(x => x.Title.ToLower().Contains(term) || x.Summary.ToLower().Contains(term)); }
        var rows = op.Db.SavedContents.AsNoTracking().Where(x => x.UserId == op.UserId && content.Any(c => c.Id == x.SharingContentId));
        var total = await rows.CountAsync(ct);
        var page = await rows.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        var ids = page.Select(x => x.SharingContentId).ToArray();
        var summaries = await content.Where(x => ids.Contains(x.Id)).Select(op.Summary).ToDictionaryAsync(x => x.Id, ct);
        return new(page.Select(x => new SavedContentItem(summaries[x.SharingContentId], x.CreatedAtUtc)).ToArray(), total, query.Page, query.PageSize);
    }
}
