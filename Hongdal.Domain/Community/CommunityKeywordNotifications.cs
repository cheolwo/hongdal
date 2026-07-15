using Hongdal.Domain.Notifications;

namespace Hongdal.Domain.Community;

public static class CommunityKeywordScanStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string RetryWaiting = "RetryWaiting";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

public static class CommunityKeywordDeliveryStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string RetryWaiting = "RetryWaiting";
    public const string Succeeded = "Succeeded";
    public const string Skipped = "Skipped";
    public const string Failed = "Failed";
}

public sealed class CommunityKeywordSubscription
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AppKey { get; set; } = "platform";
    public string Keyword { get; set; } = string.Empty;
    public string NormalizedKeyword { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class PlatformCommunityPostKeywordScan
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public PlatformCommunityPost Post { get; set; } = null!;
    public string Status { get; set; } = CommunityKeywordScanStatuses.Pending;
    public int AttemptCount { get; set; }
    public string? ProcessingToken { get; set; }
    public string? LastError { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}

public sealed class CommunityKeywordNotification
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long PostId { get; set; }
    public PlatformCommunityPost Post { get; set; } = null!;
    public string PostAppKey { get; set; } = "platform";
    public string PostCategory { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
    public string PostExcerpt { get; set; } = string.Empty;
    public string PostAuthorNickname { get; set; } = string.Empty;
    public string MatchedKeywordsJson { get; set; } = "[]";
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<CommunityKeywordNotificationDelivery> Deliveries { get; set; }
        = new List<CommunityKeywordNotificationDelivery>();
}

public sealed class CommunityKeywordNotificationDelivery
{
    public long Id { get; set; }
    public long NotificationId { get; set; }
    public CommunityKeywordNotification Notification { get; set; } = null!;
    public long InstallationId { get; set; }
    public HongdalMobilePushInstallation Installation { get; set; } = null!;
    public string Status { get; set; } = CommunityKeywordDeliveryStatuses.Pending;
    public int AttemptCount { get; set; }
    public string? ProcessingToken { get; set; }
    public string? LastError { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
