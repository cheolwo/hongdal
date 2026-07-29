namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class Kamis중심UsdaAms가격비교상태Codes
{
    public const string 완료 = "Completed";

    public const string 자료없음 = "NoData";
}

public static class Kamis중심UsdaAms매핑상태Codes
{
    public const string 후보있음 = "CandidateAvailable";

    public const string 후보없음 = "NoCandidate";
}

public static class Kamis중심UsdaAms매핑품질Codes
{
    public const string 동일품목후보 = "DirectCommodityCandidate";

    public const string 광의품목후보 = "BroadCommodityCandidate";

    public const string 후보없음 = "NoCandidate";
}

public sealed class Kamis중심UsdaAms가격비교Query
{
    public int Year { get; init; }

    public string? CategoryCode { get; init; }

    public string? ItemCode { get; init; }

    public string? Query { get; init; }

    public string? FrequencyCode { get; init; } = "Daily";

    public bool OnlyMapped { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 30;

    public int KamisPointsPerItem { get; init; } = 12;

    public int AmsPointsPerStage { get; init; } = 6;
}

public sealed record Kamis중심가격Point응답(
    string FrequencyCode,
    string ProductClassCode,
    string ProductClassName,
    DateOnly SurveyDate,
    string KindCode,
    string KindName,
    string RankCode,
    string RankName,
    string Unit,
    decimal? PriceKrw,
    bool IsPriceMissing);

public sealed record Kamis중심UsdaAms가격Point응답(
    string RecordKey,
    string SourceKey,
    string MarketStageCode,
    DateOnly ReferenceDate,
    string Commodity,
    string Variety,
    string Grade,
    string Package,
    string ItemSize,
    string Organic,
    string Origin,
    string MarketLocationName,
    string MarketLocationState,
    decimal? LowPrice,
    decimal? HighPrice,
    decimal? MostlyLowPrice,
    decimal? MostlyHighPrice,
    decimal? WeightedAveragePrice,
    int? StoreCount,
    string CurrencyCode,
    string OriginalUnit);

public sealed record Kamis중심UsdaAms시장단계가격응답(
    string MarketStageCode,
    string MarketStageLabel,
    DateOnly? LatestReferenceDate,
    IReadOnlyList<Kamis중심UsdaAms가격Point응답> PricePoints);

public sealed record Kamis중심UsdaAms품목가격응답(
    string KamisCategoryCode,
    string KamisCategoryName,
    string KamisItemCode,
    string KamisItemName,
    DateOnly LatestKamisSurveyDate,
    string MappingStatusCode,
    string MatchQualityCode,
    string MatchQualityLabel,
    IReadOnlyList<string> MatchedAmsCommodities,
    string MappingNote,
    bool AllowsDirectPriceDifference,
    IReadOnlyList<Kamis중심가격Point응답> KamisPricePoints,
    IReadOnlyList<Kamis중심UsdaAms시장단계가격응답> AmsMarketStages);

public sealed record Kamis중심UsdaAms가격비교응답(
    string StatusCode,
    DateTime GeneratedAtUtc,
    int Year,
    int ObservedKamisItemCount,
    int FilteredKamisItemCount,
    int MappedKamisItemCount,
    int UnmappedKamisItemCount,
    int Page,
    int PageSize,
    IReadOnlyList<Kamis중심UsdaAms품목가격응답> Items,
    IReadOnlyList<string> ComparisonBoundaries);
