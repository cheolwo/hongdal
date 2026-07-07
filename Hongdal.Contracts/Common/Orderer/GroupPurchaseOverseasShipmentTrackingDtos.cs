namespace Hongdal.Contracts.Common.Orderer;

public static class GroupPurchaseShipmentTransportModeCode
{
    public const string Ocean = "Ocean";
    public const string Air = "Air";
}

public static class GroupPurchaseShipmentDocumentTypeCode
{
    public const string BillOfLading = "BillOfLading";
    public const string AirWaybill = "AirWaybill";
}

public static class GroupPurchaseShipmentStatusCode
{
    public const string DocumentRegistered = "DocumentRegistered";
    public const string OverseasPacked = "OverseasPacked";
    public const string LoadedOnVesselOrFlight = "LoadedOnVesselOrFlight";
    public const string InTransit = "InTransit";
    public const string ArrivedAtPort = "ArrivedAtPort";
    public const string CustomsInProgress = "CustomsInProgress";
    public const string CustomsCleared = "CustomsCleared";
    public const string LogisticsProxyInboundReady = "LogisticsProxyInboundReady";
    public const string LogisticsProxyInboundCompleted = "LogisticsProxyInboundCompleted";
    public const string SalesListingReady = "SalesListingReady";
    public const string SalesChannelListed = "SalesChannelListed";
    public const string OutboundBatchReady = "OutboundBatchReady";
    public const string DomesticWarehouseReceived = "DomesticWarehouseReceived";
    public const string DomesticCarrierPickup = "DomesticCarrierPickup";
    public const string ApartmentDropoff = "ApartmentDropoff";
    public const string DistributionInProgress = "DistributionInProgress";
    public const string Completed = "Completed";
    public const string Exception = "Exception";
}

public sealed class GroupPurchaseOverseasShipmentTrackingQuery
{
    public string? GroupPurchaseId { get; set; }
    public string? OrdererGroupScopeKey { get; set; }
    public string? DocumentManagementNumber { get; set; }
    public string? TransportDocumentNumber { get; set; }
    public string? CurrentStatusCode { get; set; }
}

