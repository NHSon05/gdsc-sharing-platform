using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Http;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Identity.Services;

public class CurrentUserServiceTests
{
    private readonly HttpContextAccessor _httpContextAccessor = new();
    private readonly CurrentUserService _currentUserService;

    public CurrentUserServiceTests()
    {
        _currentUserService = new CurrentUserService(_httpContextAccessor);
    }

    [Fact]
    public void Properties_WhenHttpContextIsNull_ShouldReturnUnauthenticatedDefaults()
    {
        // Arrange
        _httpContextAccessor.HttpContext = null;

        // Act & Assert
        Assert.False(_currentUserService.IsAuthenticated);
        Assert.Null(_currentUserService.UserId);
        Assert.Null(_currentUserService.Email);
        Assert.Empty(_currentUserService.Roles);
    }

    [Fact]
    public void Properties_WhenUserIsAnonymous_ShouldReturnUnauthenticated()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessor.HttpContext = context;

        // Act & Assert
        Assert.False(_currentUserService.IsAuthenticated);
        Assert.Null(_currentUserService.UserId);
        Assert.Null(_currentUserService.Email);
        Assert.Empty(_currentUserService.Roles);
    }

    [Fact]
    public void Properties_WhenUserIsAuthenticatedWithJwtClaims_ShouldExtractAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "member@gdsc.app";
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("role", RoleNames.Member),
            new(ClaimTypes.Role, RoleNames.Admin)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = principal
        };

        // Act & Assert
        Assert.True(_currentUserService.IsAuthenticated);
        Assert.Equal(userId, _currentUserService.UserId);
        Assert.Equal(email, _currentUserService.Email);
        Assert.Equal(2, _currentUserService.Roles.Count);
        Assert.Contains(RoleNames.Member, _currentUserService.Roles);
        Assert.Contains(RoleNames.Admin, _currentUserService.Roles);
    }

    [Fact]
    public void Properties_WhenUserHasNameIdentifierClaim_ShouldFallbackToExtractUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@gdsc.app";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        // Act & Assert
        Assert.True(_currentUserService.IsAuthenticated);
        Assert.Equal(userId, _currentUserService.UserId);
        Assert.Equal(email, _currentUserService.Email);
    }

    [Fact]
    public void UserId_WhenSubjectClaimIsInvalidGuid_ShouldReturnNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "not-a-valid-guid")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        // Act & Assert
        Assert.Null(_currentUserService.UserId);
    }
}
