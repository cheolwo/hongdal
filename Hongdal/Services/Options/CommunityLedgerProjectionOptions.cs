namespace 홍달.Services.Options;

public sealed class CommunityLedgerProjectionOptions
{
    public const string SectionName = "CommunityLedgerProjection";

    public bool Enabled { get; set; } = true;
    public int PollingIntervalSeconds { get; set; } = 2;
    public int BatchSize { get; set; } = 20;
    public int LeaseTimeoutMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 10;
    public int RetryBaseSeconds { get; set; } = 5;
}
