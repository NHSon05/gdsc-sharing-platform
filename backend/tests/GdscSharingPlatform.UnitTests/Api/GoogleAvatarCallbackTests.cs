using System.Security.Claims;
using GdscSharingPlatform.Api.Authentication;
using GdscSharingPlatform.Api.Controllers;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Models;
using GdscSharingPlatform.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GdscSharingPlatform.UnitTests.Api;

public sealed class GoogleAvatarCallbackTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("https://lh3.googleusercontent.com/avatar=s96-c")]
    public async Task Complete_MapsOptionalPictureFromAuthenticatedPrincipal(string? picture)
    {
        var attempts = new ExternalLoginAttemptStore();
        var properties = new AuthenticationProperties();
        properties.Items["gdsc.external.attempt"] = attempts.Create();
        properties.Items["gdsc.external.challenge"] = new string('a', 43);
        var claims = new List<Claim> { new("sub", "google-sub"), new("email", "avatar@example.test"),
            new("email_verified", "true"), new("name", "Avatar Member") };
        if (picture is not null) claims.Add(new Claim("picture", picture));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, "test")),
            properties, GoogleAuthenticationExtensions.ExternalCookieScheme);
        var authentication = new StubAuthentication(ticket);
        using var services = new ServiceCollection().AddSingleton<IAuthenticationService>(authentication).BuildServiceProvider();
        var login = new CapturingLogin();
        var controller = new GoogleAuthController(login, attempts, new GoogleLoginHandoffStore(TimeProvider.System),
            Options.Create(new GoogleBffOptions { CallbackUrl = "https://app.example.test/api/auth/google/callback" }),
            NullLogger<GoogleAuthController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } }
        };

        Assert.IsType<RedirectResult>(await controller.Complete(CancellationToken.None));
        Assert.NotNull(login.Identity);
        Assert.Equal(picture, login.Identity.AvatarUrl);
        Assert.True(authentication.SignedOut);
    }

    private sealed class CapturingLogin : IExternalLoginService
    {
        public VerifiedExternalIdentity? Identity { get; private set; }
        public Task<AuthResponse> LoginAsync(VerifiedExternalIdentity identity, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            Identity = identity;
            return Task.FromResult(new AuthResponse("test-access", "test-refresh", "Bearer", 900,
                new CurrentUserDto(Guid.NewGuid(), "avatar@example.test", "Avatar Member", null, null, identity.AvatarUrl, "Active", null, ["Member"])));
        }
    }

    private sealed class StubAuthentication(AuthenticationTicket ticket) : IAuthenticationService
    {
        public bool SignedOut { get; private set; }
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => Task.FromResult(AuthenticateResult.Success(ticket));
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            SignedOut = true;
            return Task.CompletedTask;
        }
    }
}
