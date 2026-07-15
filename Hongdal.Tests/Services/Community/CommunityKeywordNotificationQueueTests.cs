using Hongdal.Domain.Community;
using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class CommunityKeywordNotificationQueueTests
{
    [Fact]
    public void Enqueue_AddsOnePendingScanToPost()
    {
        var queue = new CommunityKeywordNotificationQueue();
        var post = new PlatformCommunityPost();
        var now = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc);

        queue.Enqueue(post, now);
        var first = post.KeywordNotificationScan;
        queue.Enqueue(post, now.AddMinutes(1));

        Assert.NotNull(first);
        Assert.Same(first, post.KeywordNotificationScan);
        Assert.Same(post, first.Post);
        Assert.Equal(CommunityKeywordScanStatuses.Pending, first.Status);
        Assert.Equal(now, first.NextAttemptAtUtc);
    }
}
