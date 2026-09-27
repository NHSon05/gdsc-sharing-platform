using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class ContentCommentService(SharingOperations op, SocialOperations social) : IContentCommentService
{
    public async Task<CursorPage<CommentResponse>> ListAsync(Guid contentId, CommentQuery query, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(query, ct);
        if (!await op.VisibleContents().AnyAsync(x => x.Id == contentId, ct)) throw new NotFoundException("Content", contentId);
        var scope = $"comments:{contentId}:{query.Sort}";
        var cursor = SocialCursor.Decode(query.Cursor, scope);
        var source = op.Db.ContentComments.AsNoTracking().Where(x => x.SharingContentId == contentId);
        var newest = query.Sort == "newest";
        if (cursor is not null) source = newest
            ? source.Where(x => x.CreatedAtUtc < cursor.At || x.CreatedAtUtc == cursor.At && x.Id.CompareTo(cursor.Id) < 0)
            : source.Where(x => x.CreatedAtUtc > cursor.At || x.CreatedAtUtc == cursor.At && x.Id.CompareTo(cursor.Id) > 0);
        var rows = await (newest ? source.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            : source.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)).Take(query.PageSize + 1).ToListAsync(ct);
        var page = rows.Take(query.PageSize).ToArray();
        return new(page.Select(x => SocialOperations.CommentDto(x, op.IsAdmin)).ToArray(), rows.Count > query.PageSize
            ? new SocialCursor(page[^1].CreatedAtUtc, page[^1].Id, scope).Encode() : null);
    }

    public Task<CommentResponse> CreateAsync(Guid contentId, CreateCommentRequest request, CancellationToken ct) => CreateAsync(contentId, null, request, ct);
    public Task<CommentResponse> ReplyAsync(Guid commentId, CreateCommentRequest request, CancellationToken ct) => CreateAsync(null, commentId, request, ct);
    private async Task<CommentResponse> CreateAsync(Guid? contentId, Guid? parentId, CreateCommentRequest request, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        return await social.WriteAsync(async () =>
        {
            ContentComment? parent = null;
            if (parentId.HasValue)
            {
                parent = await FindAsync(parentId.Value, ct);
                if (parent.ParentCommentId.HasValue || parent.Status != ContentCommentStatus.Active)
                    throw new ApplicationValidationException("Only an active root comment can receive replies.");
                contentId = parent.SharingContentId;
            }
            var content = await social.PublishedAsync(contentId!.Value, ct);
            var comment = new ContentComment(content.Id, op.UserId, request.BodyMarkdown, parentId);
            op.Db.ContentComments.Add(comment);
            await op.Db.SaveChangesAsync(ct);
            var route = $"/sharing/{content.Slug}#comment-{comment.Id}";
            if (parent is not null) await social.NotifyAsync([parent.AuthorUserId], NotificationType.CommentReplied,
                "ContentComment", comment.Id, "Bình luận có phản hồi mới", "Một thành viên đã trả lời bình luận của bạn.", route, ct);
            await social.NotifyAsync(content.Authors.Select(x => x.UserId).Where(x => parent is null || x != parent.AuthorUserId),
                NotificationType.ContentCommented, "ContentComment", comment.Id,
                "Bài viết có bình luận mới", "Một thành viên đã bình luận trên bài viết của bạn.", route, ct);
            // Room events contain only IDs. Private moderation text is fetched through authorized REST.
            social.Emit(SocialEventNames.CommentCreated, $"content:{content.Id}", new { contentId = content.Id, commentId = comment.Id });
            await social.InteractionAsync(content.Id, ct);
            return SocialOperations.CommentDto(comment, op.IsAdmin);
        }, ct);
    }

    public async Task<CommentResponse> UpdateAsync(Guid commentId, UpdateCommentRequest request, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        return await ChangeAsync(commentId, comment =>
        {
            RequireAuthor(comment); SharingOperations.CheckVersion(comment.Version, request.Version);
            SharingOperations.Transition(() => comment.Update(request.BodyMarkdown, op.UserId));
        }, SocialEventNames.CommentUpdated, false, ct);
    }
    public async Task DeleteAsync(Guid commentId, CancellationToken ct) => await ChangeAsync(commentId, comment =>
    {
        RequireAuthor(comment);
        if (comment.Status != ContentCommentStatus.Deleted) SharingOperations.Transition(() => comment.Delete(op.UserId));
    }, SocialEventNames.CommentDeleted, false, ct);

    public async Task<CommentResponse> HideAsync(Guid commentId, HideCommentRequest request, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct); await op.ValidateAsync(request, ct);
        return await ChangeAsync(commentId, comment => SharingOperations.Transition(() => comment.Hide(request.Reason, op.UserId)),
            SocialEventNames.CommentHidden, true, ct);
    }
    public async Task<CommentResponse> RestoreAsync(Guid commentId, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct);
        return await ChangeAsync(commentId, comment => SharingOperations.Transition(() => comment.Restore(op.UserId)),
            SocialEventNames.CommentUpdated, true, ct);
    }
    private Task<CommentResponse> ChangeAsync(Guid id, Action<ContentComment> change, string eventName, bool moderation, CancellationToken ct) => social.WriteAsync(async () =>
    {
        var comment = await FindAsync(id, ct);
        if (!moderation) await social.PublishedAsync(comment.SharingContentId, ct);
        var version = comment.Version;
        change(comment);
        if (comment.Version != version)
        {
            await op.Db.SaveChangesAsync(ct);
            if (moderation) op.Audit(comment.Status == ContentCommentStatus.Hidden ? "HideComment" : "RestoreComment", "ContentComment", id,
                new { comment.Version, comment.ModerationReason });
            social.Emit(eventName, $"content:{comment.SharingContentId}", new { contentId = comment.SharingContentId, commentId = id, comment.Version });
            await social.InteractionAsync(comment.SharingContentId, ct);
        }
        return SocialOperations.CommentDto(comment, op.IsAdmin);
    }, ct);
    private Task<ContentComment> FindAsync(Guid id, CancellationToken ct) => FindCoreAsync(id, ct);
    private async Task<ContentComment> FindCoreAsync(Guid id, CancellationToken ct) =>
        await op.Db.ContentComments.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Comment", id);
    private void RequireAuthor(ContentComment comment)
    {
        SocialInteractionAuthorization.RequireCommentAuthor(comment.AuthorUserId, op.UserId, op.IsAdmin);
    }
}
