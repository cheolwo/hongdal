using Hongdal.Domain.Community;

namespace Hongdal.Services.Community;

public interface I커뮤니티게시글음성작업예약Service
{
    void 예약(PlatformCommunityPost post, DateTime now);
}

public sealed class 커뮤니티게시글음성작업예약Service : I커뮤니티게시글음성작업예약Service
{
    public void 예약(PlatformCommunityPost post, DateTime now)
    {
        if (post.Audio is not null)
        {
            return;
        }

        post.Audio = new PlatformCommunityPostAudio
        {
            Post = post,
            Status = 커뮤니티게시글음성상태.대기,
            Provider = "Typecast",
            NextAttemptAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }
}
