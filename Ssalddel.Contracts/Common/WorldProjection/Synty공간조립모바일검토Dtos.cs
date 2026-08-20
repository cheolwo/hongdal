namespace Ssalddel.Contracts.Common.WorldProjection;

public static class Synty공간조립모바일검토Routes
{
    public const string Base = "api/v1/platform/world-composition-reviews";
    public const string Batches = Base + "/batches";
    public const string CaptureUploads = Base + "/capture-uploads";
    public const string Decisions = Base + "/items/{reviewItemStableId}/decisions";
}

public static class Synty공간조립검토SchemaVersions
{
    public const string BatchV1 = "synty-composition-review-batch.v1";
    public const string BatchV2 = "synty-composition-review-batch.v2";
    public const string BatchV3 = "synty-composition-review-batch.v3";
}

public static class Synty공간조립검토계층Codes
{
    public const string H1 = "H1";
    public const string H2 = "H2";
    public const string H3 = "H3";
    public const string H4 = "H4";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        H1, H2, H3, H4
    };
}

public static class Synty공간조립촬영ProfileCodes
{
    public const string H1PlaceFourViews = "H1PlaceFourViews";
    public const string H2BlockFiveViews = "H2BlockFiveViews";
    public const string H3LandscapeSixViews = "H3LandscapeSixViews";
    public const string H4WorldFourViews = "H4WorldFourViews";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        H1PlaceFourViews,
        H2BlockFiveViews,
        H3LandscapeSixViews,
        H4WorldFourViews
    };
}

public static class Synty공간조립검토상태Codes
{
    public const string WaitingForCapture = "WaitingForCapture";
    public const string ReadyForReview = "ReadyForReview";
    public const string ReviewedCandidate = "ReviewedCandidate";
    public const string NeedsRevision = "NeedsRevision";
    public const string OnHold = "OnHold";
    public const string CompareCandidate = "CompareCandidate";
    public const string Stale = "Stale";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        WaitingForCapture,
        ReadyForReview,
        ReviewedCandidate,
        NeedsRevision,
        OnHold,
        CompareCandidate,
        Stale
    };
}

public static class Synty공간조립검토결정Codes
{
    public const string Good = "Good";
    public const string NeedsRevision = "NeedsRevision";
    public const string OnHold = "OnHold";
    public const string CompareCandidate = "CompareCandidate";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Good,
        NeedsRevision,
        OnHold,
        CompareCandidate
    };
}

public static class Synty공간조립검토문제Codes
{
    public const string RouteUnclear = "RouteUnclear";
    public const string TooDense = "TooDense";
    public const string PackBlendAwkward = "PackBlendAwkward";
    public const string PsychologicalReadabilityWeak = "PsychologicalReadabilityWeak";
    public const string PerformanceConcern = "PerformanceConcern";
    public const string EntranceExitUnclear = "EntranceExitUnclear";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        RouteUnclear,
        TooDense,
        PackBlendAwkward,
        PsychologicalReadabilityWeak,
        PerformanceConcern,
        EntranceExitUnclear
    };
}

public static class Synty공간조립검토EventCodes
{
    public const string MobileDecision = "MobileDecision";
    public const string SourceUpdated = "SourceUpdated";
    public const string RecaptureSubmitted = "RecaptureSubmitted";
}

public sealed class Synty공간조립검토Batch등록Request
{
    public string SchemaVersion { get; set; } = Synty공간조립검토SchemaVersions.BatchV1;
    public string BatchStableId { get; set; } = string.Empty;
    public string BatchRevision { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public List<Synty공간조립검토항목등록Request> Items { get; set; } = [];
}

public sealed class Synty공간조립검토항목등록Request
{
    public long ExpectedRevision { get; set; }
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string CompositionStableId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string H1StableId { get; set; } = string.Empty;
    public string H2StableId { get; set; } = string.Empty;
    public string H3StableId { get; set; } = string.Empty;
    public string H4StableId { get; set; } = string.Empty;
    public string ReviewTargetLevelCode { get; set; } = string.Empty;
    public string ReviewTargetStableId { get; set; } = string.Empty;
    public string CaptureProfileCode { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public string StateProfileCode { get; set; } = string.Empty;
    public string CompositionInputHash { get; set; } = string.Empty;
    public string PlanHash { get; set; } = string.Empty;
    public string RenderingProfileId { get; set; } = string.Empty;
    public string RenderingProfileRevision { get; set; } = string.Empty;
    public string RenderingProfileHash { get; set; } = string.Empty;
    public string ParentCaptureBundleHash { get; set; } = string.Empty;
    public string CaptureBundleHash { get; set; } = string.Empty;
    public List<Synty공간조립팩활용Dto> PackUsages { get; set; } = [];
    public List<Synty공간조립검토촬영Dto> Captures { get; set; } = [];
}

public sealed class Synty공간조립팩활용Dto
{
    public string PackCode { get; set; } = string.Empty;
    public int UsagePercent { get; set; }
    public string RoleCode { get; set; } = string.Empty;
}

public sealed class Synty공간조립검토촬영Dto
{
    public string CaptureStableId { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string CaptureUploadId { get; set; } = string.Empty;
    public string StorageProviderCode { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageSha256 { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public string ETag { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

public sealed class Synty공간조립검토촬영업로드Response
{
    public string CaptureUploadId { get; set; } = string.Empty;
    public string BatchStableId { get; set; } = string.Empty;
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string CaptureStableId { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string CaptureBundleHash { get; set; } = string.Empty;
    public string ParentCaptureBundleHash { get; set; } = string.Empty;
    public string SourceCompositionHash { get; set; } = string.Empty;
    public long ExpectedReviewItemRevision { get; set; }
    public string RenderingProfileHash { get; set; } = string.Empty;
    public string StorageProviderCode { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string UploadedSourceSha256 { get; set; } = string.Empty;
    public string StoredImageSha256 { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public string ETag { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

public sealed class Synty공간조립검토결정Request
{
    public long ExpectedRevision { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string DecisionCode { get; set; } = string.Empty;
    public List<string> IssueCodes { get; set; } = [];
    public string Note { get; set; } = string.Empty;
}

public sealed class Synty공간조립검토함Response
{
    public int TotalCount { get; set; }
    public int ReadyCount { get; set; }
    public int ReviewedCount { get; set; }
    public List<Synty공간조립검토항목Dto> Items { get; set; } = [];
}

public sealed class Synty공간조립검토Batch등록Response
{
    public string BatchStableId { get; set; } = string.Empty;
    public string BatchRevision { get; set; } = string.Empty;
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int UnchangedCount { get; set; }
    public int StaleCount { get; set; }
    public List<Synty공간조립검토항목Dto> Items { get; set; } = [];
}

public sealed class Synty공간조립검토항목Dto
{
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string BatchStableId { get; set; } = string.Empty;
    public string BatchRevision { get; set; } = string.Empty;
    public string BatchTitle { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string ReviewStateCode { get; set; } = string.Empty;
    public string SnapshotHash { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Synty공간조립검토항목등록Request Composition { get; set; } = new();
    public List<Synty공간조립검토결정이력Dto> History { get; set; } = [];
}

public sealed class Synty공간조립검토결정이력Dto
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string DecisionCode { get; set; } = string.Empty;
    public List<string> IssueCodes { get; set; } = [];
    public string Note { get; set; } = string.Empty;
    public string ReviewerDisplayName { get; set; } = string.Empty;
    public DateTime DecidedAtUtc { get; set; }
    public long Revision { get; set; }
}
