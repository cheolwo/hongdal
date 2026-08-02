namespace Ssalddel.Contracts.Common.TraditionalMarkets;

public sealed class TraditionalMarketLogisticsHubSearchRequest
{
    public string? Keyword { get; set; }
    public string? Province { get; set; }
    public string? CityCounty { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class TraditionalMarketLogisticsHubListResponse
{
    public IReadOnlyList<TraditionalMarketLogisticsHubResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class TraditionalMarketLogisticsHubResponse
{
    public string MarketCode { get; set; } = string.Empty;
    public string MarketName { get; set; } = string.Empty;
    public string CommunityScopeKey { get; set; } = string.Empty;
    public string HubReferenceKey { get; set; } = string.Empty;
    public string RoadAddress { get; set; } = string.Empty;
    public string LotNumberAddress { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string CityCounty { get; set; } = string.Empty;
    public string Status { get; set; } = TraditionalMarketLogisticsHubStatuses.Candidate;
    public string OperatorOrganizationName { get; set; } = string.Empty;
    public decimal ServiceRadiusKm { get; set; }
    public int DailyGroupPurchaseCapacity { get; set; }
    public bool SupportsBulkReceiving { get; set; }
    public bool SupportsSorting { get; set; }
    public bool SupportsResidentPickup { get; set; }
    public bool SupportsLastMileDelivery { get; set; }
    public bool SupportsRefrigeratedStorage { get; set; }
    public bool SupportsFrozenStorage { get; set; }
    public string ReceivingWindow { get; set; } = string.Empty;
    public string PickupWindow { get; set; } = string.Empty;
    public string OperatingNotes { get; set; } = string.Empty;
    public bool HasOperatorConsent { get; set; }
    public DateTime? OperatorConsentedAtUtc { get; set; }
    public DateTime? SiteVerifiedAtUtc { get; set; }
    public TraditionalMarketMapAnchorResponse? MapAnchor { get; set; }
    public string StatusReason { get; set; } = string.Empty;
    public long Revision { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime StatusChangedAtUtc { get; set; }
}

public sealed class TraditionalMarketLogisticsHubUpsertRequest
{
    public string OperatorOrganizationName { get; set; } = string.Empty;
    public decimal ServiceRadiusKm { get; set; }
    public int DailyGroupPurchaseCapacity { get; set; }
    public bool SupportsBulkReceiving { get; set; }
    public bool SupportsSorting { get; set; }
    public bool SupportsResidentPickup { get; set; }
    public bool SupportsLastMileDelivery { get; set; }
    public bool SupportsRefrigeratedStorage { get; set; }
    public bool SupportsFrozenStorage { get; set; }
    public string ReceivingWindow { get; set; } = string.Empty;
    public string PickupWindow { get; set; } = string.Empty;
    public string OperatingNotes { get; set; } = string.Empty;
    public bool HasOperatorConsent { get; set; }
    public bool IsSiteVerified { get; set; }
    public TraditionalMarketMapAnchorUpsertRequest? MapAnchor { get; set; }
    public long? ExpectedRevision { get; set; }
}

public sealed class TraditionalMarketMapAnchorUpsertRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string LocationPrecisionCode { get; set; } = TraditionalMarketMapLocationPrecisionCodes.MarketAddressGeocoded;
    public string SourceName { get; set; } = string.Empty;
    public string SourceHref { get; set; } = string.Empty;
    public bool ConfirmCoordinateVerification { get; set; }
}

public sealed class TraditionalMarketMapAnchorResponse
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string LocationPrecisionCode { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceHref { get; set; } = string.Empty;
    public DateTime VerifiedAtUtc { get; set; }
}

public sealed class TraditionalMarketMapMarkerResponse
{
    public string MarketCode { get; set; } = string.Empty;
    public string MarketName { get; set; } = string.Empty;
    public string CommunityScopeKey { get; set; } = string.Empty;
    public string HubReferenceKey { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string CityCounty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ServiceRadiusKm { get; set; }
    public int DailyGroupPurchaseCapacity { get; set; }
    public bool SupportsResidentPickup { get; set; }
    public bool SupportsLastMileDelivery { get; set; }
    public bool SupportsRefrigeratedStorage { get; set; }
    public bool SupportsFrozenStorage { get; set; }
    public TraditionalMarketMapAnchorResponse MapAnchor { get; set; } = new();
}

public static class TraditionalMarketMapLocationPrecisionCodes
{
    public const string MarketAddressGeocoded = "MarketAddressGeocoded";
    public const string MarketSiteRepresentative = "TraditionalMarketSiteRepresentative";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        MarketAddressGeocoded,
        MarketSiteRepresentative
    };
}

public sealed class TraditionalMarketLogisticsHubStatusChangeRequest
{
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public long? ExpectedRevision { get; set; }
}

public static class TraditionalMarketLogisticsHubStatuses
{
    public const string Candidate = "Candidate";
    public const string UnderReview = "UnderReview";
    public const string Pilot = "Pilot";
    public const string Active = "Active";
    public const string Paused = "Paused";
    public const string Closed = "Closed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Candidate,
        UnderReview,
        Pilot,
        Active,
        Paused,
        Closed
    };

    public static readonly IReadOnlySet<string> Public = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pilot,
        Active
    };

    public static string Normalize(string value)
        => All.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase))
           ?? string.Empty;
}

public static class TraditionalMarketLogisticsHubReferences
{
    public const string ReferenceType = "TraditionalMarketLogisticsHub";

    public static string Create(string marketCode)
        => $"traditional-market-hub:{marketCode.Trim().ToLowerInvariant()}";
}
