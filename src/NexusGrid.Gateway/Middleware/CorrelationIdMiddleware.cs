namespace NexusGrid.Gateway.Middleware;

/// <summary>
/// Ensures every request entering the gateway has a correlation ID.
/// This ID propagates to downstream services via YARP forwarding headers,
/// enabling distributed tracing across the microservices.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(CorrelationIdHeader))
        {
            context.Request.Headers[CorrelationIdHeader] = Guid.NewGuid().ToString();
        }

        context.Response.Headers[CorrelationIdHeader] =
            context.Request.Headers[CorrelationIdHeader].ToString();

        await _next(context);
    }
}
