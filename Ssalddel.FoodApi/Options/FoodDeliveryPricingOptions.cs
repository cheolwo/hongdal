namespace Ssalddel.FoodApi.Options;

public sealed class FoodDeliveryPricingOptions
{
    public const string SectionName = "FoodDeliveryPricing";

    public decimal BaseFee { get; set; } = 3000m;
    public int IncludedDistanceMeters { get; set; } = 1000;
    public int DistanceUnitMeters { get; set; } = 100;
    public decimal DistanceUnitFee { get; set; } = 120m;
    public decimal MinimumFee { get; set; } = 3000m;

    public decimal DriverBasePayout { get; set; } = 2500m;
    public decimal DriverDistanceUnitPayout { get; set; } = 90m;
    public decimal DriverMinimumPayout { get; set; } = 2500m;
}
