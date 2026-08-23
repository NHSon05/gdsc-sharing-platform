using GdscSharingPlatform.Api.ExceptionHandling;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Infrastructure;
using GdscSharingPlatform.Infrastructure.Persistence;

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
    // app.MapOpenApi();
    app.MapGet(
        "/development/errors/not-found",
        () =>
        {
            throw new NotFoundException(
                "Department",
                Guid.NewGuid());
        });

    app.MapGet(
        "/development/errors/conflict",
        () =>
        {
            throw new ConflictException(
                "A Department with this code already exists.");
        });

    app.MapGet(
        "/development/errors/unexpected",
        () =>
        {
            throw new InvalidOperationException(
                "Example unexpected error.");
        });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;

// Điểm khởi chạy (Entry Point) chính của dự án ASP.NET Core Web API
// File này thực hiện 2 nhiệm vụ quan trọng chính.
// 1. Đăng ký các dịch vụ (Dependency Injection) vào builder.services
// 2. Cấu hình các luồng HTTP Request thông qua app
