using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GdscSharingPlatform.IntegrationTests.Persistence;

public class DatabaseSeederIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DatabaseSeederIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AppStartup_WithAdminEnabled_ShouldSeedRolesDepartmentsAndAdminUser()
    {
        // Arrange
        var testDbName = $"SeederIntegrationDb_{Guid.NewGuid()}";
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["SeedAdmin:Enabled"] = "true",
                        ["SeedAdmin:Email"] = "bootstrap.admin@gdsc.test",
                        ["SeedAdmin:Password"] = "AdminPassword123!",
                        ["SeedAdmin:FullName"] = "Bootstrap Admin",
                        ["SeedAdmin:DepartmentCode"] = "MANAGEMENT"
                    });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(testDbName);
                });
            });
        });

        // Trigger startup by creating a client
        _ = customFactory.CreateClient();

        // Act & Assert
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // 1. Roles assertion
        foreach (var role in RoleNames.All)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);
            Assert.True(roleExists, $"Role '{role}' should be seeded.");
        }

        // 2. Department assertion
        var departments = await dbContext.Departments.ToListAsync();
        Assert.Equal(4, departments.Count);
        Assert.Contains(departments, d => d.Code == "MANAGEMENT");
        Assert.Contains(departments, d => d.Code == "SOFTWARE");
        Assert.Contains(departments, d => d.Code == "R&D");
        Assert.Contains(departments, d => d.Code == "MARKETING");

        // 3. Admin user assertion
        var admin = await userManager.FindByEmailAsync("bootstrap.admin@gdsc.test");
        Assert.NotNull(admin);
        Assert.Equal("Bootstrap Admin", admin.FullName);
        Assert.Equal(UserStatus.Active, admin.Status);
        Assert.True(admin.EmailConfirmed);

        var hasAdminRole = await userManager.IsInRoleAsync(admin, RoleNames.Admin);
        var hasMemberRole = await userManager.IsInRoleAsync(admin, RoleNames.Member);
        Assert.True(hasAdminRole);
        Assert.True(hasMemberRole);
    }

    [Fact]
    public async Task AppStartup_WithAdminDisabled_ShouldSeedRolesAndDepartmentsOnly()
    {
        // Arrange
        var testDbName = $"SeederIntegrationDb_NoAdmin_{Guid.NewGuid()}";
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["SeedAdmin:Enabled"] = "false"
                    });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(testDbName);
                });
            });
        });

        // Trigger startup
        _ = customFactory.CreateClient();

        // Act & Assert
        using var scope = customFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var roles = await dbContext.Roles.ToListAsync();
        Assert.NotEmpty(roles);

        var departments = await dbContext.Departments.ToListAsync();
        Assert.Equal(4, departments.Count);

        var users = await dbContext.Users.ToListAsync();
        Assert.Empty(users);
    }
}
