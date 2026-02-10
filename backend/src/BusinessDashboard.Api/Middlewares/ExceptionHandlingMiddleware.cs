using System.Text.Json;
using BusinessDashboard.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BusinessDashboard.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        // If the response has already started, we can't reliably write a problem payload.

        var (statusCode, title) = ex switch
        {
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Bad Request"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),

            // Back-compat: until everything is migrated to custom exceptions.
            ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Bad Request"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad Request"),

            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var detail = statusCode == StatusCodes.Status500InternalServerError && !_env.IsDevelopment()
            ? "An unexpected error occurred."
            : ex.Message;

        if (statusCode >= 500)
            _logger.LogError(ex, "Unhandled exception.");
        else
            _logger.LogWarning(ex, "Request failed with a client error.");

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path.Value
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
