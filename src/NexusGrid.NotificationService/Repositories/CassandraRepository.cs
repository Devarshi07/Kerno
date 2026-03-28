using Cassandra;
using NexusGrid.NotificationService.Models;

namespace NexusGrid.NotificationService.Repositories;

/// <summary>
/// Cassandra repository using query-first schema design with prepared statements.
///
/// Tables are denormalized around read patterns (not entity relationships):
///   - notifications_by_user: partition=user_id, clustering=created_at DESC → "get my notifications"
///   - notifications_by_status: partition=(status, date), clustering=created_at DESC → "get pending notifications today"
///   - audit_events_by_tenant: partition=(tenant_id, event_date), clustering=event_time DESC → "get today's audit trail"
///
/// This is fundamentally different from PostgreSQL/EF Core:
///   - No JOINs, no foreign keys, no ACID transactions
///   - Data is duplicated across tables (written to multiple tables per operation)
///   - Consistency: LOCAL_ONE for both reads and writes (AP in CAP theorem)
///   - Prepared statements prevent CQL injection and improve performance
/// </summary>
public sealed class CassandraRepository : ICassandraRepository
{
    private readonly Cassandra.ISession _session;
    private readonly ILogger<CassandraRepository> _logger;

    // Prepared statements — compiled once, reused for every query (like parameterized SQL)
    private PreparedStatement? _insertNotificationByUser;
    private PreparedStatement? _insertNotificationByStatus;
    private PreparedStatement? _selectNotificationsByUser;
    private PreparedStatement? _selectNotificationsByStatus;
    private PreparedStatement? _updateNotificationStatusByUser;
    private PreparedStatement? _insertAuditEvent;
    private PreparedStatement? _selectAuditEventsByTenant;

