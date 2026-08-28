using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Domain.Entities;
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

namespace GdscSharingPlatform.IntegrationTests.Auth;

public class AuthEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "AuthTestsDb_" + Guid.NewGuid();
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

    private async Task SeedUserAsync(string email, string password, string role, string departmentCode, string fullName)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        var department = await dbContext.Departments.FirstOrDefaultAsync(d => d.Code == departmentCode);
        if (department is null)
        {
            department = new Department
            {
                Id = Guid.NewGuid(),
                Code = departmentCode,
                Name = departmentCode,
                IsActive = true
            };
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FullName = fullName,
                DepartmentId = department.Id,
                Status = UserStatus.Active,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200AndTokens()
    {
        // Arrange
        var client = _factory.CreateClient();
        await SeedUserAsync("admin@test.app", "Password123!", RoleNames.Admin, "MANAGEMENT", "Admin Test");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@test.app", "Password123!"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.AccessToken);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal("Bearer", authResponse.TokenType);
        Assert.Equal("admin@test.app", authResponse.User.Email);
        Assert.Contains(RoleNames.Admin, authResponse.User.Roles);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();
        await SeedUserAsync("member@test.app", "Password123!", RoleNames.Member, "SOFTWARE", "Member Test");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("member@test.app", "WrongPassword!"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldRotateAndReturnNewTokens()
    {
        // Arrange
        var client = _factory.CreateClient();
        await SeedUserAsync("refresh@test.app", "Password123!", RoleNames.Member, "SOFTWARE", "Refresh User");

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("refresh@test.app", "Password123!"));
        var loginData = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginData);

        // Act
        var refreshRes = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginData.RefreshToken));

        // Assert
        Assert.Equal(HttpStatusCode.OK, refreshRes.StatusCode);
        var tokenData = await refreshRes.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokenData);
        Assert.NotEmpty(tokenData.AccessToken);
        Assert.NotEmpty(tokenData.RefreshToken);
        Assert.NotEqual(loginData.RefreshToken, tokenData.RefreshToken);

        // Act 2: Reusing the old refresh token must trigger Token Reuse Detection
        var reuseRes = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginData.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reuseRes.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidBearerToken_ShouldReturn200AndUserProfile()
    {
        // Arrange
        var client = _factory.CreateClient();
        await SeedUserAsync("me@test.app", "Password123!", RoleNames.Member, "R&D", "Me User");

        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("me@test.app", "Password123!"));
        var loginData = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginData);

        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginData.AccessToken);
        var meRes = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, meRes.StatusCode);
        var userDto = await meRes.Content.ReadFromJsonAsync<CurrentUserDto>();
        Assert.NotNull(userDto);
        Assert.Equal("me@test.app", userDto.Email);
        Assert.Equal("R&D", userDto.Department?.Name);
    }

    [Fact]
    public async Task Authorization_AdminEndpoint_ShouldEnforceRoleGuards()
    {
        // Arrange
        var client = _factory.CreateClient();
        await SeedUserAsync("member_role@test.app", "Password123!", RoleNames.Member, "SOFTWARE", "Member Only");
        await SeedUserAsync("admin_role@test.app", "Password123!", RoleNames.Admin, "MANAGEMENT", "Admin Only");

        // Act 1: Member tries to access Admin endpoint -> 403 Forbidden
        var memberLogin = await (await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("member_role@test.app", "Password123!")))
            .Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberLogin!.AccessToken);
        var forbiddenRes = await client.GetAsync("/api/authorization-test/admin");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenRes.StatusCode);

        // Act 2: Admin accesses Admin endpoint -> 200 OK
        var adminLogin = await (await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin_role@test.app", "Password123!")))
            .Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin!.AccessToken);
        var okRes = await client.GetAsync("/api/authorization-test/admin");
        Assert.Equal(HttpStatusCode.OK, okRes.StatusCode);

        // Act 3: Anonymous access -> 401 Unauthorized
        client.DefaultRequestHeaders.Authorization = null;
        var unauthorizedRes = await client.GetAsync("/api/authorization-test/admin");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedRes.StatusCode);
    }

    [Fact]
    public async Task LogoutAll_ShouldRevokeAllSessions()
    {
        // Arrange
        var client = _factory.CreateClient();
        await SeedUserAsync("logoutall@test.app", "Password123!", RoleNames.Member, "SOFTWARE", "Logout All User");

        var login1 = await (await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("logoutall@test.app", "Password123!")))
            .Content.ReadFromJsonAsync<AuthResponse>();
        var login2 = await (await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("logoutall@test.app", "Password123!")))
            .Content.ReadFromJsonAsync<AuthResponse>();

        // Act: Logout all
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login1!.AccessToken);
        var logoutAllRes = await client.PostAsync("/api/auth/logout-all", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutAllRes.StatusCode);

        // Assert: Both refresh tokens are now revoked
        var refresh1 = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(login1.RefreshToken));
        var refresh2 = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(login2!.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, refresh1.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh2.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldSetHttpOnlyCookies_AndAllowAccessToProtectedEndpointsWithoutBearerHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        await SeedUserAsync("cookie_user@test.app", "Password123!", RoleNames.Member, "SOFTWARE", "Cookie User");

        // Act 1: Login
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("cookie_user@test.app", "Password123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Check Set-Cookie headers in response
        Assert.True(loginResponse.Headers.Contains("Set-Cookie"));
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        Assert.Contains(cookies, c => c.Contains("accessToken") && c.Contains("httponly"));
        Assert.Contains(cookies, c => c.Contains("refreshToken") && c.Contains("httponly"));

        // Act 2: Call protected /me with Cookie header
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));
        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Add("Cookie", cookieHeader);
        var meResponse = await client.SendAsync(meRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var currentUser = await meResponse.Content.ReadFromJsonAsync<CurrentUserDto>();
        Assert.NotNull(currentUser);
        Assert.Equal("cookie_user@test.app", currentUser.Email);

        // Act 3: Refresh without body (reading refreshToken from cookie)
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshRequest.Headers.Add("Cookie", cookieHeader);
        var refreshResponse = await client.SendAsync(refreshRequest);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        // Act 4: Logout without body (reading refreshToken from cookie and clearing cookies)
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutRequest.Headers.Add("Cookie", cookieHeader);
        var logoutResponse = await client.SendAsync(logoutRequest);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
    }
}
