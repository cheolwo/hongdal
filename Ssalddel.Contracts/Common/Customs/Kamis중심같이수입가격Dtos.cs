using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Contracts.Common.Customs;

public static class Kamis중심Hs수입가격조회상태Codes
{
    public const string 조회안함 = "not_queried";

    public const string 후보제한 = "candidate_limit";

    public const string 전체조회제한 = "page_lookup_limit";
}

public sealed class Kamis중심같이수입가격Query
{
    public int Year { get; init; }

    public string? CategoryCode { get; init; }

    public string? ItemCode { get; init; }

    public string? Query { get; init; }

    public string? FrequencyCode { get; init; } = "Daily";

    public bool OnlyAmsMapped { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public int KamisPointsPerItem { get; init; } = 6;

    public int AmsPointsPerStage { get; init; } = 3;

    public string CountryCode { get; init; } = "CN";

    public string? ReferenceMonth { get; init; }

    public int ImportLookbackMonths { get; init; } = 3;

    public decimal? FxRateKrwPerUsd { get; init; }

    /// <summary>
    /// 품목별로 실제 관세청 통계 조회를 시도할 HS 후보 수입니다.
    /// 전체 HS 후보는 모두 응답하되 외부 조회는 품목당 최대 5개, 페이지 전체 최대 20개로 제한합니다.
    /// </summary>
    public int HsPriceCandidatesPerItem { get; init; } = 1;

    /// <summary>
    /// 전문가가 검토한 4~10자리 HS/HSK 코드가 있으면 KAMIS 품목코드 대신 이 값으로 품목을 연결하고 조회합니다.
    /// </summary>
    public string? HsCode { get; init; }
}

public sealed record Kamis중심Hs수입통계단가응답(
    string StatusCode,
    string CountryCode,
    string StartMonth,
    string EndMonth,
    decimal? TotalImportWeightKg,
    decimal? TotalImportValueUsd,
    decimal? AverageCifUsdPerKg,
    decimal? FxRateKrwPerUsd,
    decimal? AverageCifKrwPerKg,
    string QuantityUnit,
    string ImportValueBasis,
    string CalculationMethod,
    string Provider,
    string SourceName,
    string SourceUrl,
    DateTime CollectedAtUtc,
    string Summary);

public sealed record Kamis중심Hs코드수입가격후보응답(
    string HsCode,
    string HsCodeScheme,
    string ProductName,
    string MatchQualityCode,
    string MatchQualityLabel,
    string MappingNote,
    bool RequiresProfessionalReview,
    bool IsImportPriceLookupSelected,
    string LookupOmissionReasonCode,
    Kamis중심Hs수입통계단가응답? ImportPrice);

public sealed record Kamis중심같이수입품목가격응답(
    Kamis중심UsdaAms품목가격응답 MarketPrice,
    IReadOnlyList<Kamis중심Hs코드수입가격후보응답> HsImportPriceCandidates);

public sealed record Kamis중심같이수입가격응답(
    string StatusCode,
    DateTime GeneratedAtUtc,
    int Year,
    string CountryCode,
    string ReferenceMonth,
    int ImportLookbackMonths,
    decimal? FxRateKrwPerUsd,
    int ObservedKamisItemCount,
    int FilteredKamisItemCount,
    int MappedKamisItemCount,
    int UnmappedKamisItemCount,
    int Page,
    int PageSize,
    int ExternalLookupCount,
    int SkippedLookupCount,
    IReadOnlyList<Kamis중심같이수입품목가격응답> Items,
    IReadOnlyList<string> DecisionBoundaries);
