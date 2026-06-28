using System.Diagnostics;
using System.Net;
using System.Text.Json;
using ApolloSpoilers.Domain.Exceptions;

namespace ApolloSpoilers.Api.Middlewares;

/// <summary>
/// Catches all unhandled exceptions, maps them to the standard <see cref="ErrorResponse"/>
/// shape with appropriate HTTP status codes, and logs them.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var (status, code, message, errors) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, ve.ErrorCode, ve.Message, ve.Errors),
            NotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", exception.Message, null),
            ConflictException => (HttpStatusCode.Conflict, "CONFLICT", exception.Message, null),
            DomainException de => (HttpStatusCode.BadRequest, de.ErrorCode, exception.Message, null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", exception.Message, null),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.", null)
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", traceId);
        else
            _logger.LogWarning(exception, "Handled domain error ({Code}). TraceId={TraceId}", code, traceId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var response = new ErrorResponse
        {
            StatusCode = (int)status,
            Message = message,
            ErrorCode = code,
            TraceId = traceId,
            Errors = errors
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }
}
