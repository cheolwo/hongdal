namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class UsdaAms시장가격상태Codes
{
    public const string 완료 = "Completed";
    public const string 자료없음 = "NoData";
}

public sealed class UsdaAms시장가격수집요청
{
    public int Year { get; init; }

    public string? DateTo { get; init; }

    public IReadOnlyList<string> MarketTypes { get; init; } = [];
}

public sealed record UsdaAms시장가격수집응답(
    long CollectionRunId,
    string StatusCode,
    DateOnly DateFrom,
    DateOnly DateTo,
    int DiscoveredReportCount,
    int CompletedSliceCount,
    long FetchedCount,
    long InsertedCount,
    long ExistingCount,
    DateOnly? LatestReferenceDate,
    IReadOnlyList<string> SourceMessages);

public sealed class UsdaAms시장가격ArchiveQuery
{
    public string? SourceKey { get; init; }

    public string? MarketType { get; init; }

    public string? MarketStageCode { get; init; }

    public string? Commodity { get; init; }

    public string? Variety { get; init; }

    public string? MarketLocationState { get; init; }

    public string? Origin { get; init; }

    public int? Year { get; init; }

    public int Take { get; init; } = 100;
}

public sealed record UsdaAms시장가격관측응답(
    string RecordKey,
    string SourceKey,
    string MarketStageCode,
    string SlugId,
    string SlugName,
    string ReportTitle,
    DateOnly ReportBeginDate,
    DateOnly ReportEndDate,
    string PublishedDateRaw,
    string OfficeName,
    string OfficeState,
    string MarketType,
    string MarketLocationName,
    string MarketLocationState,
    string Commodity,
    string Variety,
    string Package,
    string UnitSales,
    string ItemSize,
    string Grade,
    string Quality,
    string Organic,
    string Origin,
    string District,
    decimal? LowPrice,
    decimal? HighPrice,
    decimal? MostlyLowPrice,
    decimal? MostlyHighPrice,
    decimal? WeightedAveragePrice,
    int? StoreCount,
    string CurrencyCode,
    string OriginalUnit,
    DateTime FirstCollectedAtUtc,
    DateTime LastSeenAtUtc);

public sealed record UsdaAms시장가격Archive응답(
    string StatusCode,
    DateTime GeneratedAtUtc,
    int TotalCount,
    IReadOnlyList<UsdaAms시장가격관측응답> Items,
    IReadOnlyList<string> Limitations);
