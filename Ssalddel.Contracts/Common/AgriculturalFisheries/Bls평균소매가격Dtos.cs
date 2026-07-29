namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class Bls평균소매가격상태Codes
{
    public const string 완료 = "Completed";
    public const string 잘못된요청 = "InvalidRequest";
    public const string 자료없음 = "NoData";
}

public static class Bls평균소매가격Mapping상태Codes
{
    public const string 후보 = "Candidate";
    public const string 검토완료 = "Reviewed";
    public const string 후보없음 = "NoCandidate";
}

public static class BlsKamis비교품질Codes
{
    public const string 직접품목후보 = "DirectCommodityCandidate";
    public const string 광의품목후보 = "BroadCommodityCandidate";
    public const string 가공연관품목 = "RelatedProcessedProduct";
}

public sealed record Bls평균소매가격Series응답(
    string SeriesId,
    string ItemCode,
    string CanonicalProductKey,
    string ProductNameKo,
    string ItemNameEn,
    string OriginalUnit,
    string AreaCode,
    string AreaName,
    string MappingStatusCode,
    string SourceUrl);

public sealed record BlsKamis품목후보응답(
    string KamisCategoryCode,
    string KamisCategoryName,
    string KamisItemCode,
    string KamisItemName,
    string MatchQualityCode,
    string ReviewStatusCode,
    bool AllowsDirectPriceComparison,
    string ReviewNote);

public sealed record BlsKamisSeries비교검토응답(
    string SeriesId,
    string BlsItemCode,
    string CanonicalProductKey,
    string BlsProductNameKo,
    string BlsItemNameEn,
    string BlsOriginalUnit,
    string MappingStatusCode,
    IReadOnlyList<BlsKamis품목후보응답> KamisCandidates);

public sealed record BlsKamis비교Catalog응답(
    DateOnly BlsCatalogObservedAt,
    int BlsSeriesCount,
    int SeriesWithCandidateCount,
    int DirectComparableCandidateSeriesCount,
    int UniqueKamisItemCodeCount,
    IReadOnlyList<BlsKamisSeries비교검토응답> Items,
    IReadOnlyList<string> ComparisonBoundaries);

public sealed class Bls평균소매가격수집요청
{
    public int YearFrom { get; init; }

    public int YearTo { get; init; }
}

public sealed record Bls평균소매가격수집응답(
    long CollectionRunId,
    string StatusCode,
    int YearFrom,
    int YearTo,
    int RequestedSeriesCount,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount,
    DateOnly? LatestReferenceMonth,
    IReadOnlyList<string> SourceMessages);

public sealed class Bls평균소매가격ArchiveQuery
{
    public string? SeriesId { get; init; }

    public string? CanonicalProductKey { get; init; }

    public int? YearFrom { get; init; }

    public int? YearTo { get; init; }

    public int Take { get; init; } = 100;
}

public sealed record Bls평균소매가격관측응답(
    string RecordKey,
    string SeriesId,
    string ItemCode,
    string CanonicalProductKey,
    string ProductNameKo,
    string ItemNameEn,
    string AreaCode,
    string AreaName,
    DateOnly ReferenceMonth,
    string PeriodCode,
    string PeriodName,
    string ValueRaw,
    decimal? PriceUsd,
    string CurrencyCode,
    string OriginalUnit,
    bool IsValueMissing,
    string Footnote,
    string SourceUrl,
    DateTime FirstCollectedAtUtc,
    DateTime LastSeenAtUtc);

public sealed record Bls평균소매가격Archive응답(
    string StatusCode,
    DateTime GeneratedAtUtc,
    int TotalCount,
    IReadOnlyList<Bls평균소매가격관측응답> Items,
    string SourceNotice,
    IReadOnlyList<string> Limitations);
