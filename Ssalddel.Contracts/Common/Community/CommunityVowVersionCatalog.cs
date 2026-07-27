using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Contracts.Common.Community;

public sealed record CommunityVowVersionDefinition(
    string Code,
    string DisplayName,
    string WorkflowTag,
    string Focus,
    string InheritedFoundation,
    string OperationalBoundary,
    bool IsCurrentFocus = false,
    bool IsFutureExploration = false);

public static class CommunityVowVersionCatalog
{
    public const string CurrentVersionCode = SsalddelProductRoadmapCatalog.CurrentVersion;

    public const string FutureVersionCode = "future";

    private static readonly IReadOnlyDictionary<string, string> LegacyWorkflowTagVersions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["살뜰 0.0 · 커뮤니티 기반"] =
                SsalddelProductRoadmapCatalog.FoundationVersion,
            ["살뜰 1.0 · 공동구매·주문자 집단화"] =
                SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            ["문화교통 1.0 · 공동구매·주문자 집단화"] =
                SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            ["살뜰 1.5 · 공급·가격·무역 준비"] =
                SsalddelProductRoadmapCatalog.TradeReadinessVersion
        };

    public static IReadOnlyList<CommunityVowVersionDefinition> All { get; } =
    [
        new(
            SsalddelProductRoadmapCatalog.FoundationVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.FoundationVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.FoundationVersion),
            "글에서 시작한 마음이 참여 의사, 가원장, 역할 합의와 완료 사례로 이어지는 공동체 기반",
            "첫 기반이므로 앞선 제품 버전 없이 독립적으로 실행되고 검증되어야 합니다.",
            "운송 주선·자동 배차·계약·결제는 확정하지 않고 당사자의 대화와 합의를 기록합니다.",
            IsCurrentFocus: true),
        new(
            SsalddelProductRoadmapCatalog.IndividualOrderVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.IndividualOrderVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.IndividualOrderVersion),
            "공개 정보에서 고른 상품과 수량·수령 조건을 한 사람의 철회 가능한 주문 의향과 개별 원장으로 관리하는 흐름",
            "0.0의 공개 데이터, 참여 동의, 원장 식별과 신뢰 기록을 이어받습니다.",
            "개별 원장은 결제·매매 계약·배송 확정이 아니며 사용자의 명시적 동의 없이 같이 주문에 포함하지 않습니다."),
        new(
            SsalddelProductRoadmapCatalog.GroupPurchaseVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.GroupPurchaseVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.GroupPurchaseVersion),
            "0.5 개별주문 가운데 같이 주문 참여에 동의한 원장을 품목·지역·수령 조건별 주문자 집단과 같이 주문 모집 원장으로 만드는 흐름",
            "0.5의 개별 원장, 철회 상태와 공동 참여 동의를 이어받습니다.",
            "결제·매매 계약·수입 신고·자동 배차를 확정하지 않고 참여자가 철회할 수 있는 수요와 합의를 기록합니다."),
        new(
            SsalddelProductRoadmapCatalog.TradeReadinessVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.TradeReadinessVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.TradeReadinessVersion),
            "공급자와 관련 기업 근거, 견적, 원가, HS·HTS 후보와 같이 수입 준비 항목을 수요 집단에 연결하는 흐름",
            "1.0의 주문자 집단과 모집 원장을 공급·가격·무역 검토의 입력으로 이어받습니다.",
            "품목 분류, 수입 적격성, 신고와 계약 판단은 자격 있는 전문가와 실제 거래 당사자의 확인을 대신하지 않습니다."),
        new(
            SsalddelProductRoadmapCatalog.TransportVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.TransportVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.TransportVersion),
            "구매와 출고 조건이 확인된 원장을 운송 의뢰, 기사·운송사 인계, 증빙과 정산 준비로 연결하는 흐름",
            "1.0의 집단 수요와 1.5의 공급·비용·무역 준비 결과를 운송 입력으로 이어받습니다.",
            "허가·제휴·계약·운영 준비 전 플랫폼이 유상 배차나 운송 주선을 확정하지 않습니다."),
        new(
            SsalddelProductRoadmapCatalog.FulfillmentVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.FulfillmentVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.FulfillmentVersion),
            "입고, 재고, 피킹, 포장, 판매채널과 주문자 집단의 최종 배분이 이어지는 흐름",
            "1.0의 주문자 집단과 2.0의 운송 인계·증빙 구조를 이어받습니다.",
            "실제 보관·재위탁·출고·판매 책임은 계약된 창고, 판매자와 수행 주체가 확인합니다."),
        new(
            SsalddelProductRoadmapCatalog.FoodDeliveryVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.FoodDeliveryVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.FoodDeliveryVersion),
            "음식점 주문이 조리, 픽업과 고객 배송까지 짧은 시간 안에 이어지는 흐름",
            "0.0의 참여 기반과 2.0의 운송·역할 인계 원칙을 이어받습니다.",
            "영업 신고, 식품 안전과 배달 운영 책임은 실제 음식점과 수행 주체가 확인합니다."),
        new(
            SsalddelProductRoadmapCatalog.MartVersion,
            ProductVersionName(SsalddelProductRoadmapCatalog.MartVersion),
            ProductVersionWorkflowTag(SsalddelProductRoadmapCatalog.MartVersion),
            "도심 재고, 피킹, 포장과 즉시배송이 동네 마트·시장과 연결되는 흐름",
            "0.0의 공동체 기반과 2.5 창고·판매, 3.0 배달의 인계 원칙을 조립합니다.",
            "재고 판매·보관·배송의 책임 주체와 허용 범위를 실제 참여자가 확인합니다."),
        new(
            FutureVersionCode,
            "살뜰 다음 서원",
            "살뜰 이후 · 다음 서원",
            "3.5 이후에도 사람들이 새 필요를 글로 발견하고 다음 제품 방향을 함께 정하는 흐름",
            "완료된 버전의 사례와 아직 풀리지 않은 문제를 근거로 삼습니다.",
            "버전 번호와 운영 약속을 미리 확정하지 않고 탐색 서원으로 기록합니다.",
            IsFutureExploration: true)
    ];

    public static CommunityVowVersionDefinition Current
        => Find(CurrentVersionCode);

    public static CommunityVowVersionDefinition Find(string? code)
        => All.FirstOrDefault(version => string.Equals(
               version.Code,
               code?.Trim(),
               StringComparison.OrdinalIgnoreCase))
           ?? Current;

    public static CommunityVowVersionDefinition? FindByWorkflowTag(string? workflowTag)
    {
        var normalized = workflowTag?.Trim();
        var current = All.FirstOrDefault(version => string.Equals(
            version.WorkflowTag,
            normalized,
            StringComparison.OrdinalIgnoreCase));
        if (current is not null || string.IsNullOrWhiteSpace(normalized))
        {
            return current;
        }

        return LegacyWorkflowTagVersions.TryGetValue(normalized, out var versionCode)
            ? Find(versionCode)
            : null;
    }

    private static string ProductVersionName(string version)
    {
        var stage = SsalddelProductRoadmapCatalog.Find(version);
        return $"{stage.ProductName} {stage.Version}";
    }

    private static string ProductVersionWorkflowTag(string version)
        => SsalddelProductRoadmapCatalog.Find(version).FullDisplayName;
}
