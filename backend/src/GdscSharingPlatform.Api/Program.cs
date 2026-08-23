using GdscSharingPlatform.Api.HealthChecks;
using GdscSharingPlatform.Api.ExceptionHandling;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Infrastructure;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// Khởi tạo builder
var builder = WebApplication.CreateBuilder(args);

// đăng ký dịch vụ
builder.Services.AddControllers();

// API Documents (OpenAPI/Swagger)
builder.Services.AddOpenApi();

// Đăng ký toàn bộ dịch vụ thuộc tầng Infracstructure
builder.Services.AddInfrastructure(
    builder.Configuration);

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "GDSC Sharing Platform API v1"
        );
        options.RoutePrefix = "swagger";

        options.DocumentTitle = "GDSC Sharing Platform API";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks(
   "/health/live",
   new HealthCheckOptions
   {
       Predicate = _ => false,
       ResponseWriter =
           HealthCheckResponseWriter.WriteAsync
   });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration =>
            registration.Tags.Contains("ready"),
        ResponseWriter =
            HealthCheckResponseWriter.WriteAsync
    });

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        Predicate = registration =>
            registration.Tags.Contains("ready"),
        ResponseWriter =
            HealthCheckResponseWriter.WriteAsync
    });

app.MapControllers();

app.Run();

public partial class Program;

// Điểm khởi chạy (Entry Point) chính của dự án ASP.NET Core Web API
// File này thực hiện 2 nhiệm vụ quan trọng chính.
// 1. Đăng ký các dịch vụ (Dependency Injection) vào builder.services
// 2. Cấu hình các luồng HTTP Request thông qua app
