namespace NexusGrid.NotificationService.Repositories;

public sealed class CassandraSettings
{
    public const string SectionName = "Cassandra";

    public string ContactPoint { get; set; } = "localhost";
    public int Port { get; set; } = 9042;
    public string Keyspace { get; set; } = "nexusgrid";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
