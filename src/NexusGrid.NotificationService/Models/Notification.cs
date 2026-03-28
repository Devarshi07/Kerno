namespace NexusGrid.NotificationService.Models;

public enum NotificationType
{
    OrderCreated,
    OrderStatusChanged,
    OrderCancelled,
    AccountCreated,
    PasswordReset,
    System
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Read
}

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = NotificationStatus.Pending.ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
