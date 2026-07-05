namespace Hongdal.Domain.Community;

public sealed class PlatformCommunityPost
{
    public long Id { get; set; }
    public string AppKey { get; set; } = "platform";
    public string Category { get; set; } = "자유";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SharedLinkUrl { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public bool IsReportBoardPost { get; set; }
    public string? ReporterDisplayName { get; set; }
    public string? ReportedDisplayName { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsOperatorPinned { get; set; }
    public DateTime? OperatorPinnedAtUtc { get; set; }
    public int RecommendationCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime? LastEngagedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<PlatformCommunityPostAttachment> Attachments { get; set; } = new List<PlatformCommunityPostAttachment>();
    public ICollection<PlatformCommunityPostComment> Comments { get; set; } = new List<PlatformCommunityPostComment>();
    public ICollection<PlatformCommunityPostRecommendation> Recommendations { get; set; } = new List<PlatformCommunityPostRecommendation>();
}
