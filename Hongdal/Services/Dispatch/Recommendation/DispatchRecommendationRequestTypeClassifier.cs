using Hongdal.Hubs;

namespace 홍달.Services.Dispatch.Recommendation;

internal static class DispatchRecommendationRequestTypeClassifier
{
    private const string GeneralCargoTransportCode = "GeneralCargoTransport";
    private const string GroupPurchaseCargoTransportCode = "GroupPurchaseCargoTransport";
    private const string GeneralCargoTransportLabel = "일반 화물";
    private const string GroupPurchaseCargoTransportLabel = "공동주문 운송";
    private const string GeneralWorkScopeLabel = "상하차";
    private const string GroupPurchaseWorkScopeLabel = "상하차 + 세대배송 범위 확인";

    public static DispatchRecommendationRequestTypeMetadata Classify(string? sourceType)
    {
        var isGroupPurchase = IsGroupPurchaseCargoTransport(sourceType);
        return isGroupPurchase
            ? new DispatchRecommendationRequestTypeMetadata(
                GroupPurchaseCargoTransportCode,
                GroupPurchaseCargoTransportLabel,
                true,
                true,
                null,
                GroupPurchaseWorkScopeLabel)
            : new DispatchRecommendationRequestTypeMetadata(
                GeneralCargoTransportCode,
                GeneralCargoTransportLabel,
                false,
                false,
                null,
                GeneralWorkScopeLabel);
    }

    public static void ApplyTo(DispatchRecommendationDto target, string? sourceType)
    {
        var metadata = Classify(sourceType);
        target.운송의뢰유형코드 = metadata.RequestTypeCode;
        target.운송의뢰유형표시 = metadata.RequestTypeLabel;
        target.공동주문운송여부 = metadata.IsGroupPurchaseTransport;
        target.세대배송포함여부 = metadata.IncludesApartmentUnitDelivery;
        target.세대배송건수 = metadata.ApartmentUnitDeliveryCount;
        target.세대배송업무표시 = metadata.ApartmentUnitDeliveryScopeLabel;
    }

    private static bool IsGroupPurchaseCargoTransport(string? sourceType)
        => string.Equals(sourceType, "ImportCargoTransport", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(sourceType, "FclCargoTransport", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(sourceType, "LclCargoTransport", StringComparison.OrdinalIgnoreCase);
}

internal sealed record DispatchRecommendationRequestTypeMetadata(
    string RequestTypeCode,
    string RequestTypeLabel,
    bool IsGroupPurchaseTransport,
    bool IncludesApartmentUnitDelivery,
    int? ApartmentUnitDeliveryCount,
    string ApartmentUnitDeliveryScopeLabel);
