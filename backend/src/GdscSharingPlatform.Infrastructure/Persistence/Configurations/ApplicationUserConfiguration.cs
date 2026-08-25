using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GdscSharingPlatform.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName)
            .HasMaxLength(150)
            .IsRequired();
        builder.Property(user => user.DisplayName)
            .HasMaxLength(2048);
        builder.Property(user => user.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(user => user.Bio)
            .HasMaxLength(1000);

        builder.Property(user => user.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(user => user.TimeZone)
            .HasMaxLength(100)
            .HasDefaultValue("Asia/Ho_Chi_Minh")
            .IsRequired();

        builder.Property(user => user.Locale)
            .HasMaxLength(10)
            .HasDefaultValue("vi-VN")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Property(user => user.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(user => user.DepartmentId);

        builder.HasIndex(user => user.Status);

        builder.HasIndex(user => user.IsDeleted);

        builder.HasOne(user => user.Department)
            .WithMany()
            .HasForeignKey(user => user.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}