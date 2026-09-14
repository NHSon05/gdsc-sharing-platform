using GdscSharingPlatform.Api.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GdscSharingPlatform.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // Sharing DTO names can overlap with existing Roadmap DTOs.
            options.CustomSchemaIds(SchemaId);
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "GDSC Sharing Platform API",
                    Version = "v1",
                    Description = "Authentication and sharing platform API."
                });

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Nhập JWT Bearer token theo định dạng: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.OperationFilter<RoadmapOperationFilter>();
            options.OperationFilter<SharingOperationFilter>();
        });

        return services;
    }

    private static string SchemaId(Type type)
    {
        var name = type.IsConstructedGenericType
            ? string.Concat(type.GetGenericArguments().Select(SchemaId)) + type.Name.Split('`')[0]
            : type.Name.Replace("[]", "Array");
        return type.Namespace == "GdscSharingPlatform.Application.Features.Sharing" ? "Sharing" + name : name;
    }

    public static IApplicationBuilder UseApiDocumentation(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "GDSC Sharing Platform API v1"
                );
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "GDSC Sharing Platform API";
            });
        }

        return app;
    }
}
