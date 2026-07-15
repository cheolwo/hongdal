namespace 홍달.Services.Options;

public sealed class CommunityKeywordNotificationOptions
{
    public const string SectionName = "CommunityKeywordNotifications";

    public bool Enabled { get; set; } = true;
    public int PollingIntervalSeconds { get; set; } = 10;
    public int BatchSize { get; set; } = 50;
    public int MaxAttempts { get; set; } = 5;
    public int RetryDelaySeconds { get; set; } = 30;
    public int LeaseTimeoutMinutes { get; set; } = 5;
    public int MaxSubscriptionsPerUser { get; set; } = 50;
}
