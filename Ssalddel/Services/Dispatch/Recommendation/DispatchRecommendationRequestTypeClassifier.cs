using Ssalddel.Hubs;
using Ssalddel.Contracts.Common.Orderer;

namespace 살뜰.Services.Dispatch.Recommendation;

public static class DispatchRecommendationRequestTypeClassifier
{
    private const string GeneralCargoTransportCode = "GeneralCargoTransport";
    private const string GroupPurchaseCargoTransportCode = "GroupPurchaseCargoTransport";
    private const string GeneralCargoTransportLabel = "일반 화물";
    private const string GroupPurchaseCargoTransportLabel = "같이 주문 운송";
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

    public static DispatchRecommendationRequestTypeMetadata Classify(PlatformEntrustedDispatchQueueDraftDto draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        return Classify(
            draft.SourceRequestType,
            draft.DestinationTypeCode,
            draft.DriverPerformsApartmentUnitDistribution,
            draft.ApartmentUnitDeliveryCount);
    }

    public static DispatchRecommendationRequestTypeMetadata Classify(
        string? sourceType,
        string? destinationTypeCode,
        bool? driverPerformsApartmentUnitDistribution,
        int? apartmentUnitDeliveryCount)
    {
        var metadata = Classify(sourceType);
        if (!metadata.IsGroupPurchaseTransport)
        {
            return metadata;
        }

        var includesApartmentUnitDelivery =
            string.Equals(destinationTypeCode, 공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution, StringComparison.OrdinalIgnoreCase) &&
            driverPerformsApartmentUnitDistribution == true;

        return metadata with
        {
            IncludesApartmentUnitDelivery = includesApartmentUnitDelivery,
            ApartmentUnitDeliveryCount = includesApartmentUnitDelivery ? apartmentUnitDeliveryCount : null,
            ApartmentUnitDeliveryScopeLabel = BuildGroupPurchaseWorkScopeLabel(
                destinationTypeCode,
                includesApartmentUnitDelivery,
                apartmentUnitDeliveryCount)
        };
    }

    public static void ApplyTo(DispatchRecommendationDto target, string? sourceType)
    {
        var metadata = Classify(sourceType);
        ApplyTo(target, metadata);
    }

    public static void ApplyTo(DispatchRecommendationDto target, PlatformEntrustedDispatchQueueDraftDto draft)
    {
        var metadata = Classify(draft);
        ApplyTo(target, metadata);
    }

    public static void ApplyTo(
        DispatchRecommendationDto target,
        string? sourceType,
        string? destinationTypeCode,
        bool? driverPerformsApartmentUnitDistribution,
        int? apartmentUnitDeliveryCount)
    {
        var metadata = Classify(
            sourceType,
            destinationTypeCode,
            driverPerformsApartmentUnitDistribution,
            apartmentUnitDeliveryCount);
        ApplyTo(target, metadata);
    }

    private static void ApplyTo(DispatchRecommendationDto target, DispatchRecommendationRequestTypeMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(target);

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

    private static string BuildGroupPurchaseWorkScopeLabel(
        string? destinationTypeCode,
        bool includesApartmentUnitDelivery,
        int? apartmentUnitDeliveryCount)
    {
        if (includesApartmentUnitDelivery)
        {
            return apartmentUnitDeliveryCount is > 0
                ? $"상하차 + 세대 문앞 {apartmentUnitDeliveryCount.Value:N0}건"
                : "상하차 + 세대 문앞 배송";
        }

        return destinationTypeCode switch
        {
            공동구매국내운송도착지유형코드.ThreePlWarehouse => "상하차 + 3PL 입고",
            공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution => "상하차 + 공동주택 거점 하차",
            공동구매국내운송도착지유형코드.OrdererGroupRepresentativeDropoff => "상하차 + 집단 대표 인계",
            _ => GroupPurchaseWorkScopeLabel
        };
    }
}

public sealed record DispatchRecommendationRequestTypeMetadata(
    string RequestTypeCode,
    string RequestTypeLabel,
    bool IsGroupPurchaseTransport,
    bool IncludesApartmentUnitDelivery,
    int? ApartmentUnitDeliveryCount,
    string ApartmentUnitDeliveryScopeLabel);
