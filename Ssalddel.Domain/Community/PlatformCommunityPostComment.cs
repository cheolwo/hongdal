namespace Ssalddel.Domain.Community;

public sealed class PlatformCommunityPostComment
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public bool IsOperatorHidden { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public PlatformCommunityPost Post { get; set; } = null!;
}
