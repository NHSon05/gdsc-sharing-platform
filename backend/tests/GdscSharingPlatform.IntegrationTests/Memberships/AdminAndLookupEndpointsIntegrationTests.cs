using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GdscSharingPlatform.IntegrationTests.Memberships;

public class AdminAndLookupEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminAndLookupEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "AdminTestsDb_" + Guid.NewGuid();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
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
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });
    }

    private async Task<string> SeedAndLoginAsync(string email, string password, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        var department = await dbContext.Departments.FirstOrDefaultAsync();
        if (department == null)
        {
            department = new Department
            {
                Id = Guid.NewGuid(),
                Code = "TECH",
                Name = "Technology",
                Slug = "technology",
                SortOrder = 10,
                IsActive = true
            };
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FullName = "Test User",
                DisplayName = "Test User",
                DepartmentId = department.Id,
                Status = UserStatus.Active,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }

        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return authResult!.AccessToken;
    }

    [Fact]
    public async Task LookupEndpoints_AsMember_ShouldReturnActiveItems()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginAsync("member_lookup@test.app", "Password123!", RoleNames.Member);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var deptResponse = await client.GetAsync("/api/departments");
        Assert.Equal(HttpStatusCode.OK, deptResponse.StatusCode);
        var depts = await deptResponse.Content.ReadFromJsonAsync<IReadOnlyList<DepartmentDetailDto>>();
        Assert.NotNull(depts);

        var genResponse = await client.GetAsync("/api/generations");
        Assert.Equal(HttpStatusCode.OK, genResponse.StatusCode);

        var rolesResponse = await client.GetAsync("/api/club-roles");
        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);
    }

    [Fact]
    public async Task AdminDepartmentEndpoints_CrudOperations_ShouldSucceed()
    {
        var client = _factory.CreateClient();
        var adminToken = await SeedAndLoginAsync("admin_dept@test.app", "Password123!", RoleNames.Admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // 1. Create
        var createRequest = new CreateDepartmentRequest("Research and Development", "research-dev", "R&D team", "#2563EB", "flask", 50);
        var createResponse = await client.PostAsJsonAsync("/api/admin/departments", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<DepartmentDetailDto>();
        Assert.NotNull(created);
        Assert.Equal("Research and Development", created.Name);

        // 2. Update
        var updateRequest = new UpdateDepartmentRequest("Research & Development", "research-dev", "Updated desc", "#1D4ED8", "flask", 60);
        var updateResponse = await client.PutAsJsonAsync($"/api/admin/departments/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<DepartmentDetailDto>();
        Assert.Equal("Research & Development", updated!.Name);

        // 3. Deactivate
        var deleteResponse = await client.DeleteAsync($"/api/admin/departments/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 4. Activate
        var activateResponse = await client.PostAsync($"/api/admin/departments/{created.Id}/activate", null);
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        var activated = await activateResponse.Content.ReadFromJsonAsync<DepartmentDetailDto>();
        Assert.True(activated!.IsActive);
    }

    [Fact]
    public async Task AdminDepartmentEndpoints_AsMember_ShouldReturn403Forbidden()
    {
        var client = _factory.CreateClient();
        var memberToken = await SeedAndLoginAsync("regular_member@test.app", "Password123!", RoleNames.Member);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);

        var createRequest = new CreateDepartmentRequest("Forbidden Dept", "forbidden-dept", null, null, null, 1);
        var response = await client.PostAsJsonAsync("/api/admin/departments", createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminGenerationEndpoints_CrudOperations_ShouldSucceed()
    {
        var client = _factory.CreateClient();
        var adminToken = await SeedAndLoginAsync("admin_gen@test.app", "Password123!", RoleNames.Admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // 1. Create
        var createRequest = new CreateGenerationRequest(10, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31));
        var createResponse = await client.PostAsJsonAsync("/api/admin/generations", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<GenerationDto>();
        Assert.NotNull(created);
        Assert.Equal(10, created.Number);

        // 2. Deactivate
        var deleteResponse = await client.DeleteAsync($"/api/admin/generations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
