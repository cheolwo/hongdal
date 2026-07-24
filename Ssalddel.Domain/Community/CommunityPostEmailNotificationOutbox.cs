namespace Ssalddel.Domain.Community;

public static class CommunityPostEmailNotificationOutboxStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Sent = "Sent";
    public const string Skipped = "Skipped";
    public const string ConfigurationRequired = "ConfigurationRequired";
    public const string Failed = "Failed";
}

public sealed class CommunityPostEmailNotificationOutbox
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public string Status { get; set; } = CommunityPostEmailNotificationOutboxStatuses.Pending;
    public int AttemptCount { get; set; }
    public DateTime NextAttemptAtUtc { get; set; }
    public string? ProcessingToken { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
}
