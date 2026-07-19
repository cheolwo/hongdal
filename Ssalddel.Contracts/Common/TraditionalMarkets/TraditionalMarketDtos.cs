namespace Ssalddel.Contracts.Common.TraditionalMarkets;

public sealed class TraditionalMarketSearchRequest
{
    public string? Keyword { get; set; }
    public string? Province { get; set; }
    public string? CityCounty { get; set; }
    public string? MarketType { get; set; }
    public bool? HasSharedLogisticsWarehouse { get; set; }
    public bool? HasDedicatedParking { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class TraditionalMarketListResponse
{
    public IReadOnlyList<TraditionalMarketResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? LastSyncedAtUtc { get; set; }
    public string SourceDatasetKey { get; set; } = string.Empty;
    public DateOnly? SourceReferenceDate { get; set; }
}

public sealed class TraditionalMarketResponse
{
    public string MarketCode { get; set; } = string.Empty;
    public string CommunityScopeKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MarketType { get; set; } = string.Empty;
    public string LotNumberAddress { get; set; } = string.Empty;
    public string RoadAddress { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string CityCounty { get; set; } = string.Empty;
    public TraditionalMarketFacilityResponse Facilities { get; set; } = new();
    public int AvailableFacilityCount { get; set; }
    public bool IsActive { get; set; }
    public DateOnly SourceReferenceDate { get; set; }
    public DateTime LastSyncedAtUtc { get; set; }
}

public sealed class TraditionalMarketFacilityResponse
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

public sealed class TraditionalMarketSyncResponse
{
    public Guid RunId { get; set; }
    public string Status { get; set; } = TraditionalMarketSyncStatuses.Running;
    public string SourceDatasetKey { get; set; } = string.Empty;
    public DateOnly SourceReferenceDate { get; set; }
    public int FetchedCount { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int UnchangedCount { get; set; }
    public int DeactivatedCount { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public static class TraditionalMarketSyncStatuses
{
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

public static class TraditionalMarketCommunityScopes
{
    public const string ScopeType = "TraditionalMarket";

    public static string Create(string marketCode)
        => $"traditional-market:{marketCode.Trim().ToLowerInvariant()}";
}
