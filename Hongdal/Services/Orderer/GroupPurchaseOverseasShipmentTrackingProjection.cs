using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Services.Orderer;

public static class GroupPurchaseOverseasShipmentTrackingProjection
{
    public static GroupPurchaseOverseasShipmentPublicDto ToPublicDto(
        GroupPurchaseOverseasShipmentTrackingDto source)
        => new()
        {
            GroupPurchaseId = source.GroupPurchaseId,
            OrdererGroupScopeKey = source.OrdererGroupScopeKey,
            OrdererGroupScopeName = source.OrdererGroupScopeName,
            ProductSummary = source.ProductSummary,
            DocumentManagementNumber = source.DocumentManagementNumber,
            TransportDocumentType = source.TransportDocumentType,
            TransportDocumentNumber = source.TransportDocumentNumber,
            TransportMode = source.TransportMode,
            CarrierName = source.CarrierName,
            VesselName = source.VesselName,
            VoyageNumber = source.VoyageNumber,
            FlightNumber = source.FlightNumber,
            OriginCountryCode = source.OriginCountryCode,
            OriginPortCode = source.OriginPortCode,
            DestinationPortCode = source.DestinationPortCode,
            EstimatedDepartureAtUtc = source.EstimatedDepartureAtUtc,
            ActualDepartureAtUtc = source.ActualDepartureAtUtc,
            EstimatedArrivalAtUtc = source.EstimatedArrivalAtUtc,
            ActualArrivalAtUtc = source.ActualArrivalAtUtc,
            CurrentStatusCode = source.CurrentStatusCode,
            CurrentLocationSummary = source.CurrentLocationSummary,
            LastMilestoneAtUtc = source.LastMilestoneAtUtc,
            Events = source.Events
                .Where(x => x.IsOrdererVisible)
                .OrderBy(x => x.OccurredAtUtc)
                .Select(ToPublicDto)
                .ToArray(),
            UpdatedAtUtc = source.UpdatedAtUtc
        };

    private static GroupPurchaseOverseasShipmentPublicEventDto ToPublicDto(
        GroupPurchaseOverseasShipmentTrackingEventDto source)
        => new()
        {
            EventCode = source.EventCode,
            DisplayName = source.DisplayName,
            LocationSummary = source.LocationSummary,
            OccurredAtUtc = source.OccurredAtUtc,
            SourcePartyCode = source.SourcePartyCode
        };
}
