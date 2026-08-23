namespace GdscSharingPlatform.Api.Middleware;

public sealed class TraceIdMiddleware(
    RequestDelegate next,
    ILogger<TraceIdMiddleware> logger
    )
{
    public const string HeaderName = "X-Trace-Id";
    private readonly RequestDelegate _next = next;
    private readonly ILogger<TraceIdMiddleware> _logger = logger;

    public async Task InvokeAsync(
        HttpContext context
    )
    {
        var traceId =
            context.TraceIdentifier;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = traceId;
            return Task.CompletedTask;
        });
        using (_logger.BeginScope(
                   new Dictionary<string, object>
                   {
                       ["TraceId"] = traceId
                   }))
        {
            await _next(context);
        }
    }
}
