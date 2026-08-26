using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Identity.Seeding;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Identity.Seeding;

public class DatabaseSeederTests
{
    private (ServiceProvider Provider, ApplicationDbContext DbContext, DatabaseSeeder Seeder) CreateSeederEnvironment(
        AdminSeedOptions? adminOptions = null)
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        services.AddLogging(builder => builder.AddDebug());

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        var optionsModel = adminOptions ?? new AdminSeedOptions
        {
            Enabled = true,
            Email = "admin@gdsc.test",
            Password = "Password123!",
            FullName = "Platform Administrator",
            DepartmentCode = "MANAGEMENT"
        };

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(optionsModel));
        services.AddScoped<DatabaseSeeder>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

        return (provider, dbContext, seeder);
    }

    [Fact]
    public async Task SeedRolesAsync_ShouldCreateAllRolesDefinedInRoleNames()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedRolesAsync();

        // Assert
        var roles = await dbContext.Roles.Select(r => r.Name).ToListAsync();
        foreach (var expectedRole in RoleNames.All)
        {
            Assert.Contains(expectedRole, roles);
        }
        Assert.Equal(RoleNames.All.Count, roles.Count);
    }

    [Fact]
    public async Task SeedRolesAsync_WhenRunMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedRolesAsync();
        await seeder.SeedRolesAsync(); // Second run

        // Assert
        var roles = await dbContext.Roles.ToListAsync();
        Assert.Equal(RoleNames.All.Count, roles.Count);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateAllDefaultDepartments()
    {
        // Arrange
        var (_, dbContext, seeder) = CreateSeederEnvironment();

        // Act
        await seeder.SeedAsync();

        // Assert
        var departments = await dbContext.Departments.ToListAsync();
        Assert.Equal(4, departments.Count);

        var codes = departments.Select(d => d.Code).ToList();
        Assert.Contains("MANAGEMENT", codes);
        Assert.Contains("SOFTWARE", codes);
        Assert.Contains("R&D", codes);
        Assert.Contains("MARKETING", codes);
    }

    [Fact]
    public async Task SeedAsync_WhenAdminEnabled_ShouldCreateAdminUserAndAssignRoles()
    {
        // Arrange
        var adminOptions = new AdminSeedOptions
        {
            Enabled = true,
            Email = "admin@gdsc.club",
            Password = "StrongPassword123!",
            FullName = "Club Admin",
            DepartmentCode = "MANAGEMENT"
        };
        var (provider, dbContext, seeder) = CreateSeederEnvironment(adminOptions);
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Act
        await seeder.SeedAsync();

        // Assert
        var user = await userManager.FindByEmailAsync("admin@gdsc.club");
        Assert.NotNull(user);
        Assert.Equal("Club Admin", user.FullName);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.True(user.EmailConfirmed);

        var mgmtDept = await dbContext.Departments.SingleAsync(d => d.Code == "MANAGEMENT");
        Assert.Equal(mgmtDept.Id, user.DepartmentId);

        var isInAdminRole = await userManager.IsInRoleAsync(user, RoleNames.Admin);
        var isInMemberRole = await userManager.IsInRoleAsync(user, RoleNames.Member);
        Assert.True(isInAdminRole);
        Assert.True(isInMemberRole);
    }

    [Fact]
    public async Task SeedAsync_WhenAdminDisabled_ShouldNotCreateAdminUser()
    {
        // Arrange
        var adminOptions = new AdminSeedOptions
        {
            Enabled = false,
            Email = "disabled@gdsc.club",
            Password = "Password123!",
            FullName = "Disabled Admin",
            DepartmentCode = "MANAGEMENT"
        };
        var (provider, dbContext, seeder) = CreateSeederEnvironment(adminOptions);
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Act
        await seeder.SeedAsync();

        // Assert
        var user = await userManager.FindByEmailAsync("disabled@gdsc.club");
        Assert.Null(user);

        // Departments & Roles should still be seeded
        var roles = await dbContext.Roles.ToListAsync();
        Assert.NotEmpty(roles);
        var departments = await dbContext.Departments.ToListAsync();
        Assert.NotEmpty(departments);
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_ShouldNotThrowAndKeepDataConsistent()
    {
        // Arrange
        var adminOptions = new AdminSeedOptions
        {
            Enabled = true,
            Email = "repeat@gdsc.club",
            Password = "Password123!",
            FullName = "Repeat Admin",
            DepartmentCode = "SOFTWARE"
        };
        var (_, dbContext, seeder) = CreateSeederEnvironment(adminOptions);

        // Act
        await seeder.SeedAsync();
        var exception = await Record.ExceptionAsync(() => seeder.SeedAsync());

        // Assert
        Assert.Null(exception);
        var users = await dbContext.Users.Where(u => u.Email == "repeat@gdsc.club").ToListAsync();
        Assert.Single(users);
    }
}
