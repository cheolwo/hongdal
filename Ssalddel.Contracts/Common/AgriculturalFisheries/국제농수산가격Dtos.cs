namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class 국제농수산가격SourceKeys
{
    public const string StatCan소비자평균소매가격 =
        "statcan-average-retail-food-prices";

    public const string Eurostat농산물절대생산자가격 =
        "eurostat-absolute-agricultural-prices";
}

public static class 국제농수산가격상태Codes
{
    public const string 완료 = "Completed";
    public const string 잘못된요청 = "InvalidRequest";
    public const string 자료없음 = "NoData";
}

public sealed record 국제농수산가격Source응답(
    string SourceKey,
    string Provider,
    string DisplayName,
    string CountryScopeCode,
    string MarketStageCode,
    string FrequencyCode,
    string DocumentationUrl,
    string ApiBaseUrl,
    bool RequiresCredential,
    IReadOnlyList<string> DatasetCodes,
    string LatestVerifiedPeriod,
    IReadOnlyList<string> Limitations);

public sealed class 국제농수산가격수집요청
{
    public string SourceKey { get; init; } = string.Empty;

    public int YearFrom { get; init; }

    public int YearTo { get; init; }
}

public sealed record 국제농수산가격수집응답(
    long CollectionRunId,
    string StatusCode,
    string SourceKey,
    int YearFrom,
    int YearTo,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount,
    DateOnly? LatestReferenceDate,
    IReadOnlyList<string> SourceMessages);

public sealed class 국제농수산가격ArchiveQuery
{
    public string? SourceKey { get; init; }

    public string? DatasetCode { get; init; }

    public string? CountryCode { get; init; }

    public string? GeographyCode { get; init; }

    public string? OfficialProductCode { get; init; }

    public string? ProductName { get; init; }

    public int? YearFrom { get; init; }

    public int? YearTo { get; init; }

    public int Take { get; init; } = 100;
}

public sealed record 국제농수산가격관측응답(
    string RecordKey,
    string SourceKey,
    string DatasetCode,
    string CountryCode,
    string CountryName,
    string GeographyCode,
    string GeographyName,
    string MarketStageCode,
    string OfficialSeriesCode,
    string OfficialProductCode,
    string ProductNameOriginal,
    string CanonicalProductKey,
    DateOnly ReferenceDate,
    string FrequencyCode,
    string ValueRaw,
    decimal? Price,
    string CurrencyCode,
    string OriginalUnit,
    bool IsIndex,
    string BasePeriod,
    bool IsValueMissing,
    string ObservationStatus,
    string SourceUrl,
    DateTime FirstCollectedAtUtc,
    DateTime LastSeenAtUtc);

public sealed record 국제농수산가격Archive응답(
    string StatusCode,
    DateTime GeneratedAtUtc,
    int TotalCount,
    IReadOnlyList<국제농수산가격관측응답> Items,
    string SourceNotice,
    IReadOnlyList<string> Limitations);
