using System.Text.Json.Serialization;
using GdscSharingPlatform.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using GdscSharingPlatform.Api.ExceptionHandling;
using GdscSharingPlatform.Api.Extensions;
using GdscSharingPlatform.Api.HealthChecks;
using GdscSharingPlatform.Application;
using GdscSharingPlatform.Infrastructure;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using GdscSharingPlatform.Api.Realtime;
using GdscSharingPlatform.Application.Features.Sharing;

// Khởi tạo builder
var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<RoadmapStatus>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<RoadmapLevel>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<RoadmapNodeType>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<RoadmapRelationType>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<RoadmapLineStyle>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ResourceType>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SharingContentStatus>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SharingScheduleStatus>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SharingAuthorRole>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SharingType>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<DeliveryMode>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<AudienceScope>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<PresenterRole>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ContentCommentStatus>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<ScheduleRsvpStatus>());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<NotificationType>());
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        if (context.HttpContext.Request.HasFormContentType && context.ModelState.Values
            .SelectMany(x => x.Errors).Any(x => x.ErrorMessage.Contains("length limit", StringComparison.OrdinalIgnoreCase)
                || x.Exception is BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge }))
        {
            var oversized = new ProblemDetails { Status = 413, Title = "Payload too large", Detail = "The upload exceeds the request limit." };
            oversized.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            return new ObjectResult(oversized) { StatusCode = 413, ContentTypes = { "application/problem+json" } };
        }
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "The submitted data is invalid."
        };
        problem.Extensions["validationErrors"] = ValidationErrorNames.ForJson(problem.Errors);
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    };
});

// API Documents (OpenAPI/Swagger)
builder.Services.AddApiDocumentation();
builder.Services.AddSharingRateLimits(builder.Configuration);

// Đăng ký dịch vụ thuộc tầng Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 16 * 1024;
    options.MaximumParallelInvocationsPerClient = 1;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.EnableDetailedErrors = false;
});
builder.Services.AddSingleton<RealtimeConnections>();
builder.Services.AddOptions<RealtimeOptions>().Bind(builder.Configuration.GetSection("Realtime"))
    .Validate(o => o.SubscribePerMinute > 0 && o.MaxSubscriptions is > 0 and <= 100
        && o.SessionRecheckSeconds is > 0 and <= 60, "Invalid realtime limits.").ValidateOnStart();
builder.Services.AddScoped<RealtimeAccess>();
builder.Services.AddScoped<IRealtimeEventPublisher, SignalRRealtimePublisher>();
builder.Services.AddHostedService<RealtimeSessionMonitor>();
builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));
var realtimeOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Realtime", policy => policy.WithOrigins(realtimeOrigins)
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .WithExposedHeaders("ETag", "Retry-After", "Content-Disposition")
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton<GdscSharingPlatform.Api.Authentication.ExternalLoginAttemptStore>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<GdscSharingPlatform.Api.Authentication.GoogleLoginHandoffStore>();
builder.Services.AddOptions<GdscSharingPlatform.Api.Authentication.GoogleBffOptions>()
    .Configure(options => options.CallbackUrl =
        builder.Configuration["Authentication:Google:BffCallbackUrl"]
        ?? (builder.Environment.IsProduction() ? "" : "http://localhost:3000/api/auth/google/callback"))
    .Validate(options => GdscSharingPlatform.Api.Authentication.GoogleBffOptions.IsValid(
        options, !builder.Environment.IsProduction()), "Invalid Google BFF callback URL; production requires HTTPS.")
    .ValidateOnStart();

var app = builder.Build();
await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler();
// CORS does not protect WebSocket upgrades. Enforce the same exact origin allowlist here.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/hubs/notifications")
        && context.Request.Headers.TryGetValue("Origin", out var origin)
        && !realtimeOrigins.Contains(origin.ToString(), StringComparer.Ordinal))
    { context.Response.StatusCode = StatusCodes.Status403Forbidden; return; }
    await next(context);
});

// Swagger UI & OpenAPI document
app.UseApiDocumentation();

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks(
   "/health/live",
   new HealthCheckOptions
   {
       Predicate = _ => false,
       ResponseWriter = HealthCheckResponseWriter.WriteAsync
   });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    });

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    });

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications", options =>
{
    options.CloseOnAuthenticationExpiration = true;
    options.ApplicationMaxBufferSize = 64 * 1024;
    options.TransportMaxBufferSize = 64 * 1024;
}).RequireCors("Realtime");

app.Run();

public partial class Program;
