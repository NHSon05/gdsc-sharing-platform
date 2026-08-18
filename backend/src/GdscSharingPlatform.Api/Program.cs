using GdscSharingPlatform.Infrastructure;

// Khởi tạo builder
var builder = WebApplication.CreateBuilder(args);

// đăng ký dịch vụ
builder.Services.AddControllers();

// API Documents (OpenAPI/Swagger)
builder.Services.AddOpenApi();

// Đăng ký toàn bộ dịch vụ thuộc tầng Infracstructure
builder.Services.AddInfrastructure(
    builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
