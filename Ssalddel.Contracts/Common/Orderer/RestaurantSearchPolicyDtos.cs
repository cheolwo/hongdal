namespace Ssalddel.Contracts.Common.Orderer;

public sealed class RestaurantSearchPolicyDto
{
    public double DefaultRadiusKm { get; set; } = 7;

    public double MinRadiusKm { get; set; } = 1;

    public double MaxRadiusKm { get; set; } = 10;

    public double RadiusStepKm { get; set; } = 0.5;

    public List<double> QuickRadiusOptions { get; set; } = [3, 5, 7, 10];

    public double RecommendedRadiusKm { get; set; } = 7;

    public double DeliveryFeeCautionRadiusKm { get; set; } = 10;

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
