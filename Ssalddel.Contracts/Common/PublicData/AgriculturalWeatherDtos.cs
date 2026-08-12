namespace Ssalddel.Contracts.Common.PublicData;

public static class 기상관측SourceTypeCodes
{
    public const string PublicObservation = "PublicObservation";
}

public static class 기상관측품질Codes
{
    public const string Valid = "Valid";
    public const string Incomplete = "Incomplete";
}

public static class 기상관측공간정밀도Codes
{
    public const string StationObservation = "StationObservation";
}

public sealed record 기상청Asos관측단위(
    string Temperature,
    string Precipitation,
    string SolarRadiation,
    string SunshineDuration,
    string RelativeHumidity);

public sealed record 기상청Asos일관측Snapshot(
    string StableId,
    int Revision,
    string SourceTypeCode,
    string DatasetKey,
    DateOnly ObservationDate,
    DateTimeOffset RetrievedAtUtc,
    string StationId,
    string StationName,
    string SpatialPrecisionCode,
    string? TargetLocationStableId,
    decimal? StationDistanceKm,
    decimal? MeanTemperatureC,
    decimal? MinimumTemperatureC,
    decimal? MaximumTemperatureC,
    decimal? DailyPrecipitationMm,
    decimal? TotalSolarRadiationMjPerSquareMeter,
    decimal? TotalSunshineHours,
    decimal? PossibleSunshineHours,
    decimal? MeanRelativeHumidityPercent,
    기상청Asos관측단위 Units,
    string QualityCode,
    bool CanUseForSimulation,
    IReadOnlyList<string> MissingFieldCodes,
    string RawPayloadHashSha256,
    string SourceHref,
    IReadOnlyList<string> Limitations);
