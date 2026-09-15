using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GdscSharingPlatform.Api.Extensions;

public static class SharingApiExtensions
{
    public static IServiceCollection AddSharingRateLimits(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            foreach (var (name, limit) in new[] { ("sharing-upload", 10), ("sharing-submit", 5) })
                options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = limit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.Headers.RetryAfter = "60";
                var problem = new ProblemDetails { Status = 429, Title = "Too many requests", Detail = "Wait before trying again." };
                problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                await context.HttpContext.Response.WriteAsJsonAsync(problem, options: (System.Text.Json.JsonSerializerOptions?)null,
                    contentType: "application/problem+json", cancellationToken: ct);
            };
        });
    }
}
