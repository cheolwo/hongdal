namespace Ssalddel.FoodApi.Application.Pricing;

public sealed class FoodDeliveryPricingService : IFoodDeliveryPricingService
{
    private readonly IFoodDeliveryPricingPolicyStore _policyStore;

    public FoodDeliveryPricingService(IFoodDeliveryPricingPolicyStore policyStore)
    {
        _policyStore = policyStore;
    }

    public FoodDeliveryFareQuote Quote(int distanceMeters)
    {
        var options = _policyStore.Get();
        var normalizedDistance = Math.Max(0, distanceMeters);
        var billableDistance = Math.Max(0, normalizedDistance - Math.Max(0, options.IncludedDistanceMeters));
        var distanceUnits = CalculateDistanceUnits(billableDistance, options.DistanceUnitMeters);

        var platformFee = Math.Max(
            options.MinimumFee,
            options.BaseFee + distanceUnits * options.DistanceUnitFee);

        var driverPayout = Math.Max(
            options.DriverMinimumPayout,
            options.DriverBasePayout + distanceUnits * options.DriverDistanceUnitPayout);

        return new FoodDeliveryFareQuote(
            normalizedDistance,
            platformFee,
            driverPayout,
            platformFee - driverPayout,
            $"{options.IncludedDistanceMeters}m 포함, 이후 {options.DistanceUnitMeters}m당 {options.DistanceUnitFee:N0}원");
    }

    private static int CalculateDistanceUnits(int billableDistanceMeters, int distanceUnitMeters)
    {
        var unit = Math.Max(1, distanceUnitMeters);
        return (int)Math.Ceiling(billableDistanceMeters / (double)unit);
    }
}
