namespace Ssalddel.Domain.AgriculturalFisheries;

public static class 주간국가농수산물비교상태Codes
{
    public const string 관측값있음 = "Available";
    public const string 검증관측값없음 = "NoVerifiedObservation";
    public const string 원천미등록 = "SourceNotConfigured";
}

public sealed class 주간국가농수산물비교Snapshot
{
    public long Id { get; set; }

    public string PeriodKey { get; set; } = string.Empty;

    public DateOnly WeekStartDate { get; set; }

    public DateOnly WeekEndDate { get; set; }

    public int AvailableObservationCount { get; set; }

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<주간국가농수산물비교항목> Items { get; set; } =
        new List<주간국가농수산물비교항목>();
}

public sealed class 주간국가농수산물비교항목
{
    public long Id { get; set; }

    public long SnapshotId { get; set; }

    public 주간국가농수산물비교Snapshot? Snapshot { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string ProductNameKo { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string CountryNameKo { get; set; } = string.Empty;

    public string StatusCode { get; set; } = 주간국가농수산물비교상태Codes.검증관측값없음;

    public string SourceKey { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public DateOnly? ReferenceDate { get; set; }

    public string OriginalProductName { get; set; } = string.Empty;

    public string MarketStage { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string ComparisonNote { get; set; } = string.Empty;
}
