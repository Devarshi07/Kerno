using System.Net;
using System.Text.Json;
using NexusGrid.Shared.Models;

namespace NexusGrid.Gateway.Middleware;

/// <summary>
/// Gateway-level auth middleware that validates JWT tokens before YARP forwards requests.
///
/// Public routes (login, register, health) bypass auth.
/// All other routes require a valid Bearer token.
///
/// This is different from the UserService's [Authorize] attribute:
///   - Gateway auth = "is this a valid token?" (perimeter security)
///   - Service auth = "does this user have the right role?" (fine-grained RBAC)
/// </summary>
public sealed class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthMiddleware> _logger;

    private static readonly string[] PublicPaths =
    [
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/health",
        "/swagger",
        "/metrics"
    ];

    public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip auth for public routes
        if (PublicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Let the built-in JWT middleware handle validation.
        // If the user is not authenticated after JWT middleware ran, reject.
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            _logger.LogWarning("Unauthorized request to {Path} from {IP}",
                path, context.Connection.RemoteIpAddress);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            var response = new ErrorResponse("Authentication required.", "UNAUTHORIZED");
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
            return;
        }

        await _next(context);
    }
}
