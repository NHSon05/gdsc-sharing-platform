using GdscSharingPlatform.Infrastructure.Identity;

namespace GdscSharingPlatform.UnitTests.Infrastructure;

public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_Initialization_ShouldHaveDefaultValues()
    {
        // Act
        var refreshToken = new RefreshToken
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        // Assert
        Assert.NotEqual(Guid.Empty, refreshToken.Id);
        Assert.Equal(Guid.Empty, refreshToken.UserId);
        Assert.Equal(string.Empty, refreshToken.TokenHash);
        Assert.True(refreshToken.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.False(refreshToken.IsRevoked);
        Assert.Null(refreshToken.RevokedAt);
        Assert.Null(refreshToken.ReplacedByTokenHash);
        Assert.Null(refreshToken.RevocationReason);
        Assert.Null(refreshToken.CreatedByIp);
        Assert.Null(refreshToken.UserAgent);
        Assert.False(refreshToken.IsExpired);
        Assert.True(refreshToken.IsActive);
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInPast_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1)
        };

        // Act & Assert
        Assert.True(refreshToken.IsExpired);
        Assert.False(refreshToken.IsActive);
    }

    [Fact]
    public void IsActive_WhenTokenIsRevoked_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = true,
            RevokedAt = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.False(refreshToken.IsExpired);
        Assert.False(refreshToken.IsActive);
    }
}
