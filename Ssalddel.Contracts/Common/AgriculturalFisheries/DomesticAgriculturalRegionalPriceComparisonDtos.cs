namespace Ssalddel.Contracts.Common.AgriculturalFisheries;

public static class 농산물지역가격비교기준Codes
{
    public const string 원산지 = "Origin";
    public const string 도매시장 = "WholesaleMarket";
}

public sealed class 농산물지역가격비교선택지요청
{
    public string SourceKey { get; init; } =
        국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement;

    public string? SettlementDate { get; init; }

    public string? ItemName { get; init; }
}

public sealed class 농산물지역가격비교선택지응답
{
    public bool Success { get; init; }

    public string StatusCode { get; init; } =
        국내농산물경락가격조회상태Codes.자료조회불가;

    public string? ErrorMessage { get; init; }

    public 국내농산물경락가격원천응답? Source { get; init; }

    public DateOnly? SettlementDate { get; init; }

    public IReadOnlyList<string> ItemNames { get; init; } = [];

    public IReadOnlyList<string> VarietyNames { get; init; } = [];

    public IReadOnlyList<농산물가격비교지역선택지> OriginRegions { get; init; } = [];

    public IReadOnlyList<농산물가격비교지역선택지> WholesaleMarkets { get; init; } = [];

    public IReadOnlyList<string> Notices { get; init; } = [];
}

public sealed class 농산물가격비교지역선택지
{
    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

public sealed class 농산물지역가격비교요청
{
    public string SourceKey { get; init; } =
        국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement;

    public string ItemName { get; init; } = string.Empty;

    public string? VarietyName { get; init; }

    public string? StartDate { get; init; }

    public string? EndDate { get; init; }

    public string RegionBasisCode { get; init; } = 농산물지역가격비교기준Codes.원산지;
}

public sealed class 농산물지역가격비교응답
{
    public bool Success { get; init; }

    public string StatusCode { get; init; } =
        국내농산물경락가격조회상태Codes.자료조회불가;

    public string? ErrorMessage { get; init; }

    public 국내농산물경락가격원천응답? Source { get; init; }

    public 농산물지역가격비교요청 Query { get; init; } = new();

    public DateOnly? ResolvedStartDate { get; init; }

    public DateOnly? ResolvedEndDate { get; init; }

    public string CurrencyCode { get; init; } = "KRW";

    public string PriceUnit { get; init; } = "원/kg";

    public decimal? OverallAveragePriceKrwPerKg { get; init; }

    public IReadOnlyList<농산물지역가격비교항목> Regions { get; init; } = [];

    public DateTimeOffset? LatestCollectedAtUtc { get; init; }

    public IReadOnlyList<string> Notices { get; init; } = [];
}

public sealed class 농산물지역가격비교항목
{
    public string RegionCode { get; init; } = string.Empty;

    public string RegionName { get; init; } = string.Empty;

    public int ObservationCount { get; init; }

    public int TradingDayCount { get; init; }

    public decimal TotalQuantityKg { get; init; }

    public decimal AveragePriceKrwPerKg { get; init; }

    public decimal MinimumPriceKrwPerKg { get; init; }

    public decimal MaximumPriceKrwPerKg { get; init; }

    public decimal ComparisonIndex { get; init; }
}
