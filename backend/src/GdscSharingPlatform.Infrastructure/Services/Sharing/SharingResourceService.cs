using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Sharing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Services.Sharing;

public sealed class SharingResourceService(SharingOperations op, IFileStorage storage,
    ILogger<SharingResourceService> logger) : ISharingResourceService
{
    public async Task<IReadOnlyList<ResourceResponse>> ListAsync(Guid contentId, CancellationToken ct)
    {
        await RequireReadAsync(contentId, ct);
        return await op.Db.SharingResources.AsNoTracking().Where(x => x.SharingContentId == contentId && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(SharingOperations.Resource).ToListAsync(ct);
    }
    public async Task<ResourceMutationResponse> CreateAsync(Guid contentId, ResourceRequest request, FileUpload? file, long version, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        if (file is null && !SharingRules.HttpUrl(request.ExternalUrl)) throw new ApplicationValidationException("externalUrl", "An HTTP(S) URL is required.");
        if (file is not null && request.ExternalUrl is not null) throw new ApplicationValidationException("externalUrl", "File and Link are mutually exclusive.");
        StoredFile? saved = null;
        try
        {
            return await op.WriteAsync(async () =>
            {
                var content = await op.EditableContentAsync(contentId, version, ct);
                var resource = new SharingResource(contentId, request.Title, file is null ? ResourceType.Link : ResourceType.File, request.SortOrder, op.UserId);
                resource.Update(request.Title, request.Description, request.ExternalUrl, request.SortOrder);
                if (file is not null)
                {
                    saved = await storage.SaveAsync(file, ct);
                    resource.SetFile(saved.OriginalFileName, saved.StorageKey, saved.FileSize, saved.ContentType);
                }
                op.Db.Add(resource); content.Touch(op.UserId);
                op.Audit(file is null ? "AddLink" : "Upload", "Content", contentId, new { ResourceId = resource.Id, content.Version });
                return new ResourceMutationResponse(SharingOperations.ResourceDto(resource), content.Version);
            }, ct);
        }
        catch { if (saved is not null) await CleanupAsync(saved.StorageKey); throw; }
    }
    public async Task<ResourceMutationResponse> UpdateAsync(Guid id, ResourceRequest request, long version, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var resource = await FindAsync(id, ct);
            var content = await op.EditableContentAsync(resource.SharingContentId, version, ct);
            resource.Update(request.Title, request.Description, request.ExternalUrl, request.SortOrder);
            content.Touch(op.UserId);
            op.Audit("UpdateResource", "Content", content.Id, new { ResourceId = id, content.Version });
            return new ResourceMutationResponse(SharingOperations.ResourceDto(resource), content.Version);
        }, ct);
    }
    public async Task<ResourceMutationResponse> ReplaceAsync(Guid id, FileUpload file, long version, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        StoredFile? saved = null;
        string? old = null;
        ResourceMutationResponse result;
        try
        {
            result = await op.WriteAsync(async () =>
            {
                var resource = await FindAsync(id, ct);
                var content = await op.EditableContentAsync(resource.SharingContentId, version, ct);
                if (resource.ResourceType != ResourceType.File) throw new ApplicationValidationException("Only File resources can be replaced.");
                old = resource.StorageKey;
                saved = await storage.SaveAsync(file, ct);
                resource.SetFile(saved.OriginalFileName, saved.StorageKey, saved.FileSize, saved.ContentType);
                content.Touch(op.UserId);
                op.Audit("ReplaceFile", "Content", content.Id, new { ResourceId = id, content.Version });
                return new ResourceMutationResponse(SharingOperations.ResourceDto(resource), content.Version);
            }, ct);
        }
        catch { if (saved is not null) await CleanupAsync(saved.StorageKey); throw; }
        if (old is not null) await CleanupAsync(old);
        return result;
    }
    public async Task<long> DeleteAsync(Guid id, long version, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        // Soft delete retains file metadata and bytes for audit/history.
        return await op.WriteAsync(async () =>
        {
            var resource = await FindAsync(id, ct);
            var content = await op.EditableContentAsync(resource.SharingContentId, version, ct);
            resource.Deactivate(); content.Touch(op.UserId);
            op.Audit("DeactivateResource", "Content", content.Id, new { ResourceId = id, content.Version });
            return content.Version;
        }, ct);
    }
    public async Task<long> ReorderAsync(Guid contentId, ReorderResourcesRequest request, long version, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct); await op.ValidateAsync(request, ct);
        return await op.WriteAsync(async () =>
        {
            var content = await op.EditableContentAsync(contentId, version, ct);
            var resources = await op.Db.SharingResources.Where(x => x.SharingContentId == contentId && x.IsActive).ToDictionaryAsync(x => x.Id, ct);
            if (!resources.Keys.ToHashSet().SetEquals(request.Ids)) throw new ApplicationValidationException("ids", "Reorder must include every active resource exactly once.");
            for (var i = 0; i < request.Ids.Count; i++) resources[request.Ids[i]].Reorder(i);
            content.Touch(op.UserId); op.Audit("ReorderResources", "Content", contentId, new { content.Version });
            return content.Version;
        }, ct);
    }
    public async Task<SharingDownload> DownloadAsync(Guid id, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        var resource = await op.Db.SharingResources.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsActive && x.ResourceType == ResourceType.File, ct)
            ?? throw new NotFoundException("Resource", id);
        await RequireReadAsync(resource.SharingContentId, ct);
        return new(await storage.OpenReadAsync(resource.StorageKey!, ct), resource.OriginalFileName!, resource.ContentType!);
    }
    private async Task RequireReadAsync(Guid id, CancellationToken ct)
    {
        await op.RequireReaderAsync(ct);
        if (!await op.VisibleContents().AnyAsync(x => x.Id == id, ct)) throw new NotFoundException("Content", id);
    }
    private async Task<SharingResource> FindAsync(Guid id, CancellationToken ct) =>
        await op.Db.SharingResources.SingleOrDefaultAsync(x => x.Id == id && x.IsActive, ct) ?? throw new NotFoundException("Resource", id);
    private async Task CleanupAsync(string key)
    {
        try
        {
            // Commit cancellation may have an uncertain outcome; never delete bytes still referenced in either module.
            if (!await op.Db.SharingResources.AsNoTracking().AnyAsync(x => x.StorageKey == key, CancellationToken.None)
                && !await op.Db.LearningResources.AsNoTracking().AnyAsync(x => x.StorageKey == key, CancellationToken.None))
                await storage.DeleteAsync(key, CancellationToken.None);
        }
        catch { logger.LogError("Sharing file cleanup requires retry for storage key {StorageKey}.", key); }
    }
}
