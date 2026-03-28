using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NexusGrid.Gateway.Middleware;
using Xunit;

namespace NexusGrid.Gateway.Tests.Middleware;

public sealed class AuthMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PublicPath_SkipsAuth()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var logger = Mock.Of<ILogger<AuthMiddleware>>();
        var middleware = new AuthMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/auth/login";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().NotBe(401);
    }

    [Fact]
    public async Task InvokeAsync_HealthEndpoint_SkipsAuth()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var logger = Mock.Of<ILogger<AuthMiddleware>>();
        var middleware = new AuthMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UnauthenticatedProtectedPath_Returns401()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var logger = Mock.Of<ILogger<AuthMiddleware>>();
        var middleware = new AuthMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/orders";
        // No authenticated user

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_PassesThrough()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var logger = Mock.Of<ILogger<AuthMiddleware>>();
        var middleware = new AuthMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/orders";

        // Simulate authenticated user
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_RegisterEndpoint_SkipsAuth()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var logger = Mock.Of<ILogger<AuthMiddleware>>();
        var middleware = new AuthMiddleware(next, logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/auth/register";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }
}
