namespace Ssalddel.Domain.Community;

public sealed class 커뮤니티활동공개Projection
{
    public long Id { get; set; }

    public string AggregateKey { get; set; } = string.Empty;

    public string AppKey { get; set; } = string.Empty;

    public string CommunityScope { get; set; } = string.Empty;

    public string ActivityKind { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string PublicSummary { get; set; } = string.Empty;

    public string TopicTagsJson { get; set; } = "[]";

    public DateTime TimeBucketStartUtc { get; set; }

    public DateTime TimeBucketEndUtc { get; set; }

    public int ActivityCount { get; set; }

    public string VisibilityScope { get; set; } = string.Empty;

    public string PrivacyPolicyVersion { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 커뮤니티활동처리기록
{
    public long Id { get; set; }

    public string OccurrenceKey { get; set; } = string.Empty;

    public string AggregateKey { get; set; } = string.Empty;

    public string SourceKind { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public DateTime RecordedAtUtc { get; set; }
}
