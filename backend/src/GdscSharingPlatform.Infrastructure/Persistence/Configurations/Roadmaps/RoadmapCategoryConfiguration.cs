using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Roadmaps;

public sealed class RoadmapCategoryConfiguration : IEntityTypeConfiguration<RoadmapCategory>
{
    public void Configure(EntityTypeBuilder<RoadmapCategory> builder)
    {
        builder.ToTable("RoadmapCategories", table =>
        {
            table.HasCheckConstraint("CK_RoadmapCategories_SortOrder", """
                "SortOrder" >= 0
                """);
            table.HasCheckConstraint("CK_RoadmapCategories_Name", """
                btrim("Name") <> ''
                """);
            table.HasCheckConstraint("CK_RoadmapCategories_Slug", """
                "Slug" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'
                """);
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.SortOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Icon).HasMaxLength(2048);
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
