using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Application.Features.Roadmaps.Models;

public sealed record CategoryRequest(string Name, string Slug, string? Description = null,
    string? Icon = null, string? Color = null, int SortOrder = 0);

public sealed record RoadmapRequest(Guid CategoryId, string Title, string Slug,
    string ShortDescription, RoadmapLevel Level, string? Description = null,
    string? ThumbnailUrl = null, string? EstimatedDuration = null,
    string? Prerequisites = null, int SortOrder = 0);

public sealed record NodeRequest(string Title, string Slug, RoadmapNodeType NodeType = RoadmapNodeType.Topic,
    decimal PositionX = 0, decimal PositionY = 0, decimal? Width = null,
    string? Description = null, string? LearningObjectives = null,
    string? EstimatedDuration = null, string? Color = null, string? Icon = null, int SortOrder = 0);

public sealed record EdgeRequest(Guid SourceNodeId, Guid TargetNodeId,
    RoadmapRelationType RelationType = RoadmapRelationType.Required,
    RoadmapLineStyle LineStyle = RoadmapLineStyle.Solid, string? Label = null, int SortOrder = 0);

public sealed record LinkResourceRequest(string Title, string ExternalUrl, string? Description = null, int SortOrder = 0);
public sealed record FileResourceRequest(string Title, string? Description = null, int SortOrder = 0);
public sealed record UpdateResourceRequest(string Title, string? Description = null, string? ExternalUrl = null, int SortOrder = 0);
public sealed record ActiveStatusRequest(bool? IsActive);
public sealed record RoadmapStatusRequest(RoadmapStatus? Status);
public sealed record ReorderRequest(IReadOnlyList<Guid> Ids);
public sealed record NodePositionRequest(Guid Id, decimal PositionX, decimal PositionY);
public sealed record NodePositionsRequest(IReadOnlyList<NodePositionRequest> Nodes);

public sealed class RoadmapQuery
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public RoadmapLevel? Level { get; init; }
    public RoadmapStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
