using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Roadmaps;

public sealed class LearningResourceConfiguration : IEntityTypeConfiguration<LearningResource>
{
    public void Configure(EntityTypeBuilder<LearningResource> builder)
    {
        builder.ToTable("LearningResources", table =>
        {
            table.HasCheckConstraint("CK_LearningResources_SortOrder", """
                "SortOrder" >= 0
                """);
            table.HasCheckConstraint("CK_LearningResources_Title", """
                btrim("Title") <> ''
                """);
            table.HasCheckConstraint("CK_LearningResources_ResourceMetadata", """
                ("ResourceType" = 0 AND "ExternalUrl" IS NOT NULL AND btrim("ExternalUrl") <> '' AND "OriginalFileName" IS NULL AND "StoredFileName" IS NULL AND "StorageKey" IS NULL AND "FileSize" IS NULL AND "ContentType" IS NULL) OR ("ResourceType" = 1 AND "ExternalUrl" IS NULL AND "OriginalFileName" IS NOT NULL AND btrim("OriginalFileName") <> '' AND "StoredFileName" IS NOT NULL AND btrim("StoredFileName") <> '' AND "StorageKey" IS NOT NULL AND btrim("StorageKey") <> '' AND "FileSize" IS NOT NULL AND "FileSize" >= 0 AND "ContentType" IS NOT NULL AND btrim("ContentType") <> '')
                """);
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.SortOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ResourceType).HasConversion<int>().IsRequired();
        builder.Property(x => x.ExternalUrl).HasMaxLength(2048);
        builder.Property(x => x.OriginalFileName).HasMaxLength(255);
        builder.Property(x => x.StoredFileName).HasMaxLength(255);
        builder.Property(x => x.StorageKey).HasMaxLength(1024);
        builder.Property(x => x.ContentType).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.HasIndex(x => new { x.RoadmapNodeId, x.IsActive, x.SortOrder });
        builder.HasOne(x => x.RoadmapNode).WithMany(x => x.Resources)
            .HasForeignKey(x => x.RoadmapNodeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany()
            .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
