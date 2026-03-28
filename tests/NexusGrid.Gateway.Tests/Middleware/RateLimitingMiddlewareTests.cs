using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NexusGrid.Gateway.Configuration;
using NexusGrid.Gateway.Middleware;
using StackExchange.Redis;
using Xunit;

namespace NexusGrid.Gateway.Tests.Middleware;

public sealed class RateLimitingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoRedis_PassesThrough()
    {
        // Arrange — no Redis registered, should fail-open
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };

        var settings = Options.Create(new RateLimitSettings { MaxRequests = 10, WindowSeconds = 60 });
        var logger = Mock.Of<ILogger<RateLimitingMiddleware>>();

        var middleware = new RateLimitingMiddleware(next, settings, logger);

        var context = new DefaultHttpContext();
        context.RequestServices = new ServiceCollection().BuildServiceProvider();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_RedisDisconnected_PassesThrough()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };

        var settings = Options.Create(new RateLimitSettings { MaxRequests = 10, WindowSeconds = 60 });
        var logger = Mock.Of<ILogger<RateLimitingMiddleware>>();

        var redisMock = new Mock<IConnectionMultiplexer>();
        redisMock.Setup(r => r.IsConnected).Returns(false);

        var services = new ServiceCollection();
        services.AddSingleton(redisMock.Object);

        var middleware = new RateLimitingMiddleware(next, settings, logger);

        var context = new DefaultHttpContext();
        context.RequestServices = services.BuildServiceProvider();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }
}
