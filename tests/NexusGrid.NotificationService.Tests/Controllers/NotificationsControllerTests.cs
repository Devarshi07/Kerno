using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NexusGrid.NotificationService.Controllers;
using NexusGrid.NotificationService.Models;
using NexusGrid.NotificationService.Services;
using Xunit;

namespace NexusGrid.NotificationService.Tests.Controllers;

public sealed class NotificationsControllerTests
{
    private readonly Mock<INotificationService> _serviceMock;
    private readonly NotificationsController _sut;

    public NotificationsControllerTests()
    {
        _serviceMock = new Mock<INotificationService>();
        _sut = new NotificationsController(_serviceMock.Object);
    }

    [Fact]
    public async Task CreateNotificationAsync_ValidRequest_Returns201()
    {
        // Arrange
        var request = new CreateNotificationRequest(
            Guid.NewGuid(), "OrderCreated", "Title", "Message", null);
        var dto = new NotificationDto(
            Guid.NewGuid(), request.UserId, "OrderCreated", "Pending",
            "Title", "Message", [], DateTime.UtcNow);

        _serviceMock.Setup(s => s.CreateNotificationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _sut.CreateNotificationAsync(request);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<NotificationDto>
        {
            new(Guid.NewGuid(), userId, "OrderCreated", "Pending", "Title", "Msg", [], DateTime.UtcNow)
        };

        _serviceMock.Setup(s => s.GetNotificationsByUserIdAsync(userId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IReadOnlyList<NotificationDto>>().Subject;
        items.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var request = new UpdateNotificationStatusRequest("Sent");

        // Act
        var result = await _sut.UpdateStatusAsync(userId, createdAt, id, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
