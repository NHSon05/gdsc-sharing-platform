using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity;

namespace GdscSharingPlatform.UnitTests.Domain;

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_Initialization_ShouldHaveDefaultValues()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        Assert.Equal(string.Empty, user.FullName);
        Assert.Null(user.DisplayName);
        Assert.Null(user.AvatarUrl);
        Assert.Null(user.Bio);
        Assert.Null(user.DepartmentId);
        Assert.Null(user.Department);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal("Asia/Ho_Chi_Minh", user.TimeZone);
        Assert.Equal("vi-VN", user.Locale);
        Assert.False(user.IsDeleted);
        Assert.Null(user.DeletedAt);
        Assert.Null(user.UpdatedAt);
        Assert.True(user.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ApplicationUser_SetProperties_ShouldAssignCorrectValues()
    {
        // Arrange
        var deptId = Guid.NewGuid();

        // Act
        var user = new ApplicationUser
        {
            UserName = "admin@gdsc.com",
            Email = "admin@gdsc.com",
            FullName = "Administrator",
            DisplayName = "Admin User",
            DepartmentId = deptId,
            Status = UserStatus.Active
        };

        // Assert
        Assert.Equal("admin@gdsc.com", user.UserName);
        Assert.Equal("admin@gdsc.com", user.Email);
        Assert.Equal("Administrator", user.FullName);
        Assert.Equal("Admin User", user.DisplayName);
        Assert.Equal(deptId, user.DepartmentId);
        Assert.Equal(UserStatus.Active, user.Status);
    }
}
