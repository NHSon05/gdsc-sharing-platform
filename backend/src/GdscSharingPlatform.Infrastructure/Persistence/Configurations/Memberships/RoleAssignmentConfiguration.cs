using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Memberships;

public class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.ToTable("RoleAssignments");

        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.DepartmentMembershipId)
            .IsRequired();

        builder.Property(ra => ra.ClubRoleId)
            .IsRequired();

        builder.Property(ra => ra.AssignedAtUtc)
            .IsRequired();

        builder.Property(ra => ra.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(ra => new { ra.DepartmentMembershipId, ra.ClubRoleId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");

        builder.HasIndex(ra => ra.DepartmentMembershipId);

        builder.HasIndex(ra => ra.ClubRoleId);

        builder.HasIndex(ra => ra.AssignedByUserId);

        builder.HasIndex(ra => ra.IsActive);

        builder.HasOne(ra => ra.DepartmentMembership)
            .WithMany(dm => dm.RoleAssignments)
            .HasForeignKey(ra => ra.DepartmentMembershipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ra => ra.ClubRole)
            .WithMany(cr => cr.RoleAssignments)
            .HasForeignKey(ra => ra.ClubRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(ra => ra.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
