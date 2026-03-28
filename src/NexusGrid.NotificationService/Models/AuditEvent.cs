namespace NexusGrid.NotificationService.Models;

public sealed class AuditEvent
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Details { get; set; } = [];
}
