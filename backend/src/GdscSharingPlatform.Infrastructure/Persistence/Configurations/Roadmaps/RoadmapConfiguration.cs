using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Roadmaps;

public sealed class RoadmapConfiguration : IEntityTypeConfiguration<Roadmap>
{
    public void Configure(EntityTypeBuilder<Roadmap> builder)
    {
        builder.ToTable("Roadmaps", table =>
        {
            table.HasCheckConstraint("CK_Roadmaps_SortOrder", """
                "SortOrder" >= 0
                """);
            table.HasCheckConstraint("CK_Roadmaps_Title", """
                btrim("Title") <> ''
                """);
            table.HasCheckConstraint("CK_Roadmaps_ShortDescription", """
                btrim("ShortDescription") <> ''
                """);
            table.HasCheckConstraint("CK_Roadmaps_Slug", """
                "Slug" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'
                """);
            table.HasCheckConstraint("CK_Roadmaps_Status", """
                "Status" IN (0, 1, 2)
                """);
            table.HasCheckConstraint("CK_Roadmaps_Level", """
                "Level" IN (0, 1, 2, 3)
                """);
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.SortOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ShortDescription).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Prerequisites).HasColumnType("text");
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(2048);
        builder.Property(x => x.EstimatedDuration).HasMaxLength(100);
        builder.Property(x => x.Level).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>()
            .HasDefaultValue(RoadmapStatus.Published).HasSentinel((RoadmapStatus)(-1)).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.Status, x.SortOrder });
        builder.HasIndex(x => new { x.CategoryId, x.Status });
        builder.HasOne(x => x.Category).WithMany(x => x.Roadmaps)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany()
            .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany()
            .HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
