using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.External.Apify;
using Ssalddel.Services.External.Apify.YouTube;
using Ssalddel.Services.External.YouTube;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Content;

public sealed class ApifyYouTubeCommentSourceTests
{
    [Fact]
    public async Task GetAsync_Apify댓글을_정규화하고_입력한도를전달한다()
    {
        var gateway = new CapturingGateway(
            """
            [
              {
                "comment": "  First   comment  ",
                "cid": "comment-1",
                "author": "@food",
                "videoId": "abc123_DEF-1",
                "publishedAt": "2026-07-22T01:02:03Z",
                "publishedTimeText": "1 day ago",
                "replyCount": 2,
                "voteCount": "1.2K",
                "authorIsChannelOwner": true,
                "hasCreatorHeart": true,
                "isPinned": true,
                "type": "comment"
              },
              {
                "comment": "Reply",
                "cid": "comment-2",
                "replyToCid": "comment-1",
                "author": "@viewer",
                "videoId": "abc123_DEF-1",
                "voteCount": 3,
                "type": "reply"
              }
            ]
            """);
        var source = CreateSource(gateway);

        var result = await source.GetAsync(
            new YouTubeCommentSourceRequest(
                "abc123_DEF-1",
                MaxComments: 2,
                Sort: YouTubeCommentSortCodes.Newest),
            CancellationToken.None);

        Assert.Equal("abc123_DEF-1", result.VideoId);
        Assert.Equal("Apify YouTube Comments Scraper", result.Provider);
        Assert.Equal(2, result.Comments.Count);
        Assert.Equal("First comment", result.Comments[0].Text);
        Assert.Equal(1_200L, result.Comments[0].LikeCount);
        Assert.Equal(new DateTime(2026, 7, 22, 1, 2, 3, DateTimeKind.Utc), result.Comments[0].PublishedAtUtc);
        Assert.True(result.Comments[0].IsChannelOwner);
        Assert.True(result.Comments[0].HasCreatorHeart);
        Assert.True(result.Comments[0].IsPinned);
        Assert.True(result.Comments[1].IsReply);
        Assert.Equal("comment-1", result.Comments[1].ParentCommentId);

        var request = Assert.IsType<ApifyActorSyncRequest>(gateway.Request);
        Assert.Equal("streamers~youtube-comments-scraper", request.ActorId);
        Assert.Equal(2, request.MaxItems);
        Assert.Equal(2, request.Input.GetProperty("maxComments").GetInt32());
        Assert.Equal("NEWEST_FIRST", request.Input.GetProperty("sortCommentsBy").GetString());
        Assert.Equal(
            "https://www.youtube.com/watch?v=abc123_DEF-1",
            request.Input
                .GetProperty("startUrls")[0]
                .GetProperty("url")
                .GetString());
    }

    [Fact]
    public async Task GetAsync_식별자가없는댓글은_결정적식별자를생성한다()
    {
        var gateway = new CapturingGateway(
            """
            [
              {
                "comment": "Same comment",
                "author": "@food",
                "videoId": "abc123_DEF-1"
              },
              {
                "comment": "Same comment",
                "author": "@food",
                "videoId": "abc123_DEF-1"
              },
              {
                "comment": "Different video",
                "videoId": "other-video"
              }
            ]
            """);
        var source = CreateSource(gateway);

        var result = await source.GetAsync(
            new YouTubeCommentSourceRequest("abc123_DEF-1"),
            CancellationToken.None);

        var comment = Assert.Single(result.Comments);
        Assert.StartsWith("generated-", comment.CommentId, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAsync_Actor오류항목만있으면_실패로처리한다()
    {
        var gateway = new CapturingGateway(
            """
            [
              {
                "url": "https://www.youtube.com/watch?v=abc123_DEF-1",
                "error": "VIDEO_UNAVAILABLE"
              }
            ]
            """);
        var source = CreateSource(gateway);

        var action = () => source.GetAsync(
            new YouTubeCommentSourceRequest("abc123_DEF-1"),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Contains("VIDEO_UNAVAILABLE", exception.Message, StringComparison.Ordinal);
    }

    private static ApifyYouTubeCommentSource CreateSource(CapturingGateway gateway)
        => new(
            gateway,
            Options.Create(new ApifyYouTubeCommentsOptions
            {
                Enabled = true,
                ActorId = "streamers~youtube-comments-scraper",
                DefaultMaxComments = 10,
                MaxDatasetItems = 100,
                MaxCommentsPerRequest = 300
            }),
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 23, 1, 2, 3, TimeSpan.Zero)));

    private sealed class CapturingGateway : IApifyActorGateway
    {
        private readonly string _body;

        public CapturingGateway(string body) => _body = body;

        public ApifyActorSyncRequest? Request { get; private set; }

        public Task<ApifyActorSyncResult> RunSyncGetDatasetItemsAsync(
            ApifyActorSyncRequest request,
            CancellationToken cancellationToken)
        {
            Request = request;
            using var document = JsonDocument.Parse(_body);
            var items = document.RootElement
                .EnumerateArray()
                .Select(item => item.Clone())
                .ToArray();
            return Task.FromResult(new ApifyActorSyncResult(request.ActorId, items));
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
