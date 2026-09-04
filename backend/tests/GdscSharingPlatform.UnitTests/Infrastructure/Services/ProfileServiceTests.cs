using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Features.Profile.Models;
using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Services;

public class ProfileServiceTests
{
    private static (ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) CreateContextAndUserManager()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);

        var userStore = new UserStore<ApplicationUser, IdentityRole<Guid>, ApplicationDbContext, Guid>(dbContext);
#pragma warning disable CS8625
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null,
            new PasswordHasher<ApplicationUser>(),
            null,
            null,
            null,
            null,
            null,
            null);
#pragma warning restore CS8625

        return (dbContext, userManager);
    }

    [Fact]
    public async Task GetMyProfile_CompleteProfile_Calculates100Percent()
    {
        var (dbContext, userManager) = CreateContextAndUserManager();
        var service = new ProfileService(
            userManager,
            dbContext,
            new FakeFileStorageService(),
            NullLogger<ProfileService>.Instance);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "an@example.com",
            Email = "an@example.com",
            FullName = "Nguyen Van An",
            DisplayName = "Nguyen Van An",
            PhoneNumber = "+84901234567",
            StudentCode = "21IT001",
            GitHubUrl = "https://github.com/an",
            Bio = "Backend Dev",
            AvatarUrl = "https://cdn.example.com/avatar.webp"
        };
        await userManager.CreateAsync(user);

        var gen = new ClubGeneration(3);
        var dept = new Department { Id = Guid.NewGuid(), Code = "SW", Name = "Software", Slug = "software" };
        var role = new ClubRole("CORETEAM", "Core Team", 40);
        dbContext.ClubGenerations.Add(gen);
        dbContext.Departments.Add(dept);
        dbContext.ClubRoles.Add(role);
        await dbContext.SaveChangesAsync();

        var clubMembership = new ClubMembership(user.Id, gen.Id);
        dbContext.ClubMemberships.Add(clubMembership);
        await dbContext.SaveChangesAsync();

        var deptMembership = new DepartmentMembership(clubMembership.Id, dept.Id, isPrimary: true);
        dbContext.DepartmentMemberships.Add(deptMembership);

        var assignment = new RoleAssignment(deptMembership.Id, role.Id, user.Id);
        dbContext.RoleAssignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        var profile = await service.GetMyProfileAsync(user.Id);

        Assert.Equal(100, profile.ProfileCompletionPercentage);
        Assert.Empty(profile.MissingProfileFields);
        Assert.Equal("Nguyen Van An", profile.DisplayName);
        Assert.Single(profile.Memberships);
    }

    [Fact]
    public async Task UpdateMyProfile_DuplicateStudentCode_ThrowsConflictException()
    {
        var (dbContext, userManager) = CreateContextAndUserManager();
        var service = new ProfileService(
            userManager,
            dbContext,
            new FakeFileStorageService(),
            NullLogger<ProfileService>.Instance);

        var user1 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u1@example.com", FullName = "User 1", StudentCode = "21IT001" };
        var user2 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u2@example.com", FullName = "User 2" };
        await userManager.CreateAsync(user1);
        await userManager.CreateAsync(user2);

        var updateRequest = new UpdateProfileRequest("User 2 Updated", null, null, "21IT001", null, null);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateMyProfileAsync(user2.Id, updateRequest));
    }

    [Fact]
    public async Task ChangeEmail_DirectlyUpdatesEmailWithoutConfirmation()
    {
        var (dbContext, userManager) = CreateContextAndUserManager();
        var service = new ProfileService(
            userManager,
            dbContext,
            new FakeFileStorageService(),
            NullLogger<ProfileService>.Instance);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "old@example.com",
            Email = "old@example.com",
            FullName = "Member Old",
            DisplayName = "Member Old"
        };
        await userManager.CreateAsync(user);

        var request = new ChangeEmailRequest("newdirect@example.com");
        var updatedProfile = await service.ChangeEmailAsync(user.Id, request);

        Assert.Equal("newdirect@example.com", updatedProfile.Email);

        var userInDb = await userManager.FindByIdAsync(user.Id.ToString());
        Assert.Equal("newdirect@example.com", userInDb!.Email);
        Assert.Equal("newdirect@example.com", userInDb.UserName);
        Assert.True(userInDb.EmailConfirmed);
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<string> UploadAvatarAsync(Guid userId, Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
            => Task.FromResult($"/uploads/avatars/{userId}/avatar.webp");

        public Task DeleteAvatarAsync(string avatarUrl, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
