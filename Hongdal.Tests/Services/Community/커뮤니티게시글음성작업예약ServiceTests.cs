using Hongdal.Domain.Community;
using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class 커뮤니티게시글음성작업예약ServiceTests
{
    [Fact]
    public void 예약_게시글과같이저장될_대기작업을_한번만_생성한다()
    {
        var now = new DateTime(2026, 7, 13, 14, 0, 0, DateTimeKind.Utc);
        var post = new PlatformCommunityPost { Title = "게시글" };
        var sut = new 커뮤니티게시글음성작업예약Service();

        sut.예약(post, now);
        var first = post.Audio;
        sut.예약(post, now.AddMinutes(1));

        Assert.NotNull(first);
        Assert.Same(first, post.Audio);
        Assert.Same(post, first!.Post);
        Assert.Equal(커뮤니티게시글음성상태.대기, first.Status);
        Assert.Equal(now, first.NextAttemptAtUtc);
    }
}
