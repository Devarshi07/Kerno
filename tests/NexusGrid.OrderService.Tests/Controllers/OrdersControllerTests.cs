using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NexusGrid.OrderService.Controllers;
using NexusGrid.OrderService.Models;
using NexusGrid.OrderService.Services;
using NexusGrid.Shared.Models;
using Xunit;

namespace NexusGrid.OrderService.Tests.Controllers;

public sealed class OrdersControllerTests
{
    private readonly Mock<IOrderService> _serviceMock;
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _serviceMock = new Mock<IOrderService>();
        _sut = new OrdersController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ExistingOrder_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = CreateSampleDto(orderId);
        _serviceMock.Setup(s => s.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _sut.GetOrderByIdAsync(orderId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        dto.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            [new OrderItemDto("PROD-1", "Widget", 2, 9.99m)]
        );
        var createdDto = CreateSampleDto(Guid.NewGuid());
        _serviceMock.Setup(s => s.CreateOrderAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task DeleteOrderAsync_ReturnsNoContent()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = await _sut.DeleteOrderAsync(orderId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsPaginatedOk()
    {
        // Arrange
        var response = new PaginatedResponse<OrderDto>(
            [CreateSampleDto(Guid.NewGuid())], 1, 20, 1, 1);
        _serviceMock.Setup(s => s.GetOrdersAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetOrdersAsync();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var paginated = okResult.Value.Should().BeOfType<PaginatedResponse<OrderDto>>().Subject;
        paginated.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest("Confirmed");
        var dto = CreateSampleDto(orderId) with { Status = "Confirmed" };
        _serviceMock.Setup(s => s.UpdateOrderStatusAsync(orderId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _sut.UpdateOrderStatusAsync(orderId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var updated = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        updated.Status.Should().Be("Confirmed");
    }

    private static OrderDto CreateSampleDto(Guid id)
    {
        return new OrderDto(
            id,
            Guid.NewGuid(),
            [new OrderItemDto("PROD-1", "Widget", 1, 9.99m)],
            "Pending",
            9.99m,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
    }
}
