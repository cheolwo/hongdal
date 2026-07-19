namespace Ssalddel.Contracts.Common.Operations;

public static class OperatingMarketMeasurementConverter
{
    private const decimal KilometersPerMile = 1.609344m;
    private const decimal KilogramsPerPound = 0.45359237m;

    public static decimal ConvertDistance(decimal value, string fromUnitCode, string toUnitCode)
    {
        EnsureNonNegative(value, nameof(value));

        if (SameUnit(fromUnitCode, toUnitCode))
        {
            return value;
        }

        if (SameUnit(fromUnitCode, OperatingDistanceUnitCodes.Kilometer) &&
            SameUnit(toUnitCode, OperatingDistanceUnitCodes.Mile))
        {
            return value / KilometersPerMile;
        }

        if (SameUnit(fromUnitCode, OperatingDistanceUnitCodes.Mile) &&
            SameUnit(toUnitCode, OperatingDistanceUnitCodes.Kilometer))
        {
            return value * KilometersPerMile;
        }

        throw UnsupportedConversion(fromUnitCode, toUnitCode);
    }

    public static decimal ConvertWeight(decimal value, string fromUnitCode, string toUnitCode)
    {
        EnsureNonNegative(value, nameof(value));

        if (SameUnit(fromUnitCode, toUnitCode))
        {
            return value;
        }

        if (SameUnit(fromUnitCode, OperatingWeightUnitCodes.Kilogram) &&
            SameUnit(toUnitCode, OperatingWeightUnitCodes.Pound))
        {
            return value / KilogramsPerPound;
        }

        if (SameUnit(fromUnitCode, OperatingWeightUnitCodes.Pound) &&
            SameUnit(toUnitCode, OperatingWeightUnitCodes.Kilogram))
        {
            return value * KilogramsPerPound;
        }

        throw UnsupportedConversion(fromUnitCode, toUnitCode);
    }

    public static decimal DistanceFromKilometers(decimal kilometers, string marketCode)
        => ConvertDistance(
            kilometers,
            OperatingDistanceUnitCodes.Kilometer,
            OperatingMarketProfileCatalog.Get(marketCode).DistanceUnitCode);

    public static decimal WeightFromKilograms(decimal kilograms, string marketCode)
        => ConvertWeight(
            kilograms,
            OperatingWeightUnitCodes.Kilogram,
            OperatingMarketProfileCatalog.Get(marketCode).WeightUnitCode);

    private static bool SameUnit(string? left, string? right)
        => string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

    private static void EnsureNonNegative(decimal value, string parameterName)
    {
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Measurement values cannot be negative.");
        }
    }

    private static ArgumentException UnsupportedConversion(string fromUnitCode, string toUnitCode)
        => new($"Unsupported measurement conversion: {fromUnitCode} -> {toUnitCode}.");
}
