using GdscSharingPlatform.Domain.Departments;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Persistence;

public class DbContextModelTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void ModelBuilder_ShouldConfigureClubGenerationsCorrectly()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(ClubGeneration));

        Assert.NotNull(entityType);
        Assert.Equal("ClubGenerations", entityType.GetTableName());

        // PK
        var pk = entityType.FindPrimaryKey();
        Assert.NotNull(pk);
        Assert.Equal(nameof(ClubGeneration.Id), pk.Properties.Single().Name);

        // Unique index on Number
        var numberIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(ClubGeneration.Number)));
        Assert.NotNull(numberIndex);
        Assert.True(numberIndex.IsUnique);
    }

    [Fact]
    public void ModelBuilder_ShouldConfigureClubRolesCorrectly()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(ClubRole));

        Assert.NotNull(entityType);
        Assert.Equal("ClubRoles", entityType.GetTableName());

        var codeIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(ClubRole.Code)));
        Assert.NotNull(codeIndex);
        Assert.True(codeIndex.IsUnique);
    }

    [Fact]
    public void ModelBuilder_ShouldConfigureClubMembershipsCorrectly()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(ClubMembership));

        Assert.NotNull(entityType);
        Assert.Equal("ClubMemberships", entityType.GetTableName());

        // Composite unique index on (UserId, GenerationId)
        var userGenIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(ClubMembership.UserId), nameof(ClubMembership.GenerationId) }));
        Assert.NotNull(userGenIndex);
        Assert.True(userGenIndex.IsUnique);

        // Delete behavior Restrict
        var fks = entityType.GetForeignKeys().ToList();
        Assert.All(fks, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void ModelBuilder_ShouldConfigureDepartmentMembershipsCorrectly()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(DepartmentMembership));

        Assert.NotNull(entityType);
        Assert.Equal("DepartmentMemberships", entityType.GetTableName());

        // Composite unique index on (ClubMembershipId, DepartmentId)
        var compositeIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(DepartmentMembership.ClubMembershipId), nameof(DepartmentMembership.DepartmentId) }));
        Assert.NotNull(compositeIndex);
        Assert.True(compositeIndex.IsUnique);

        // Delete behavior Restrict
        var fks = entityType.GetForeignKeys().ToList();
        Assert.All(fks, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void ModelBuilder_ShouldConfigureRoleAssignmentsCorrectly()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(RoleAssignment));

        Assert.NotNull(entityType);
        Assert.Equal("RoleAssignments", entityType.GetTableName());

        // Filtered unique index on (DepartmentMembershipId, ClubRoleId)
        var activeRoleIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(RoleAssignment.DepartmentMembershipId), nameof(RoleAssignment.ClubRoleId) }));
        Assert.NotNull(activeRoleIndex);
        Assert.True(activeRoleIndex.IsUnique);
        Assert.Contains("IsActive", activeRoleIndex.GetFilter() ?? "");

        // Delete behavior Restrict
        var fks = entityType.GetForeignKeys().ToList();
        Assert.All(fks, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void ModelBuilder_ShouldConfigureDepartmentsSlugUniqueIndex()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(Department));

        Assert.NotNull(entityType);

        var slugIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(Department.Slug)));
        Assert.NotNull(slugIndex);
        Assert.True(slugIndex.IsUnique);
    }

    [Fact]
    public void ModelBuilder_ShouldConfigureApplicationUserStudentCodeAndBio()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(ApplicationUser));

        Assert.NotNull(entityType);

        var studentCodeProp = entityType.FindProperty(nameof(ApplicationUser.StudentCode));
        Assert.NotNull(studentCodeProp);
        Assert.Equal(30, studentCodeProp.GetMaxLength());

        var bioProp = entityType.FindProperty(nameof(ApplicationUser.Bio));
        Assert.NotNull(bioProp);
        Assert.Equal(500, bioProp.GetMaxLength());

        var githubUrlProp = entityType.FindProperty(nameof(ApplicationUser.GitHubUrl));
        Assert.NotNull(githubUrlProp);
        Assert.Equal(200, githubUrlProp.GetMaxLength());

        var studentCodeIndex = entityType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Any(p => p.Name == nameof(ApplicationUser.StudentCode)));
        Assert.NotNull(studentCodeIndex);
        Assert.True(studentCodeIndex.IsUnique);
        Assert.Contains("StudentCode", studentCodeIndex.GetFilter() ?? "");
    }
}
