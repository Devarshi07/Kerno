using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NexusGrid.OrderService.Models;
using NexusGrid.OrderService.Repositories;
using NexusGrid.OrderService.Services;
using NexusGrid.Shared.Exceptions;
using Xunit;

namespace NexusGrid.OrderService.Tests.Services;

public sealed class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<NexusGrid.OrderService.Services.OrderService>> _loggerMock;
    private readonly IOrderService _sut;

    public OrderServiceTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<NexusGrid.OrderService.Services.OrderService>>();
        _sut = new NexusGrid.OrderService.Services.OrderService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ExistingOrder_ReturnsOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateSampleOrder(orderId);
        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.GetOrderByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.Status.Should().Be("Pending");
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrderByIdAsync_NonExistingOrder_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var act = () => _sut.GetOrderByIdAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ReturnsCreatedOrder()
    {
        // Arrange
        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            [new OrderItemDto("PROD-1", "Widget", 2, 9.99m)]
        );

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) => o);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(request.UserId);
        result.TotalAmount.Should().Be(19.98m);
        result.Status.Should().Be("Pending");
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOrderAsync_EmptyItems_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateOrderRequest(Guid.NewGuid(), []);

        // Act
        var act = () => _sut.CreateOrderAsync(request);

        // Assert
        await act.Should().ThrowAsync<Shared.Exceptions.ValidationException>()
            .WithMessage("*at least one item*");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidStatus_ReturnsUpdatedOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateSampleOrder(orderId);
        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var request = new UpdateOrderStatusRequest("Confirmed");

        // Act
        var result = await _sut.UpdateOrderStatusAsync(orderId, request);

        // Assert
        result.Status.Should().Be("Confirmed");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_InvalidStatus_ThrowsValidationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateSampleOrder(orderId);
        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var request = new UpdateOrderStatusRequest("InvalidStatus");

        // Act
        var act = () => _sut.UpdateOrderStatusAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<Shared.Exceptions.ValidationException>()
            .WithMessage("*Invalid order status*");
    }

    [Fact]
    public async Task DeleteOrderAsync_ExistingOrder_CallsRepository()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        await _sut.DeleteOrderAsync(orderId);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsPagedResults()
    {
        // Arrange
        var orders = new List<Order> { CreateSampleOrder(Guid.NewGuid()), CreateSampleOrder(Guid.NewGuid()) };
        _repositoryMock.Setup(r => r.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);
        _repositoryMock.Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _sut.GetOrdersAsync(1, 20);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.TotalCount.Should().Be(2);
    }

    private static Order CreateSampleOrder(Guid id)
    {
        return new Order
        {
            Id = id,
            UserId = Guid.NewGuid(),
            Items =
            [
                new OrderItem
                {
                    ProductId = "PROD-1",
                    ProductName = "Widget",
                    Quantity = 1,
                    UnitPrice = 9.99m
                }
            ],
            Status = OrderStatus.Pending,
            TotalAmount = 9.99m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
