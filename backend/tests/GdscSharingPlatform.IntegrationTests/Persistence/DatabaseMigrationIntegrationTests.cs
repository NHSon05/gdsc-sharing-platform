using GdscSharingPlatform.Domain.Entities;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.IntegrationTests.Persistence;

public class DatabaseMigrationIntegrationTests
{
    [Fact]
    public void MigrationsAssembly_ShouldContainInitialIdentityMigration()
    {
        // Arrange & Act
        var infrastructureAssembly = typeof(ApplicationDbContext).Assembly;
        var migrationTypes = infrastructureAssembly.GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        // Assert
        Assert.NotEmpty(migrationTypes);
        Assert.Contains(migrationTypes, t => t.Name.Contains("InitialIdentity"));
    }

    [Fact]
    public void ModelSnapshot_ShouldDefineAllRequiredTablesAndRelationships()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ModelSnapshotTestDb")
            .Options;

        using var dbContext = new ApplicationDbContext(options);

        // Act
        var model = dbContext.Model;

        // Assert
        var userEntity = model.FindEntityType(typeof(ApplicationUser));
        var departmentEntity = model.FindEntityType(typeof(Department));

        Assert.NotNull(userEntity);
        Assert.NotNull(departmentEntity);

        // Verify Department primary key
        var deptPk = departmentEntity.FindPrimaryKey();
        Assert.NotNull(deptPk);
        Assert.Equal(nameof(Department.Id), deptPk.Properties.Single().Name);

        // Verify User -> Department Foreign Key relationship
        var foreignKeys = userEntity.GetForeignKeys();
        var deptFk = foreignKeys.FirstOrDefault(fk => fk.PrincipalEntityType == departmentEntity);
        Assert.NotNull(deptFk);
        Assert.Equal(nameof(ApplicationUser.DepartmentId), deptFk.Properties.Single().Name);
    }
}
