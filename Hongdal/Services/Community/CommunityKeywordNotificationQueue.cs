using Hongdal.Domain.Community;

namespace Hongdal.Services.Community;

public interface ICommunityKeywordNotificationQueue
{
    void Enqueue(PlatformCommunityPost post, DateTime nowUtc);
}

public sealed class CommunityKeywordNotificationQueue : ICommunityKeywordNotificationQueue
{
    public void Enqueue(PlatformCommunityPost post, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(post);
        if (post.KeywordNotificationScan is not null)
        {
            return;
        }

        post.KeywordNotificationScan = new PlatformCommunityPostKeywordScan
        {
            Post = post,
            Status = CommunityKeywordScanStatuses.Pending,
            NextAttemptAtUtc = nowUtc,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }
}
