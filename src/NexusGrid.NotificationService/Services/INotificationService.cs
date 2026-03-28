using NexusGrid.NotificationService.Models;

namespace NexusGrid.NotificationService.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDto>> GetNotificationsByUserIdAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDto>> GetNotificationsByStatusAsync(string status, DateOnly? date = null, int limit = 50, CancellationToken cancellationToken = default);
    Task UpdateNotificationStatusAsync(Guid userId, DateTime createdAt, Guid id, UpdateNotificationStatusRequest request, CancellationToken cancellationToken = default);
    Task<AuditEventDto> CreateAuditEventAsync(CreateAuditEventRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditEventDto>> GetAuditEventsByTenantAsync(string tenantId, DateOnly? date = null, int limit = 50, CancellationToken cancellationToken = default);
}
