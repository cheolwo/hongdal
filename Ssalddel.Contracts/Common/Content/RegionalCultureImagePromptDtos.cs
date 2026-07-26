using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Content;

public static class RegionalCultureImagePromptCountryCodes
{
    public const string Korea = "KR";
    public const string UnitedStates = "US";
    public const string China = "CN";

    public static IReadOnlyList<string> All { get; } = [Korea, UnitedStates, China];
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
    DateTime UpdatedAtUtc);

public sealed record RegionalCultureImagePromptListResponse(
    string? CountryCode,
    int TotalCount,
    IReadOnlyList<RegionalCultureImagePromptDto> Items);
