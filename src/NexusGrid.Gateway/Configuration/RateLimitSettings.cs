namespace NexusGrid.Gateway.Configuration;

public sealed class RateLimitSettings
{
    public const string SectionName = "RateLimit";

    public int WindowSeconds { get; set; } = 60;
    public int MaxRequests { get; set; } = 100;
}
