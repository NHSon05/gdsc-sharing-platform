using System.Net;
using System.Text.Json;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GdscSharingPlatform.IntegrationTests.HealthChecks;

public class HealthCheckEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthCheckEndpointsTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
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
                    options.UseInMemoryDatabase($"HealthCheckTestsDb_{Guid.NewGuid()}");
                });
            });
        });

        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task Get_HealthLive_ShouldReturnOk_AndJsonPayload()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("traceId", out var traceIdProp) && !string.IsNullOrEmpty(traceIdProp.GetString()));
        Assert.True(root.TryGetProperty("checks", out var checksProp));
        // Predicate = _ => false for liveness, so checks count is 0
        Assert.Equal(0, checksProp.GetArrayLength());
    }

    [Fact]
    public async Task Get_HealthReady_ShouldReturnOk_AndIncludeDatabaseCheck()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        var checks = root.GetProperty("checks");
        Assert.True(checks.GetArrayLength() > 0);

        var checkNames = checks.EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Contains("postgresql", checkNames);
    }

    [Fact]
    public async Task Get_Health_ShouldReturnOk_AndIncludeReadyChecks()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
    }
}
