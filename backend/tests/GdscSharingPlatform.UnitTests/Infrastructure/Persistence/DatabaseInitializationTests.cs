using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Identity.Seeding;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Persistence;

public class DatabaseInitializationTests
{
    [Fact]
    public async Task InitializeDatabaseAsync_OnInMemoryDatabase_ShouldEnsureCreatedAndSeed()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
        });

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddSingleton(Options.Create(new AdminSeedOptions
        {
            Enabled = false
        }));

        services.AddScoped<DatabaseSeeder>();

        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeDatabaseAsync();

        // Assert
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var roles = await dbContext.Roles.ToListAsync();
        Assert.Equal(RoleNames.All.Count, roles.Count);

        var departments = await dbContext.Departments.ToListAsync();
        Assert.Equal(4, departments.Count);
    }

    [Fact]
    public void ApplicationDbContext_ModelCreation_ShouldHaveExpectedEntityMappings()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new ApplicationDbContext(options);

        // Act
        var model = dbContext.Model;

        // Assert
        var userEntity = model.FindEntityType(typeof(ApplicationUser));
        var deptEntity = model.FindEntityType(typeof(GdscSharingPlatform.Domain.Entities.Department));

        Assert.NotNull(userEntity);
        Assert.NotNull(deptEntity);

        // Check Department navigation on User
        var deptNavigation = userEntity.FindNavigation(nameof(ApplicationUser.Department));
        Assert.NotNull(deptNavigation);
    }
}
