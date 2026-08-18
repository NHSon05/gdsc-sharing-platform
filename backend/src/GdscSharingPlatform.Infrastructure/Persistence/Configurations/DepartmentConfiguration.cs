using GdscSharingPlatform.Domain.Entities;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharing.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration
    : IEntityTypeConfiguration<Department>
{
    public void Configure(
        EntityTypeBuilder<Department> builder)
    {
        // Đặt tên bảng
        builder.ToTable("Departments");

        // Đánh dấu là khoá chính
        builder.HasKey(department => department.Id);

        builder.Property(department => department.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(department => department.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(department => department.Description)
            .HasMaxLength(1000);

        builder.Property(department => department.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(department => department.IsActive)
            .HasDefaultValue(true);

        builder.Property(department => department.CreatedAt)
            .IsRequired();

        builder.Property(department => department.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(department => department.Code)
            .IsUnique();

        builder.HasIndex(department => department.Name);

        builder.HasIndex(department => department.IsActive);

        builder.HasIndex(department => department.LeaderId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(department => department.LeaderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(
            department => !department.IsDeleted);
    }
}

// Đây là lớp cấu hình EF Core cho Entity Department
// Quy định ánh xạ giữa đối tượng Department trong C# với bảng Departments trong db,
// gồm tên bảng, constraints, index, relationship và global filter
