using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Roadmaps;

public sealed class RoadmapEdgeConfiguration : IEntityTypeConfiguration<RoadmapEdge>
{
    public void Configure(EntityTypeBuilder<RoadmapEdge> builder)
    {
        builder.ToTable("RoadmapEdges", table =>
        {
            table.HasCheckConstraint("CK_RoadmapEdges_SortOrder", """
                "SortOrder" >= 0
                """);
            table.HasCheckConstraint("CK_RoadmapEdges_NoSelfLoop", """
                "SourceNodeId" <> "TargetNodeId"
                """);
            table.HasCheckConstraint("CK_RoadmapEdges_RelationType", """
                "RelationType" IN (0, 1, 2)
                """);
            table.HasCheckConstraint("CK_RoadmapEdges_LineStyle", """
                "LineStyle" IN (0, 1)
                """);
            table.HasCheckConstraint("CK_RoadmapEdges_RequiredSolid", """
                "RelationType" <> 0 OR "LineStyle" = 0
                """);
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.SortOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.RelationType).HasConversion<int>().IsRequired();
        builder.Property(x => x.LineStyle).HasConversion<int>().IsRequired();
        builder.Property(x => x.Label).HasMaxLength(250);
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.HasIndex(x => new { x.RoadmapId, x.SourceNodeId, x.TargetNodeId, x.RelationType }).IsUnique();
        builder.HasIndex(x => new { x.RoadmapId, x.IsActive });
        builder.HasOne(x => x.Roadmap).WithMany(x => x.Edges)
            .HasForeignKey(x => x.RoadmapId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SourceNode).WithMany(x => x.OutgoingEdges)
            .HasForeignKey(x => new { x.RoadmapId, x.SourceNodeId })
            .HasPrincipalKey(x => new { x.RoadmapId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetNode).WithMany(x => x.IncomingEdges)
            .HasForeignKey(x => new { x.RoadmapId, x.TargetNodeId })
            .HasPrincipalKey(x => new { x.RoadmapId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
