using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Memberships;
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

    public DbSet<ClubGeneration> ClubGenerations =>
        Set<ClubGeneration>();

    public DbSet<ClubRole> ClubRoles =>
        Set<ClubRole>();

    public DbSet<ClubMembership> ClubMemberships =>
        Set<ClubMembership>();

    public DbSet<DepartmentMembership> DepartmentMemberships =>
        Set<DepartmentMembership>();

    public DbSet<RoleAssignment> RoleAssignments =>
        Set<RoleAssignment>();


    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Cấu hình Schema mặc định trong cơ sở dữ liệu
        builder.HasDefaultSchema("gdsc");

        // 2. Đổi tên các bảng mặc định của ASP.NET Core Identity thành tên ngắn gọn, chuẩn REST
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // 3. Tự động quét và nạp toàn bộ cấu hình chi tiết từ các file IEntityTypeConfiguration<T>
        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}