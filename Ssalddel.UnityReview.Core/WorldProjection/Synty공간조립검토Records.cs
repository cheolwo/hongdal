using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Services.WorldProjection;

public sealed class Synty공간조립검토ConcurrencyException(
    string reviewItemStableId,
    long currentRevision)
    : InvalidOperationException(
        $"Synty 공간 조립 검토 원장이 변경되었습니다. ReviewItemStableId={reviewItemStableId}, CurrentRevision={currentRevision}")
{
    public string ReviewItemStableId { get; } = reviewItemStableId;
    public long CurrentRevision { get; } = currentRevision;
}

public sealed class Synty공간조립검토원장Record
{
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string BatchStableId { get; set; } = string.Empty;
    public string BatchRevision { get; set; } = string.Empty;
    public string BatchTitle { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string ReviewStateCode { get; set; } = Synty공간조립검토상태Codes.WaitingForCapture;
    public string SnapshotHash { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<Synty공간조립검토결정이력Record> History { get; set; } = [];
}

public sealed class Synty공간조립검토결정이력Record
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string DecisionCode { get; set; } = string.Empty;
    public List<string> IssueCodes { get; set; } = [];
    public string Note { get; set; } = string.Empty;
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerDisplayName { get; set; } = string.Empty;
    public DateTime DecidedAtUtc { get; set; }
    public long Revision { get; set; }
}

public sealed class Synty공간조립검토촬영업로드Record
{
    public string CaptureUploadId { get; set; } = string.Empty;
    public string BatchStableId { get; set; } = string.Empty;
    public string ReviewItemStableId { get; set; } = string.Empty;
    public string CaptureStableId { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string CaptureBundleHash { get; set; } = string.Empty;
    public string ParentCaptureBundleHash { get; set; } = string.Empty;
    public string SourceCompositionHash { get; set; } = string.Empty;
    public long ExpectedReviewItemRevision { get; set; }
    public string RenderingProfileHash { get; set; } = string.Empty;
    public string StorageProviderCode { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string UploadedSourceSha256 { get; set; } = string.Empty;
    public string StoredImageSha256 { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public string ETag { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}
