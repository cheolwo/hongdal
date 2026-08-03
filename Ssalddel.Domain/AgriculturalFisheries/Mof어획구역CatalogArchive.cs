namespace Ssalddel.Domain.AgriculturalFisheries;

public static class Mof어획구역Catalog수집상태Codes
{
    public const string 비활성 = "Disabled";
    public const string 실행중 = "Running";
    public const string 완료 = "Completed";
    public const string 실패 = "Failed";
}

public sealed class Mof어획구역Catalog수집Run
{
    public long Id { get; set; }
    public string RunKey { get; set; } = string.Empty;
    public string SourceKey { get; set; } = "mof-fishing-area-catalog";
    public string DatasetVersion { get; set; } = string.Empty;
    public string StatusCode { get; set; } = Mof어획구역Catalog수집상태Codes.실행중;
    public string ContentSha256 { get; set; } = string.Empty;
    public int SourceRowCount { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public long? SnapshotId { get; set; }
    public Mof어획구역Catalog영속Snapshot? Snapshot { get; set; }
}

public sealed class Mof어획구역Catalog영속Snapshot
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = "mof-fishing-area-catalog";
    public string SourceUrl { get; set; } = string.Empty;
    public string DatasetVersion { get; set; } = string.Empty;
    public string ContentSha256 { get; set; } = string.Empty;
    public DateTime CollectedAtUtc { get; set; }
    public DateTime StoredAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public int SourceRowCount { get; set; }
    public int NormalizedRecordCount { get; set; }
    public string FreshnessCode { get; set; } = "fresh";
    public string NormalizedRecordsJson { get; set; } = "[]";
    public ICollection<Mof어획구역Catalog수집Run> CollectionRuns { get; set; } =
        new List<Mof어획구역Catalog수집Run>();
}
