using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.WorldProjection;

public static class CommunityMarketSquareRoutes
{
    public const string PublicSnapshot = "api/v1/community/world/zones/community-market-square";
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Contract,
    "커뮤니티·시장 광장에 표시할 공개 게시판·게시글·활동 신호·원장 요약을 전달한다.",
    FlowOrder = 10,
    Boundary = "공개 읽기 전용이며 작성자 식별자, 댓글, 연락처, 원장 담당자와 실행 행동을 포함하지 않는다.")]
public sealed class CommunityMarketSquareSnapshotResponse
{
    public string StableId { get; set; } = "community-market-square:public";
    public string Revision { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public IReadOnlyList<CommunitySquareBoardResponse> Boards { get; set; } = [];
    public IReadOnlyList<CommunitySquarePostResponse> Posts { get; set; } = [];
    public IReadOnlyList<CommunitySquareActivityResponse> ActivitySignals { get; set; } = [];
    public IReadOnlyList<CommunitySquareLedgerResponse> Ledgers { get; set; } = [];
}

public sealed class CommunitySquareBoardResponse
{
    public string StableId { get; set; } = string.Empty;
    public string BoardKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GroupDisplayName { get; set; } = string.Empty;
    public string PostingAccessCode { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public DateTimeOffset? LatestPostAtUtc { get; set; }
}

public sealed class CommunitySquarePostResponse
{
    public string StableId { get; set; } = string.Empty;
    public long PostId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string WorkflowTag { get; set; } = string.Empty;
    public string RoleTag { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string TopicClassificationCode { get; set; } = string.Empty;
    public bool IsSystemGenerated { get; set; }
    public bool IsInterestGatheringEnabled { get; set; }
    public int RecommendationCount { get; set; }
    public int CommentCount { get; set; }
    public DateTimeOffset PublishedAtUtc { get; set; }
    public string DetailHref { get; set; } = string.Empty;
}

public sealed class CommunitySquareActivityResponse
{
    public string StableId { get; set; } = string.Empty;
    public string CommunityScope { get; set; } = string.Empty;
    public string ActivityKind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string TimeBucketLabel { get; set; } = string.Empty;
    public string TimePrecision { get; set; } = string.Empty;
    public int AggregationCount { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string PrivacyPolicyVersion { get; set; } = string.Empty;
}

public sealed class CommunitySquareLedgerResponse
{
    public string StableId { get; set; } = string.Empty;
    public string SourcePostStableId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public bool DetailAvailable { get; set; }
    public string DetailHref { get; set; } = string.Empty;
}
