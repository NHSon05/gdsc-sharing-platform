using System.IO;
using System.Text.Json;
using GdscSharingPlatform.Api.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GdscSharingPlatform.UnitTests.Api.HealthChecks;

public class HealthCheckResponseWriterTests
{
    [Fact]
    public async Task WriteAsync_ShouldSetContentType_ToApplicationJsonUtf8()
    {
        // Arrange: là một memo
        // Tạo đối tượng httpContext giả lập trong bộ nhớ
        // HttpContext chỉ được sinh ra khi có 1 HTTP Request thực tế từ client -> server
        // Unit Test không có server chạy thì dùng => DefaultHttpContext -> Tạo context ảo
        var context = new DefaultHttpContext();
        // Gán luồng dữ liệu response thành 1 Memorystream
        // Mặc định trên test, Response.Body là Nullstream
        // Làm nơi chứa dữ liệu mà hàm sắp ghi ra
        context.Response.Body = new MemoryStream();

        var report = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Healthy,
            TimeSpan.FromMilliseconds(50)
        );

        // Act
        await HealthCheckResponseWriter.WriteAsync(context, report);

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
    }

    [Fact]
    public async Task WriteAsync_WithHealthyReport_ShouldWriteExpectedJsonStructure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var traceId = "test-trace-id-123";
        context.TraceIdentifier = traceId;

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new(
                HealthStatus.Healthy,
                "Database is reachable",
                TimeSpan.FromMilliseconds(12.5),
                exception: null,
                data: null
            )
        };

        var report = new HealthReport(entries, HealthStatus.Healthy, TimeSpan.FromMilliseconds(15.0));

        // Act
        await HealthCheckResponseWriter.WriteAsync(context, report);

        // Assert
        memoryStream.Position = 0;
        using var jsonDoc = await JsonDocument.ParseAsync(memoryStream);
        var root = jsonDoc.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.Equal(15.0, root.GetProperty("totalDuration").GetDouble());
        Assert.Equal(traceId, root.GetProperty("traceId").GetString());

        var checks = root.GetProperty("checks");
        Assert.Equal(1, checks.GetArrayLength());

        var check0 = checks[0];
        Assert.Equal("database", check0.GetProperty("name").GetString());
        Assert.Equal("Healthy", check0.GetProperty("status").GetString());
        Assert.Equal(12.5, check0.GetProperty("duration").GetDouble());
        Assert.Equal("Database is reachable", check0.GetProperty("description").GetString());
    }

    [Fact]
    public async Task WriteAsync_WithUnhealthyReport_ShouldSerializeUnhealthyStatusAndDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "unhealthy-trace-456";

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["postgresql"] = new(
                HealthStatus.Unhealthy,
                "Connection failed",
                TimeSpan.FromMilliseconds(100),
                exception: new InvalidOperationException("DB offline"),
                data: null
            )
        };

        var report = new HealthReport(entries, HealthStatus.Unhealthy, TimeSpan.FromMilliseconds(105));

        // Act
        await HealthCheckResponseWriter.WriteAsync(context, report);

        // Assert
        memoryStream.Position = 0;
        using var jsonDoc = await JsonDocument.ParseAsync(memoryStream);
        var root = jsonDoc.RootElement;

        Assert.Equal("Unhealthy", root.GetProperty("status").GetString());
        Assert.Equal("unhealthy-trace-456", root.GetProperty("traceId").GetString());

        var checks = root.GetProperty("checks");
        Assert.Equal(1, checks.GetArrayLength());

        var postgresCheck = checks[0];
        Assert.Equal("postgresql", postgresCheck.GetProperty("name").GetString());
        Assert.Equal("Unhealthy", postgresCheck.GetProperty("status").GetString());
        Assert.Equal("Connection failed", postgresCheck.GetProperty("description").GetString());
    }
}
