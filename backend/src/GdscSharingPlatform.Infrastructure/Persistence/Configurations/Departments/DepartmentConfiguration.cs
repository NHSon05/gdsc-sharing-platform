using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations.Departments;

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
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(department => department.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(department => department.Description)
            .HasMaxLength(500);

        builder.Property(department => department.Color)
            .HasMaxLength(20);

        builder.Property(department => department.Icon)
            .HasMaxLength(100);

        builder.Property(department => department.SortOrder)
            .HasColumnName("DisplayOrder")
            .HasDefaultValue(0);

        builder.Ignore(department => department.DisplayOrder);
        builder.Ignore(department => department.CreatedAtUtc);
        builder.Ignore(department => department.UpdatedAtUtc);
        builder.Ignore(department => department.DeletedAtUtc);

        builder.Property(department => department.IsActive)
            .HasDefaultValue(true);

        builder.Property(department => department.CreatedAt)
            .IsRequired();

        builder.Property(department => department.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(department => department.Code)
            .IsUnique();

        builder.HasIndex(department => department.Slug)
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
