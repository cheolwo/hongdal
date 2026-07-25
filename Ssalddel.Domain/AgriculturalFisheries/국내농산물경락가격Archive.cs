namespace Ssalddel.Domain.AgriculturalFisheries;

public static class 국내농산물경락가격수집상태Codes
{
    public const string 실행중 = "Running";
    public const string 완료 = "Completed";
    public const string 실패 = "Failed";
}

public sealed class 국내농산물경락가격수집Run
{
    public long Id { get; set; }

    public string RunKey { get; set; } = string.Empty;

    public string SourceKey { get; set; } = string.Empty;

    public string StatusCode { get; set; } = 국내농산물경락가격수집상태Codes.실행중;

    public DateOnly SettlementDate { get; set; }

    public int FetchedCount { get; set; }

    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int ExistingCount { get; set; }

    public int CompletedPages { get; set; }

    public bool IsTruncated { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public ICollection<국내농산물경락가격관측> NewObservations { get; set; } =
        new List<국내농산물경락가격관측>();
}

public sealed class 국내농산물경락가격관측
{
    public long Id { get; set; }

    public long FirstCollectionRunId { get; set; }

    public 국내농산물경락가격수집Run? FirstCollectionRun { get; set; }

    public string RecordKey { get; set; } = string.Empty;

    public string SourceKey { get; set; } = string.Empty;

    public DateOnly SettlementDate { get; set; }

    public string WholesaleMarketCode { get; set; } = string.Empty;

    public string CorporationCode { get; set; } = string.Empty;

    public string SlipNumber { get; set; } = string.Empty;

    public string AuctionSequence1 { get; set; } = string.Empty;

    public string AuctionSequence2 { get; set; } = string.Empty;

    public string TradingMethodCode { get; set; } = string.Empty;

    public string LargeCategoryCode { get; set; } = string.Empty;

    public string MiddleCategoryCode { get; set; } = string.Empty;

    public string SmallCategoryCode { get; set; } = string.Empty;

    public string CorporationItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string VarietyName { get; set; } = string.Empty;

    public decimal? UnitWeight { get; set; }

    public string UnitCode { get; set; } = string.Empty;

    public string PackageCode { get; set; } = string.Empty;

    public string SizeCode { get; set; } = string.Empty;

    public string GradeCode { get; set; } = string.Empty;

    public decimal? Quantity { get; set; }

    public decimal? AuctionPriceKrw { get; set; }

    public string OriginCode { get; set; } = string.Empty;

    public string OriginName { get; set; } = string.Empty;

    public decimal? TotalQuantity { get; set; }

    public decimal? TotalAmountKrw { get; set; }

    public string AwardedTime { get; set; } = string.Empty;

    public DateTime FirstCollectedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;
}
