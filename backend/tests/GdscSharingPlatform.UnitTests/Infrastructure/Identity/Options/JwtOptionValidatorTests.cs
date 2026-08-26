using GdscSharingPlatform.Infrastructure.Identity.Options;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Identity.Options;

public class JwtOptionValidatorTests
{
    private readonly JwtOptionValidator _validator = new();

    [Fact]
    public void Validate_WithValidOptions_ShouldReturnSuccess()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "GdscSharingPlatform",
            Audience = "GdscSharingPlatformClient",
            SecretKey = "super_secret_key_at_least_32_characters_long_12345",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7,
            ClockSkewSeconds = 0
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.FailureMessage);
    }

    [Theory]
    [InlineData("", "GdscSharingPlatformClient", "super_secret_key_at_least_32_characters_long_12345", "Jwt:Issuer is required.")]
    [InlineData("GdscSharingPlatform", "", "super_secret_key_at_least_32_characters_long_12345", "Jwt:Audience is required.")]
    [InlineData("GdscSharingPlatform", "GdscSharingPlatformClient", "", "Jwt:SecretKey is required.")]
    public void Validate_WithMissingRequiredFields_ShouldReturnFailure(
        string issuer,
        string audience,
        string secretKey,
        string expectedError)
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = issuer,
            Audience = audience,
            SecretKey = secretKey
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains(expectedError, result.FailureMessage);
    }

    [Fact]
    public void Validate_WithSecretKeyShorterThan32Characters_ShouldReturnFailure()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "GdscSharingPlatform",
            Audience = "GdscSharingPlatformClient",
            SecretKey = "too_short_secret_key" // < 32 chars
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("Jwt:SecretKey must contain at least 32 characters.", result.FailureMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1441)]
    public void Validate_WithInvalidAccessTokenExpirationMinutes_ShouldReturnFailure(int minutes)
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "GdscSharingPlatform",
            Audience = "GdscSharingPlatformClient",
            SecretKey = "super_secret_key_at_least_32_characters_long_12345",
            AccessTokenExpirationMinutes = minutes
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("Jwt:AccessTokenExpirationMinutes must be between 1 and 1440.", result.FailureMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(366)]
    public void Validate_WithInvalidRefreshTokenExpirationDays_ShouldReturnFailure(int days)
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "GdscSharingPlatform",
            Audience = "GdscSharingPlatformClient",
            SecretKey = "super_secret_key_at_least_32_characters_long_12345",
            RefreshTokenExpirationDays = days
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("Jwt:RefreshTokenExpirationDays must be between 1 and 365.", result.FailureMessage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(301)]
    public void Validate_WithInvalidClockSkewSeconds_ShouldReturnFailure(int clockSkewSeconds)
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "GdscSharingPlatform",
            Audience = "GdscSharingPlatformClient",
            SecretKey = "super_secret_key_at_least_32_characters_long_12345",
            ClockSkewSeconds = clockSkewSeconds
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("Jwt:ClockSkewSeconds must be between 0 and 300.", result.FailureMessage);
    }
}
