using Microsoft.AspNetCore.Mvc;
using NexusGrid.NotificationService.Models;
using NexusGrid.NotificationService.Services;
using NexusGrid.Shared.Models;

namespace NexusGrid.NotificationService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationDto>> CreateNotificationAsync(
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.CreateNotificationAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetByUserIdAsync(
        Guid userId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetNotificationsByUserIdAsync(userId, limit, cancellationToken);
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetByStatusAsync(
        string status,
        [FromQuery] string? date = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        DateOnly? queryDate = date is not null ? DateOnly.Parse(date) : null;
        var result = await _notificationService.GetNotificationsByStatusAsync(status, queryDate, limit, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{userId:guid}/{createdAt:datetime}/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateStatusAsync(
        Guid userId,
        DateTime createdAt,
        Guid id,
        [FromBody] UpdateNotificationStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _notificationService.UpdateNotificationStatusAsync(userId, createdAt, id, request, cancellationToken);
        return NoContent();
    }
}
