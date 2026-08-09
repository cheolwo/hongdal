using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Domain.PublicData;

public static class 외부데이터수집StatusCodes
{
    public const string Running = "Running";
    public const string Success = "Success";
    public const string Partial = "Partial";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
}

public sealed class 외부데이터수집Run
{
    public long Id { get; set; }
    public string RunKey { get; set; } = Guid.NewGuid().ToString("N");
    public string SourceId { get; set; } = string.Empty;
    public string DatasetId { get; set; } = string.Empty;
    public string StatusCode { get; set; } = 외부데이터수집StatusCodes.Running;
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public int FetchedCount { get; set; }
    public int NormalizedCount { get; set; }
    public int RejectedCount { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ExistingCount { get; set; }
    public string SourceVersion { get; set; } = string.Empty;
    public string DataRevision { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorSummary { get; set; } = string.Empty;
}

public sealed class 외부데이터RawSnapshot
{
    public long Id { get; set; }
    public long FirstCollectionRunId { get; set; }
    public 외부데이터수집Run? FirstCollectionRun { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string DatasetId { get; set; } = string.Empty;
    public string SourceVersion { get; set; } = string.Empty;
    public DateTimeOffset CollectedAtUtc { get; set; }
    public DateTimeOffset? EvidenceAsOfUtc { get; set; }
    public string ContentHashSha256 { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StorageContainer { get; set; } = string.Empty;
    public string StorageObjectName { get; set; } = string.Empty;
    public string StorageLocation { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
}

public sealed class 외부데이터정규화Record
{
    public long Id { get; set; }
    public long RawSnapshotId { get; set; }
    public 외부데이터RawSnapshot? RawSnapshot { get; set; }
    public string RecordKey { get; set; } = string.Empty;
    public string StableId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string DatasetId { get; set; } = string.Empty;
    public string RegionStableId { get; set; } = string.Empty;
    public string MetricCode { get; set; } = string.Empty;
    public decimal? NumericValue { get; set; }
    public string TextValue { get; set; } = string.Empty;
    public string UnitCode { get; set; } = string.Empty;
    public DateTimeOffset EvidenceAsOfUtc { get; set; }
    public DateTimeOffset CollectedAtUtc { get; set; }
    public string SpatialPrecisionCode { get; set; } = string.Empty;
    public string TemporalPrecisionCode { get; set; } = string.Empty;
    public string QualityCode { get; set; } = string.Empty;
    public string LimitationCode { get; set; } = string.Empty;
    public string DimensionKey { get; set; } = string.Empty;
    public string SourceVersion { get; set; } = string.Empty;
    public string DataRevision { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
}

public sealed class 외부지역CodeMapping
{
    public long Id { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string ExternalRegionCode { get; set; } = string.Empty;
    public string RegionStableId { get; set; } = string.Empty;
    public string SpatialPrecisionCode { get; set; } = string.Empty;
    public string MappingRevision { get; set; } = string.Empty;
    public DateTimeOffset? ValidFromUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }
}

public sealed record 외부데이터정규화저장Result(
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount);

public interface I외부데이터수집Store
{
    Task<외부데이터수집Run> StartRunAsync(
        외부데이터수집Run run,
        CancellationToken cancellationToken = default);

    Task<외부데이터RawSnapshot?> FindRawSnapshotAsync(
        string sourceId,
        string datasetId,
        string contentHashSha256,
        CancellationToken cancellationToken = default);

    Task<외부데이터RawSnapshot> SaveRawSnapshotAsync(
        외부데이터RawSnapshot snapshot,
        CancellationToken cancellationToken = default);

    Task<외부데이터정규화저장Result> UpsertNormalizedAsync(
        IReadOnlyCollection<외부데이터정규화Record> records,
        CancellationToken cancellationToken = default);

    Task CompleteRunAsync(
        외부데이터수집Run run,
        CancellationToken cancellationToken = default);
}

public interface I외부지역MappingStore
{
    Task<외부지역CodeMapping?> FindAsync(
        string sourceId,
        string externalRegionCode,
        CancellationToken cancellationToken = default);
}

public static class RegionStableIdRules
{
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var segments = value.Trim().Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2) return false;
        if (segments[0] is not ("country" or "region" or "point" or "grid" or "area" or "raster"))
            return false;
        return segments.All(segment => segment.All(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.'));
    }

    public static string EnsureValid(string value, string parameterName)
        => IsValid(value)
            ? value.Trim().ToLowerInvariant()
            : throw new ArgumentException("RegionStableIdInvalid", parameterName);
}

public static class 외부데이터RecordKey
{
    public static string Create(
        string sourceId,
        string datasetId,
        string regionStableId,
        string metricCode,
        DateTimeOffset evidenceAsOfUtc,
        string dimensionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(datasetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(metricCode);
        var normalizedRegion = RegionStableIdRules.EnsureValid(regionStableId, nameof(regionStableId));
        var canonical = string.Join('|',
            sourceId.Trim().ToLowerInvariant(),
            datasetId.Trim().ToLowerInvariant(),
            normalizedRegion,
            metricCode.Trim().ToLowerInvariant(),
            evidenceAsOfUtc.ToUniversalTime().ToString("O"),
            dimensionKey?.Trim() ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}
