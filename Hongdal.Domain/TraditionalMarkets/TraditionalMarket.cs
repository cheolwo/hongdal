namespace Hongdal.Domain.TraditionalMarkets;

public sealed class TraditionalMarket
{
    public string MarketCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MarketType { get; set; } = string.Empty;
    public string LotNumberAddress { get; set; } = string.Empty;
    public string RoadAddress { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string CityCounty { get; set; } = string.Empty;
    public TraditionalMarketFacilities Facilities { get; set; } = new();
    public string SourceDatasetKey { get; set; } = string.Empty;
    public DateOnly SourceReferenceDate { get; set; }
    public string SourceHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime LastSyncedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class TraditionalMarketFacilities
{
    public bool? HasArcade { get; set; }
    public bool? HasElevatorOrEscalator { get; set; }
    public bool? HasCustomerSupportCenter { get; set; }
    public bool? HasSprinkler { get; set; }
    public bool? HasFireDetector { get; set; }
    public bool? HasChildrenPlayroom { get; set; }
    public bool? HasCallCenter { get; set; }
    public bool? HasCustomerLounge { get; set; }
    public bool? HasNursingCenter { get; set; }
    public bool? HasLocker { get; set; }
    public bool? HasBicycleStorage { get; set; }
    public bool? HasSportsFacility { get; set; }
    public bool? HasLibrary { get; set; }
    public bool? HasShoppingCart { get; set; }
    public bool? HasForeignVisitorCenter { get; set; }
    public bool? HasCustomerPath { get; set; }
    public bool? HasBroadcastCenter { get; set; }
    public bool? HasCultureClassroom { get; set; }
    public bool? HasSharedLogisticsWarehouse { get; set; }
    public bool? HasDedicatedParking { get; set; }
    public bool? HasTrainingRoom { get; set; }
    public bool? HasMeetingRoom { get; set; }
    public bool? HasAed { get; set; }
}
