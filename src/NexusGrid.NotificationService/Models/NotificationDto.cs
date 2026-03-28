namespace NexusGrid.NotificationService.Models;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Status,
    string Title,
    string Message,
    Dictionary<string, string> Metadata,
    DateTime CreatedAt
);

public sealed record CreateNotificationRequest(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    Dictionary<string, string>? Metadata = null
);

public sealed record UpdateNotificationStatusRequest(
    string Status
);

public sealed record AuditEventDto(
    Guid Id,
    string TenantId,
    string EventDate,
    DateTime EventTime,
    string EventType,
    string ActorId,
    string ResourceType,
    string ResourceId,
    string Description,
    Dictionary<string, string> Details
);

public sealed record CreateAuditEventRequest(
    string TenantId,
    string EventType,
    string ActorId,
    string ResourceType,
    string ResourceId,
    string Description,
    Dictionary<string, string>? Details = null
);
