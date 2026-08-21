using GdscSharingPlatform.Domain.Entities;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.UnitTests.Infrastructure;

public class ApplicationDbContextTests
{
    private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddDepartment_ShouldPersistToDatabase()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var department = new Department
        {
            Code = "SOFTWARE",
            Name = "Software Engineering",
            Description = "Software department"
        };

        // Act
        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await dbContext.Departments.FirstOrDefaultAsync(d => d.Code == "SOFTWARE");
        Assert.NotNull(retrieved);
        Assert.Equal("Software Engineering", retrieved.Name);
    }

    [Fact]
    public async Task SoftDeletedDepartment_ShouldBeFilteredOutByQueryFilter()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var dbContext = CreateInMemoryDbContext(dbName))
        {
            var activeDepartment = new Department
            {
                Code = "MGMT",
                Name = "Management",
                IsDeleted = false
            };

            var deletedDepartment = new Department
            {
                Code = "OLD_DEPT",
                Name = "Old Department",
                IsDeleted = true
            };

            dbContext.Departments.AddRange(activeDepartment, deletedDepartment);
            await dbContext.SaveChangesAsync();
        }

        // Act & Assert
        using (var dbContext = CreateInMemoryDbContext(dbName))
        {
            var departments = await dbContext.Departments.ToListAsync();
            Assert.Single(departments);
            Assert.Equal("MGMT", departments[0].Code);

            var deleted = await dbContext.Departments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Code == "OLD_DEPT");
            Assert.NotNull(deleted);
            Assert.True(deleted.IsDeleted);
        }
    }

    [Fact]
    public async Task UserWithDepartment_ShouldLinkCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var departmentId = Guid.NewGuid();

        using (var dbContext = CreateInMemoryDbContext(dbName))
        {
            var dept = new Department
            {
                Id = departmentId,
                Code = "DESIGN",
                Name = "Design Department"
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "designer@gdsc.com",
                Email = "designer@gdsc.com",
                FullName = "Design Lead",
                DepartmentId = departmentId
            };

            dbContext.Departments.Add(dept);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        // Act
        using (var dbContext = CreateInMemoryDbContext(dbName))
        {
            var user = await dbContext.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email == "designer@gdsc.com");

            // Assert
            Assert.NotNull(user);
            Assert.NotNull(user.Department);
            Assert.Equal("DESIGN", user.Department.Code);
        }
    }
}
