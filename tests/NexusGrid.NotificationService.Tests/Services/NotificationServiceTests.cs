using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NexusGrid.NotificationService.Models;
using NexusGrid.NotificationService.Repositories;
using NexusGrid.NotificationService.Services;
using NexusGrid.Shared.Exceptions;
using Xunit;

namespace NexusGrid.NotificationService.Tests.Services;

public sealed class NotificationServiceTests
{
    private readonly Mock<ICassandraRepository> _repositoryMock;
    private readonly Mock<ILogger<NexusGrid.NotificationService.Services.NotificationService>> _loggerMock;
    private readonly INotificationService _sut;

    public NotificationServiceTests()
    {
        _repositoryMock = new Mock<ICassandraRepository>();
        _loggerMock = new Mock<ILogger<NexusGrid.NotificationService.Services.NotificationService>>();
        _sut = new NexusGrid.NotificationService.Services.NotificationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateNotificationAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateNotificationRequest(
            Guid.NewGuid(), "OrderCreated", "New Order", "Your order has been placed.", null);

        _repositoryMock.Setup(r => r.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        // Act
        var result = await _sut.CreateNotificationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId);
        result.Type.Should().Be("OrderCreated");
        result.Status.Should().Be("Pending");
        result.Title.Should().Be("New Order");
    }

    [Fact]
    public async Task CreateNotificationAsync_EmptyTitle_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateNotificationRequest(Guid.NewGuid(), "OrderCreated", "", "body", null);

        // Act
        var act = () => _sut.CreateNotificationAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*title is required*");
    }

    [Fact]
    public async Task CreateNotificationAsync_InvalidType_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateNotificationRequest(Guid.NewGuid(), "InvalidType", "Title", "body", null);

        // Act
        var act = () => _sut.CreateNotificationAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid notification type*");
    }

    [Fact]
    public async Task GetNotificationsByUserIdAsync_ReturnsNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            CreateSampleNotification(userId),
            CreateSampleNotification(userId)
        };

        _repositoryMock.Setup(r => r.GetNotificationsByUserIdAsync(userId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _sut.GetNotificationsByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(n => n.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetNotificationsByStatusAsync_ReturnsNotifications()
    {
        // Arrange
        var notifications = new List<Notification> { CreateSampleNotification(Guid.NewGuid()) };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _repositoryMock.Setup(r => r.GetNotificationsByStatusAsync("Pending", today, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _sut.GetNotificationsByStatusAsync("Pending", today);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateNotificationStatusAsync_InvalidStatus_ThrowsValidationException()
    {
        // Arrange
        var request = new UpdateNotificationStatusRequest("BadStatus");

        // Act
        var act = () => _sut.UpdateNotificationStatusAsync(
            Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid status*");
    }

    [Fact]
    public async Task UpdateNotificationStatusAsync_ValidStatus_CallsRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var request = new UpdateNotificationStatusRequest("Sent");

        // Act
        await _sut.UpdateNotificationStatusAsync(userId, createdAt, id, request);

        // Assert
        _repositoryMock.Verify(r => r.UpdateNotificationStatusAsync(
            userId, createdAt, id, "Sent", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAuditEventAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateAuditEventRequest(
            "tenant-1", "OrderCreated", "user-123", "Order", "order-456", "Order was created", null);

        _repositoryMock.Setup(r => r.CreateAuditEventAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditEvent e, CancellationToken _) => e);

        // Act
        var result = await _sut.CreateAuditEventAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be("tenant-1");
        result.EventType.Should().Be("OrderCreated");
        result.ActorId.Should().Be("user-123");
    }

    [Fact]
    public async Task CreateAuditEventAsync_EmptyTenantId_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateAuditEventRequest("", "OrderCreated", "user-1", "Order", "1", "desc", null);

        // Act
        var act = () => _sut.CreateAuditEventAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*TenantId is required*");
    }

    [Fact]
    public async Task GetAuditEventsByTenantAsync_ReturnsEvents()
    {
        // Arrange
        var events = new List<AuditEvent>
        {
            new()
            {
                Id = Guid.NewGuid(), TenantId = "tenant-1",
                EventDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EventTime = DateTime.UtcNow, EventType = "OrderCreated",
                ActorId = "user-1", ResourceType = "Order", ResourceId = "1",
                Description = "Created"
            }
        };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        _repositoryMock.Setup(r => r.GetAuditEventsByTenantAsync("tenant-1", today, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _sut.GetAuditEventsByTenantAsync("tenant-1", today);

        // Assert
        result.Should().HaveCount(1);
        result[0].TenantId.Should().Be("tenant-1");
    }

    private static Notification CreateSampleNotification(Guid userId)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = "OrderCreated",
            Status = "Pending",
            Title = "New Order",
            Message = "Your order was placed",
            Metadata = new Dictionary<string, string> { ["orderId"] = Guid.NewGuid().ToString() },
            CreatedAt = DateTime.UtcNow
        };
    }
}
