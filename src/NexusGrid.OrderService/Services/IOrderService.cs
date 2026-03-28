using NexusGrid.OrderService.Models;
using NexusGrid.Shared.Models;

namespace NexusGrid.OrderService.Services;

public interface IOrderService
{
    Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<OrderDto>> GetOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<OrderDto>> GetOrdersByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}
