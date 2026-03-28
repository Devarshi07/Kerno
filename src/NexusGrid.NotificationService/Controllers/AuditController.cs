using Microsoft.AspNetCore.Mvc;
using NexusGrid.NotificationService.Models;
using NexusGrid.NotificationService.Services;
using NexusGrid.Shared.Models;

namespace NexusGrid.NotificationService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuditController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public AuditController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuditEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuditEventDto>> CreateAuditEventAsync(
        [FromBody] CreateAuditEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.CreateAuditEventAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditEventDto>>> GetByTenantAsync(
        string tenantId,
        [FromQuery] string? date = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        DateOnly? queryDate = date is not null ? DateOnly.Parse(date) : null;
        var result = await _notificationService.GetAuditEventsByTenantAsync(tenantId, queryDate, limit, cancellationToken);
        return Ok(result);
    }
}
