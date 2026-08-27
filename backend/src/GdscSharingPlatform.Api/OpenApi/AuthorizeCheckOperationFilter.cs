using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GdscSharingPlatform.Api.OpenApi;

public sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var metadata = context.ApiDescription
            .ActionDescriptor
            .EndpointMetadata;

        var allowAnonymous = metadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (allowAnonymous)
        {
            return;
        }

        var requiresAuthorization = metadata
            .OfType<IAuthorizeData>()
            .Any();

        if (!requiresAuthorization)
        {
            return;
        }

        operation.Responses.TryAdd(
            StatusCodes.Status401Unauthorized.ToString(),
            new OpenApiResponse
            {
                Description = "Unauthorized"
            });

        operation.Responses.TryAdd(
            StatusCodes.Status403Forbidden.ToString(),
            new OpenApiResponse
            {
                Description = "Forbidden"
            });

        operation.Security ??=
            new List<OpenApiSecurityRequirement>();

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }
                ] = Array.Empty<string>()
            });
    }
}