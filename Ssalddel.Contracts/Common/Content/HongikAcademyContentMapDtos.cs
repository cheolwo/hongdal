using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Content;

public static class HongikAcademyContentMapRoutes
{
    public const string LayerApi = "api/v1/community/content-map/hongik-academy";
}

public static class HongikAcademyContentMapLayerKeys
{
    public const string PhilosophyVideo = "hongik-academy-philosophy-video";
}

public static class HongikAcademyContentMapProvenanceStatusCodes
{
    public const string NoVerifiedGeographicRecords = "NoVerifiedGeographicRecords";
}

public sealed record HongikAcademyContentMapSourceDto(
    string SourceKey,
    string DisplayName,
    string ProvenanceStatusCode,
    string? SourceUrl,
    int VerifiedGeographicRecordCount,
    string Limitations);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.HongikAcademyContentMap,
    SsalddelCodeLayer.Contract,
    "홍익학당 철학·영상 지도 레이어의 지리 범위와 원천 검증 상태를 전달",
    FlowOrder = 10,
    Boundary = "검증된 공개 지리 레코드가 없으면 빈 상태를 명시하고 개인·장소·추정 위치를 반환하지 않습니다.")]
public sealed record HongikAcademyContentMapLayerResponse(
    string LayerKey,
    string DisplayName,
    string ViewingContextLabel,
    string GeographicScopeCode,
    string GeographicScopeLabel,
    bool HasVerifiedGeographicRecords,
    int VerifiedGeographicRecordCount,
    DateTime CheckedAtUtc,
    IReadOnlyList<HongikAcademyContentMapSourceDto> Sources,
    IReadOnlyList<string> Notices);
