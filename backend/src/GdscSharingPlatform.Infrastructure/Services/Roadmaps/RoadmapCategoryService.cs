using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Mapping;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Services.Roadmaps;

public sealed class RoadmapCategoryService(RoadmapOperations op) : IRoadmapCategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> ListAsync(bool admin = false, CancellationToken ct = default)
    {
        if (admin) op.RequireAdmin(); else op.RequireReader();
        return await op.Db.RoadmapCategories.AsNoTracking().Where(x => admin || x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new CategoryResponse(x.Id, x.Name, x.Slug, x.Description, x.Icon, x.Color, x.SortOrder, x.IsActive)).ToListAsync(ct);
    }
    public async Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var category = new RoadmapCategory(request.Name, request.Slug, request.SortOrder);
            await EnsureUniqueAsync(category, ct);
            Apply(category, request);
            op.Db.Add(category);
            return category.ToResponse();
        }, ct);
    }
    public async Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var category = await FindAsync(id, ct);
            category.Update(request.Name, request.Slug, request.SortOrder);
            await EnsureUniqueAsync(category, ct);
            Apply(category, request);
            return category.ToResponse();
        }, ct);
    }
    public async Task SetStatusAsync(Guid id, ActiveStatusRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () => { (await FindAsync(id, ct)).SetActive(request.IsActive!.Value); return true; }, ct);
    }
    private async Task<RoadmapCategory> FindAsync(Guid id, CancellationToken ct) =>
        await op.Db.RoadmapCategories.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Category", id);
    private async Task EnsureUniqueAsync(RoadmapCategory category, CancellationToken ct)
    {
        if (await op.Db.RoadmapCategories.AnyAsync(x => x.Id != category.Id
            && (x.Slug == category.Slug || x.Name.ToLower() == category.Name.ToLower()), ct))
            throw new ConflictException("A category with this name or slug already exists.");
    }
    private static void Apply(RoadmapCategory category, CategoryRequest request)
    {
        category.Description = request.Description;
        category.Icon = request.Icon;
        category.Color = request.Color;
    }
}
