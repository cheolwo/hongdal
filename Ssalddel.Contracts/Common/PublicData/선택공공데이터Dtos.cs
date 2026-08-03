using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Contracts.Common.PublicData;

public static class 선택공공데이터Feature
{
    public const string Key = "selected-public-data-sources";
}

[SsalddelCodeMetadata(
    선택공공데이터Feature.Key,
    SsalddelCodeLayer.Contract,
    "역사 수산물 산지 위판가격 관측을 출처와 수록기간을 보존해 전달",
    FlowOrder = 1,
    Boundary = "1999-01-01부터 2016-01-19까지의 역사 관측이며 현재 가격, 가용 재고 또는 거래 가능성을 의미하지 않음")]
public sealed class 수산물산지위판가격Request
{
    public DateOnly CollectionDate { get; init; }

    public string? MarketName { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 100;
}

public sealed class 수산물산지위판가격Response
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public string SourceKey { get; init; } = "mafra-fisheries-origin-auction-historical";

    public string Provider { get; init; } = "농림수산식품교육문화정보원";

    public DateOnly CoverageStart { get; init; } = new(1999, 1, 1);

    public DateOnly CoverageEnd { get; init; } = new(2016, 1, 19);

    public int? TotalCount { get; init; }

    public IReadOnlyList<수산물산지위판가격Item> Items { get; init; } = [];

    public DateTimeOffset ObservedAt { get; init; }
}

public sealed record 수산물산지위판가격Item
{
    public DateOnly CollectionDate { get; init; }

    public string AuctionCooperativeCode { get; init; } = string.Empty;

    public string StatisticsFishSpeciesCode { get; init; } = string.Empty;

    public string FishCooperativeItemCode { get; init; } = string.Empty;

    public string FishingMethodCode { get; init; } = string.Empty;

    public string FishSpeciesName { get; init; } = string.Empty;

    public decimal? TotalQuantity { get; init; }

    public decimal? TotalTransactionVolume { get; init; }

    public decimal? TotalAmountKrw { get; init; }

    public decimal? HighestPriceKrw { get; init; }

    public decimal? LowestPriceKrw { get; init; }

    public decimal? AveragePriceKrw { get; init; }

    public string UnitName { get; init; } = string.Empty;

    public string PackageName { get; init; } = string.Empty;

    public string SizeName { get; init; } = string.Empty;

    public string QualityName { get; init; } = string.Empty;

    public string MarketName { get; init; } = string.Empty;
}
