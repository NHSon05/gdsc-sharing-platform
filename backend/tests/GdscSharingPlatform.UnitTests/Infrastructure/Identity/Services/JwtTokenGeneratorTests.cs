using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Infrastructure.Identity.Options;
using GdscSharingPlatform.Infrastructure.Identity.Services;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Identity.Services;

public class JwtTokenGeneratorTests
{
    private readonly JwtOptions _options = new()
    {
        Issuer = "GdscSharingPlatformTest",
        Audience = "GdscSharingPlatformTestClient",
        SecretKey = "test_super_secret_key_at_least_32_characters_long_12345",
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7,
        ClockSkewSeconds = 0
    };

    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var optionsMock = Microsoft.Extensions.Options.Options.Create(_options);
        _generator = new JwtTokenGenerator(optionsMock);
    }

    [Fact]
    public void GenerateAccessToken_WithValidInputs_ShouldReturnValidTokenAndExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "admin@gdsc.app";
        var fullName = "Admin User";
        var roles = new[] { RoleNames.Admin, RoleNames.Member };
        var departmentId = Guid.NewGuid();
        var status = "Active";

        // Act
        var (token, expiresInSeconds) = _generator.GenerateAccessToken(
            userId, email, fullName, roles, departmentId, status);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal(15 * 60, expiresInSeconds);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal(_options.Issuer, jwtToken.Issuer);
        Assert.Contains(_options.Audience, jwtToken.Audiences);
        Assert.Equal("HS256", jwtToken.Header.Alg);

        // Check Claims
        Assert.Equal(userId.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(email, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(fullName, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));
        Assert.Equal(status, jwtToken.Claims.First(c => c.Type == AuthClaimTypes.Status).Value);
        Assert.Equal(departmentId.ToString(), jwtToken.Claims.First(c => c.Type == AuthClaimTypes.DepartmentId).Value);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
        Assert.Contains(RoleNames.Admin, roleClaims);
        Assert.Contains(RoleNames.Member, roleClaims);
    }

    [Fact]
    public void GenerateAccessToken_WithoutDepartment_ShouldNotIncludeDepartmentClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var (token, _) = _generator.GenerateAccessToken(
            userId, "user@gdsc.app", "Test User", [RoleNames.Member], null, "Active");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Null(jwtToken.Claims.FirstOrDefault(c => c.Type == AuthClaimTypes.DepartmentId));
    }

    [Fact]
    public void GenerateAccessToken_WithDuplicateRoles_ShouldDeduplicateRoleClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var duplicateRoles = new[] { RoleNames.Member, RoleNames.Member, "  ", RoleNames.Member };

        // Act
        var (token, _) = _generator.GenerateAccessToken(
            userId, "user@gdsc.app", "Test User", duplicateRoles, null, "Active");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
        Assert.Single(roleClaims);
        Assert.Equal(RoleNames.Member, roleClaims[0]);
    }

    [Theory]
    [InlineData("", "Valid Name", "Active")]
    [InlineData("email@gdsc.app", "", "Active")]
    [InlineData("email@gdsc.app", "Valid Name", "")]
    public void GenerateAccessToken_WithInvalidStringArguments_ShouldThrowArgumentException(
        string email, string fullName, string status)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _generator.GenerateAccessToken(userId, email, fullName, [RoleNames.Member], null, status));
    }

    [Fact]
    public void GenerateAccessToken_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _generator.GenerateAccessToken(Guid.Empty, "user@gdsc.app", "User", [RoleNames.Member], null, "Active"));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldProduceCryptographicallySecureUniqueTokens()
    {
        // Act
        var tokens = Enumerable.Range(0, 1000)
            .Select(_ => _generator.GenerateRefreshToken())
            .ToList();

        // Assert
        Assert.Equal(1000, tokens.Distinct().Count());
        Assert.All(tokens, token =>
        {
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.True(token.Length >= 64);
        });
    }

    [Fact]
    public void HashToken_WithSameInput_ShouldReturnConsistentHash()
    {
        // Arrange
        var rawToken = _generator.GenerateRefreshToken();

        // Act
        var hash1 = _generator.HashToken(rawToken);
        var hash2 = _generator.HashToken(rawToken);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.NotEmpty(hash1);
        Assert.Equal(64, hash1.Length); // SHA-256 Hex is 64 hex characters
    }

    [Fact]
    public void HashToken_WithDifferentInputs_ShouldReturnDifferentHashes()
    {
        // Arrange
        var rawToken1 = _generator.GenerateRefreshToken();
        var rawToken2 = _generator.GenerateRefreshToken();

        // Act
        var hash1 = _generator.HashToken(rawToken1);
        var hash2 = _generator.HashToken(rawToken2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HashToken_WithEmptyInput_ShouldThrowArgumentException(string invalidInput)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _generator.HashToken(invalidInput));
    }
}
