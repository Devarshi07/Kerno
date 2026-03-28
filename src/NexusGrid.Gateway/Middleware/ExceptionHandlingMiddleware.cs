using System.Net;
using System.Text.Json;
using NexusGrid.Shared.Models;

namespace NexusGrid.Gateway.Middleware;

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
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? context.TraceIdentifier;

            _logger.LogError(ex, "Gateway error. CorrelationId: {CorrelationId}", correlationId);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadGateway;

            var response = new ErrorResponse("Service unavailable. Please try again later.", "BAD_GATEWAY");
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
