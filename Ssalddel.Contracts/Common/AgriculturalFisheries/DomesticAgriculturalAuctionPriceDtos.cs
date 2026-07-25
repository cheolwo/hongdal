namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class 국내농산물경락가격출처Keys
{
    public const string MafraWholesaleMarketSettlement =
        "mafra-wholesale-market-settlement-auction";
}

public static class 국내농산물경락가격조회상태Codes
{
    public const string 완료 = "Completed";
    public const string 잘못된요청 = "InvalidRequest";
    public const string 지원하지않는출처 = "UnsupportedSource";
    public const string 설정안됨 = "NotConfigured";
    public const string 자료조회불가 = "DataUnavailable";
}

public sealed class 국내농산물경락가격조회요청
{
    public string SourceKey { get; init; } =
        국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement;

    public string SettlementDate { get; init; } = string.Empty;

    public string? WholesaleMarketCode { get; init; }

    public string? CorporationCode { get; init; }

    public string? ItemName { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 100;
}

public sealed class 국내농산물경락가격조회응답
{
    public bool Success { get; init; }

    public string StatusCode { get; init; } = 국내농산물경락가격조회상태Codes.자료조회불가;

    public string? ErrorMessage { get; init; }

    public 국내농산물경락가격원천응답? Source { get; init; }

    public 국내농산물경락가격조회요청 Query { get; init; } = new();

    public IReadOnlyList<국내농산물경락가격항목> Items { get; init; } = [];

    public int TotalCount { get; init; }

    public DateTimeOffset? LatestCollectedAtUtc { get; init; }

    public IReadOnlyList<string> Notices { get; init; } = [];
}

public sealed class 국내농산물경락가격원천응답
{
    public string Key { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string TransactionStageCode { get; init; } = "AuctionSettlement";

    public string TransactionStageLabel { get; init; } = "도매시장 경락·정산";

    public string UpdateCycle { get; init; } = string.Empty;

    public string DocumentationUrl { get; init; } = string.Empty;

    public string CurrencyCode { get; init; } = "KRW";

    public string PriceBasis { get; init; } = "원/거래단위";

    public bool IsConfigured { get; init; }
}

public sealed class 국내농산물경락가격항목
{
    public string RecordKey { get; init; } = string.Empty;

    public string SourceKey { get; init; } = string.Empty;

    public DateOnly SettlementDate { get; init; }

    public string WholesaleMarketCode { get; init; } = string.Empty;

    public string CorporationCode { get; init; } = string.Empty;

    public string SlipNumber { get; init; } = string.Empty;

    public string AuctionSequence1 { get; init; } = string.Empty;

    public string AuctionSequence2 { get; init; } = string.Empty;

    public string TradingMethodCode { get; init; } = string.Empty;

    public string LargeCategoryCode { get; init; } = string.Empty;

    public string MiddleCategoryCode { get; init; } = string.Empty;

    public string SmallCategoryCode { get; init; } = string.Empty;

    public string CorporationItemCode { get; init; } = string.Empty;

    public string ItemName { get; init; } = string.Empty;

    public string VarietyName { get; init; } = string.Empty;

    public decimal? UnitWeight { get; init; }

    public string UnitCode { get; init; } = string.Empty;

    public string PackageCode { get; init; } = string.Empty;

    public string SizeCode { get; init; } = string.Empty;

    public string GradeCode { get; init; } = string.Empty;

    public decimal? Quantity { get; init; }

    public decimal? AuctionPriceKrw { get; init; }

    public string OriginCode { get; init; } = string.Empty;

    public string OriginName { get; init; } = string.Empty;

    public decimal? TotalQuantity { get; init; }

    public decimal? TotalAmountKrw { get; init; }

    public string AwardedTime { get; init; } = string.Empty;

    public DateTimeOffset CollectedAtUtc { get; init; }
}
