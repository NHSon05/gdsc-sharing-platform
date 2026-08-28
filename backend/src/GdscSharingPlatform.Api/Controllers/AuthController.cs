using FluentValidation;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize(Policy = AuthPolicies.RequireActiveUser)]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;
    private readonly IValidator<LogoutRequest> _logoutValidator;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator,
        IValidator<LogoutRequest> logoutValidator)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _logoutValidator = logoutValidator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(AuthResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(HttpValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var response = await _authService.LoginAsync(
            request,
            GetClientIpAddress(),
            GetUserAgent(),
            cancellationToken);

        SetTokenCookies(response.AccessToken, response.RefreshToken);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(
        typeof(TokenResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(HttpValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> RefreshToken(
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        // Hỗ trợ lấy refresh token từ Cookie hoặc Request Body (cho backward compatibility với mobile/swagger)
        var rawRefreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(rawRefreshToken) &&
            Request.Cookies.TryGetValue("refreshToken", out var cookieRefreshToken))
        {
            rawRefreshToken = cookieRefreshToken;
        }

        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            await _refreshValidator.ValidateAndThrowAsync(
                request ?? new RefreshTokenRequest(string.Empty),
                cancellationToken);
        }

        var response = await _authService.RefreshTokenAsync(
            rawRefreshToken!,
            GetClientIpAddress(),
            GetUserAgent(),
            cancellationToken);

        SetTokenCookies(response.AccessToken, response.RefreshToken);

        return Ok(response);
    }

    // Cho phép logout khi access token đã hết hạn.
    // Refresh token vẫn được dùng để xác định session cần thu hồi.
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(HttpValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] LogoutRequest? request,
        CancellationToken cancellationToken)
    {
        var rawRefreshToken = request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(rawRefreshToken) &&
            Request.Cookies.TryGetValue("refreshToken", out var cookieRefreshToken))
        {
            rawRefreshToken = cookieRefreshToken;
        }

        if (!string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            Guid? currentUserId = _currentUserService.IsAuthenticated
                ? _currentUserService.UserId
                : null;

            await _authService.LogoutAsync(
                rawRefreshToken,
                currentUserId,
                cancellationToken);
        }

        ClearTokenCookies();

        return NoContent();
    }

    [HttpPost("logout-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        await _authService.LogoutAllAsync(
            userId,
            cancellationToken);

        ClearTokenCookies();

        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType(
        typeof(CurrentUserDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var response = await _authService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        return Ok(response);
    }

    private Guid GetRequiredUserId()
    {
        return _currentUserService.UserId
               ?? throw new AuthenticationException(
                   "Authenticated user identifier is missing.");
    }

    private string? GetClientIpAddress()
    {
        return HttpContext.Connection
            .RemoteIpAddress?
            .ToString();
    }

    private string? GetUserAgent()
    {
        var userAgent = Request.Headers.UserAgent.ToString();

        return string.IsNullOrWhiteSpace(userAgent)
            ? null
            : userAgent;
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        Response.Cookies.Append("accessToken", accessToken, cookieOptions);

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);
    }
    private void ClearTokenCookies()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
    }
}