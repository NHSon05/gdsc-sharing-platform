using GdscSharingPlatform.Infrastructure.Identity.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure;

public static class GoogleAuthenticationExtensions
{
    public const string GoogleScheme = "Google";
    public const string ExternalCookieScheme = "GoogleExternal";
    public static IServiceCollection AddGoogleAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<GoogleAuthenticationOptions>()
            .Bind(configuration.GetSection(
                GoogleAuthenticationOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ClientId),
                "Authentication:Google:ClientId is required"
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ClientSecret),
                "Authentication:Google:ClientSecret is required."
            )
            .Validate(
                options => options.CallbackPath == "/api/auth/google/callback",
                "Authentication:Google:CallbackPath must be " + "'/api/auth/google/callback'."
            )
            .ValidateOnStart();
        
       services
            .AddAuthentication()
            .AddCookie(ExternalCookieScheme, options =>
            {
                options.Cookie.Name = "__Host-gdsc-google-external";
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
            })
            .AddOpenIdConnect(GoogleScheme, _ => { });

        services
            .AddOptions<OpenIdConnectOptions>(GoogleScheme)
            .Configure<IOptions<GoogleAuthenticationOptions>>(
                (options, configuredGoogle) =>
                {
                    var google = configuredGoogle.Value;

                    options.Authority = "https://accounts.google.com";
                    options.ClientId = google.ClientId;
                    options.ClientSecret = google.ClientSecret;
                    options.CallbackPath = google.CallbackPath;

                    options.SignInScheme = ExternalCookieScheme;

                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.ResponseMode = OpenIdConnectResponseMode.Query;
                    options.UsePkce = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");

                    options.RequireHttpsMetadata = true;
                    options.SaveTokens = false;
                    options.GetClaimsFromUserInfoEndpoint = false;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters.ValidateIssuer = true;
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidateLifetime = true;
                    options.TokenValidationParameters.RequireSignedTokens = true;
                    options.TokenValidationParameters.NameClaimType = "name";

                    options.ProtocolValidator.RequireNonce = true;

                    options.CorrelationCookie.HttpOnly = true;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.CorrelationCookie.SameSite = SameSiteMode.None;

                    options.NonceCookie.HttpOnly = true;
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.NonceCookie.SameSite = SameSiteMode.None;
                    options.Events.OnRemoteFailure = async context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("GoogleOidc");

                        logger.LogWarning(
                            "Google OIDC callback failed. TraceId {TraceId}.",
                            context.HttpContext.TraceIdentifier);

                        await context.HttpContext.SignOutAsync(
                            ExternalCookieScheme);

                        context.HandleResponse();

                        context.Response.Headers["Cache-Control"] = "no-store";
                        context.Response.Headers["Referrer-Policy"] = "no-referrer";

                        context.Response.Redirect("/api/auth/google/error");
                    };
                });
        return services;
    }
}