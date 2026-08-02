using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.Community;

public static class 운송시뮬레이션ModeCodes
{
    public const string GroundCargo = "ground-cargo";
    public const string Aviation = "aviation";
    public const string Maritime = "maritime";
}

public static class 운송시뮬레이션SourceKindCodes
{
    public const string SimulatedFixture = "simulated-fixture";
    public const string OfficialCatalogCandidate = "official-catalog-candidate";
}

public static class 운송시뮬레이션AdapterDecisionCodes
{
    public const string CatalogOnly = "catalog-only";
}

public sealed record 운송시뮬레이션RoutePointDto(
    double Latitude,
    double Longitude);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Contract,
    "지도에서 화물·항공·해상 흐름을 설명하는 비실시간 교육용 시뮬레이션 경로를 전달",
    FlowOrder = 11,
    Effects = SsalddelCodeEffect.None,
    Boundary = "실제 운행 추적, 배차 확정, 개인 위치, 항공편·선박 식별자와 타 조직 운영정보를 포함하지 않습니다.")]
public sealed record 운송시뮬레이션RouteDto(
    string StableId,
    string ModeCode,
    string DisplayName,
    string StatusCode,
    string StatusLabel,
    IReadOnlyList<운송시뮬레이션RoutePointDto> Route,
    string SourceCode,
    string SourceName,
    string SourceKindCode,
    DateTimeOffset BasisAtUtc,
    string FreshnessLabel,
    bool IsSimulation,
    string SimulationMark,
    string PositionMeaning,
    double AnimationCycleSeconds,
    string Color);

public sealed record 운송공개데이터AdapterCatalogEntryDto(
    string SourceCode,
    string ModeCode,
    string AuthorityName,
    string CatalogName,
    string CatalogHref,
    string AccessAndReuseSummary,
    string PositionDataBoundary,
    string AdapterDecisionCode,
    DateOnly ReviewedOn);

public static class 운송시뮬레이션MapFixtureCatalog
{
    public const string FixtureSourceCode = "ssalddel-education-fixture-v1";
    public const string SimulationMark = "SIMULATED · 교육용 · 비실시간";
    public const string FreshnessLabel = "고정 교육 예시 · 자동 갱신 없음";
    public const string PositionMeaning = "경로 위의 합성 진행률이며 실제 위치가 아닙니다.";

    private static readonly DateTimeOffset FixtureBasisAtUtc =
        new(2026, 8, 2, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<운송시뮬레이션RouteDto> Routes { get; } =
    [
        Create(
            "sim-ground-cargo-001",
            운송시뮬레이션ModeCodes.GroundCargo,
            "가상 내륙 화물 흐름 A → B",
            "education-in-transit",
            "교육 경로 이동 중",
            18,
            "#2f6fab",
            new(34.8, 126.2),
            new(36.1, 129.0),
            new(34.4, 132.4)),
        Create(
            "sim-aviation-001",
            운송시뮬레이션ModeCodes.Aviation,
            "가상 항공 흐름 A → B",
            "education-en-route",
            "교육 항로 비행 중",
            15,
            "#7c3aed",
            new(37.0, -122.0),
            new(44.0, -98.0),
            new(40.0, -74.0)),
        Create(
            "sim-maritime-001",
            운송시뮬레이션ModeCodes.Maritime,
            "가상 해상 흐름 A → B",
            "education-underway",
            "교육 항로 운항 중",
            24,
            "#0f766e",
            new(1.4, 103.6),
            new(18.0, 117.0),
            new(34.0, 139.0))
    ];

    private static 운송시뮬레이션RouteDto Create(
        string stableId,
        string modeCode,
        string displayName,
        string statusCode,
        string statusLabel,
        double animationCycleSeconds,
        string color,
        params 운송시뮬레이션RoutePointDto[] route)
        => new(
            stableId,
            modeCode,
            displayName,
            statusCode,
            statusLabel,
            route,
            FixtureSourceCode,
            "살뜰 검증 고정 시뮬레이션 fixture",
            운송시뮬레이션SourceKindCodes.SimulatedFixture,
            FixtureBasisAtUtc,
            FreshnessLabel,
            true,
            SimulationMark,
            PositionMeaning,
            animationCycleSeconds,
            color);
}

public static class 운송공개데이터AdapterCatalog
{
    public static IReadOnlyList<운송공개데이터AdapterCatalogEntryDto> All { get; } =
    [
        new(
            "molit-tago-domestic-flight",
            운송시뮬레이션ModeCodes.Aviation,
            "국토교통부",
            "TAGO 국내항공운항정보",
            "https://www.data.go.kr/data/15098526/openapi.do",
            "활용신청형 REST API · 무료 · 이용허락범위 제한 없음으로 게시되어 있으나 운영 전 명세·재배포 조건 재검토 필요",
            "출도착 일정·현황 자료이며 항공기의 실제 좌표 추적으로 사용하지 않습니다.",
            운송시뮬레이션AdapterDecisionCodes.CatalogOnly,
            new(2026, 8, 2)),
        new(
            "mof-vessel-operation",
            운송시뮬레이션ModeCodes.Maritime,
            "해양수산부",
            "선박운항정보",
            "https://www.data.go.kr/data/15006353/openapi.do",
            "활용신청형 XML API · 무료 · 이용허락범위 제한 없음으로 게시되어 있으나 운영 전 명세·재배포 조건 재검토 필요",
            "호출부호·선명·입출항시각 등 운영 식별정보를 초기 지도에 표시하거나 위치 추적으로 전환하지 않습니다.",
            운송시뮬레이션AdapterDecisionCodes.CatalogOnly,
            new(2026, 8, 2)),
        new(
            "faa-swim-sfdps",
            운송시뮬레이션ModeCodes.Aviation,
            "미국 연방항공청(FAA)",
            "SWIM Flight Data Publication Service",
            "https://www.faa.gov/air_traffic/technology/swim/sfdps",
            "승인된 소비자와 보안 게이트웨이 접근이 필요한 서비스 · 공개 무인 API로 가정하지 않음",
            "계약·접근·재배포 조건 확인 전 수집하지 않으며 실제 항공편 위치를 초기 지도에 표시하지 않습니다.",
            운송시뮬레이션AdapterDecisionCodes.CatalogOnly,
            new(2026, 8, 2))
    ];
}
