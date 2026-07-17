namespace Hongdal.Contracts.Common.Community;

public static class CommunityDynamicTopicCodes
{
    public const string Food = "food";
    public const string Cargo = "cargo";

    public static IReadOnlyList<string> All { get; } = [Food, Cargo];

    public static bool IsSupported(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && All.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}

public sealed class CommunityDynamicTopicResponse
{
    public string TopicKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsDerivedFromPost { get; set; } = true;
    public string FeedEndpoint { get; set; } = string.Empty;
    public IReadOnlyList<string> MatchedSignals { get; set; } = [];
}

public sealed class CommunityDynamicTopicFeedResponse
{
    public string TopicKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GenerationPolicy { get; set; } = string.Empty;
    public IReadOnlyList<CommunityDynamicTopicFeedItemResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class CommunityDynamicTopicFeedItemResponse
{
    public long PostId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyList<string> MatchedSignals { get; set; } = [];
}

public sealed class CommunityPostContextDiscoveryRequest
{
    public decimal? CurrentLatitude { get; set; }
    public decimal? CurrentLongitude { get; set; }
    public decimal RadiusKm { get; set; } = 7m;
    public bool ConfirmTransientLocationUse { get; set; }
}

public sealed class CommunityPostContextDiscoveryResponse
{
    public long PostId { get; set; }
    public IReadOnlyList<CommunityDynamicTopicResponse> DynamicTopics { get; set; } = [];
    public CommunityTransientLocationPolicyResponse LocationPolicy { get; set; } = new();
    public IReadOnlyList<CommunityNearbyRestaurantCandidateResponse> NearbyRestaurants { get; set; } = [];
    public IReadOnlyList<CommunityFreightProviderCandidateResponse> FreightProviderCandidates { get; set; } = [];
    public IReadOnlyList<CommunityPublicFreightCandidateResponse> PublicFreightCandidates { get; set; } = [];
    public bool InformationOnly { get; set; } = true;
    public bool IsBrokerageEnabled { get; set; }
    public bool AutomaticallySelectsProvider { get; set; }
    public bool AutomaticallyDispatchesFreight { get; set; }
    public string FacilitatorBoundaryNotice { get; set; } = string.Empty;
}

public sealed class CommunityTransientLocationPolicyResponse
{
    public decimal MaximumRadiusKm { get; set; } = 7m;
    public decimal AppliedRadiusKm { get; set; } = 7m;
    public bool RequiresExplicitConsent { get; set; } = true;
    public bool ConsentConfirmed { get; set; }
    public bool LocationProvided { get; set; }
    public bool LocationPersisted { get; set; }
    public bool RestaurantSourceAvailable { get; set; }
    public bool RestaurantSourceIsSimulation { get; set; } = true;
    public string Notice { get; set; } = string.Empty;
}

public sealed class CommunityNearbyRestaurantCandidateResponse
{
    public long RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AreaSummary { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool OrderAvailable { get; set; }
    public string SourceCode { get; set; } = string.Empty;
}

public sealed class CommunityFreightProviderCandidateResponse
{
    public string CandidateKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public bool PlatformRoleVerified { get; set; }
    public bool ExternalLicenseVerificationRequired { get; set; } = true;
    public string VerificationNotice { get; set; } = string.Empty;
}

public sealed class CommunityPublicFreightCandidateResponse
{
    public string CandidateKey { get; set; } = string.Empty;
    public string CargoType { get; set; } = string.Empty;
    public decimal? CargoWeightKg { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string PickupAreaSummary { get; set; } = string.Empty;
    public string DropoffAreaSummary { get; set; } = string.Empty;
    public DateTime? PickupWindowStartUtc { get; set; }
    public bool IsExplicitPublicDispatch { get; set; }
    public string Notice { get; set; } = string.Empty;
}
