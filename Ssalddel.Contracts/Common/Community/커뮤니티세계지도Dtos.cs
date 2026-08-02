using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Community;

public static class 커뮤니티세계지도Routes
{
    public const string ObservationApi = "api/v1/community/world-map/observations";
    public const string KoreaPriceDetail = "/information/kamis-domestic-price-comparison";
    public const string UnitedStatesPriceDetail = "/information/usda-us-price-comparison";
}

public static class 커뮤니티세계지도LayerCodes
{
    public const string RegionalCulture = "regional-culture";
    public const string PublicPrice = "public-price";
    public const string LearningChannel = "learning-channel";
    public const string ScriptureAndClassics = "scripture-classics";
}

public static class 커뮤니티세계지도EvidenceStatusCodes
{
    public const string Curated = "curated";
    public const string OfficialSourceLinked = "official-source-linked";
}

public sealed record 커뮤니티세계지도LayerDto(
    string Code,
    string DatasetCode,
    string DisplayName,
    string Description,
    string Color,
    string MarkerShape);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Contract,
    "한 개의 세계 지도에 표시할 분야별 공개 관측과 출처·시각·상세 경로를 전달",
    FlowOrder = 10,
    Effects = SsalddelCodeEffect.None,
    Boundary = "개인 정밀 위치와 결제·계약·배차 실행 상태는 포함하지 않습니다.")]
public sealed record 커뮤니티세계지도ObservationDto(
    string StableId,
    string DatasetCode,
    string LayerCode,
    string CountryCode,
    string CountryName,
    double Latitude,
    double Longitude,
    string Title,
    string Summary,
    string SourceName,
    DateTimeOffset? EvidenceAsOfUtc,
    string EvidenceStatusCode,
    string DetailHref);

public sealed record 커뮤니티세계지도SnapshotDto(
    string DatasetCode,
    string Revision,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<커뮤니티세계지도LayerDto> Layers,
    IReadOnlyList<커뮤니티세계지도ObservationDto> Observations);

public static class 커뮤니티세계지도LayerCatalog
{
    public static IReadOnlyList<커뮤니티세계지도LayerDto> All { get; } =
    [
        new(
            커뮤니티세계지도LayerCodes.RegionalCulture,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "지역 문화",
            "지역의 생활권·음식·특산물 맥락",
            "#176b4d",
            "circle"),
        new(
            커뮤니티세계지도LayerCodes.PublicPrice,
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "가격·시장",
            "공식 가격 관측과 원 거래 단위",
            "#ef8f3c",
            "diamond"),
        new(
            커뮤니티세계지도LayerCodes.LearningChannel,
            CommunityPageRoutes.WorldMapNightLearningDataset,
            "생각·성찰 자료",
            "철학·심리·윤리·아이디어를 가볍게 알아차리는 공개 자료",
            "#6750a4",
            "circle"),
        new(
            커뮤니티세계지도LayerCodes.ScriptureAndClassics,
            CommunityPageRoutes.WorldMapNightLearningDataset,
            "경전·고전 자료",
            "종교 교육·마음챙김 관련 공개 자료를 가볍게 알아차림",
            "#b7791f",
            "diamond")
    ];

    public static IReadOnlyList<커뮤니티세계지도LayerDto> ForDataset(string datasetCode)
        => All.Where(layer => string.Equals(
                layer.DatasetCode,
                datasetCode,
                StringComparison.Ordinal))
            .ToArray();
}
