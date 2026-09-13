using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Application.Features.Roadmaps.Models;

public sealed record PageResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems, int TotalPages);
public sealed record CategoryResponse(Guid Id, string Name, string Slug, string? Description,
    string? Icon, string? Color, int SortOrder, bool IsActive);
public sealed record CategorySummary(Guid Id, string Name);
public sealed record RoadmapSummary(Guid Id, string Title, string Slug, string ShortDescription,
    string? ThumbnailUrl, CategorySummary Category, RoadmapLevel Level, string? EstimatedDuration,
    RoadmapStatus Status, int SortOrder, int NodeCount)
{
    // Kept for the original list contract; the graph uses nodes rather than sections.
    public int SectionCount => NodeCount;
}
public sealed record NodePosition(decimal X, decimal Y);
public sealed record NodeResponse(Guid Id, string Title, string Slug, string? Description,
    RoadmapNodeType NodeType, NodePosition Position, decimal? Width, string? Color, string? Icon,
    int SortOrder, bool IsActive, int ResourceCount);
public sealed record EdgeResponse(Guid Id, Guid SourceNodeId, Guid TargetNodeId,
    RoadmapRelationType RelationType, RoadmapLineStyle LineStyle, string? Label, int SortOrder, bool IsActive);
public sealed record RoadmapResponse(Guid Id, string Title, string Slug, string ShortDescription,
    string? Description, string? ThumbnailUrl, CategorySummary Category, RoadmapLevel Level,
    string? EstimatedDuration, string? Prerequisites, RoadmapStatus Status, int SortOrder,
    DateTimeOffset? PublishedAtUtc, IReadOnlyList<NodeResponse> Nodes, IReadOnlyList<EdgeResponse> Edges);
public sealed record ResourceResponse(Guid Id, string Title, string? Description, ResourceType ResourceType,
    string? ExternalUrl, string? OriginalFileName, long? FileSize, string? ContentType,
    string? DownloadUrl, int SortOrder, bool IsActive);
public sealed record NodeReference(Guid Id, string Title, string Slug, RoadmapRelationType RelationType);
public sealed record NodeDetailResponse(NodeResponse Node, string? LearningObjectives,
    string? EstimatedDuration, IReadOnlyList<ResourceResponse> Resources,
    IReadOnlyList<NodeReference> Prerequisites, IReadOnlyList<NodeReference> NextNodes);
public sealed record ResourceDownload(Stream Content, string FileName, string ContentType);
