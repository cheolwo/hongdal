using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Community;

/// <summary>
/// 지도에 연결된 원장의 성숙도를 표현합니다.
/// 기존 가원장·실원장에 저장되는 code와 같은 값을 사용합니다.
/// </summary>
public static class 커뮤니티세계지도원장성숙도Codes
{
    public const string Proposed = "Proposed";
    public const string Provisional = CommunityPostProvisionalLedgerPolicy.LedgerMaturityCode;
    public const string Established = 지도신청가원장정책.실원장성숙도Code;

    public static IReadOnlyList<string> All { get; } =
    [
        Proposed,
        Provisional,
        Established
    ];
}

/// <summary>
/// 원장의 업무 상태를 지도 표시용 최소 상태로 축소한 code입니다.
/// 공개 가능 여부는 이 code만으로 결정하지 않고 ViewerScope 정책을 함께 적용합니다.
/// </summary>
public static class 커뮤니티세계지도원장공개상태Codes
{
    public const string Proposed = "proposed";
    public const string ProvisionalDraft = "provisional-draft";
    public const string ConsentReviewRequired = "consent-review-required";
    public const string Submitted = "submitted";
    public const string Active = "active";
    public const string OnHold = "on-hold";
    public const string Completed = "completed";
    public const string Withdrawn = "withdrawn";
    public const string Cancelled = "cancelled";

    public static IReadOnlyList<string> All { get; } =
    [
        Proposed,
        ProvisionalDraft,
        ConsentReviewRequired,
        Submitted,
        Active,
        OnHold,
        Completed,
        Withdrawn,
        Cancelled
    ];
}

/// <summary>
/// 같은 projection이라도 요청자가 볼 수 있는 최대 범위를 구분합니다.
/// 범위가 넓다는 이유만으로 하위 범위의 개인정보가 자동 포함되지는 않습니다.
/// </summary>
public static class 커뮤니티세계지도원장ViewerScopeCodes
{
    public const string Public = "public";
    public const string Owner = "owner";
    public const string Participant = "participant";
    public const string Operator = "operator";
    public const string Reviewer = "reviewer";

    public static IReadOnlyList<string> All { get; } =
    [
        Public,
        Owner,
        Participant,
        Operator,
        Reviewer
    ];
}

/// <summary>
/// 공개 집계의 노출 수준을 표현합니다. 임계값과 구간 규칙은 별도 projection 정책이 결정합니다.
/// </summary>
public static class 커뮤니티세계지도원장집계BucketCodes
{
    public const string Suppressed = "suppressed";
    public const string ThresholdMet = "threshold-met";
    public const string Coarsened = "coarsened";

    public static IReadOnlyList<string> All { get; } =
    [
        Suppressed,
        ThresholdMet,
        Coarsened
    ];
}

/// <summary>
/// 지도 원장 projection이 공개할 수 있는 최대 위치 정밀도입니다.
/// 상세 주소와 정밀 좌표를 나타내는 mode는 의도적으로 제공하지 않습니다.
/// </summary>
public static class 커뮤니티세계지도원장위치공개ModeCodes
{
    public const string None = "none";
    public const string AdministrativeRegion = "administrative-region";
    public const string Country = "country";

    public static IReadOnlyList<string> All { get; } =
    [
        None,
        AdministrativeRegion,
        Country
    ];
}

public static class 커뮤니티세계지도원장ActionCodes
{
    public const string ViewEvidence = "view-evidence";
    public const string ViewLedger = "view-ledger";
    public const string ContinueDraft = "continue-draft";
    public const string ReviewConsent = "review-consent";
    public const string Submit = "submit";
    public const string Withdraw = "withdraw";

    public static IReadOnlyList<string> All { get; } =
    [
        ViewEvidence,
        ViewLedger,
        ContinueDraft,
        ReviewConsent,
        Submit,
        Withdraw
    ];
}

/// <summary>
/// 원장 원문을 지도 client에 전달하지 않고 지도에 필요한 최소 상태만 전달합니다.
/// 원장 식별자, 개인정보, 상세 주소, 개인 수량, 계약·재고·정밀 경로는 포함하지 않습니다.
/// </summary>
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Contract,
    "세계지도에 연결된 원장의 공개 가능 최소 상태와 권한별 action을 전달",
    FlowOrder = 11,
    Effects = SsalddelCodeEffect.None,
    Boundary = "원장 ID, 개인 식별자, 연락처, 상세 주소, 정밀 좌표·경로, 계약·재고·금액과 원장 원문은 포함하지 않습니다.")]
public sealed class 커뮤니티세계지도원장ProjectionDto
{
    public string ProjectionId { get; set; } = string.Empty;
    public long ProjectionVersion { get; set; }
    public string? MapMarkerId { get; set; }
    public string? AdministrativeRegionKey { get; set; }
    public string? CountryCode { get; set; }
    public string LedgerTemplateKey { get; set; } = string.Empty;
    public string LedgerMaturityCode { get; set; } = 커뮤니티세계지도원장성숙도Codes.Proposed;
    public string PublicStatusCode { get; set; } = 커뮤니티세계지도원장공개상태Codes.Proposed;
    public string EvidenceFreshnessCode { get; set; } = 커뮤니티세계지도FreshnessCodes.Unknown;
    public string? EvidenceSnapshotVersion { get; set; }
    public int? PublicAggregateCount { get; set; }
    public string AggregateBucketCode { get; set; } = 커뮤니티세계지도원장집계BucketCodes.Suppressed;
    public IReadOnlyList<string> AvailableActionCodes { get; set; } = [];
    public DateTimeOffset LastProjectedAtUtc { get; set; }
    public string SourceEventId { get; set; } = string.Empty;
    public string ViewerScopeCode { get; set; } = 커뮤니티세계지도원장ViewerScopeCodes.Public;
}

public sealed class 커뮤니티세계지도원장ProjectionBatchDto
{
    public IReadOnlyList<커뮤니티세계지도원장ProjectionDto> Items { get; set; } = [];
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int ReturnedCount { get; set; }
    public int AvailableCount { get; set; }
    public bool HasMore { get; set; }
    public bool SourceMayBeTruncated { get; set; }
}
