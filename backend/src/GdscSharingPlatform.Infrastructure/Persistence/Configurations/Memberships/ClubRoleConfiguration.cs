using GdscSharingPlatform.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Memberships;

public class ClubRoleConfiguration : IEntityTypeConfiguration<ClubRole>
{
    public void Configure(EntityTypeBuilder<ClubRole> builder)
    {
        builder.ToTable("ClubRoles");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cr => cr.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(cr => cr.Level)
            .IsRequired();

        builder.Property(cr => cr.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(cr => cr.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(cr => cr.Code)
            .IsUnique();

        builder.HasIndex(cr => cr.Level);

        builder.HasIndex(cr => cr.IsActive);

        builder.HasMany(cr => cr.RoleAssignments)
            .WithOne(ra => ra.ClubRole)
            .HasForeignKey(ra => ra.ClubRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
