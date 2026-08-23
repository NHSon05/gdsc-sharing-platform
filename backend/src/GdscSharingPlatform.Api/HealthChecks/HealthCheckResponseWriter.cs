using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GdscSharingPlatform.Api.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static async Task WriteAsync(
        HttpContext context,
        HealthReport report
    )
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            traceId = context.TraceIdentifier,

            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description
            })
        };
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }
        );
    }
}