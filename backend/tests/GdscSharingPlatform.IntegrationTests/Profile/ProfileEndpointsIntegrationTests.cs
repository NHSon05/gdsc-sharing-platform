using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Application.Features.Profile.Models;
using GdscSharingPlatform.Domain.Departments;
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

namespace GdscSharingPlatform.IntegrationTests.Profile;

public class ProfileEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProfileEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "ProfileTestsDb_" + Guid.NewGuid();
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

    private async Task<string> SeedAndLoginUserAsync(string email, string password, string role = RoleNames.Member)
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
                Code = "TEST",
                Name = "Test Dept",
                Slug = "test-dept"
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
                FullName = "Profile Test User",
                DisplayName = "Profile Test User",
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
    public async Task GetMyProfile_WithoutToken_ShouldReturn401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/profile/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_WithValidToken_ShouldReturn200AndProfile()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("member1@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("member1@profile.app", profile.Email);
        Assert.Equal("MEMBER", profile.SystemRoles);
    }

    [Fact]
    public async Task UpdateMyProfile_ValidRequest_ShouldReturn200AndUpdatedProfile()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("member2@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateProfileRequest(
            "Nguyen Van An",
            null,
            "+84901234567",
            "21IT001",
            "https://github.com/nguyenvanan",
            "Backend developer interested in distributed systems.");

        var response = await client.PatchAsJsonAsync("/api/profile/me", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("Nguyen Van An", profile.DisplayName);
        Assert.Equal("+84901234567", profile.PhoneNumber);
        Assert.Equal("21IT001", profile.StudentCode);
        Assert.Equal("https://github.com/nguyenvanan", profile.GithubUrl);
    }

    [Fact]
    public async Task UpdateMyProfile_DuplicateStudentCode_ShouldReturn409()
    {
        var client1 = _factory.CreateClient();
        var token1 = await SeedAndLoginUserAsync("user1@profile.app", "Password123!");
        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        await client1.PatchAsJsonAsync("/api/profile/me", new UpdateProfileRequest("User 1", null, null, "DUPLICATE_CODE", null, null));

        var client2 = _factory.CreateClient();
        var token2 = await SeedAndLoginUserAsync("user2@profile.app", "Password123!");
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var response = await client2.PatchAsJsonAsync("/api/profile/me", new UpdateProfileRequest("User 2", null, null, "DUPLICATE_CODE", null, null));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AvatarFlow_UploadAndDelete_ShouldWorkCorrectly()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("avatar@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Upload valid JPEG avatar
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(jpegBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "avatar", "avatar.jpg");

        var uploadResponse = await client.PostAsync("/api/profile/me/avatar", content);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var avatarResult = await uploadResponse.Content.ReadFromJsonAsync<AvatarUploadResponse>();
        Assert.NotNull(avatarResult);
        Assert.NotEmpty(avatarResult.AvatarUrl);

        // Verify profile has AvatarUrl
        var getProfileResponse = await client.GetAsync("/api/profile/me");
        var profile = await getProfileResponse.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal(avatarResult.AvatarUrl, profile!.AvatarUrl);

        // 2. Delete avatar
        var deleteResponse = await client.DeleteAsync("/api/profile/me/avatar");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify profile avatar is null
        var afterDeleteResponse = await client.GetAsync("/api/profile/me");
        var profileAfter = await afterDeleteResponse.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Null(profileAfter!.AvatarUrl);
    }

    [Fact]
    public async Task ChangeEmail_DirectlyUpdatesEmail_ShouldReturn200()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("emaildirect@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PatchAsJsonAsync("/api/profile/me/email", new ChangeEmailRequest("newdirect@profile.app"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedProfile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(updatedProfile);
        Assert.Equal("newdirect@profile.app", updatedProfile.Email);

        // Verify with subsequent GET /api/profile/me
        var getResponse = await client.GetAsync("/api/profile/me");
        var profile = await getResponse.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal("newdirect@profile.app", profile!.Email);
    }

    [Fact]
    public async Task ChangeEmail_DuplicateEmail_ShouldReturn409()
    {
        await SeedAndLoginUserAsync("existing_other@profile.app", "Password123!");

        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("emaildup@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PatchAsJsonAsync("/api/profile/me/email", new ChangeEmailRequest("existing_other@profile.app"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_WithDirectEmail_ShouldReturn200AndUpdatedEmail()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("inline_email@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateProfileRequest(
            "Inline User",
            "inline_new@profile.app",
            null,
            null,
            null,
            null);

        var response = await client.PatchAsJsonAsync("/api/profile/me", updateRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("inline_new@profile.app", profile.Email);
    }

    [Fact]
    public async Task PatchMyProfile_PartialUpdate_ShouldOnlyUpdateProvidedFields()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("patch_member@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var patchRequest = new UpdateProfileRequest(Bio: "Only Bio Updated");
        var response = await client.PatchAsJsonAsync("/api/profile/me", patchRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal("Only Bio Updated", profile!.Bio);
        Assert.Equal("Profile Test User", profile.DisplayName);
    }

    [Fact]
    public async Task PatchMyProfile_Email_ShouldReturn200()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("patch_email@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PatchAsJsonAsync("/api/profile/me/email", new ChangeEmailRequest("patched_email@profile.app"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal("patched_email@profile.app", profile!.Email);
    }

    [Fact]
    public async Task PutMyProfile_ShouldReturn405MethodNotAllowed()
    {
        var client = _factory.CreateClient();
        var token = await SeedAndLoginUserAsync("put_check@profile.app", "Password123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync("/api/profile/me", new UpdateProfileRequest("Put Test"));
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
