namespace Ssalddel.Domain.AgriculturalFisheries;

public static class UsdaAms시장가격Archive상태Codes
{
    public const string 실행중 = "Running";
    public const string 완료 = "Completed";
    public const string 실패 = "Failed";
}

public sealed class UsdaAms시장가격수집Run
{
    public long Id { get; set; }

    public string RunKey { get; set; } = Guid.NewGuid().ToString("N");

    public string StatusCode { get; set; } = UsdaAms시장가격Archive상태Codes.실행중;

    public DateOnly DateFrom { get; set; }

    public DateOnly DateTo { get; set; }

    public string RequestedMarketTypesJson { get; set; } = "[]";

    public int DiscoveredReportCount { get; set; }

    public int CompletedSliceCount { get; set; }

    public long FetchedCount { get; set; }

    public long InsertedCount { get; set; }

    public long ExistingCount { get; set; }

    public DateOnly? LatestReferenceDate { get; set; }

    public string SourceUrl { get; set; } = string.Empty;

    public string SourceMessagesJson { get; set; } = "[]";

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public ICollection<UsdaAms시장가격관측> NewObservations { get; set; } =
        new List<UsdaAms시장가격관측>();
}

public sealed class UsdaAms시장가격관측
{
    public long Id { get; set; }

    public long FirstCollectionRunId { get; set; }

    public UsdaAms시장가격수집Run? FirstCollectionRun { get; set; }

    public string RecordKey { get; set; } = string.Empty;

    public string SourceKey { get; set; } = string.Empty;

    public string MarketStageCode { get; set; } = string.Empty;

    public string SlugId { get; set; } = string.Empty;

    public string SlugName { get; set; } = string.Empty;

    public string ReportTitle { get; set; } = string.Empty;

    public DateOnly ReportBeginDate { get; set; }

    public DateOnly ReportEndDate { get; set; }

    public string PublishedDateRaw { get; set; } = string.Empty;

    public string OfficeName { get; set; } = string.Empty;

    public string OfficeState { get; set; } = string.Empty;

    public string OfficeCity { get; set; } = string.Empty;

    public string MarketType { get; set; } = string.Empty;

    public string MarketLocationName { get; set; } = string.Empty;

    public string MarketLocationState { get; set; } = string.Empty;

    public string MarketLocationCity { get; set; } = string.Empty;

    public string Community { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Commodity { get; set; } = string.Empty;

    public string Variety { get; set; } = string.Empty;

    public string Repack { get; set; } = string.Empty;

    public string Package { get; set; } = string.Empty;

    public string Storage { get; set; } = string.Empty;

    public string TransportationMode { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;

    public string UnitSales { get; set; } = string.Empty;

    public string ItemSize { get; set; } = string.Empty;

    public string Appearance { get; set; } = string.Empty;

    public string Quality { get; set; } = string.Empty;

    public string Condition { get; set; } = string.Empty;

    public string Organic { get; set; } = string.Empty;

    public string Crop { get; set; } = string.Empty;

    public string Origin { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string Environment { get; set; } = string.Empty;

    public decimal? LowPrice { get; set; }

    public decimal? HighPrice { get; set; }

    public decimal? MostlyLowPrice { get; set; }

    public decimal? MostlyHighPrice { get; set; }

    public decimal? WeightedAveragePrice { get; set; }

    public int? StoreCount { get; set; }

    public string CurrencyCode { get; set; } = "USD";

    public string OriginalUnit { get; set; } = string.Empty;

    public string RawJson { get; set; } = "{}";

    public DateTime FirstCollectedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class UsdaAms연도상품Catalog
{
    public long Id { get; set; }

    public int Year { get; set; }

    public string Commodity { get; set; } = string.Empty;

    public DateOnly FirstObservedDate { get; set; }

    public DateOnly LastObservedDate { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
