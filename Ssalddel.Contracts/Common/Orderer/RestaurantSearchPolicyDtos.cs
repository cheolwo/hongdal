namespace Ssalddel.Contracts.Common.Orderer;

public static class RestaurantSearchPolicyDefaults
{
    public const int DefaultRadiusKm = 7;
    public const int MinRadiusKm = 1;
    public const int MaxRadiusKm = 10;
    public const double RadiusStepKm = 0.5d;
    public const int RecommendedRadiusKm = 7;
    public const int DeliveryFeeCautionRadiusKm = 10;

    public static List<double> CreateQuickRadiusOptions()
        => [3, 5, 7, 10];
}

public sealed class RestaurantSearchPolicyDto
{
    public double DefaultRadiusKm { get; set; } = RestaurantSearchPolicyDefaults.DefaultRadiusKm;

    public double MinRadiusKm { get; set; } = RestaurantSearchPolicyDefaults.MinRadiusKm;

    public double MaxRadiusKm { get; set; } = RestaurantSearchPolicyDefaults.MaxRadiusKm;

    public double RadiusStepKm { get; set; } = RestaurantSearchPolicyDefaults.RadiusStepKm;

    public List<double> QuickRadiusOptions { get; set; } = RestaurantSearchPolicyDefaults.CreateQuickRadiusOptions();

    public double RecommendedRadiusKm { get; set; } = RestaurantSearchPolicyDefaults.RecommendedRadiusKm;

    public double DeliveryFeeCautionRadiusKm { get; set; } = RestaurantSearchPolicyDefaults.DeliveryFeeCautionRadiusKm;

    public string? UpdatedBy { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class RestaurantSearchPolicyUpdateRequest
{
    public double DefaultRadiusKm { get; set; }

    public double MinRadiusKm { get; set; }

    public double MaxRadiusKm { get; set; }

    public double RadiusStepKm { get; set; }

    public List<double> QuickRadiusOptions { get; set; } = [];

    public double RecommendedRadiusKm { get; set; }

    public double DeliveryFeeCautionRadiusKm { get; set; }
}
