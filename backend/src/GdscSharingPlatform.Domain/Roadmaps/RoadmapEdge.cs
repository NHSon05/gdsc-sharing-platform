using GdscSharingPlatform.Domain.Common;
using GdscSharingPlatform.Domain.Enums;

namespace GdscSharingPlatform.Domain.Roadmaps;

public sealed class RoadmapEdge : BaseEntity
{
    private RoadmapEdge() { }

    public RoadmapEdge(Guid roadmapId, Guid sourceNodeId, Guid targetNodeId,
        RoadmapRelationType relationType = RoadmapRelationType.Required,
        RoadmapLineStyle lineStyle = RoadmapLineStyle.Solid, int sortOrder = 0)
    {
        RoadmapId = RoadmapValidation.RequiredId(roadmapId, nameof(roadmapId));
        SourceNodeId = RoadmapValidation.RequiredId(sourceNodeId, nameof(sourceNodeId));
        TargetNodeId = RoadmapValidation.RequiredId(targetNodeId, nameof(targetNodeId));
        if (sourceNodeId == targetNodeId)
        {
            throw new ArgumentException("An edge cannot connect a node to itself.", nameof(targetNodeId));
        }

        RelationType = RoadmapValidation.DefinedEnum(relationType);
        LineStyle = RoadmapValidation.DefinedEnum(lineStyle);
        if (relationType == RoadmapRelationType.Required && lineStyle != RoadmapLineStyle.Solid)
        {
            throw new ArgumentException("Required edges must use a solid line.", nameof(lineStyle));
        }

        SortOrder = RoadmapValidation.SortOrder(sortOrder);
    }

    public Guid RoadmapId { get; private set; }
    public Guid SourceNodeId { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public RoadmapRelationType RelationType { get; private set; }
    public string? Label { get; set; }
    public RoadmapLineStyle LineStyle { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Roadmap Roadmap { get; set; } = null!;
    public RoadmapNode SourceNode { get; set; } = null!;
    public RoadmapNode TargetNode { get; set; } = null!;

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
    public void Update(Guid sourceNodeId, Guid targetNodeId, RoadmapRelationType relationType,
        RoadmapLineStyle lineStyle, int sortOrder)
    {
        var validated = new RoadmapEdge(RoadmapId, sourceNodeId, targetNodeId, relationType, lineStyle, sortOrder);
        SourceNodeId = validated.SourceNodeId;
        TargetNodeId = validated.TargetNodeId;
        RelationType = validated.RelationType;
        LineStyle = validated.LineStyle;
        SortOrder = validated.SortOrder;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

}
