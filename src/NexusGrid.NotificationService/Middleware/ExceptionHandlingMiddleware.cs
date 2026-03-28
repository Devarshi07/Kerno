using System.Net;
using System.Text.Json;
using NexusGrid.Shared.Exceptions;
using NexusGrid.Shared.Models;

namespace NexusGrid.NotificationService.Middleware;

public sealed class ExceptionHandlingMiddleware
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
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? context.TraceIdentifier;

        var (statusCode, response) = exception switch
        {
            NotFoundException notFound => (
                HttpStatusCode.NotFound,
                new ErrorResponse(notFound.Message, notFound.ErrorCode)
            ),
            Shared.Exceptions.ValidationException validation => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(validation.Message, validation.ErrorCode, validation.Errors)
            ),
            ConflictException conflict => (
                HttpStatusCode.Conflict,
                new ErrorResponse(conflict.Message, conflict.ErrorCode)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("An unexpected error occurred.", "INTERNAL_ERROR")
            )
        };

        _logger.LogError(exception,
            "Unhandled exception. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}",
            correlationId, (int)statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
