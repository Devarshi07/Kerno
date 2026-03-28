using NexusGrid.NotificationService.Models;

namespace NexusGrid.NotificationService.Repositories;

public interface ICassandraRepository
{
    // Notifications
    Task<Notification> CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(Guid userId, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetNotificationsByStatusAsync(string status, DateOnly date, int limit, CancellationToken cancellationToken = default);
    Task UpdateNotificationStatusAsync(Guid userId, DateTime createdAt, Guid id, string status, CancellationToken cancellationToken = default);

    // Audit Events
    Task<AuditEvent> CreateAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEvent>> GetAuditEventsByTenantAsync(string tenantId, DateOnly eventDate, int limit, CancellationToken cancellationToken = default);

    // Schema
    Task InitializeSchemaAsync(CancellationToken cancellationToken = default);
}
