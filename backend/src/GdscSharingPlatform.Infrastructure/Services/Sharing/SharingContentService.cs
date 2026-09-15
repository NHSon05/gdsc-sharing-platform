using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class SharingContentService(SharingOperations op) : ISharingContentService
{
    public async Task<SharingPage<ContentSummary>> ListAsync(ContentQuery query, bool mine, bool admin, CancellationToken ct)
    {
        if (admin) await op.RequireAdminAsync(ct); else await op.RequireReaderAsync(ct);
        await op.ValidateAsync(query, ct);
        var uid = op.UserId;
        var source = op.VisibleContents().AsNoTracking();
        if (mine) source = source.Where(x => x.Authors.Any(a => a.UserId == uid));
        else if (!admin) source = source.Where(x => x.Status == SharingContentStatus.Published || x.Authors.Any(a => a.UserId == uid));
        if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status);
        if (query.TagId.HasValue) source = source.Where(x => x.Tags.Any(t => t.SharingTagId == query.TagId));
        if (query.AuthorId.HasValue) source = source.Where(x => x.Authors.Any(a => a.UserId == query.AuthorId));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x => x.Title.ToLower().Contains(term) || x.Summary.ToLower().Contains(term));
        }
        var total = await source.CountAsync(ct);
        var sorted = query.Sort switch
        {
            "title" => source.OrderBy(x => x.Title),
            "oldest" => source.OrderBy(x => x.PublishedAtUtc ?? x.SubmittedAtUtc ?? x.CreatedAtUtc),
            _ when admin && query.Status == SharingContentStatus.PendingReview => source.OrderBy(x => x.SubmittedAtUtc),
            _ => source.OrderByDescending(x => x.PublishedAtUtc ?? x.SubmittedAtUtc ?? x.CreatedAtUtc)
        };
        var items = await sorted.ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(op.Summary).ToListAsync(ct);
        return new(items, total, query.Page, query.PageSize);
    }
    public async Task<ContentResponse> GetBySlugAsync(string slug, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        var id = await op.Db.SharingContents.Where(x => x.Slug == slug && x.Status == SharingContentStatus.Published)
            .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("Content", slug);
        return await GetAsync(id, false, false, ct);
    }
    public async Task<ContentResponse> GetAsync(Guid id, bool mine, bool admin, CancellationToken ct)
    {
        if (admin) await op.RequireAdminAsync(ct); else await op.RequireReaderAsync(ct);
        var uid = op.UserId;
        var query = op.VisibleContents().AsNoTracking().Where(x => x.Id == id);
        if (mine) query = query.Where(x => x.Authors.Any(a => a.UserId == uid));
        var summary = await query.Select(op.Summary).SingleOrDefaultAsync(ct) ?? throw new NotFoundException("Content", id);
        var detail = await query.Select(x => new { x.BodyMarkdown, x.ReviewNote, x.SubmittedAtUtc, x.ReviewedAtUtc, x.ReviewedByUserId }).SingleAsync(ct);
        var authors = summary.Authors;
        var tags = summary.Tags;
        var resources = await op.Db.SharingResources.AsNoTracking().Where(x => x.SharingContentId == id && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(SharingOperations.Resource).ToListAsync(ct);
        var schedules = await op.VisibleSchedules().AsNoTracking().Where(x => x.Contents.Any(c => c.SharingContentId == id))
            .OrderBy(x => x.StartsAtUtc).Select(x => new ScheduleReference(x.Id, x.Title, x.StartsAtUtc, x.Status)).ToListAsync(ct);
        var authorOrAdmin = op.IsAdmin || authors.Any(x => x.UserId == uid);
        return new(summary, detail.BodyMarkdown, authorOrAdmin ? detail.ReviewNote : null,
            detail.SubmittedAtUtc, detail.ReviewedAtUtc, detail.ReviewedByUserId, authors, tags, resources, schedules);
    }
    public async Task<ContentResponse> CreateAsync(ContentRequest request, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        await op.ValidateAsync(request, ct);
        var id = await op.WriteAsync(async () =>
        {
            var content = new SharingContent(request.Title, request.Slug, request.Summary, request.BodyMarkdown, op.UserId);
            content.SetCoverImage(request.CoverImageUrl);
            op.Db.Add(content);
            await SetAssociationsAsync(content, request, ct);
            op.Audit("Create", "Content", content.Id);
            return content.Id;
        }, ct);
        return await GetAsync(id, false, false, ct);
    }
    public async Task<ContentResponse> UpdateAsync(Guid id, ContentRequest request, long version, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            var content = await op.EditableContentAsync(id, version, ct);
            var owner = content.Authors.Single(x => x.AuthorRole == SharingAuthorRole.Owner);
            if (op.IsAdmin || owner.UserId == op.UserId) await SetAssociationsAsync(content, request, ct);
            else if (!content.Authors.Where(x => x.AuthorRole == SharingAuthorRole.Contributor).Select(x => x.UserId).ToHashSet().SetEquals(request.ContributorUserIds)
                || !content.Tags.Select(x => x.SharingTagId).ToHashSet().SetEquals(request.TagIds))
                throw new ForbiddenAccessException("Contributors cannot change authors or tags.");
            content.Update(request.Title, request.Slug, request.Summary, request.BodyMarkdown, op.UserId);
            content.SetCoverImage(request.CoverImageUrl);
            op.Audit("Update", "Content", id, new { content.Version, content.Status });
            return true;
        }, ct);
        return await GetAsync(id, false, false, ct);
    }
    public async Task<ContentResponse> TransitionAsync(Guid id, ContentAction action, long version, string? note, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        if (action is not (ContentAction.Submit or ContentAction.Withdraw)) await op.RequireAdminAsync(ct);
        if (action == ContentAction.Reject) await op.ValidateAsync(new ReviewRequest(note!), ct);
        await op.WriteAsync(async () =>
        {
            var content = await op.ContentAsync(id, ct);
            if (action is ContentAction.Submit or ContentAction.Withdraw) op.RequireOwner(content);
            SharingOperations.CheckVersion(content.Version, version);
            SharingOperations.Transition(() =>
            {
                switch (action)
                {
                    case ContentAction.Submit: content.Submit(op.UserId); break;
                    case ContentAction.Withdraw: content.Withdraw(op.UserId); break;
                    case ContentAction.Approve: content.Approve(op.UserId); break;
                    case ContentAction.Reject: content.Reject(op.UserId, note!); break;
                    case ContentAction.ReturnToDraft: content.ReturnToDraft(op.UserId); break;
                    case ContentAction.Archive: content.Archive(op.UserId); break;
                    default: throw new ApplicationValidationException("Unknown content action.");
                }
            });
            op.Audit(action.ToString(), "Content", id, new { content.Version });
            return true;
        }, ct);
        return await GetAsync(id, false, false, ct);
    }
    private async Task SetAssociationsAsync(SharingContent content, ContentRequest request, CancellationToken ct)
    {
        var owner = content.Authors.Single(x => x.AuthorRole == SharingAuthorRole.Owner).UserId;
        if (request.ContributorUserIds.Contains(owner)) throw new ApplicationValidationException("contributorUserIds", "Owner cannot also be a Contributor.");
        if (await op.Db.Users.CountAsync(x => request.ContributorUserIds.Contains(x.Id) && !x.IsDeleted && x.Status == UserStatus.Active, ct) != request.ContributorUserIds.Count)
            throw new ApplicationValidationException("contributorUserIds", "Choose active users.");
        if (await op.Db.SharingTags.CountAsync(x => request.TagIds.Contains(x.Id) && x.IsActive, ct) != request.TagIds.Count)
            throw new ApplicationValidationException("tagIds", "Choose active tags.");
        foreach (var a in content.Authors.Where(a => a.AuthorRole == SharingAuthorRole.Contributor && !request.ContributorUserIds.Contains(a.UserId)).ToArray())
        { content.Authors.Remove(a); op.Db.Remove(a); op.Audit("RemoveAuthor", "Content", content.Id, new { a.UserId }); }
        foreach (var uid in request.ContributorUserIds.Where(uid => !content.Authors.Any(a => a.UserId == uid)))
        {
            var author = new SharingContentAuthor(content.Id, uid, SharingAuthorRole.Contributor, content.Authors.Count);
            content.Authors.Add(author);
            // Application-generated Guid keys need an explicit Added state on an already tracked aggregate.
            op.Db.Add(author);
            op.Audit("AddAuthor", "Content", content.Id, new { UserId = uid });
        }
        foreach (var t in content.Tags.Where(t => !request.TagIds.Contains(t.SharingTagId)).ToArray())
        { content.Tags.Remove(t); op.Db.Remove(t); }
        foreach (var tag in request.TagIds.Where(id => !content.Tags.Any(t => t.SharingTagId == id)))
        {
            var link = new SharingContentTag(content.Id, tag);
            content.Tags.Add(link); op.Db.Add(link);
        }
    }
}
