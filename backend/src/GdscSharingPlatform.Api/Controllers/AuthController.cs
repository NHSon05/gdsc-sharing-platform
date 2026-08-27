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
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await _refreshValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var response = await _authService.RefreshTokenAsync(
            request.RefreshToken,
            GetClientIpAddress(),
            GetUserAgent(),
            cancellationToken);

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
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await _logoutValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        Guid? currentUserId = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : null;

        await _authService.LogoutAsync(
            request.RefreshToken,
            currentUserId,
            cancellationToken);

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
}