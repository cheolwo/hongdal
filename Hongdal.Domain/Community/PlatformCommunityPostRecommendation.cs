namespace Hongdal.Domain.Community;

public sealed class PlatformCommunityPostRecommendation
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public string RecommenderKey { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public PlatformCommunityPost Post { get; set; } = null!;
}
