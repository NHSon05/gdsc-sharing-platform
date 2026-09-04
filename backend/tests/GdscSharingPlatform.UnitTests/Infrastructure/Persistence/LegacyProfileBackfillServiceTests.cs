using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Persistence.Backfill;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Persistence;

public class LegacyProfileBackfillServiceTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("2", 2)]
    [InlineData("Gen 1", 1)]
    [InlineData("gen 2", 2)]
    [InlineData("GEN 3", 3)]
    [InlineData("gen4", 4)]
    [InlineData("Gen 02", 2)]
    [InlineData("  Gen 5  ", 5)]
    public void TryParseGenerationNumber_WithValidInputs_ShouldReturnTrueAndCorrectNumber(string input, int expectedNumber)
    {
        var success = LegacyProfileBackfillService.TryParseGenerationNumber(input, out var number);

        Assert.True(success);
        Assert.Equal(expectedNumber, number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("Gen")]
    [InlineData("Gen -1")]
    [InlineData("0")]
    [InlineData("Gen 0")]
    [InlineData("Gen 1000")]
    public void TryParseGenerationNumber_WithInvalidInputs_ShouldReturnFalse(string? input)
    {
        var success = LegacyProfileBackfillService.TryParseGenerationNumber(input, out var number);

        Assert.False(success);
        Assert.Equal(0, number);
    }

    [Fact]
    public async Task BackfillAsync_WithValidUser_ShouldCreateMembershipsAndRoles()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "SOFTWARE",
            Name = "Software",
            Slug = "software"
        };
        dbContext.Departments.Add(department);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "member@gdsc.club",
            Email = "member@gdsc.club",
            FullName = "Club Member",
            Generation = "Gen 2",
            DepartmentId = department.Id,
            JoinedAt = new DateTimeOffset(2025, 9, 1, 0, 0, 0, TimeSpan.Zero)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var backfillService = new LegacyProfileBackfillService(dbContext, NullLogger<LegacyProfileBackfillService>.Instance);

        // Act
        var result = await backfillService.BackfillAsync();

        // Assert
        Assert.Equal(1, result.TotalScanned);
        Assert.Equal(1, result.MigratedUsersCount);
        Assert.Empty(result.SkippedUsers);

        var generation = await dbContext.ClubGenerations.SingleOrDefaultAsync(g => g.Number == 2);
        Assert.NotNull(generation);
        Assert.Equal("Gen 2", generation.Name);

        var membership = await dbContext.ClubMemberships.SingleOrDefaultAsync(m => m.UserId == user.Id && m.GenerationId == generation.Id);
        Assert.NotNull(membership);
        Assert.True(membership.IsActive);
        Assert.Equal(new DateOnly(2025, 9, 1), membership.JoinedAt);

        var deptMembership = await dbContext.DepartmentMemberships.SingleOrDefaultAsync(dm => dm.ClubMembershipId == membership.Id && dm.DepartmentId == department.Id);
        Assert.NotNull(deptMembership);
        Assert.True(deptMembership.IsPrimary);
        Assert.True(deptMembership.IsActive);

        var roleAssignment = await dbContext.RoleAssignments.SingleOrDefaultAsync(ra => ra.DepartmentMembershipId == deptMembership.Id);
        Assert.NotNull(roleAssignment);
        Assert.True(roleAssignment.IsActive);

        var defaultRole = await dbContext.ClubRoles.FindAsync(roleAssignment.ClubRoleId);
        Assert.NotNull(defaultRole);
        Assert.Equal(SystemClubRoles.CoreTeam, defaultRole.Code);
    }

    [Fact]
    public async Task BackfillAsync_WithInvalidGeneration_ShouldSkipAndRecordWarning()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "invalid@gdsc.club",
            Email = "invalid@gdsc.club",
            FullName = "Invalid Gen User",
            Generation = "NotAGen"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var backfillService = new LegacyProfileBackfillService(dbContext, NullLogger<LegacyProfileBackfillService>.Instance);

        // Act
        var result = await backfillService.BackfillAsync();

        // Assert
        Assert.Equal(1, result.TotalScanned);
        Assert.Equal(0, result.MigratedUsersCount);
        Assert.Single(result.SkippedUsers);
        Assert.Equal(user.Id, result.SkippedUsers[0].UserId);
        Assert.Contains("invalid", result.SkippedUsers[0].Reason);

        Assert.Empty(await dbContext.ClubGenerations.ToListAsync());
        Assert.Empty(await dbContext.ClubMemberships.ToListAsync());
    }

    [Fact]
    public async Task BackfillAsync_WhenRunMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "AI",
            Name = "AI",
            Slug = "ai"
        };
        dbContext.Departments.Add(department);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "idempotent@gdsc.club",
            Email = "idempotent@gdsc.club",
            FullName = "Idempotent User",
            Generation = "1",
            DepartmentId = department.Id
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var backfillService = new LegacyProfileBackfillService(dbContext, NullLogger<LegacyProfileBackfillService>.Instance);

        // Act
        await backfillService.BackfillAsync();
        var secondRunResult = await backfillService.BackfillAsync();

        // Assert
        Assert.Equal(1, secondRunResult.TotalScanned);
        Assert.Equal(1, secondRunResult.MigratedUsersCount);

        Assert.Single(await dbContext.ClubGenerations.ToListAsync());
        Assert.Single(await dbContext.ClubMemberships.ToListAsync());
        Assert.Single(await dbContext.DepartmentMemberships.ToListAsync());
        Assert.Single(await dbContext.RoleAssignments.ToListAsync());
    }
}
