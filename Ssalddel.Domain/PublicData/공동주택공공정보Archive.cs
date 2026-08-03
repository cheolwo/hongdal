namespace Ssalddel.Domain.PublicData;

public static class 공동주택공공정보수집상태Codes
{
    public const string 비활성 = "Disabled";
    public const string 실행중 = "Running";
    public const string 완료 = "Completed";
    public const string 실패 = "Failed";
}

public sealed class 공동주택공공정보수집Run
{
    public long Id { get; set; }
    public string RunKey { get; set; } = string.Empty;
    public string ScopeKey { get; set; } = string.Empty;
    public string ComplexCode { get; set; } = string.Empty;
    public string ComplexName { get; set; } = string.Empty;
    public string TargetMonth { get; set; } = string.Empty;
    public string StatusCode { get; set; } = 공동주택공공정보수집상태Codes.실행중;
    public int RequestCount { get; set; }
    public long? SnapshotId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public sealed class 공동주택공공정보Snapshot
{
    public long Id { get; set; }
    public string SourceKey { get; set; } = "data-go-kr-kapt";
    public string SourceUrl { get; set; } = "https://www.data.go.kr/";
    public string SourceVersion { get; set; } = string.Empty;
    public string SpatialKey { get; set; } = string.Empty;
    public string ComplexCode { get; set; } = string.Empty;
    public string ComplexName { get; set; } = string.Empty;
    public string TargetMonth { get; set; } = string.Empty;
    public DateTime CollectedAtUtc { get; set; }
    public string ContentSha256 { get; set; } = string.Empty;
    public string NormalizedJson { get; set; } = string.Empty;
    public string FreshnessStatusCode { get; set; } = "Current";
    public DateTime UpdatedAtUtc { get; set; }
}
