namespace Hongdal.Controllers.Admin.Evidence04;

public sealed class 파일POD응답
{
    public Guid Id { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string UploadStatus { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
