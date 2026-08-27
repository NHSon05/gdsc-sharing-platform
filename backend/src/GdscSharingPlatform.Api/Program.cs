using GdscSharingPlatform.Api.ExceptionHandling;
using GdscSharingPlatform.Api.Extensions;
using GdscSharingPlatform.Api.HealthChecks;
using GdscSharingPlatform.Application;
using GdscSharingPlatform.Infrastructure;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// Khởi tạo builder
var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Controllers
builder.Services.AddControllers();

// API Documents (OpenAPI/Swagger)
builder.Services.AddApiDocumentation();

// Đăng ký dịch vụ thuộc tầng Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler();

// Swagger UI & OpenAPI document
app.UseApiDocumentation();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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

app.Run();

public partial class Program;
