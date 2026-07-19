namespace Ssalddel.Services.LogisticsProcessing.Warehouse;

public interface IWarehouseDistanceCostEstimator
{
    WarehouseDistanceCostEstimate Estimate(
        decimal? warehouseLatitude,
        decimal? warehouseLongitude,
        decimal? destinationLatitude,
        decimal? destinationLongitude);
}

public sealed record WarehouseDistanceCostEstimate(decimal? DistanceKm, decimal? EstimatedTransportCost);

public sealed class WarehouseDistanceCostEstimator : IWarehouseDistanceCostEstimator
{
    private const decimal BaseCost = 3000m;
    private const decimal CostPerKm = 1300m;

    public WarehouseDistanceCostEstimate Estimate(
        decimal? warehouseLatitude,
        decimal? warehouseLongitude,
        decimal? destinationLatitude,
        decimal? destinationLongitude)
    {
        if (!warehouseLatitude.HasValue
            || !warehouseLongitude.HasValue
            || !destinationLatitude.HasValue
            || !destinationLongitude.HasValue)
        {
            return new WarehouseDistanceCostEstimate(null, null);
        }

        var distanceKm = CalculateDistanceKm(
            (double)warehouseLatitude.Value,
            (double)warehouseLongitude.Value,
            (double)destinationLatitude.Value,
            (double)destinationLongitude.Value);

        var roundedDistance = Math.Round((decimal)distanceKm, 2, MidpointRounding.AwayFromZero);
        var cost = BaseCost + Math.Ceiling(roundedDistance) * CostPerKm;
        return new WarehouseDistanceCostEstimate(roundedDistance, cost);
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
