using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Memberships;

public class DepartmentMembershipConfiguration : IEntityTypeConfiguration<DepartmentMembership>
{
    public void Configure(EntityTypeBuilder<DepartmentMembership> builder)
    {
        builder.ToTable("DepartmentMemberships");

        builder.HasKey(dm => dm.Id);

        builder.Property(dm => dm.ClubMembershipId)
            .IsRequired();

        builder.Property(dm => dm.DepartmentId)
            .IsRequired();

        builder.Property(dm => dm.IsPrimary)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(dm => dm.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(dm => dm.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(dm => new { dm.ClubMembershipId, dm.DepartmentId })
            .IsUnique();

        builder.HasIndex(dm => dm.ClubMembershipId);

        builder.HasIndex(dm => dm.DepartmentId);

        builder.HasIndex(dm => dm.IsActive);

        builder.HasIndex(dm => dm.IsPrimary);

        builder.HasOne(dm => dm.ClubMembership)
            .WithMany(cm => cm.DepartmentMemberships)
            .HasForeignKey(dm => dm.ClubMembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dm => dm.Department)
            .WithMany(d => d.DepartmentMemberships)
            .HasForeignKey(dm => dm.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(dm => dm.RoleAssignments)
            .WithOne(ra => ra.DepartmentMembership)
            .HasForeignKey(ra => ra.DepartmentMembershipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
