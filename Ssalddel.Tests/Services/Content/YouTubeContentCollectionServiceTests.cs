using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Content;
using Ssalddel.Services.External.YouTube;

namespace Ssalddel.Tests.Services.Content;

public sealed class YouTubeContentCollectionServiceTests
{
    [Fact]
    public async Task CollectAsync_영상과자막과댓글을_하나의응답으로합친다()
    {
        var video = CreateVideo();
        var transcript = new YouTubeTranscriptResponse(
            video.VideoId,
            video.OriginalUrl,
            "ko",
            "transcript-provider",
            new DateTime(2026, 7, 23, 1, 0, 0, DateTimeKind.Utc),
            [new YouTubeTranscriptSegmentDto(0, 2, "김치")],
            "김치");
        var comments = new YouTubeCommentCollectionResponse(
            video.VideoId,
            video.OriginalUrl,
            "comments-provider",
            new DateTime(2026, 7, 23, 1, 1, 0, DateTimeKind.Utc),
            [
                new YouTubeCommentDto(
                    "comment-1",
                    null,
                    "@viewer",
                    "재료가 궁금합니다.",
                    null,
                    "1 day ago",
                    5,
                    0,
                    false,
                    false,
                    false,
                    false)
            ]);
        var transcriptSource = new StubTranscriptSource(true, _ => Task.FromResult<YouTubeTranscriptResponse?>(transcript));
        var commentSource = new StubCommentSource(true, _ => Task.FromResult(comments));
        var service = CreateService(video, transcriptSource, commentSource);

        var result = await service.CollectAsync(
            video.VideoId,
            new YouTubeContentCollectionRequest
            {
                MaxComments = 20,
                CommentSort = YouTubeCommentSortCodes.Newest
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(video, result.Video);
        Assert.Same(transcript, result.Transcript);
        Assert.Same(comments, result.Comments);
        Assert.True(result.IsComplete);
        Assert.True(result.HasPartialData);
        Assert.Equal("ko", transcriptSource.Request?.TargetLanguage);
        Assert.Equal(20, commentSource.Request?.MaxComments);
        Assert.Equal(YouTubeCommentSortCodes.Newest, commentSource.Request?.Sort);
        Assert.All(result.Sources, source => Assert.True(source.Succeeded));
    }

    [Fact]
    public async Task CollectAsync_한원천실패를_부분응답상태로남긴다()
    {
        var video = CreateVideo();
        var transcript = new YouTubeTranscriptResponse(
            video.VideoId,
            video.OriginalUrl,
            "ko",
            "transcript-provider",
            DateTime.UtcNow,
            [new YouTubeTranscriptSegmentDto(0, 1, "된장")],
            "된장");
        var service = CreateService(
            video,
            new StubTranscriptSource(true, _ => Task.FromResult<YouTubeTranscriptResponse?>(transcript)),
            new StubCommentSource(
                true,
                _ => throw new HttpRequestException("provider unavailable")));

        var result = await service.CollectAsync(
            video.VideoId,
            new YouTubeContentCollectionRequest(),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Transcript);
        Assert.Null(result.Comments);
        Assert.False(result.IsComplete);
        Assert.True(result.HasPartialData);
        var failure = Assert.Single(
            result.Sources,
            source => source.SourceKey == YouTubeContentCollectionSourceKeys.Comments);
        Assert.False(failure.Succeeded);
        Assert.Equal("외부 수집 원천을 조회하지 못했습니다.", failure.Message);
    }

    [Fact]
    public async Task CollectAsync_비활성원천은_호출하지않고상태에표시한다()
    {
        var video = CreateVideo();
        var transcriptSource = new StubTranscriptSource(false, _ => throw new InvalidOperationException());
        var commentSource = new StubCommentSource(false, _ => throw new InvalidOperationException());
        var service = CreateService(video, transcriptSource, commentSource);

        var result = await service.CollectAsync(
            video.VideoId,
            new YouTubeContentCollectionRequest(),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.IsComplete);
        Assert.False(result.HasPartialData);
        Assert.Equal(0, transcriptSource.CallCount);
        Assert.Equal(0, commentSource.CallCount);
        Assert.Equal(2, result.Sources.Count(source => !source.Enabled));
    }

    private static YouTubeContentCollectionService CreateService(
        YouTubeSocialContextVideoDto video,
        StubTranscriptSource transcriptSource,
        StubCommentSource commentSource)
        => new(
            new StubVideoSource(video),
            transcriptSource,
            commentSource,
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 23, 2, 3, 4, TimeSpan.Zero)),
            NullLogger<YouTubeContentCollectionService>.Instance);

    private static YouTubeSocialContextVideoDto CreateVideo()
        => new(
            "video-1",
            "Food channel",
            "Kimchi",
            "Summary",
            "https://www.youtube.com/watch?v=video-1",
            null,
            new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc),
            "KR",
            "ko");

    private sealed class StubVideoSource : IYouTubeSocialContextVideoSource
    {
        private readonly YouTubeSocialContextVideoDto _video;

        public StubVideoSource(YouTubeSocialContextVideoDto video) => _video = video;

        public Task<YouTubeSocialContextVideoDto?> GetAsync(
            string videoId,
            CancellationToken cancellationToken)
            => Task.FromResult<YouTubeSocialContextVideoDto?>(
                string.Equals(videoId, _video.VideoId, StringComparison.Ordinal)
                    ? _video
                    : null);
    }

    private sealed class StubTranscriptSource : IYouTubeTranscriptSource
    {
        private readonly Func<YouTubeTranscriptRequest, Task<YouTubeTranscriptResponse?>> _collect;

        public StubTranscriptSource(
            bool enabled,
            Func<YouTubeTranscriptRequest, Task<YouTubeTranscriptResponse?>> collect)
        {
            IsEnabled = enabled;
            _collect = collect;
        }

        public bool IsEnabled { get; }

        public string Provider => "transcript-provider";

        public int CallCount { get; private set; }

        public YouTubeTranscriptRequest? Request { get; private set; }

        public Task<YouTubeTranscriptResponse?> GetAsync(
            YouTubeTranscriptRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            return _collect(request);
        }
    }

    private sealed class StubCommentSource : IYouTubeCommentSource
    {
        private readonly Func<YouTubeCommentSourceRequest, Task<YouTubeCommentCollectionResponse>> _collect;

        public StubCommentSource(
            bool enabled,
            Func<YouTubeCommentSourceRequest, Task<YouTubeCommentCollectionResponse>> collect)
        {
            IsEnabled = enabled;
            _collect = collect;
        }

        public bool IsEnabled { get; }

        public string Provider => "comments-provider";

        public int CallCount { get; private set; }

        public YouTubeCommentSourceRequest? Request { get; private set; }

        public Task<YouTubeCommentCollectionResponse> GetAsync(
            YouTubeCommentSourceRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            return _collect(request);
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
