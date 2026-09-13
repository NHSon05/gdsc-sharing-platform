using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Mapping;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Application.Features.Roadmaps.Rules;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Roadmaps;

public sealed class RoadmapService(RoadmapOperations op) : IRoadmapService
{
    public async Task<PageResponse<RoadmapSummary>> ListAsync(RoadmapQuery query, bool admin = false, CancellationToken ct = default)
    {
        if (admin) op.RequireAdmin(); else op.RequireReader();
        await op.ValidateAsync(query, ct);
        var source = op.Db.Roadmaps.AsNoTracking().Where(x => admin || x.Status == RoadmapStatus.Published || x.Status == RoadmapStatus.Archived);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x => x.Title.ToLower().Contains(search));
        }
        if (query.CategoryId.HasValue) source = source.Where(x => x.CategoryId == query.CategoryId);
        if (query.Level.HasValue) source = source.Where(x => x.Level == query.Level);
        if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status);
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new RoadmapSummary(x.Id, x.Title, x.Slug, x.ShortDescription, x.ThumbnailUrl,
                new CategorySummary(x.CategoryId, x.Category.Name), x.Level, x.EstimatedDuration,
                x.Status, x.SortOrder, x.Nodes.Count(n => admin || n.IsActive))).ToListAsync(ct);
        return new(items, query.Page, query.PageSize, total, (int)Math.Ceiling((double)total / query.PageSize));
    }
    public async Task<RoadmapResponse> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        op.RequireReader();
        var roadmap = await op.Db.Roadmaps.AsNoTracking().Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Slug == slug && (op.IsAdmin || x.Status == RoadmapStatus.Published || x.Status == RoadmapStatus.Archived), ct)
            ?? throw new NotFoundException("Roadmap", slug);
        return await DetailAsync(roadmap, op.IsAdmin, ct);
    }
    public async Task<RoadmapResponse> GetAsync(Guid id, CancellationToken ct = default)
    {
        op.RequireAdmin();
        var roadmap = await op.Db.Roadmaps.AsNoTracking().Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Roadmap", id);
        return await DetailAsync(roadmap, true, ct);
    }
    public async Task<RoadmapResponse> CreateAsync(RoadmapRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            await op.EnsureActiveCategoryAsync(request.CategoryId, ct);
            var roadmap = new Roadmap(request.CategoryId, request.Title, request.Slug, request.ShortDescription,
                request.Level, op.UserId, request.SortOrder);
            await EnsureUniqueAsync(roadmap, ct);
            Apply(roadmap, request);
            roadmap.Category = await op.Db.RoadmapCategories.SingleAsync(x => x.Id == roadmap.CategoryId, ct);
            op.Db.Add(roadmap);
            return roadmap.ToResponse([], []);
        }, ct);
    }
    public async Task<RoadmapResponse> UpdateAsync(Guid id, RoadmapRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var roadmap = await op.RoadmapAsync(id, ct);
            await op.EnsureActiveCategoryAsync(request.CategoryId, ct);
            roadmap.Update(request.CategoryId, request.Title, request.Slug, request.ShortDescription,
                request.Level, request.SortOrder, op.UserId);
            await EnsureUniqueAsync(roadmap, ct);
            roadmap.Category = await op.Db.RoadmapCategories.SingleAsync(x => x.Id == roadmap.CategoryId, ct);
            Apply(roadmap, request);
            return await DetailAsync(roadmap, true, ct);
        }, ct);
    }
    public async Task SetStatusAsync(Guid id, RoadmapStatusRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            var roadmap = await op.RoadmapAsync(id, ct);
            var hasNodes = await op.Db.RoadmapNodes.AnyAsync(x => x.RoadmapId == id && x.IsActive, ct);
            if (request.Status == RoadmapStatus.Published && roadmap.Status != request.Status && !hasNodes)
                throw new ConflictException("A roadmap needs an active node before it can be published.");
            roadmap.ChangeStatus(request.Status!.Value, hasNodes, op.UserId);
            return true;
        }, ct);
    }
    public async Task ReorderAsync(ReorderRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            var roadmaps = await op.Db.Roadmaps.ToDictionaryAsync(x => x.Id, ct);
            RoadmapRules.EnsureCompleteReorder(roadmaps.Keys, request.Ids);
            for (var i = 0; i < request.Ids.Count; i++) roadmaps[request.Ids[i]].Reorder(i, op.UserId);
            return true;
        }, ct);
    }
    private async Task<RoadmapResponse> DetailAsync(Roadmap roadmap, bool admin, CancellationToken ct) =>
        roadmap.ToResponse(await op.NodesAsync(roadmap.Id, admin, ct), await op.EdgesAsync(roadmap.Id, admin, ct));
    private async Task EnsureUniqueAsync(Roadmap roadmap, CancellationToken ct)
    {
        if (await op.Db.Roadmaps.AnyAsync(x => x.Id != roadmap.Id && x.Slug == roadmap.Slug, ct))
            throw new ConflictException("A roadmap with this slug already exists.");
    }
    private static void Apply(Roadmap roadmap, RoadmapRequest request)
    {
        roadmap.Description = request.Description;
        roadmap.ThumbnailUrl = request.ThumbnailUrl;
        roadmap.EstimatedDuration = request.EstimatedDuration;
        roadmap.Prerequisites = request.Prerequisites;
    }
}
