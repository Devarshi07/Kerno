using NexusGrid.OrderService.Models;
using NexusGrid.OrderService.Repositories;
using NexusGrid.Shared.Exceptions;
using NexusGrid.Shared.Models;

namespace NexusGrid.OrderService.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repository, ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        return MapToDto(order);
    }

    public async Task<PaginatedResponse<OrderDto>> GetOrdersAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetAllAsync(page, pageSize, cancellationToken);
        var totalCount = await _repository.GetTotalCountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<OrderDto>(
            orders.Select(MapToDto).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<PaginatedResponse<OrderDto>> GetOrdersByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        var totalCount = await _repository.GetCountByUserIdAsync(userId, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<OrderDto>(
            orders.Select(MapToDto).ToList(),
            page,
            pageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<OrderDto> CreateOrderAsync(
        CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new Shared.Exceptions.ValidationException("Order must contain at least one item.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            Status = OrderStatus.Pending,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(order, cancellationToken);

        _logger.LogInformation("Order {OrderId} created for user {UserId} with total {Total}",
            created.Id, created.UserId, created.TotalAmount);

        return MapToDto(created);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(
        Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            throw new Shared.Exceptions.ValidationException(
                $"Invalid order status: '{request.Status}'. Valid values: {string.Join(", ", Enum.GetNames<OrderStatus>())}");
        }

        order.Status = newStatus;
        await _repository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to {Status}", id, newStatus);

        return MapToDto(order);
    }

    public async Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Order {OrderId} deleted", id);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.UserId,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice
            )).ToList(),
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt
        );
    }
}
