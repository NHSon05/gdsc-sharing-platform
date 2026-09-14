using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GdscSharingPlatform.Api.OpenApi;

public sealed class SharingOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? "";
        if (!path.StartsWith("api/sharing") && !path.StartsWith("api/admin/sharing")) return;
        operation.Description = (operation.Description ?? "") +
            " Requires an active Member or Admin. Admin routes require Admin. Draft content is private to Owner/Admin; Member-created schedules belong to their creator.";
        if (context.ApiDescription.HttpMethod is "PATCH" or "DELETE" or "POST"
            && !path.Contains("/tags") && path.Contains('{'))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match", In = ParameterLocation.Header, Required = true,
                Description = "One quoted version from the latest ETag. Resource mutations use the parent Content version. Stale writes return 412; missing/invalid header returns 400.",
                Schema = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("\"0\"") }
            });
        }
        if (path.EndsWith("/download"))
            operation.Responses["200"] = new OpenApiResponse
            { Description = "Authorized private file download", Content = { ["application/octet-stream"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string", Format = "binary" } } } };
        foreach (var response in operation.Responses.Where(x => x.Key is "200" or "201" or "204"))
            response.Value.Headers["ETag"] = new OpenApiHeader { Description = "Current aggregate version where applicable.", Schema = new OpenApiSchema { Type = "string" } };
    }
}
