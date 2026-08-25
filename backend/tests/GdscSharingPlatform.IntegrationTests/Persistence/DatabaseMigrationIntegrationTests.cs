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
    public void MigrationsAssembly_ShouldContainExpectedMigrations()
    {
        // Arrange & Act
        var infrastructureAssembly = typeof(ApplicationDbContext).Assembly;
        var migrationTypes = infrastructureAssembly.GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        // Assert
        Assert.NotEmpty(migrationTypes);
        Assert.Contains(migrationTypes, t => t.Name.Contains("InitialIdentity"));
        Assert.Contains(migrationTypes, t => t.Name.Contains("AddRefreshTokenTable"));
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
        var refreshTokenEntity = model.FindEntityType(typeof(RefreshToken));

        Assert.NotNull(userEntity);
        Assert.NotNull(departmentEntity);
        Assert.NotNull(refreshTokenEntity);

        // Verify Department primary key
        var deptPk = departmentEntity.FindPrimaryKey();
        Assert.NotNull(deptPk);
        Assert.Equal(nameof(Department.Id), deptPk.Properties.Single().Name);

        // Verify User -> Department Foreign Key relationship
        var foreignKeys = userEntity.GetForeignKeys();
        var deptFk = foreignKeys.FirstOrDefault(fk => fk.PrincipalEntityType == departmentEntity);
        Assert.NotNull(deptFk);
        Assert.Equal(nameof(ApplicationUser.DepartmentId), deptFk.Properties.Single().Name);

        // Verify RefreshToken table and primary key
        Assert.Equal("RefreshTokens", refreshTokenEntity.GetTableName());

        var refreshTokenPk = refreshTokenEntity.FindPrimaryKey();
        Assert.NotNull(refreshTokenPk);
        Assert.Equal(nameof(RefreshToken.Id), refreshTokenPk.Properties.Single().Name);

        // Verify RefreshToken indexes
        var tokenHashIndex = refreshTokenEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual([nameof(RefreshToken.TokenHash)]));

        Assert.NotNull(tokenHashIndex);
        Assert.True(tokenHashIndex.IsUnique);

        var activeTokensIndex = refreshTokenEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                [
                    nameof(RefreshToken.UserId),
                    nameof(RefreshToken.IsRevoked),
                    nameof(RefreshToken.ExpiresAt)
                ]));

        Assert.NotNull(activeTokensIndex);

        // Verify RefreshToken -> ApplicationUser Foreign Key relationship
        var refreshTokenFk = refreshTokenEntity.GetForeignKeys()
            .SingleOrDefault(fk =>
                fk.PrincipalEntityType == userEntity &&
                fk.Properties
                    .Select(property => property.Name)
                    .SequenceEqual([nameof(RefreshToken.UserId)]));

        Assert.NotNull(refreshTokenFk);
        Assert.Equal(DeleteBehavior.Cascade, refreshTokenFk.DeleteBehavior);
    }
}
