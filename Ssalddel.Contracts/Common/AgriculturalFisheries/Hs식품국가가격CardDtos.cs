namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class Hs식품국가가격Card상태Codes
{
    public const string 완료 = "Completed";
    public const string 일부자료 = "PartialData";
    public const string 품목없음 = "ItemNotFound";
}

public static class Hs식품국가가격관측상태Codes
{
    public const string 관측됨 = "Observed";
    public const string 자료없음 = "NoData";
    public const string 조회불가 = "Unavailable";
}

public static class Hs식품국가가격맥락Codes
{
    public const string 국내시장조사가격 = "DomesticMarketSurvey";
    public const string 수입통계단가 = "ImportStatisticalUnitValue";
}

public sealed class Hs식품국가가격CardQuery
{
    public string Month { get; init; } = string.Empty;

    public int LookbackMonths { get; init; } = 3;
}

public sealed record Hs식품국가가격관측응답(
    string PriceContextCode,
    string PriceContextLabel,
    string MarketStageCode,
    string MarketStageLabel,
    string DataStatusCode,
    string ReferencePeriod,
    string CurrencyCode,
    string Unit,
    decimal? AveragePrice,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    int ObservationCount,
    string ComparisonGroupCode,
    bool AllowsComparisonWithinGroup,
    string SourceKey,
    string SourceName,
    string SourceUrl,
    string CalculationBasis,
    string Note);

public sealed record Hs식품국가가격응답(
    int DisplayOrder,
    string CountryCode,
    string CountryName,
    string DataStatusCode,
    IReadOnlyList<Hs식품국가가격관측응답> PriceObservations,
    string Summary);

public sealed record Hs식품국가가격Card응답(
    string StatusCode,
    DateTimeOffset GeneratedAtUtc,
    string HsCode,
    string HsCodeScheme,
    string ProductName,
    string? RepresentativeImageUrl,
    string ImageReviewStatusCode,
    string ReferenceMonth,
    int LookbackMonths,
    IReadOnlyList<Hs식품국가가격응답> Countries,
    IReadOnlyList<string> ComparisonBoundaries,
    bool InformationOnly);
