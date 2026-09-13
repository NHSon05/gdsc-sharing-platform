using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Mapping;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Application.Features.Roadmaps.Rules;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Services.Roadmaps;

public sealed class LearningResourceService(RoadmapOperations op, IFileStorage storage,
    ILogger<LearningResourceService> logger) : ILearningResourceService
{
    public async Task<IReadOnlyList<ResourceResponse>> ListAsync(Guid nodeId, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ResourceNodeAsync(nodeId, false, ct);
        var resources = await op.Db.LearningResources.AsNoTracking().Where(x => x.RoadmapNodeId == nodeId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        return resources.Select(x => x.ToResponse()).ToArray();
    }
    public async Task<ResourceResponse> CreateLinkAsync(Guid nodeId, LinkResourceRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            await op.ResourceNodeAsync(nodeId, true, ct);
            var resource = LearningResource.CreateLink(nodeId, request.Title, request.ExternalUrl, op.UserId, request.SortOrder);
            resource.Description = request.Description;
            op.Db.Add(resource);
            return resource.ToResponse();
        }, ct);
    }
    public async Task<ResourceResponse> CreateFileAsync(Guid nodeId, FileResourceRequest request, FileUpload upload, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        StoredFile? file = null;
        try
        {
            return await op.WriteAsync(async () =>
            {
                await op.ResourceNodeAsync(nodeId, true, ct);
                file = await storage.SaveAsync(upload, ct);
                var resource = LearningResource.CreateFile(nodeId, request.Title, file.OriginalFileName, file.StoredFileName,
                    file.StorageKey, file.FileSize, file.ContentType, op.UserId, request.SortOrder);
                resource.Description = request.Description;
                op.Db.Add(resource);
                return resource.ToResponse();
            }, ct);
        }
        catch
        {
            if (file is not null) await DeleteIfUnreferencedAsync(file.StorageKey);
            throw;
        }
    }
    public async Task<ResourceResponse> UpdateAsync(Guid id, UpdateResourceRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var resource = await FindAsync(id, ct);
            if (resource.ResourceType == ResourceType.Link && string.IsNullOrWhiteSpace(request.ExternalUrl))
                throw new ApplicationValidationException("externalUrl", "A link must contain an HTTP(S) URL.");
            if (resource.ResourceType == ResourceType.File && request.ExternalUrl is not null)
                throw new ApplicationValidationException("externalUrl", "File resources cannot contain an external URL.");
            resource.Update(request.Title, request.Description, request.ExternalUrl, request.SortOrder);
            return resource.ToResponse();
        }, ct);
    }
    public async Task<ResourceResponse> ReplaceFileAsync(Guid id, FileUpload upload, CancellationToken ct = default)
    {
        op.RequireAdmin();
        StoredFile? file = null;
        string? oldKey = null;
        ResourceResponse response;
        try
        {
            response = await op.WriteAsync(async () =>
            {
                var resource = await FindAsync(id, ct);
                if (resource.ResourceType != ResourceType.File) throw new ConflictException("Only file resources can be replaced.");
                oldKey = resource.StorageKey;
                file = await storage.SaveAsync(upload, ct);
                resource.ReplaceFile(file.OriginalFileName, file.StoredFileName, file.StorageKey, file.FileSize, file.ContentType);
                return resource.ToResponse();
            }, ct);
        }
        catch
        {
            if (file is not null) await DeleteIfUnreferencedAsync(file.StorageKey);
            throw;
        }
        // Delete the old file only after the database commit succeeds.
        if (oldKey is not null) await DeleteIfUnreferencedAsync(oldKey);
        return response;
    }
    public async Task SetStatusAsync(Guid id, ActiveStatusRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () => { (await FindAsync(id, ct)).SetActive(request.IsActive!.Value); return true; }, ct);
    }
    // DELETE is a soft delete. File and metadata remain available for administration.
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => SetStatusAsync(id, new(false), ct);

    public async Task ReorderAsync(Guid nodeId, ReorderRequest request, CancellationToken ct = default)
    {
        op.RequireAdmin();
        await op.ValidateAsync(request, ct);
        await op.WriteAsync(async () =>
        {
            await op.ResourceNodeAsync(nodeId, false, ct);
            var resources = await op.Db.LearningResources.Where(x => x.RoadmapNodeId == nodeId).ToDictionaryAsync(x => x.Id, ct);
            RoadmapRules.EnsureCompleteReorder(resources.Keys, request.Ids);
            for (var i = 0; i < request.Ids.Count; i++) resources[request.Ids[i]].Reorder(i);
            return true;
        }, ct);
    }
    public async Task<ResourceDownload> DownloadAsync(Guid id, CancellationToken ct = default)
    {
        op.RequireReader();
        var resource = await op.Db.LearningResources.AsNoTracking().Include(x => x.RoadmapNode).ThenInclude(x => x.Roadmap)
            .SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Resource", id);
        if (resource.ResourceType != ResourceType.File || (!op.IsAdmin && (!resource.IsActive || !resource.RoadmapNode.IsActive
            || !RoadmapRules.IsMemberVisible(resource.RoadmapNode.Roadmap.Status))))
            throw new NotFoundException("Resource", id);
        var stream = await storage.OpenReadAsync(resource.StorageKey!, ct);
        return new(stream, resource.OriginalFileName!, resource.ContentType!);
    }
    private async Task<LearningResource> FindAsync(Guid id, CancellationToken ct) =>
        await op.Db.LearningResources.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Resource", id);

    private async Task DeleteIfUnreferencedAsync(string key)
    {
        try
        {
            // A failed/cancelled commit can have an uncertain outcome. Never delete a referenced file.
            if (!await op.Db.LearningResources.AsNoTracking().AnyAsync(x => x.StorageKey == key, CancellationToken.None))
                await storage.DeleteAsync(key, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Resource file cleanup needs retry for storage key {StorageKey}.", key);
        }
    }
}
