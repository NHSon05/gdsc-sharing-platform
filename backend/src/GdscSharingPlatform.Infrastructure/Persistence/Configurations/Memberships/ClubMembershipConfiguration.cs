using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Memberships;

public class ClubMembershipConfiguration : IEntityTypeConfiguration<ClubMembership>
{
    public void Configure(EntityTypeBuilder<ClubMembership> builder)
    {
        builder.ToTable("ClubMemberships");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.UserId)
            .IsRequired();

        builder.Property(cm => cm.GenerationId)
            .IsRequired();

        builder.Property(cm => cm.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(cm => cm.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(cm => new { cm.UserId, cm.GenerationId })
            .IsUnique();

        builder.HasIndex(cm => cm.UserId);

        builder.HasIndex(cm => cm.GenerationId);

        builder.HasIndex(cm => cm.IsActive);

        builder.HasOne<ApplicationUser>()
            .WithMany(u => u.ClubMemberships)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Generation)
            .WithMany(cg => cg.Memberships)
            .HasForeignKey(cm => cm.GenerationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cm => cm.DepartmentMemberships)
            .WithOne(dm => dm.ClubMembership)
            .HasForeignKey(dm => dm.ClubMembershipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
