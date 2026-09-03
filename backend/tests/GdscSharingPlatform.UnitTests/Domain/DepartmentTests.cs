using GdscSharingPlatform.Domain.Departments;

namespace GdscSharingPlatform.UnitTests.Domain;

public class DepartmentTests
{
    [Fact]
    public void Department_Initialization_ShouldHaveDefaultValues()
    {
        // Act
        var department = new Department();

        // Assert
        Assert.NotEqual(Guid.Empty, department.Id);
        Assert.Equal(string.Empty, department.Code);
        Assert.Equal(string.Empty, department.Name);
        Assert.Null(department.Description);
        Assert.Null(department.LeaderId);
        Assert.Equal(0, department.DisplayOrder);
        Assert.True(department.IsActive);
        Assert.False(department.IsDeleted);
        Assert.Null(department.DeletedAt);
        Assert.Null(department.UpdatedAt);
        Assert.True(department.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Department_SetProperties_ShouldAssignCorrectValues()
    {
        // Arrange
        var leaderId = Guid.NewGuid();

        // Act
        var department = new Department
        {
            Code = "SOFTWARE",
            Name = "Software Department",
            Description = "Software engineering team",
            LeaderId = leaderId,
            DisplayOrder = 1,
            IsActive = true
        };

        // Assert
        Assert.Equal("SOFTWARE", department.Code);
        Assert.Equal("Software Department", department.Name);
        Assert.Equal("Software engineering team", department.Description);
        Assert.Equal(leaderId, department.LeaderId);
        Assert.Equal(1, department.DisplayOrder);
    }

    [Fact]
    public void SystemDepartments_ShouldContainCoreDepartments()
    {
        Assert.NotEmpty(SystemDepartments.All);
        Assert.Contains(SystemDepartments.Software, SystemDepartments.All);
        Assert.Contains(SystemDepartments.AI, SystemDepartments.All);
        Assert.Contains(SystemDepartments.Marketing, SystemDepartments.All);
        Assert.Contains(SystemDepartments.Media, SystemDepartments.All);
        Assert.Contains(SystemDepartments.Community, SystemDepartments.All);
    }

    [Fact]
    public void Department_NewPropertiesAndAliases_ShouldWorkCorrectly()
    {
        var dept = new Department
        {
            Code = "SOFTWARE",
            Name = "Software",
            Slug = "software",
            Color = "#3B82F6",
            Icon = "code",
            SortOrder = 10
        };

        Assert.Equal("software", dept.Slug);
        Assert.Equal("#3B82F6", dept.Color);
        Assert.Equal("code", dept.Icon);
        Assert.Equal(10, dept.SortOrder);
        Assert.Equal(10, dept.DisplayOrder); // alias

        var now = DateTimeOffset.UtcNow;
        dept.CreatedAtUtc = now;
        Assert.Equal(now, dept.CreatedAt); // alias
    }
}
