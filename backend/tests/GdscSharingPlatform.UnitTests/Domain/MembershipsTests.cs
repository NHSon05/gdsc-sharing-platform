using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Memberships;

namespace GdscSharingPlatform.UnitTests.Domain;

public class MembershipsTests
{
    [Fact]
    public void ClubGeneration_Constructor_WithValidData_ShouldInitializeProperties()
    {
        // Arrange & Act
        var startDate = new DateOnly(2026, 9, 1);
        var endDate = new DateOnly(2027, 8, 31);
        var gen = new ClubGeneration(3, startDate, endDate);

        // Assert
        Assert.NotEqual(Guid.Empty, gen.Id);
        Assert.Equal(3, gen.Number);
        Assert.Equal("Gen 3", gen.Name);
        Assert.Equal(startDate, gen.StartDate);
        Assert.Equal(endDate, gen.EndDate);
        Assert.True(gen.IsActive);
        Assert.Empty(gen.Memberships);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ClubGeneration_Constructor_WithInvalidNumber_ShouldThrowArgumentOutOfRangeException(int number)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClubGeneration(number));
    }

    [Fact]
    public void ClubGeneration_Constructor_WhenEndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        var startDate = new DateOnly(2027, 1, 1);
        var endDate = new DateOnly(2026, 1, 1);

        Assert.Throws<ArgumentException>(() => new ClubGeneration(1, startDate, endDate));
    }

    [Fact]
    public void ClubGeneration_ActivateAndDeactivate_ShouldUpdateState()
    {
        var gen = new ClubGeneration(1);
        Assert.True(gen.IsActive);

        gen.Deactivate();
        Assert.False(gen.IsActive);

        gen.Activate();
        Assert.True(gen.IsActive);
    }

    [Fact]
    public void ClubRole_Constructor_WithValidData_ShouldNormalizeCode()
    {
        var role = new ClubRole("lead", "Lead", 10);

        Assert.NotEqual(Guid.Empty, role.Id);
        Assert.Equal("LEAD", role.Code);
        Assert.Equal("Lead", role.Name);
        Assert.Equal(10, role.Level);
        Assert.True(role.IsActive);
        Assert.Empty(role.RoleAssignments);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ClubRole_Constructor_WithInvalidCode_ShouldThrow(string? code)
    {
        Assert.ThrowsAny<ArgumentException>(() => new ClubRole(code!, "Name", 10));
    }

    [Fact]
    public void ClubMembership_Constructor_WithValidData_ShouldInitialize()
    {
        var userId = Guid.NewGuid();
        var genId = Guid.NewGuid();
        var joined = new DateOnly(2026, 9, 1);

        var membership = new ClubMembership(userId, genId, joined);

        Assert.NotEqual(Guid.Empty, membership.Id);
        Assert.Equal(userId, membership.UserId);
        Assert.Equal(genId, membership.GenerationId);
        Assert.Equal(joined, membership.JoinedAt);
        Assert.Null(membership.LeftAt);
        Assert.True(membership.IsActive);
    }

    [Fact]
    public void ClubMembership_End_ShouldSetInactiveAndLeftAt()
    {
        var membership = new ClubMembership(Guid.NewGuid(), Guid.NewGuid());
        var leftDate = new DateOnly(2027, 8, 31);

        membership.End(leftDate);

        Assert.False(membership.IsActive);
        Assert.Equal(leftDate, membership.LeftAt);

        membership.Reactivate();
        Assert.True(membership.IsActive);
        Assert.Null(membership.LeftAt);
    }

    [Fact]
    public void DepartmentMembership_SetPrimaryAndEnd_ShouldWorkCorrectly()
    {
        var clubMembershipId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var deptMembership = new DepartmentMembership(clubMembershipId, deptId, isPrimary: false);

        Assert.False(deptMembership.IsPrimary);
        deptMembership.SetPrimary(true);
        Assert.True(deptMembership.IsPrimary);

        deptMembership.End();
        Assert.False(deptMembership.IsActive);
        Assert.NotNull(deptMembership.LeftAt);
    }

    [Fact]
    public void RoleAssignment_EndAndReactivate_ShouldManageState()
    {
        var deptMembershipId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var assignment = new RoleAssignment(deptMembershipId, roleId);

        Assert.True(assignment.IsActive);
        Assert.Null(assignment.EndedAtUtc);

        var endedAt = DateTimeOffset.UtcNow;
        assignment.End(endedAt);

        Assert.False(assignment.IsActive);
        Assert.Equal(endedAt, assignment.EndedAtUtc);

        assignment.Reactivate();
        Assert.True(assignment.IsActive);
        Assert.Null(assignment.EndedAtUtc);
    }
}
