using GdscSharingPlatform.Api.Authentication;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Infrastructure;
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
    GoogleLoginHandoffStore handoffStore,
    IOptions<GoogleBffOptions> bffOptions,
    ILogger<GoogleAuthController> logger) : ControllerBase
{
    private const string AttemptKey = "gdsc.external.attempt";
    private const string ChallengeKey = "gdsc.external.challenge";

    [HttpGet("start")]
    public async Task<IActionResult> Start([FromQuery] string? challenge)
    {
        if (!GoogleLoginHandoffStore.IsChallenge(challenge)) return BadRequest();
        await HttpContext.SignOutAsync(
            GoogleAuthenticationExtensions.ExternalCookieScheme);

        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/google/complete",
            IsPersistent = false
        };

        properties.Items[AttemptKey] = attemptStore.Create();
        properties.Items[ChallengeKey] = challenge;

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

            result.Properties.Items.TryGetValue(ChallengeKey, out var challenge);
            if (!GoogleLoginHandoffStore.IsChallenge(challenge)) return LoginFailed();

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
                DisplayName: principal.FindFirst("name")?.Value,
                AvatarUrl: principal.FindFirst("picture")?.Value);

            var response = await externalLoginService.LoginAsync(
                identity,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);

            var code = handoffStore.Issue(response, challenge!);
            return Redirect(bffOptions.Value.CallbackUrl + "?code=" + Uri.EscapeDataString(code));
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

            return LoginFailed();
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

    public sealed record ExchangeRequest(string? Code, string? Verifier);

    [HttpPost("exchange")]
    public IActionResult Exchange([FromBody] ExchangeRequest request)
    {
        var session = handoffStore.Redeem(request.Code, request.Verifier);
        return session is null ? Unauthorized(new { code = "external_login_failed" }) : Ok(session);
    }

    private RedirectResult LoginFailed()
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        return Redirect(bffOptions.Value.CallbackUrl + "?error=external_login_failed");
    }
}
