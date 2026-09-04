using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Services;

public class MemberMembershipServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AssignMemberToGen_Valid_ShouldCreateMembership()
    {
        using var dbContext = CreateDbContext();
        var service = new MemberMembershipService(dbContext, NullLogger<MemberMembershipService>.Instance);

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "test@example.com", FullName = "Test User" };
        var gen = new ClubGeneration(3);
        dbContext.Users.Add(user);
        dbContext.ClubGenerations.Add(gen);
        await dbContext.SaveChangesAsync();

        var request = new AssignMemberToGenRequest(gen.Id, new DateOnly(2026, 9, 1));
        var result = await service.AssignMemberToGenAsync(user.Id, request);

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(gen.Id, result.GenerationId);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task AssignMemberToGen_AlreadyAssigned_ShouldThrowConflict()
    {
        using var dbContext = CreateDbContext();
        var service = new MemberMembershipService(dbContext, NullLogger<MemberMembershipService>.Instance);

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "test@example.com", FullName = "Test User" };
        var gen = new ClubGeneration(3);
        dbContext.Users.Add(user);
        dbContext.ClubGenerations.Add(gen);
        await dbContext.SaveChangesAsync();

        var request = new AssignMemberToGenRequest(gen.Id, new DateOnly(2026, 9, 1));
        await service.AssignMemberToGenAsync(user.Id, request);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.AssignMemberToGenAsync(user.Id, request));
    }

    [Fact]
    public async Task ReplaceRoles_ShouldEndRemovedRoles_KeepExisting_AndAddNewRoles()
    {
        using var dbContext = CreateDbContext();
        var service = new MemberMembershipService(dbContext, NullLogger<MemberMembershipService>.Instance);

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "test@example.com", FullName = "Test User" };
        var admin = new ApplicationUser { Id = Guid.NewGuid(), UserName = "admin@example.com", FullName = "Admin User" };
        var gen = new ClubGeneration(3);
        var dept = new Department { Id = Guid.NewGuid(), Code = "AI", Name = "AI", Slug = "ai" };

        var roleCore = new ClubRole("CORETEAM", "Core Team", 40);
        var roleSubLead = new ClubRole("SUBLEAD", "Sub Lead", 20);
        var roleLead = new ClubRole("LEAD", "Lead", 10);

        dbContext.Users.AddRange(user, admin);
        dbContext.ClubGenerations.Add(gen);
        dbContext.Departments.Add(dept);
        dbContext.ClubRoles.AddRange(roleCore, roleSubLead, roleLead);
        await dbContext.SaveChangesAsync();

        var clubMembership = new ClubMembership(user.Id, gen.Id);
        dbContext.ClubMemberships.Add(clubMembership);
        await dbContext.SaveChangesAsync();

        // Initially has CORETEAM and SUBLEAD
        var deptMembership = new DepartmentMembership(clubMembership.Id, dept.Id, isPrimary: true);
        dbContext.DepartmentMemberships.Add(deptMembership);

        var assignment1 = new RoleAssignment(deptMembership.Id, roleCore.Id, admin.Id);
        var assignment2 = new RoleAssignment(deptMembership.Id, roleSubLead.Id, admin.Id);
        dbContext.RoleAssignments.AddRange(assignment1, assignment2);
        await dbContext.SaveChangesAsync();

        // Now Admin replaces roles to: CORETEAM and LEAD (removing SUBLEAD, adding LEAD, keeping CORETEAM)
        var replaceRequest = new ReplaceRolesRequest(new List<Guid> { roleCore.Id, roleLead.Id });
        var updatedRoles = await service.ReplaceRolesAsync(user.Id, deptMembership.Id, replaceRequest, admin.Id);

        Assert.Equal(2, updatedRoles.Count);
        Assert.Contains(updatedRoles, r => r.Code == "CORETEAM");
        Assert.Contains(updatedRoles, r => r.Code == "LEAD");
        Assert.DoesNotContain(updatedRoles, r => r.Code == "SUBLEAD");

        // Verify in DB
        var dbAssignments = await dbContext.RoleAssignments
            .Where(ra => ra.DepartmentMembershipId == deptMembership.Id)
            .ToListAsync();

        Assert.Equal(3, dbAssignments.Count);

        var coreAssignment = dbAssignments.Single(ra => ra.ClubRoleId == roleCore.Id);
        Assert.True(coreAssignment.IsActive);
        Assert.Equal(assignment1.Id, coreAssignment.Id); // Untouched/kept

        var subleadAssignment = dbAssignments.Single(ra => ra.ClubRoleId == roleSubLead.Id);
        Assert.False(subleadAssignment.IsActive); // Ended
        Assert.NotNull(subleadAssignment.EndedAtUtc);

        var leadAssignment = dbAssignments.Single(ra => ra.ClubRoleId == roleLead.Id);
        Assert.True(leadAssignment.IsActive); // Added new
    }

    [Fact]
    public async Task EndDepartmentMembership_ShouldEndDeptAndAllItsRoles()
    {
        using var dbContext = CreateDbContext();
        var service = new MemberMembershipService(dbContext, NullLogger<MemberMembershipService>.Instance);

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "test@example.com", FullName = "Test User" };
        var gen = new ClubGeneration(3);
        var dept = new Department { Id = Guid.NewGuid(), Code = "AI", Name = "AI", Slug = "ai" };
        var role = new ClubRole("CORETEAM", "Core Team", 40);

        dbContext.Users.Add(user);
        dbContext.ClubGenerations.Add(gen);
        dbContext.Departments.Add(dept);
        dbContext.ClubRoles.Add(role);
        await dbContext.SaveChangesAsync();

        var clubMembership = new ClubMembership(user.Id, gen.Id);
        dbContext.ClubMemberships.Add(clubMembership);
        await dbContext.SaveChangesAsync();

        var deptMembership = new DepartmentMembership(clubMembership.Id, dept.Id);
        dbContext.DepartmentMemberships.Add(deptMembership);
        var assignment = new RoleAssignment(deptMembership.Id, role.Id, user.Id);
        dbContext.RoleAssignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        await service.EndDepartmentMembershipAsync(user.Id, deptMembership.Id);

        var updatedDept = await dbContext.DepartmentMemberships.SingleAsync(d => d.Id == deptMembership.Id);
        Assert.False(updatedDept.IsActive);
        Assert.NotNull(updatedDept.LeftAt);

        var updatedAssignment = await dbContext.RoleAssignments.SingleAsync(ra => ra.Id == assignment.Id);
        Assert.False(updatedAssignment.IsActive);
        Assert.NotNull(updatedAssignment.EndedAtUtc);
    }
}
