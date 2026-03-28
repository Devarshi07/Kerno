using Microsoft.AspNetCore.Mvc;
using NexusGrid.OrderService.Models;
using NexusGrid.OrderService.Services;
using NexusGrid.Shared.Models;

namespace NexusGrid.OrderService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<OrderDto>>> GetOrdersAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetOrdersAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(PaginatedResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<OrderDto>>> GetOrdersByUserIdAsync(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetOrdersByUserIdAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrderAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderByIdAsync), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatusAsync(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteOrderAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _orderService.DeleteOrderAsync(id, cancellationToken);
        return NoContent();
    }
}
