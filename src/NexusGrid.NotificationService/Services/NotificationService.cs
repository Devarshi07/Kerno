using NexusGrid.NotificationService.Models;
using NexusGrid.NotificationService.Repositories;

namespace NexusGrid.NotificationService.Services;

public sealed class NotificationService : INotificationService
{
    private readonly ICassandraRepository _repository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ICassandraRepository repository, ILogger<NotificationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new Shared.Exceptions.ValidationException("Notification title is required.");

        if (!Enum.TryParse<NotificationType>(request.Type, ignoreCase: true, out _))
            throw new Shared.Exceptions.ValidationException(
                $"Invalid notification type: '{request.Type}'. Valid values: {string.Join(", ", Enum.GetNames<NotificationType>())}");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            Status = NotificationStatus.Pending.ToString(),
            Title = request.Title,
            Message = request.Message,
            Metadata = request.Metadata ?? [],
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateNotificationAsync(notification, cancellationToken);

        _logger.LogInformation("Notification {NotificationId} of type {Type} created for user {UserId}",
            created.Id, created.Type, created.UserId);

        return MapToDto(created);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsByUserIdAsync(
        Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var notifications = await _repository.GetNotificationsByUserIdAsync(userId, limit, cancellationToken);
        return notifications.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsByStatusAsync(
        string status, DateOnly? date = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var queryDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var notifications = await _repository.GetNotificationsByStatusAsync(status, queryDate, limit, cancellationToken);
        return notifications.Select(MapToDto).ToList();
    }

    public async Task UpdateNotificationStatusAsync(
        Guid userId, DateTime createdAt, Guid id, UpdateNotificationStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationStatus>(request.Status, ignoreCase: true, out _))
            throw new Shared.Exceptions.ValidationException(
                $"Invalid status: '{request.Status}'. Valid values: {string.Join(", ", Enum.GetNames<NotificationStatus>())}");

        await _repository.UpdateNotificationStatusAsync(userId, createdAt, id, request.Status, cancellationToken);

        _logger.LogInformation("Notification {NotificationId} status updated to {Status}", id, request.Status);
    }

    public async Task<AuditEventDto> CreateAuditEventAsync(
        CreateAuditEventRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TenantId))
            throw new Shared.Exceptions.ValidationException("TenantId is required.");

        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EventTime = DateTime.UtcNow,
            EventType = request.EventType,
            ActorId = request.ActorId,
            ResourceType = request.ResourceType,
            ResourceId = request.ResourceId,
            Description = request.Description,
            Details = request.Details ?? []
        };

        var created = await _repository.CreateAuditEventAsync(auditEvent, cancellationToken);

        return MapToDto(created);
    }

    public async Task<IReadOnlyList<AuditEventDto>> GetAuditEventsByTenantAsync(
        string tenantId, DateOnly? date = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var queryDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var events = await _repository.GetAuditEventsByTenantAsync(tenantId, queryDate, limit, cancellationToken);
        return events.Select(MapToDto).ToList();
    }

    private static NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto(n.Id, n.UserId, n.Type, n.Status, n.Title, n.Message, n.Metadata, n.CreatedAt);
    }

    private static AuditEventDto MapToDto(AuditEvent e)
    {
        return new AuditEventDto(e.Id, e.TenantId, e.EventDate.ToString("yyyy-MM-dd"),
            e.EventTime, e.EventType, e.ActorId, e.ResourceType, e.ResourceId, e.Description, e.Details);
    }
}
