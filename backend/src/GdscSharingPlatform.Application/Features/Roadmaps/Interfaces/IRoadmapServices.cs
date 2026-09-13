using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;

namespace GdscSharingPlatform.Application.Features.Roadmaps.Interfaces;

public interface IRoadmapCategoryService
{
    Task<IReadOnlyList<CategoryResponse>> ListAsync(bool admin = false, CancellationToken ct = default);
    Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken ct = default);
    Task<CategoryResponse> UpdateAsync(Guid id, CategoryRequest request, CancellationToken ct = default);
    Task SetStatusAsync(Guid id, ActiveStatusRequest request, CancellationToken ct = default);
}
public interface IRoadmapService
{
    Task<PageResponse<RoadmapSummary>> ListAsync(RoadmapQuery query, bool admin = false, CancellationToken ct = default);
    Task<RoadmapResponse> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<RoadmapResponse> GetAsync(Guid id, CancellationToken ct = default);
    Task<RoadmapResponse> CreateAsync(RoadmapRequest request, CancellationToken ct = default);
    Task<RoadmapResponse> UpdateAsync(Guid id, RoadmapRequest request, CancellationToken ct = default);
    Task SetStatusAsync(Guid id, RoadmapStatusRequest request, CancellationToken ct = default);
    Task ReorderAsync(ReorderRequest request, CancellationToken ct = default);
}
public interface IRoadmapNodeService
{
    Task<IReadOnlyList<NodeResponse>> ListAsync(Guid roadmapId, CancellationToken ct = default);
    Task<NodeDetailResponse> GetAsync(Guid roadmapId, Guid nodeId, CancellationToken ct = default);
    Task<NodeResponse> CreateAsync(Guid roadmapId, NodeRequest request, CancellationToken ct = default);
    Task<NodeResponse> UpdateAsync(Guid roadmapId, Guid nodeId, NodeRequest request, CancellationToken ct = default);
    Task SetStatusAsync(Guid roadmapId, Guid nodeId, ActiveStatusRequest request, CancellationToken ct = default);
    Task SavePositionsAsync(Guid roadmapId, NodePositionsRequest request, CancellationToken ct = default);
}
public interface IRoadmapEdgeService
{
    Task<IReadOnlyList<EdgeResponse>> ListAsync(Guid roadmapId, CancellationToken ct = default);
    Task<EdgeResponse> CreateAsync(Guid roadmapId, EdgeRequest request, CancellationToken ct = default);
    Task<EdgeResponse> UpdateAsync(Guid roadmapId, Guid edgeId, EdgeRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid roadmapId, Guid edgeId, CancellationToken ct = default);
}
public interface ILearningResourceService
{
    Task<IReadOnlyList<ResourceResponse>> ListAsync(Guid nodeId, CancellationToken ct = default);
    Task<ResourceResponse> CreateLinkAsync(Guid nodeId, LinkResourceRequest request, CancellationToken ct = default);
    Task<ResourceResponse> CreateFileAsync(Guid nodeId, FileResourceRequest request, FileUpload upload, CancellationToken ct = default);
    Task<ResourceResponse> UpdateAsync(Guid id, UpdateResourceRequest request, CancellationToken ct = default);
    Task<ResourceResponse> ReplaceFileAsync(Guid id, FileUpload upload, CancellationToken ct = default);
    Task SetStatusAsync(Guid id, ActiveStatusRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderAsync(Guid nodeId, ReorderRequest request, CancellationToken ct = default);
    Task<ResourceDownload> DownloadAsync(Guid id, CancellationToken ct = default);
}
