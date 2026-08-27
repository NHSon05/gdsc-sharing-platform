using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Identity.Options;
using GdscSharingPlatform.Infrastructure.Identity.Seeding;
using GdscSharingPlatform.Infrastructure.Identity.Services;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                options.UseNpgsql(connectionString);
            });

        services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags:
                [
                    "ready",
                    "database"
                ]
            );

        services
            .AddIdentityCore<ApplicationUser>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(15);
                })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddJwtAuthentication(configuration);
        services.AddApplicationAuthorization();

        services
            .AddOptions<AdminSeedOptions>()
            .Bind(
                configuration.GetSection(
                    AdminSeedOptions.SectionName))
            .Validate(
                options =>
                    !options.Enabled ||
                    !string.IsNullOrWhiteSpace(options.Email),
                "SeedAdmin:Email is required.")
            .Validate(
                options =>
                    !options.Enabled ||
                    !string.IsNullOrWhiteSpace(options.Password),
                "SeedAdmin:Password is required.")
            .Validate(
                options =>
                    !options.Enabled ||
                    !string.IsNullOrWhiteSpace(options.FullName),
                "SeedAdmin:FullName is required.")
            .Validate(
                options =>
                    !options.Enabled ||
                    !string.IsNullOrWhiteSpace(
                        options.DepartmentCode),
                "SeedAdmin:DepartmentCode is required.")
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .Bind(
                configuration.GetSection(
                    JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<JwtOptions>,
            JwtOptionValidator>();

        services.AddHttpContextAccessor();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<DatabaseSeeder>();


        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetRequiredSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Giữ nguyên tên claim: sub, role, status...
                options.MapInboundClaims = false;

                options.RequireHttpsMetadata = false;
                options.SaveToken = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,

                        ClockSkew = TimeSpan.FromSeconds(
                            jwtOptions.ClockSkewSeconds),

                        NameClaimType = "name",
                        RoleClaimType = AuthClaimTypes.Role
                    };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        await WriteProblemAsync(
                            context.Response,
                            StatusCodes.Status401Unauthorized,
                            "Unauthorized",
                            "Access token is missing, invalid or expired.",
                            context.HttpContext.TraceIdentifier,
                            context.HttpContext.RequestAborted);
                    },

                    OnForbidden = async context =>
                    {
                        await WriteProblemAsync(
                            context.Response,
                            StatusCodes.Status403Forbidden,
                            "Forbidden",
                            "You do not have permission to access this resource.",
                            context.HttpContext.TraceIdentifier,
                            context.HttpContext.RequestAborted);
                    }
                };
            });

        return services;
    }

    private static IServiceCollection AddApplicationAuthorization(
            this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                AuthPolicies.RequireActiveUser,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireClaim(
                        AuthClaimTypes.Status,
                        UserStatus.Active.ToString());
                })
            .AddPolicy(
                AuthPolicies.AdminOnly,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireClaim(
                        AuthClaimTypes.Status,
                        UserStatus.Active.ToString());

                    policy.RequireRole(RoleNames.Admin);
                })
            .AddPolicy(
                AuthPolicies.MemberOnly,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireClaim(
                        AuthClaimTypes.Status,
                        UserStatus.Active.ToString());

                    policy.RequireRole(RoleNames.Member);
                });

        return services;
    }
    private static Task WriteProblemAsync(
        HttpResponse response,
        int statusCode,
        string title,
        string detail,
        string traceId,
        CancellationToken cancellationToken)
    {
        if (response.HasStarted)
        {
            return Task.CompletedTask;
        }

        response.StatusCode = statusCode;
        response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        problem.Extensions["traceId"] = traceId;

        return response.WriteAsJsonAsync(
            problem,
            cancellationToken);
    }
}
