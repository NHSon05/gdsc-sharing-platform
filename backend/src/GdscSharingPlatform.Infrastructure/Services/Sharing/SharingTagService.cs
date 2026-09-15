using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class SharingTagService(SharingOperations op) : ISharingTagService
{
    public async Task<IReadOnlyList<TagResponse>> ListAsync(bool admin, CancellationToken ct)
    {
        if (admin) await op.RequireAdminAsync(ct); else await op.RequireReaderAsync(ct);
        return await op.Db.SharingTags.AsNoTracking().Where(x => admin || x.IsActive).OrderBy(x => x.Name)
            .Select(x => new TagResponse(x.Id, x.Name, x.Slug, x.Color, x.IsActive)).ToListAsync(ct);
    }
    public async Task<TagResponse> SaveAsync(Guid? id, TagRequest request, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct); await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var tag = id.HasValue ? await op.Db.SharingTags.SingleOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new NotFoundException("Tag", id) : new SharingTag(request.Name, request.Slug);
            if (!id.HasValue) op.Db.Add(tag);
            tag.Update(request.Name, request.Slug, request.Color);
            op.Audit(id.HasValue ? "Update" : "Create", "Tag", tag.Id);
            return new TagResponse(tag.Id, tag.Name, tag.Slug, tag.Color, tag.IsActive);
        }, ct);
    }
    public async Task<TagResponse> SetStatusAsync(Guid id, TagStatusRequest request, CancellationToken ct)
    {
        await op.RequireAdminAsync(ct); await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var tag = await op.Db.SharingTags.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Tag", id);
            tag.SetActive(request.IsActive!.Value);
            op.Audit("SetStatus", "Tag", id, new { tag.IsActive });
            return new TagResponse(tag.Id, tag.Name, tag.Slug, tag.Color, tag.IsActive);
        }, ct);
    }
}
