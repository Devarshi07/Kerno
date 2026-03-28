using System.Text;
using Microsoft.Extensions.Options;
using NexusGrid.Gateway.Configuration;
using StackExchange.Redis;

namespace NexusGrid.Gateway.Middleware;

/// <summary>
/// Redis-based response caching for GET requests on cacheable paths.
///
/// Unlike in-memory caching, Redis cache is shared across gateway instances
/// (important when running multiple replicas in Kubernetes).
///
/// Cache key: "cache:{method}:{path}:{query}" — TTL from config.
/// Only caches 200 OK responses with non-empty bodies.
/// </summary>
public sealed class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CacheSettings _settings;
    private readonly ILogger<ResponseCachingMiddleware> _logger;

    public ResponseCachingMiddleware(
        RequestDelegate next,
        IOptions<CacheSettings> settings,
        ILogger<ResponseCachingMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only cache GET requests
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Only cache configured paths
        var path = context.Request.Path.Value ?? "";
        if (!_settings.CacheablePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var redis = context.RequestServices.GetService<IConnectionMultiplexer>();
        if (redis is null || !redis.IsConnected)
        {
            await _next(context);
            return;
        }

        var db = redis.GetDatabase();
        var cacheKey = BuildCacheKey(context.Request);

        // Try cache hit
        var cached = await db.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            _logger.LogDebug("Cache HIT for {CacheKey}", cacheKey);
            context.Response.Headers["X-Cache"] = "HIT";
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cached.ToString());
            return;
        }

        // Cache miss — capture the response
        context.Response.Headers["X-Cache"] = "MISS";

        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        // Only cache successful responses
        if (context.Response.StatusCode == 200 && !string.IsNullOrEmpty(responseBody))
        {
            await db.StringSetAsync(cacheKey, responseBody, TimeSpan.FromSeconds(_settings.DefaultTtlSeconds));
            _logger.LogDebug("Cached response for {CacheKey} with TTL {Ttl}s", cacheKey, _settings.DefaultTtlSeconds);
        }

        // Write the response back to the original stream
        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

    private static string BuildCacheKey(HttpRequest request)
    {
        var sb = new StringBuilder("cache:");
        sb.Append(request.Method);
        sb.Append(':');
        sb.Append(request.Path);
        if (request.QueryString.HasValue)
        {
            sb.Append(request.QueryString.Value);
        }
        return sb.ToString();
    }
}
