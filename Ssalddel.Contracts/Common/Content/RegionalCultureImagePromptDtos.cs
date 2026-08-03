using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Content;

public static class RegionalCultureImagePromptCountryCodes
{
    public const string Korea = "KR";
    public const string UnitedStates = "US";
    public const string China = "CN";

    public static IReadOnlyList<string> All { get; } = [Korea, UnitedStates, China];
}

public static class RegionalCultureAnimationStyleCodes
{
    public const string CinematicStylized3D = "CinematicStylized3D";
    public const int TargetImagesPerRegion = 10;
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalCultureImagePrompt,
    SsalddelCodeLayer.Contract,
    "지역문화 이미지 생성 전 조사 초안과 안전 경계를 전달",
    FlowOrder = 10,
    Boundary = "프롬프트는 지역 전체의 단일 대표성이 아니라 생성 전 공식 근거 재검토가 필요한 조사 초안입니다.")]
public sealed record RegionalCultureImagePromptDto(
    string RegionKey,
    string CountryCode,
    string SubdivisionCode,
    string RegionNameKo,
    string RegionNameEn,
    string RegionNameLocal,
    string RegionTypeCode,
    string GeographySummaryKo,
    string CultureSummaryKo,
    IReadOnlyList<string> VisualAnchors,
    IReadOnlyList<string> AvoidExpressions,
    string PromptKo,
    string AspectRatio,
    string SafeCrop,
    string ReviewStatusCode,
    bool RequiresEvidenceReview,
    string EvidenceNotesKo,
    int PromptVersion,
    string VisualStyleCode,
    int TargetImageCount,
    DateTime UpdatedAtUtc);

public sealed record RegionalCultureImagePromptListResponse(
    string? CountryCode,
    int TotalCount,
    IReadOnlyList<RegionalCultureImagePromptDto> Items);

public static class RegionalCultureImageGenerationSlotStatusCodes
{
    public const string Missing = "Missing";
    public const string Queued = "Queued";
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

public sealed record RegionalCultureImageGenerationSlotDto(
    int SceneNumber,
    string TargetIdentifier,
    string StatusCode,
    string? ImageUrl,
    DateTime? CompletedAtUtc,
    string? FailureReason);

public sealed record RegionalCultureImageGenerationProgressItemDto(
    string RegionKey,
    string CountryCode,
    string RegionNameKo,
    string ReviewStatusCode,
    bool ReadyForGeneration,
    int TargetCount,
    int CompletedCount,
    int RunningCount,
    int FailedCount,
    int RemainingCount,
    IReadOnlyList<RegionalCultureImageGenerationSlotDto> Slots);

public sealed record RegionalCultureImageGenerationProgressResponse(
    string? CountryCode,
    string VisualStyleCode,
    int TargetImagesPerRegion,
    int RegionCount,
    int TotalTargetCount,
    int CompletedCount,
    int RunningCount,
    int FailedCount,
    int RemainingCount,
    IReadOnlyList<RegionalCultureImageGenerationProgressItemDto> Items);

public sealed class RegionalCultureImageGenerationApprovalRequest
{
    public bool OfficialSourcesReviewed { get; set; }

    public bool StereotypeRiskReviewed { get; set; }

    public IReadOnlyList<string> ReviewedSourceKeys { get; set; } = [];

    public string ReviewNoteKo { get; set; } = string.Empty;
}

public sealed record RegionalCultureImageGenerationApprovalResponse(
    string RegionKey,
    string ReviewStatusCode,
    bool RequiresEvidenceReview,
    DateTime UpdatedAtUtc);

public sealed class RegionalCultureImageGenerationNextRequest
{
    public int MaxCount { get; set; } = 1;

    public bool IncludeFailed { get; set; }
}

public sealed record RegionalCultureImageGenerationJobDto(
    long JobId,
    string JobCode,
    string TargetIdentifier,
    string StatusCode,
    DateTime CreatedAtUtc);

public sealed record RegionalCultureImageGenerationNextResponse(
    bool Accepted,
    string ResultCode,
    string Message,
    int CreatedCount,
    IReadOnlyList<RegionalCultureImageGenerationJobDto> Jobs);
