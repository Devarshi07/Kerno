namespace NexusGrid.OrderService.Models;

public sealed record OrderDto(
    Guid Id,
    Guid UserId,
    List<OrderItemDto> Items,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record OrderItemDto(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public sealed record CreateOrderRequest(
    Guid UserId,
    List<OrderItemDto> Items
);

public sealed record UpdateOrderStatusRequest(
    string Status
);