public sealed class GroupPurchaseOverseasShipmentTrackingDto
{
    public string TrackingId { get; set; } = string.Empty;
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string TransportDocumentType { get; set; } = GroupPurchaseShipmentDocumentTypeCode.BillOfLading;
    public string TransportDocumentNumber { get; set; } = string.Empty;
    public string TransportMode { get; set; } = GroupPurchaseShipmentTransportModeCode.Ocean;
    public string CarrierName { get; set; } = string.Empty;
    public string VesselName { get; set; } = string.Empty;
    public string VoyageNumber { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginCountryCode { get; set; } = string.Empty;
    public string OriginPortCode { get; set; } = string.Empty;
    public string DestinationPortCode { get; set; } = string.Empty;
    public DateTime? EstimatedDepartureAtUtc { get; set; }
    public DateTime? ActualDepartureAtUtc { get; set; }
    public DateTime? EstimatedArrivalAtUtc { get; set; }
    public DateTime? ActualArrivalAtUtc { get; set; }
    public string CurrentStatusCode { get; set; } = GroupPurchaseShipmentStatusCode.DocumentRegistered;
    public string CurrentLocationSummary { get; set; } = string.Empty;
    public DateTime? LastMilestoneAtUtc { get; set; }
    public IReadOnlyList<GroupPurchaseOverseasShipmentTrackingEventDto> Events { get; set; } = [];
    public string AdminMemo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseOverseasShipmentPublicDto
{
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string TransportDocumentType { get; set; } = GroupPurchaseShipmentDocumentTypeCode.BillOfLading;
    public string TransportDocumentNumber { get; set; } = string.Empty;
    public string TransportMode { get; set; } = GroupPurchaseShipmentTransportModeCode.Ocean;
    public string CarrierName { get; set; } = string.Empty;
    public string VesselName { get; set; } = string.Empty;
    public string VoyageNumber { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginCountryCode { get; set; } = string.Empty;
    public string OriginPortCode { get; set; } = string.Empty;
    public string DestinationPortCode { get; set; } = string.Empty;
    public DateTime? EstimatedDepartureAtUtc { get; set; }
    public DateTime? ActualDepartureAtUtc { get; set; }
    public DateTime? EstimatedArrivalAtUtc { get; set; }
    public DateTime? ActualArrivalAtUtc { get; set; }
    public string CurrentStatusCode { get; set; } = GroupPurchaseShipmentStatusCode.DocumentRegistered;
    public string CurrentLocationSummary { get; set; } = string.Empty;
    public DateTime? LastMilestoneAtUtc { get; set; }
    public IReadOnlyList<GroupPurchaseOverseasShipmentPublicEventDto> Events { get; set; } = [];
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseOverseasShipmentPublicEventDto
{
    public string EventCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string LocationSummary { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string SourcePartyCode { get; set; } = string.Empty;
}

public sealed class GroupPurchaseOverseasShipmentTrackingEventDto
{
    public string EventCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string LocationSummary { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string SourcePartyCode { get; set; } = string.Empty;
    public string EvidenceReference { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public bool IsOrdererVisible { get; set; } = true;
}

public sealed class GroupPurchaseOverseasShipmentTrackingUpsertRequest
{
    public string? TrackingId { get; set; }
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string TransportDocumentType { get; set; } = GroupPurchaseShipmentDocumentTypeCode.BillOfLading;
    public string TransportDocumentNumber { get; set; } = string.Empty;
    public string TransportMode { get; set; } = GroupPurchaseShipmentTransportModeCode.Ocean;
    public string CarrierName { get; set; } = string.Empty;
    public string VesselName { get; set; } = string.Empty;
    public string VoyageNumber { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginCountryCode { get; set; } = string.Empty;
    public string OriginPortCode { get; set; } = string.Empty;
    public string DestinationPortCode { get; set; } = string.Empty;
    public DateTime? EstimatedDepartureAtUtc { get; set; }
    public DateTime? ActualDepartureAtUtc { get; set; }
    public DateTime? EstimatedArrivalAtUtc { get; set; }
    public DateTime? ActualArrivalAtUtc { get; set; }
    public string CurrentStatusCode { get; set; } = GroupPurchaseShipmentStatusCode.DocumentRegistered;
    public string CurrentLocationSummary { get; set; } = string.Empty;
    public DateTime? LastMilestoneAtUtc { get; set; }
    public IReadOnlyList<GroupPurchaseOverseasShipmentTrackingEventDto> Events { get; set; } = [];
    public string AdminMemo { get; set; } = string.Empty;
}

public sealed class GroupPurchaseOverseasShipmentTrackingEventAppendRequest
{
    public string EventCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string LocationSummary { get; set; } = string.Empty;
    public DateTime? OccurredAtUtc { get; set; }
    public string SourcePartyCode { get; set; } = string.Empty;
    public string EvidenceReference { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public bool IsOrdererVisible { get; set; } = true;
}

public sealed class GroupPurchaseOverseasShipmentCustomsSyncRequest
{
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string CustomsCargoManagementNumber { get; set; } = string.Empty;
    public string MasterBillOfLadingNumber { get; set; } = string.Empty;
    public string HouseBillOfLadingNumber { get; set; } = string.Empty;
    public int? BillOfLadingYear { get; set; }
    public bool IsOrdererVisible { get; set; } = true;
}

public sealed class GroupPurchaseOverseasShipmentCustomsSyncResult
{
    public bool Synced { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CustomsStageName { get; set; } = string.Empty;
    public string CustomsLocationSummary { get; set; } = string.Empty;
    public DateTimeOffset QueriedAtUtc { get; set; }
    public GroupPurchaseOverseasShipmentTrackingDto? Shipment { get; set; }
}
