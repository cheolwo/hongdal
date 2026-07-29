namespace Ssalddel.Domain.AgriculturalFisheries;

public static class Bls평균소매가격Archive상태Codes
{
    public const string 실행중 = "Running";
    public const string 완료 = "Completed";
    public const string 실패 = "Failed";
}

public sealed class Bls평균소매가격수집Run
{
    public long Id { get; set; }

    public string RunKey { get; set; } = Guid.NewGuid().ToString("N");

    public string StatusCode { get; set; } = Bls평균소매가격Archive상태Codes.실행중;

    public int YearFrom { get; set; }

    public int YearTo { get; set; }

    public int RequestedSeriesCount { get; set; }

    public string QuerySummary { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public DateOnly? LatestReferenceMonth { get; set; }

    public int FetchedCount { get; set; }

    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int ExistingCount { get; set; }

    public string SourceMessagesJson { get; set; } = "[]";

    public string ErrorMessage { get; set; } = string.Empty;

    public ICollection<Bls평균소매가격관측> NewObservations { get; set; } =
        new List<Bls평균소매가격관측>();
}

public sealed class Bls평균소매가격관측
{
    public long Id { get; set; }

    public long FirstCollectionRunId { get; set; }

    public Bls평균소매가격수집Run? FirstCollectionRun { get; set; }

    public string RecordKey { get; set; } = string.Empty;

    public string SeriesId { get; set; } = string.Empty;

    public string ItemCode { get; set; } = string.Empty;

    public string CanonicalProductKey { get; set; } = string.Empty;

    public string ProductNameKo { get; set; } = string.Empty;

    public string ItemNameEn { get; set; } = string.Empty;

    public string AreaCode { get; set; } = "0000";

    public string AreaName { get; set; } = "U.S. city average";

    public DateOnly ReferenceMonth { get; set; }

    public string PeriodCode { get; set; } = string.Empty;

    public string PeriodName { get; set; } = string.Empty;

    public string ValueRaw { get; set; } = string.Empty;

    public decimal? PriceUsd { get; set; }

    public string CurrencyCode { get; set; } = "USD";

    public string OriginalUnit { get; set; } = string.Empty;

    public bool IsValueMissing { get; set; }

    public string Footnote { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public string RawJson { get; set; } = "{}";

    public DateTime FirstCollectedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;
}
