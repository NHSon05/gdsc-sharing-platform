using FluentValidation;
using GdscSharingPlatform.Application.Features.Auth.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<
            LoginRequestValidator>();

        return services;
    }
}