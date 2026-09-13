using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Roadmaps;

public sealed class RoadmapNodeConfiguration : IEntityTypeConfiguration<RoadmapNode>
{
    public void Configure(EntityTypeBuilder<RoadmapNode> builder)
    {
        builder.ToTable("RoadmapNodes", table =>
        {
            table.HasCheckConstraint("CK_RoadmapNodes_SortOrder", """
                "SortOrder" >= 0
                """);
            table.HasCheckConstraint("CK_RoadmapNodes_Title", """
                btrim("Title") <> ''
                """);
            table.HasCheckConstraint("CK_RoadmapNodes_Slug", """
                "Slug" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'
                """);
            table.HasCheckConstraint("CK_RoadmapNodes_NodeType", """
                "NodeType" IN (0, 1, 2)
                """);
            table.HasCheckConstraint("CK_RoadmapNodes_Width", """
                "Width" IS NULL OR "Width" > 0
                """);
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.SortOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.LearningObjectives).HasColumnType("text");
        builder.Property(x => x.EstimatedDuration).HasMaxLength(100);
        builder.Property(x => x.NodeType).HasConversion<int>().IsRequired();
        builder.Property(x => x.PositionX).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.PositionY).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Width).HasPrecision(18, 4);
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.Property(x => x.Icon).HasMaxLength(2048);
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.HasIndex(x => new { x.RoadmapId, x.Slug }).IsUnique();
        builder.HasIndex(x => new { x.RoadmapId, x.IsActive });
        // Composite edge FKs ensure both endpoints belong to the edge's roadmap.
        builder.HasAlternateKey(x => new { x.RoadmapId, x.Id });
        builder.HasOne(x => x.Roadmap).WithMany(x => x.Nodes)
            .HasForeignKey(x => x.RoadmapId).OnDelete(DeleteBehavior.Restrict);
    }
}
