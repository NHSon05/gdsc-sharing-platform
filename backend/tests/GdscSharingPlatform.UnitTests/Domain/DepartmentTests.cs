using GdscSharingPlatform.Domain.Entities;

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
}
