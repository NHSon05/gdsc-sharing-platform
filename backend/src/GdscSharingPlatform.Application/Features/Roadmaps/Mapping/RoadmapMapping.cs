using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;

namespace GdscSharingPlatform.Application.Features.Roadmaps.Mapping;

public static class RoadmapMapping
{
    public static CategoryResponse ToResponse(this RoadmapCategory x) =>
        new(x.Id, x.Name, x.Slug, x.Description, x.Icon, x.Color, x.SortOrder, x.IsActive);
    public static NodeResponse ToResponse(this RoadmapNode x, int resourceCount) =>
        new(x.Id, x.Title, x.Slug, x.Description, x.NodeType, new(x.PositionX, x.PositionY),
            x.Width, x.Color, x.Icon, x.SortOrder, x.IsActive, resourceCount);
    public static EdgeResponse ToResponse(this RoadmapEdge x) =>
        new(x.Id, x.SourceNodeId, x.TargetNodeId, x.RelationType, x.LineStyle, x.Label, x.SortOrder, x.IsActive);
    public static ResourceResponse ToResponse(this LearningResource x) =>
        new(x.Id, x.Title, x.Description, x.ResourceType, x.ExternalUrl, x.OriginalFileName,
            x.FileSize, x.ContentType, x.ResourceType == ResourceType.File ? $"/api/roadmap-resources/{x.Id}/download" : null,
            x.SortOrder, x.IsActive);
    public static RoadmapResponse ToResponse(this Roadmap x, IReadOnlyList<NodeResponse> nodes, IReadOnlyList<EdgeResponse> edges) =>
        new(x.Id, x.Title, x.Slug, x.ShortDescription, x.Description, x.ThumbnailUrl,
            new(x.CategoryId, x.Category.Name), x.Level, x.EstimatedDuration, x.Prerequisites,
            x.Status, x.SortOrder, x.PublishedAtUtc, nodes, edges);
}