    public CassandraRepository(Cassandra.ISession session, ILogger<CassandraRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task InitializeSchemaAsync(CancellationToken cancellationToken = default)
    {
        // notifications_by_user: "Show me all notifications for user X, newest first"
        await _session.ExecuteAsync(new SimpleStatement(@"
            CREATE TABLE IF NOT EXISTS notifications_by_user (
                user_id uuid,
                created_at timestamp,
                id uuid,
                type text,
                status text,
                title text,
                message text,
                metadata map<text, text>,
                PRIMARY KEY (user_id, created_at, id)
            ) WITH CLUSTERING ORDER BY (created_at DESC, id ASC)
        "));

        // notifications_by_status: "Show me all pending notifications for today"
        await _session.ExecuteAsync(new SimpleStatement(@"
            CREATE TABLE IF NOT EXISTS notifications_by_status (
                status text,
                date text,
                created_at timestamp,
                id uuid,
                user_id uuid,
                type text,
                title text,
                message text,
                metadata map<text, text>,
                PRIMARY KEY ((status, date), created_at, id)
            ) WITH CLUSTERING ORDER BY (created_at DESC, id ASC)
        "));

        // audit_events_by_tenant: "Show me today's audit trail for tenant X"
        await _session.ExecuteAsync(new SimpleStatement(@"
            CREATE TABLE IF NOT EXISTS audit_events_by_tenant (
                tenant_id text,
                event_date text,
                event_time timestamp,
                id uuid,
                event_type text,
                actor_id text,
                resource_type text,
                resource_id text,
                description text,
                details map<text, text>,
                PRIMARY KEY ((tenant_id, event_date), event_time, id)
            ) WITH CLUSTERING ORDER BY (event_time DESC, id ASC)
        "));

        await PrepareStatementsAsync();

        _logger.LogInformation("Cassandra schema initialized and statements prepared");
    }

    private async Task PrepareStatementsAsync()
    {
        _insertNotificationByUser = await _session.PrepareAsync(@"
            INSERT INTO notifications_by_user (user_id, created_at, id, type, status, title, message, metadata)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        ");

        _insertNotificationByStatus = await _session.PrepareAsync(@"
            INSERT INTO notifications_by_status (status, date, created_at, id, user_id, type, title, message, metadata)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        ");

        _selectNotificationsByUser = await _session.PrepareAsync(@"
            SELECT user_id, created_at, id, type, status, title, message, metadata
            FROM notifications_by_user
            WHERE user_id = ?
            LIMIT ?
        ");

        _selectNotificationsByStatus = await _session.PrepareAsync(@"
            SELECT status, date, created_at, id, user_id, type, title, message, metadata
            FROM notifications_by_status
            WHERE status = ? AND date = ?
            LIMIT ?
        ");

        _updateNotificationStatusByUser = await _session.PrepareAsync(@"
            UPDATE notifications_by_user SET status = ?
            WHERE user_id = ? AND created_at = ? AND id = ?
        ");

        _insertAuditEvent = await _session.PrepareAsync(@"
            INSERT INTO audit_events_by_tenant (tenant_id, event_date, event_time, id, event_type, actor_id, resource_type, resource_id, description, details)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ");

        _selectAuditEventsByTenant = await _session.PrepareAsync(@"
            SELECT tenant_id, event_date, event_time, id, event_type, actor_id, resource_type, resource_id, description, details
            FROM audit_events_by_tenant
            WHERE tenant_id = ? AND event_date = ?
            LIMIT ?
        ");
    }

    public async Task<Notification> CreateNotificationAsync(
        Notification notification, CancellationToken cancellationToken = default)
    {
        notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
        notification.CreatedAt = DateTime.UtcNow;

        var dateStr = DateOnly.FromDateTime(notification.CreatedAt).ToString("yyyy-MM-dd");

        // Write to both tables (denormalized — same data, different access patterns)
        var batch = new BatchStatement()
            .Add(_insertNotificationByUser!.Bind(
                notification.UserId, notification.CreatedAt, notification.Id,
                notification.Type, notification.Status, notification.Title,
                notification.Message, notification.Metadata))
            .Add(_insertNotificationByStatus!.Bind(
                notification.Status, dateStr, notification.CreatedAt, notification.Id,
                notification.UserId, notification.Type, notification.Title,
                notification.Message, notification.Metadata));

        await _session.ExecuteAsync(batch);

        _logger.LogInformation("Notification {NotificationId} created for user {UserId}",
            notification.Id, notification.UserId);

        return notification;
    }

    public async Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(
        Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        var rs = await _session.ExecuteAsync(_selectNotificationsByUser!.Bind(userId, limit));
        return MapNotifications(rs);
    }

    public async Task<IReadOnlyList<Notification>> GetNotificationsByStatusAsync(
        string status, DateOnly date, int limit, CancellationToken cancellationToken = default)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        var rs = await _session.ExecuteAsync(_selectNotificationsByStatus!.Bind(status, dateStr, limit));

        return rs.Select(row => new Notification
        {
            Id = row.GetValue<Guid>("id"),
            UserId = row.GetValue<Guid>("user_id"),
            Type = row.GetValue<string>("type"),
            Status = row.GetValue<string>("status"),
            Title = row.GetValue<string>("title"),
            Message = row.GetValue<string>("message"),
            Metadata = row.GetValue<IDictionary<string, string>>("metadata")?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? [],
            CreatedAt = row.GetValue<DateTimeOffset>("created_at").UtcDateTime
        }).ToList();
    }

    public async Task UpdateNotificationStatusAsync(
        Guid userId, DateTime createdAt, Guid id, string status, CancellationToken cancellationToken = default)
    {
        await _session.ExecuteAsync(_updateNotificationStatusByUser!.Bind(status, userId, createdAt, id));

        _logger.LogInformation("Notification {NotificationId} status updated to {Status}", id, status);
    }

    public async Task<AuditEvent> CreateAuditEventAsync(
        AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        auditEvent.Id = auditEvent.Id == Guid.Empty ? Guid.NewGuid() : auditEvent.Id;
        auditEvent.EventTime = DateTime.UtcNow;
        auditEvent.EventDate = DateOnly.FromDateTime(auditEvent.EventTime);

        var dateStr = auditEvent.EventDate.ToString("yyyy-MM-dd");

        await _session.ExecuteAsync(_insertAuditEvent!.Bind(
            auditEvent.TenantId, dateStr, auditEvent.EventTime, auditEvent.Id,
            auditEvent.EventType, auditEvent.ActorId, auditEvent.ResourceType,
            auditEvent.ResourceId, auditEvent.Description, auditEvent.Details));

        _logger.LogInformation("Audit event {EventId} created for tenant {TenantId}",
            auditEvent.Id, auditEvent.TenantId);

        return auditEvent;
    }

    public async Task<IReadOnlyList<AuditEvent>> GetAuditEventsByTenantAsync(
        string tenantId, DateOnly eventDate, int limit, CancellationToken cancellationToken = default)
    {
        var dateStr = eventDate.ToString("yyyy-MM-dd");
        var rs = await _session.ExecuteAsync(_selectAuditEventsByTenant!.Bind(tenantId, dateStr, limit));

        return rs.Select(row => new AuditEvent
        {
            Id = row.GetValue<Guid>("id"),
            TenantId = row.GetValue<string>("tenant_id"),
            EventDate = DateOnly.Parse(row.GetValue<string>("event_date")),
            EventTime = row.GetValue<DateTimeOffset>("event_time").UtcDateTime,
            EventType = row.GetValue<string>("event_type"),
            ActorId = row.GetValue<string>("actor_id"),
            ResourceType = row.GetValue<string>("resource_type"),
            ResourceId = row.GetValue<string>("resource_id"),
            Description = row.GetValue<string>("description"),
            Details = row.GetValue<IDictionary<string, string>>("details")?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? []
        }).ToList();
    }

    private static List<Notification> MapNotifications(RowSet rs)
    {
        return rs.Select(row => new Notification
        {
            Id = row.GetValue<Guid>("id"),
            UserId = row.GetValue<Guid>("user_id"),
            Type = row.GetValue<string>("type"),
            Status = row.GetValue<string>("status"),
            Title = row.GetValue<string>("title"),
            Message = row.GetValue<string>("message"),
            Metadata = row.GetValue<IDictionary<string, string>>("metadata")?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? [],
            CreatedAt = row.GetValue<DateTimeOffset>("created_at").UtcDateTime
        }).ToList();
    }
}
