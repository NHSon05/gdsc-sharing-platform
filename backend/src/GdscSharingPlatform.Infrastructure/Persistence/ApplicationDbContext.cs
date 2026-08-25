using GdscSharingPlatform.Domain.Entities;
using GdscSharingPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>(options)
{
    public DbSet<Department> Departments =>
        Set<Department>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tự động quét và áp dụng tất cả các cấu hình EntityTypeConfiguration trong Assembly
        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}