using Hongdal.Contracts.Common.Orderer;
using 홍달.Services.External.Customs;
using 홍달.도메인.통관;

namespace Hongdal.Services.Orderer;

public static class GroupPurchaseOverseasShipmentCustomsStageMapper
{
    public static GroupPurchaseOverseasShipmentTrackingEventAppendRequest ToShipmentEvent(
        화물통관진행조회Result source,
        bool isOrdererVisible)
        => new()
        {
            EventCode = ToShipmentStatusCode(source.진행단계),
            DisplayName = ResolveDisplayName(source.처리단계명),
            LocationSummary = ResolveLocationSummary(source.장치장명),
            OccurredAtUtc = source.조회시각.UtcDateTime,
            SourcePartyCode = GroupPurchaseLogisticsWorkflowPartyCode.CustomsBroker,
            EvidenceReference = "KCS CargoTracking OpenAPI",
            Memo = "관세청 화물통관진행정보 조회 결과를 공동구매 해외 선적 원장에 반영했습니다.",
            IsOrdererVisible = isOrdererVisible
        };

    public static string ToShipmentStatusCode(통관진행단계 stage)
        => stage switch
        {
            통관진행단계.반입전 => GroupPurchaseShipmentStatusCode.InTransit,
            통관진행단계.반입완료 => GroupPurchaseShipmentStatusCode.ArrivedAtPort,
            통관진행단계.신고진행중 => GroupPurchaseShipmentStatusCode.CustomsInProgress,
            통관진행단계.검사대상 => GroupPurchaseShipmentStatusCode.CustomsInProgress,
            통관진행단계.신고수리 => GroupPurchaseShipmentStatusCode.CustomsCleared,
            통관진행단계.반출가능 => GroupPurchaseShipmentStatusCode.CustomsCleared,
            통관진행단계.반출완료 => GroupPurchaseShipmentStatusCode.CustomsCleared,
            통관진행단계.완료 => GroupPurchaseShipmentStatusCode.CustomsCleared,
            통관진행단계.보류 => GroupPurchaseShipmentStatusCode.Exception,
            _ => GroupPurchaseShipmentStatusCode.CustomsInProgress
        };

    public static string ResolveLocationSummary(string? customsLocation)
        => string.IsNullOrWhiteSpace(customsLocation)
            ? "관세청 위치 정보 미제공"
            : customsLocation.Trim();

    private static string ResolveDisplayName(string? customsStageName)
        => string.IsNullOrWhiteSpace(customsStageName)
            ? "관세청 통관 상태 조회"
            : $"관세청 통관 상태: {customsStageName.Trim()}";
}
