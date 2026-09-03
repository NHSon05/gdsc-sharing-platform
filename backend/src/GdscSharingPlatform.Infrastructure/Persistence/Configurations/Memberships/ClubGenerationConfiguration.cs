using GdscSharingPlatform.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Memberships;

public class ClubGenerationConfiguration : IEntityTypeConfiguration<ClubGeneration>
{
    public void Configure(EntityTypeBuilder<ClubGeneration> builder)
    {
        builder.ToTable("ClubGenerations");

        builder.HasKey(cg => cg.Id);

        builder.Property(cg => cg.Number)
            .IsRequired();

        builder.Property(cg => cg.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cg => cg.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(cg => cg.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(cg => cg.Number)
            .IsUnique();

        builder.HasIndex(cg => cg.Name);

        builder.HasIndex(cg => cg.IsActive);

        builder.HasMany(cg => cg.Memberships)
            .WithOne(cm => cm.Generation)
            .HasForeignKey(cm => cm.GenerationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
