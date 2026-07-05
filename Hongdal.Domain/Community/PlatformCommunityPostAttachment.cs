namespace Hongdal.Domain.Community;

public sealed class PlatformCommunityPostAttachment
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int CommentCount { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public PlatformCommunityPost? Post { get; set; }
    public ICollection<PlatformCommunityPostAttachmentComment> Comments { get; set; } = new List<PlatformCommunityPostAttachmentComment>();
}
