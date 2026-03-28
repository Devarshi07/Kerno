using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NexusGrid.Gateway.Middleware;
using Xunit;

namespace NexusGrid.Gateway.Tests.Middleware;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoCorrelationId_GeneratesOne()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Request.Headers["X-Correlation-Id"].ToString().Should().NotBeNullOrEmpty();
        context.Response.Headers["X-Correlation-Id"].ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ExistingCorrelationId_PreservesIt()
    {
        // Arrange
        var existingId = "test-correlation-123";
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = existingId;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Request.Headers["X-Correlation-Id"].ToString().Should().Be(existingId);
        context.Response.Headers["X-Correlation-Id"].ToString().Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_ResponseHeaderMatchesRequest()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var requestId = context.Request.Headers["X-Correlation-Id"].ToString();
        var responseId = context.Response.Headers["X-Correlation-Id"].ToString();
        requestId.Should().Be(responseId);
    }
}
