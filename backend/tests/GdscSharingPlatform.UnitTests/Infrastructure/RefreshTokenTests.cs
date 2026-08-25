using GdscSharingPlatform.Infrastructure.Identity;

namespace GdscSharingPlatform.UnitTests.Infrastructure;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "sample_hash_value";
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        var ip = " 127.0.0.1 ";
        var userAgent = " Mozilla/5.0 ";

        // Act
        var token = new RefreshToken(userId, tokenHash, createdAt, expiresAt, ip, userAgent);

        // Assert
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(tokenHash, token.TokenHash);
        Assert.Equal(createdAt, token.CreatedAt);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.Equal("127.0.0.1", token.CreatedByIp);
        Assert.Equal("Mozilla/5.0", token.UserAgent);
        Assert.False(token.IsRevoked);
        Assert.Null(token.RevokedAt);
        Assert.Null(token.RevocationReason);
        Assert.Null(token.ReplacedByTokenHash);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(7);

        var ex = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(Guid.Empty, "hash", createdAt, expiresAt, null, null));

        Assert.Equal("userId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTokenHash_ShouldThrowArgumentException(string? invalidHash)
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(7);

        var ex = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(userId, invalidHash!, createdAt, expiresAt, null, null));

        Assert.Equal("tokenHash", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithExpiresAtBeforeOrEqualToCreatedAt_ShouldThrowArgumentException()
    {
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var ex = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(userId, "hash", createdAt, createdAt, null, null));

        Assert.Equal("expiresAt", ex.ParamName);
    }

    [Fact]
    public void IsActiveAt_WhenNotRevokedAndNotExpired_ShouldReturnTrue()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        var token = new RefreshToken(Guid.NewGuid(), "hash", createdAt, expiresAt, null, null);

        var checkTime = createdAt.AddDays(1);
        Assert.True(token.IsActiveAt(checkTime));
    }

    [Fact]
    public void IsActiveAt_WhenExpired_ShouldReturnFalse()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        var token = new RefreshToken(Guid.NewGuid(), "hash", createdAt, expiresAt, null, null);

        var checkTime = expiresAt.AddSeconds(1);
        Assert.False(token.IsActiveAt(checkTime));
    }

    [Fact]
    public void IsActiveAt_WhenRevoked_ShouldReturnFalse()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(7);
        var token = new RefreshToken(Guid.NewGuid(), "hash", createdAt, expiresAt, null, null);

        token.Revoke(createdAt.AddHours(1), "User logged out");

        var checkTime = createdAt.AddHours(2);
        Assert.False(token.IsActiveAt(checkTime));
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_WithValidParameters_ShouldSetRevocationFields()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var token = new RefreshToken(Guid.NewGuid(), "hash", createdAt, createdAt.AddDays(7), null, null);
        var revokedAt = createdAt.AddHours(2);

        token.Revoke(revokedAt, " Replaced by new token ", " new_hash_123 ");

        Assert.True(token.IsRevoked);
        Assert.Equal(revokedAt, token.RevokedAt);
        Assert.Equal("Replaced by new token", token.RevocationReason);
        Assert.Equal("new_hash_123", token.ReplacedByTokenHash);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldBeIdempotent()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var token = new RefreshToken(Guid.NewGuid(), "hash", createdAt, createdAt.AddDays(7), null, null);
        var firstRevokedAt = createdAt.AddHours(1);
        token.Revoke(firstRevokedAt, "First reason");

        // Act - revoke again
        token.Revoke(createdAt.AddHours(2), "Second reason");

        // Assert - fields should remain from the first call
        Assert.True(token.IsRevoked);
        Assert.Equal(firstRevokedAt, token.RevokedAt);
        Assert.Equal("First reason", token.RevocationReason);
    }

    [Fact]
    public void Revoke_WithRevokedAtBeforeCreatedAt_ShouldThrowArgumentException()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var token = new RefreshToken(Guid.NewGuid(), "hash", createdAt, createdAt.AddDays(7), null, null);

        var ex = Assert.Throws<ArgumentException>(() =>
            token.Revoke(createdAt.AddMinutes(-5), "Reason"));

        Assert.Equal("revokedAt", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Revoke_WithEmptyReason_ShouldThrowArgumentException(string? invalidReason)
    {
        var createdAt = DateTimeOffset.UtcNow;
        var token = new RefreshToken(Guid.NewGuid(), "hash", createdAt, createdAt.AddDays(7), null, null);

        var ex = Assert.Throws<ArgumentException>(() =>
            token.Revoke(createdAt.AddHours(1), invalidReason!));

        Assert.Equal("reason", ex.ParamName);
    }
}
