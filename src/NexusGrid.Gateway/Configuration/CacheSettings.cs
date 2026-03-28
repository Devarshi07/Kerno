namespace NexusGrid.Gateway.Configuration;

public sealed class CacheSettings
{
    public const string SectionName = "Cache";

    public int DefaultTtlSeconds { get; set; } = 30;
    public string[] CacheablePaths { get; set; } = ["/api/v1/orders"];
}
