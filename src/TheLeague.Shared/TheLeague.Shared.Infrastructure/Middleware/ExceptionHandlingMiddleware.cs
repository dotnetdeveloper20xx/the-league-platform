using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TheLeague.Shared.Infrastructure.Exceptions;

namespace TheLeague.Shared.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException ve => (ve.StatusCode, "Validation Error", ve.Message, ve.Errors),
            DomainException de => (de.StatusCode, GetTitle(de.StatusCode), de.Message, (List<FieldError>?)null),
            _ => (500, "Internal Server Error", "An unexpected error occurred.", (List<FieldError>?)null)
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception (CorrelationId: {CorrelationId})", correlationId);
        }
        else
        {
            _logger.LogWarning("Domain exception (CorrelationId: {CorrelationId}): {Message}", correlationId, exception.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            type = $"https://theleague.com/errors/{title.ToLower().Replace(" ", "-")}",
            title,
            status = statusCode,
            detail,
            instance = context.Request.Path.Value,
            traceId = correlationId,
            errors = errors?.Select(e => new { field = e.Field, message = e.Message })
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        429 => "Too Many Requests",
        503 => "Service Unavailable",
        _ => "Error"
    };
}
