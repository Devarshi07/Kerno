using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NexusGrid.Gateway.Configuration;
using NexusGrid.Shared.Models;
using StackExchange.Redis;

namespace NexusGrid.Gateway.Middleware;

/// <summary>
/// Sliding window rate limiter using Redis sorted sets.
///
/// How it works (like a FastAPI rate limiter but at the gateway level):
///   1. Each client gets a Redis sorted set keyed by their IP/API key
///   2. Each request adds a timestamped entry
///   3. Old entries outside the window are pruned
///   4. If count > max, return 429 Too Many Requests
///
/// Redis sorted sets give O(log N) operations and automatic deduplication.
/// The sliding window is more fair than fixed windows (no burst at boundaries).
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitSettings _settings;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IOptions<RateLimitSettings> settings,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var redis = context.RequestServices.GetService<IConnectionMultiplexer>();

        // If Redis is unavailable, allow the request through (fail-open)
        if (redis is null || !redis.IsConnected)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var key = $"ratelimit:{clientId}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (_settings.WindowSeconds * 1000);

        var db = redis.GetDatabase();

        // Lua script for atomic sliding window — all operations in one round trip
        var script = @"
            redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', ARGV[1])
            redis.call('ZADD', KEYS[1], ARGV[2], ARGV[3])
            redis.call('EXPIRE', KEYS[1], ARGV[4])
            return redis.call('ZCARD', KEYS[1])
        ";

        var requestCount = (long)await db.ScriptEvaluateAsync(
            script,
            new RedisKey[] { key },
            new RedisValue[] { windowStart, now, $"{now}:{Guid.NewGuid()}", _settings.WindowSeconds * 2 }
        );

        // Add rate limit headers (standard convention)
        context.Response.Headers["X-RateLimit-Limit"] = _settings.MaxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, _settings.MaxRequests - requestCount).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow
            .AddSeconds(_settings.WindowSeconds).ToUnixTimeSeconds().ToString();

        if (requestCount > _settings.MaxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}. Count: {Count}/{Max}",
                clientId, requestCount, _settings.MaxRequests);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = _settings.WindowSeconds.ToString();

            var response = new ErrorResponse("Rate limit exceeded. Try again later.", "RATE_LIMIT_EXCEEDED");
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Prefer API key header, fall back to IP address
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
            return $"key:{apiKey}";

        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = forwarded ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }
}
