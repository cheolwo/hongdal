namespace Ssalddel.Domain.AgriculturalFisheries;

public static class 국제농수산가격Archive상태Codes
{
    public const string 실행중 = "Running";
    public const string 완료 = "Completed";
    public const string 실패 = "Failed";
}

public sealed class 국제농수산가격수집Run
{
    public long Id { get; set; }

    public string RunKey { get; set; } = Guid.NewGuid().ToString("N");

    public string SourceKey { get; set; } = string.Empty;

    public string StatusCode { get; set; } = 국제농수산가격Archive상태Codes.실행중;

    public int YearFrom { get; set; }

    public int YearTo { get; set; }

    public string QuerySummary { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    public DateOnly? LatestReferenceDate { get; set; }

    public int FetchedCount { get; set; }

    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int ExistingCount { get; set; }

    public string SourceMessagesJson { get; set; } = "[]";

    public string ErrorMessage { get; set; } = string.Empty;

    public ICollection<국제농수산가격관측> NewObservations { get; set; } =
        new List<국제농수산가격관측>();
}

public sealed class 국제농수산가격관측
{
    public long Id { get; set; }

    public long FirstCollectionRunId { get; set; }

    public 국제농수산가격수집Run? FirstCollectionRun { get; set; }

    public string RecordKey { get; set; } = string.Empty;

    public string SourceKey { get; set; } = string.Empty;

    public string DatasetCode { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public string GeographyCode { get; set; } = string.Empty;

    public string GeographyName { get; set; } = string.Empty;

    public string MarketStageCode { get; set; } = string.Empty;

    public string OfficialSeriesCode { get; set; } = string.Empty;

    public string OfficialProductCode { get; set; } = string.Empty;

    public string ProductNameOriginal { get; set; } = string.Empty;

    public string CanonicalProductKey { get; set; } = string.Empty;

    public DateOnly ReferenceDate { get; set; }

    public string FrequencyCode { get; set; } = string.Empty;

    public string ValueRaw { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string OriginalUnit { get; set; } = string.Empty;

    public bool IsIndex { get; set; }

    public string BasePeriod { get; set; } = string.Empty;

    public bool IsValueMissing { get; set; }

    public string ObservationStatus { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public string RawJson { get; set; } = "{}";

    public DateTime FirstCollectedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;
}
