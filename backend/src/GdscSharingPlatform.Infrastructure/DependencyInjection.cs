using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Features.Auth.Interfaces;
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

        services.AddAuthentication();

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

        services.AddSingleton<
            IJwtTokenGenerator,
            JwtTokenGenerator>();

        services.AddScoped<
            ICurrentUserService,
            CurrentUserService>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<DatabaseSeeder>();


        return services;
    }
}