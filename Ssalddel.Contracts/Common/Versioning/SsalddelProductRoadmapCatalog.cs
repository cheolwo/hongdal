namespace Ssalddel.Contracts.Common.Versioning;

public sealed record SsalddelProductRoadmapStage(
    string Version,
    string ProductName,
    string DisplayName,
    int SortOrder,
    string PrerequisiteVersion,
    string ExitOutcome,
    bool IsCurrent = false)
{
    public string FullDisplayName => $"{ProductName} {Version} · {DisplayName}";

    public bool IsCultureTransport
        => string.Equals(
            ProductName,
            SsalddelProductRoadmapCatalog.CultureTransportName,
            StringComparison.Ordinal);
}

public static class SsalddelProductRoadmapCatalog
{
    public const string CultureTransportName = "문화교통";
    public const string DefaultProductName = "살뜰";
    public const string FoundationVersion = "0.0";
    public const string GroupPurchaseVersion = "1.0";
    public const string TradeReadinessVersion = "1.5";
    public const string TransportVersion = "2.0";
    public const string FulfillmentVersion = "2.5";
    public const string FoodDeliveryVersion = "3.0";
    public const string MartVersion = "3.5";
    public const string CurrentVersion = TradeReadinessVersion;

    public static IReadOnlyList<SsalddelProductRoadmapStage> All { get; } =
    [
        new(
            FoundationVersion,
            CultureTransportName,
            "커뮤니티·공공데이터 기반",
            0,
            string.Empty,
            "공개 음식·재료 데이터와 참여 의사를 안전하게 기록할 수 있습니다."),
        new(
            GroupPurchaseVersion,
            CultureTransportName,
            "공동구매·주문자 집단화",
            100,
            FoundationVersion,
            "품목·지역·수령 조건별 비구속 수요를 서버가 집단화하고 모집 원장으로 보여 줍니다."),
        new(
            TradeReadinessVersion,
            CultureTransportName,
            "공급·가격·무역 준비",
            150,
            GroupPurchaseVersion,
            "공급자 근거, 견적, 원가, HS·HTS 후보와 수입 준비 체크포인트를 연결합니다.",
            IsCurrent: true),
        new(
            TransportVersion,
            DefaultProductName,
            "국내 화물·운송 이행",
            200,
            TradeReadinessVersion,
            "확정된 구매·출고 필요를 운송 의뢰, 배차, 증빙과 정산 흐름에 인계합니다."),
        new(
            FulfillmentVersion,
            DefaultProductName,
            "창고·판매 이행",
            250,
            TransportVersion,
            "입고, 재고, 피킹, 포장, 판매채널과 최종 배분을 연결합니다."),
        new(
            FoodDeliveryVersion,
            DefaultProductName,
            "음식점 배달",
            300,
            TransportVersion,
            "음식 주문을 조리, 픽업과 고객 배송으로 연결합니다."),
        new(
            MartVersion,
            DefaultProductName,
            "마트·도심 물류",
            350,
            FulfillmentVersion,
            "도심 재고, 피킹, 포장과 즉시배송을 연결합니다.")
    ];

    public static SsalddelProductRoadmapStage Find(string? version)
        => All.FirstOrDefault(stage => string.Equals(
               stage.Version,
               version?.Trim(),
               StringComparison.OrdinalIgnoreCase))
           ?? All.Single(stage => stage.IsCurrent);

    public static bool IsCultureTransportVersion(string? version)
        => All.Any(stage =>
            stage.IsCultureTransport
            && string.Equals(
                stage.Version,
                version?.Trim(),
                StringComparison.OrdinalIgnoreCase));
}
