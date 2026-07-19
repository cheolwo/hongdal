using Ssalddel.Contracts.Common.Operations;

namespace Ssalddel.Tests.Contracts.Common.Operations;

public sealed class OperatingMarketMeasurementConverterTests
{
    [Fact]
    public void ConvertDistance_MilesAndKilometers_RoundTrips()
    {
        var miles = OperatingMarketMeasurementConverter.ConvertDistance(
            160.9344m,
            OperatingDistanceUnitCodes.Kilometer,
            OperatingDistanceUnitCodes.Mile);
        var kilometers = OperatingMarketMeasurementConverter.ConvertDistance(
            miles,
            OperatingDistanceUnitCodes.Mile,
            OperatingDistanceUnitCodes.Kilometer);

        Assert.Equal(100m, miles);
        Assert.Equal(160.9344m, kilometers);
    }

    [Fact]
    public void ConvertWeight_PoundsAndKilograms_RoundTrips()
    {
        var kilograms = OperatingMarketMeasurementConverter.ConvertWeight(
            100m,
            OperatingWeightUnitCodes.Pound,
            OperatingWeightUnitCodes.Kilogram);
        var pounds = OperatingMarketMeasurementConverter.ConvertWeight(
            kilograms,
            OperatingWeightUnitCodes.Kilogram,
            OperatingWeightUnitCodes.Pound);

        Assert.Equal(45.359237m, kilograms);
        Assert.Equal(100m, pounds);
    }

    [Fact]
    public void MarketHelpers_UseTheSelectedMarketUnits()
    {
        Assert.Equal(10m, OperatingMarketMeasurementConverter.DistanceFromKilometers(10m, OperatingMarketCodes.Korea));
        Assert.Equal(
            10m / 1.609344m,
            OperatingMarketMeasurementConverter.DistanceFromKilometers(10m, OperatingMarketCodes.UnitedStates));
        Assert.Equal(
            10m / 0.45359237m,
            OperatingMarketMeasurementConverter.WeightFromKilograms(10m, OperatingMarketCodes.UnitedStates));
    }

    [Fact]
    public void NegativeMeasurement_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OperatingMarketMeasurementConverter.ConvertWeight(-1m, "kg", "lb"));
    }
}
