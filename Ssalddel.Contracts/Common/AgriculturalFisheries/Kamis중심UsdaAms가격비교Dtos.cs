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

public static class Kamis중심상품코드연결상태Codes
{
    public const string 확인됨 = "Confirmed";

    public const string 후보 = "Candidate";

    public const string 후보없음 = "NoCandidate";

    public const string 전문가검토필요 = "ExpertReviewRequired";
}

public static class 농수산유통비교단계Codes
{
    public const string 산지 = "Origin";

    public const string 도매 = "Wholesale";

    public const string 소매 = "Retail";
}

public static class 농수산유통가격대상태Codes
{
    public const string 관측값있음 = "Observed";

    public const string 관측값없음 = "NoObservation";
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
    string SourcePackageLabel,
    string ComparisonUnit,
    string PriceNormalizationCode,
    string PriceNormalizationBasis,
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

public sealed record Kamis중심Hs분류후보응답(
    string CodeScheme,
    string Code,
    string ProductName,
    string RelationStatusCode,
    string MatchQualityCode,
    string MatchQualityLabel,
    string ReviewNote);

public sealed record Kamis중심국가세번검토응답(
    string CountryCode,
    string CodeScheme,
    string? Code,
    string RelationStatusCode,
    string ReviewNote);

public sealed record Kamis중심상품코드연결응답(
    string InternalProductKey,
    string KamisCategoryCode,
    string KamisItemCode,
    string KamisRelationStatusCode,
    IReadOnlyList<string> UsdaAmsCommodityCandidates,
    string UsdaAmsRelationStatusCode,
    IReadOnlyList<Kamis중심Hs분류후보응답> HsClassificationCandidates,
    IReadOnlyList<Kamis중심국가세번검토응답> NationalTariffReviews);

public sealed record Kamis중심유통단계가격대응답(
    int StageOrder,
    string ComparisonStageCode,
    string ComparisonStageLabel,
    string CountryCode,
    string CountryName,
    string SourceKey,
    string SourceMarketStageCode,
    string SourceMarketStageLabel,
    string DataStatusCode,
    DateOnly? LatestReferenceDate,
    string CurrencyCode,
    string OriginalUnit,
    IReadOnlyList<string> SourcePackageLabels,
    string PriceNormalizationCode,
    string PriceNormalizationBasis,
    decimal? LowObservedPrice,
    decimal? HighObservedPrice,
    int ObservationCount,
    bool AllowsDirectComparison,
    string ComparisonNote);

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
    IReadOnlyList<Kamis중심UsdaAms시장단계가격응답> AmsMarketStages,
    Kamis중심상품코드연결응답 ProductCodeConnection,
    IReadOnlyList<Kamis중심유통단계가격대응답> DistributionStagePriceBands);

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
