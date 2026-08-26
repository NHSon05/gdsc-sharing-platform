using GdscSharingPlatform.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GdscSharingPlatform.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly IHostEnvironment _environment = environment;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Lấy mã HTTP Status tương ứng
        var error = MapException(exception);

        // Ghi lại vết lỗi kèm Method, Path, TraceId
        LogException(
            httpContext,
            exception,
            error.StatusCode);

        httpContext.Response.StatusCode =
            error.StatusCode;

        // Gọi đối tượng ProblemDetails theo chuẩn RESTful
        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Type = error.Type,
            Detail = GetErrorDetail(
                exception,
                error.StatusCode),
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        if (exception is ApplicationValidationException
            validationException)
        {
            problemDetails.Extensions["errors"] =
                validationException.Errors;
        }

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
    }
    private static ErrorMetadata MapException(
        Exception exception)
    {
        return exception switch
        {
            ApplicationValidationException =>
                new ErrorMetadata(
                    StatusCodes.Status400BadRequest,
                    "Validation failed",
                    "https://httpstatuses.com/400"),

            AuthenticationException =>
                new ErrorMetadata(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "https://httpstatuses.com/401"),

            UnauthorizedAccessException =>
                new ErrorMetadata(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "https://httpstatuses.com/401"),

            ForbiddenAccessException =>
                new ErrorMetadata(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "https://httpstatuses.com/403"),

            NotFoundException =>
                new ErrorMetadata(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    "https://httpstatuses.com/404"),

            ConflictException =>
                new ErrorMetadata(
                    StatusCodes.Status409Conflict,
                    "Resource conflict",
                    "https://httpstatuses.com/409"),

            BadHttpRequestException =>
                new ErrorMetadata(
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    "https://httpstatuses.com/400"),

            _ =>
                new ErrorMetadata(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred",
                    "https://httpstatuses.com/500")
        };
    }
    private string GetErrorDetail(
       Exception exception,
       int statusCode)
    {
        if (statusCode <
            StatusCodes.Status500InternalServerError)
        {
            return exception.Message;
        }

        if (_environment.IsDevelopment())
        {
            return exception.Message;
        }

        return "An unexpected error occurred. " +
               "Use the traceId when contacting support.";
    }

    private void LogException(
        HttpContext httpContext,
        Exception exception,
        int statusCode)
    {
        if (statusCode >=
            StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. " +
                "TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);

            return;
        }

        _logger.LogWarning(
            exception,
            "Handled application exception for " +
            "{Method} {Path}. StatusCode: {StatusCode}. " +
            "TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            statusCode,
            httpContext.TraceIdentifier);
    }

    private sealed record ErrorMetadata(
        int StatusCode,
        string Title,
        string Type);
}