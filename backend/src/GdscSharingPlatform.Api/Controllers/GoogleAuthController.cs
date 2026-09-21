using GdscSharingPlatform.Api.Authentication;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Infrastructure;
using GdscSharingPlatform.Infrastructure.Identity.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth/google")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class GoogleAuthController(
    IExternalLoginService externalLoginService,
    ExternalLoginAttemptStore attemptStore,
    IOptions<JwtOptions> jwtOptions,
    ILogger<GoogleAuthController> logger) : ControllerBase
{
    private const string AttemptKey = "gdsc.external.attempt";

    [HttpGet("start")]
    public async Task<IActionResult> Start()
    {
        await HttpContext.SignOutAsync(
            GoogleAuthenticationExtensions.ExternalCookieScheme);

        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/google/complete",
            IsPersistent = false
        };

        properties.Items[AttemptKey] = attemptStore.Create();

        return Challenge(
            properties,
            GoogleAuthenticationExtensions.GoogleScheme);
    }

    [HttpGet("complete")]
    public async Task<IActionResult> Complete(
        CancellationToken cancellationToken)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";

        try
        {
            var result = await HttpContext.AuthenticateAsync(
                GoogleAuthenticationExtensions.ExternalCookieScheme);

            if (!result.Succeeded ||
                result.Principal is null ||
                result.Properties is null)
            {
                return LoginFailed();
            }

            result.Properties.Items.TryGetValue(
                AttemptKey,
                out var attemptId);

            if (!attemptStore.TryConsume(attemptId))
            {
                return LoginFailed();
            }

            var principal = result.Principal;
            var subject = principal.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(subject))
            {
                return LoginFailed();
            }

            var emailVerified =
                bool.TryParse(
                    principal.FindFirst("email_verified")?.Value,
                    out var verified)
                && verified;

            var identity = new VerifiedExternalIdentity(
                Provider: "Google",
                Subject: subject,
                Email: principal.FindFirst("email")?.Value,
                EmailVerified: emailVerified,
                DisplayName: principal.FindFirst("name")?.Value);

            var response = await externalLoginService.LoginAsync(
                identity,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);

            WriteSessionCookies(response);

            // Chỉ trả profile để kiểm thử backend.
            // Không trả access/refresh token vào JSON của browser.
            return Ok(response.User);
        }
        catch (ExternalLoginException exception)
        {
            logger.LogWarning(
                "Google login rejected. Code {Code}, TraceId {TraceId}.",
                exception.Code,
                HttpContext.TraceIdentifier);

            // Client nhận mã chung; log giữ mã nghiệp vụ an toàn.
            return LoginFailed();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Không log exception object/message có thể chứa dữ liệu provider.
            logger.LogError(
                "Google login failed. ErrorType {ErrorType}, " +
                "TraceId {TraceId}.",
                exception.GetType().Name,
                HttpContext.TraceIdentifier);

            return LoginFailed(StatusCodes.Status500InternalServerError);
        }
        finally
        {
            await HttpContext.SignOutAsync(
                GoogleAuthenticationExtensions.ExternalCookieScheme);
        }
    }

    [HttpGet("error")]
    public IActionResult Error()
    {
        return LoginFailed();
    }

    private ObjectResult LoginFailed(
        int status = StatusCodes.Status401Unauthorized)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = "Không thể hoàn tất đăng nhập Google.",
            Detail = "Vui lòng thử lại hoặc đăng nhập bằng tài khoản GDSC."
        };

        problem.Extensions["code"] = "external_login_failed";
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = status
        };
    }

    private void WriteSessionCookies(AuthResponse response)
    {
        Response.Cookies.Append(
            "accessToken",
            response.AccessToken,
            CreateCookieOptions(
                TimeSpan.FromSeconds(response.ExpiresIn)));

        Response.Cookies.Append(
            "refreshToken",
            response.RefreshToken,
            CreateCookieOptions(
                TimeSpan.FromDays(
                    jwtOptions.Value.RefreshTokenExpirationDays)));
    }

    private static CookieOptions CreateCookieOptions(TimeSpan lifetime)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = lifetime
        };
    }
}