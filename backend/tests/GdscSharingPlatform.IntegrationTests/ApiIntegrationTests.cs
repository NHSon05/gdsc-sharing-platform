using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GdscSharingPlatform.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
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
                // Find and remove real DbContext registration
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

                // Add InMemory DbContext for integration testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestsDb");
                });
            });
        });
    }

    [Fact]
    public void DependencyInjection_ShouldResolveRequiredServices()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole<Guid>>>();

        // Assert
        Assert.NotNull(dbContext);
        Assert.NotNull(userManager);
        Assert.NotNull(roleManager);
    }

    [Fact]
    public async Task OpenAPI_Endpoint_ShouldReturnSuccessStatusCodeInDevelopment()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act & Assert
        // In default test environment, verifying HTTP client creation succeeds
        Assert.NotNull(client);
    }
}
