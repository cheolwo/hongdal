using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.External.YouTube;
using 살뜰.Services.Options;

namespace Ssalddel.Services.External.Apify.YouTube;

public sealed class ApifyYouTubeCommentSource : IYouTubeCommentSource
{
    private const string ProviderName = "Apify YouTube Comments Scraper";

    private readonly IApifyActorGateway _gateway;
    private readonly ApifyYouTubeCommentsOptions _options;
    private readonly TimeProvider _timeProvider;

    public ApifyYouTubeCommentSource(
        IApifyActorGateway gateway,
        IOptions<ApifyYouTubeCommentsOptions> options,
        TimeProvider timeProvider)
    {
        _gateway = gateway;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public bool IsEnabled => _options.Enabled;

    public string Provider => ProviderName;

    public async Task<YouTubeCommentCollectionResponse> GetAsync(
        YouTubeCommentSourceRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureEnabled();

        var videoId = YouTubeVideoIdentity.Normalize(request.VideoId, nameof(request.VideoId));
        var videoUrl = YouTubeVideoIdentity.BuildWatchUrl(videoId);
        var maxComments = ResolveMaxComments(request.MaxComments);
        var sort = ResolveSort(request.Sort);
        var input = JsonSerializer.SerializeToElement(new
        {
            startUrls = new[]
            {
                new
                {
                    url = videoUrl,
                    method = "GET"
                }
            },
            maxComments,
            sortCommentsBy = sort
        });

        var result = await _gateway.RunSyncGetDatasetItemsAsync(
            new ApifyActorSyncRequest(
                _options.ActorId,
                input,
                _options.ActorTimeoutSeconds,
                _options.MemoryMegabytes,
                maxComments,
                _options.MaxTotalChargeUsd),
            cancellationToken);

        string? actorError = null;
        var comments = new List<YouTubeCommentDto>();
        foreach (var item in result.Items)
        {
            actorError ??= ApifyYouTubeDatasetJson.NormalizeText(
                ApifyYouTubeDatasetJson.GetString(item, "error"),
                120);
            var parsed = ParseComment(item, videoId);
            if (parsed is not null)
            {
                comments.Add(parsed);
            }
        }

        if (comments.Count == 0 && actorError is not null)
        {
            throw new InvalidOperationException(
                $"Apify YouTube 댓글 Actor가 영상을 수집하지 못했습니다: {actorError}");
        }

        var normalizedComments = comments
            .GroupBy(comment => comment.CommentId, StringComparer.Ordinal)
            .Select(group => group.First())
            .Take(maxComments)
            .ToArray();
        return new YouTubeCommentCollectionResponse(
            videoId,
            videoUrl,
            ProviderName,
            _timeProvider.GetUtcNow().UtcDateTime,
            normalizedComments);
    }

    private YouTubeCommentDto? ParseComment(JsonElement item, string requestedVideoId)
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var itemVideoId = ApifyYouTubeDatasetJson.GetString(item, "videoId", "video_id");
        if (!string.IsNullOrWhiteSpace(itemVideoId)
            && !string.Equals(itemVideoId.Trim(), requestedVideoId, StringComparison.Ordinal))
        {
            return null;
        }

        var text = ApifyYouTubeDatasetJson.NormalizeText(
            ApifyYouTubeDatasetJson.GetString(item, "comment", "text", "content"),
            Math.Clamp(_options.MaxCommentTextCharacters, 1, 50_000));
        if (text is null)
        {
            return null;
        }

        var author = ApifyYouTubeDatasetJson.NormalizeText(
            ApifyYouTubeDatasetJson.GetString(item, "author", "authorName", "authorText"),
            Math.Clamp(_options.MaxAuthorCharacters, 1, 1_000));
        var parentCommentId = ApifyYouTubeDatasetJson.NormalizeText(
            ApifyYouTubeDatasetJson.GetString(item, "replyToCid", "parentCommentId", "parentId"),
            200);
        var commentId = ApifyYouTubeDatasetJson.NormalizeText(
                            ApifyYouTubeDatasetJson.GetString(item, "cid", "commentId", "id"),
                            200)
                        ?? CreateDeterministicCommentId(requestedVideoId, author, text, parentCommentId);
        var type = ApifyYouTubeDatasetJson.GetString(item, "type");
        var replyCount = ApifyYouTubeDatasetJson.GetNonNegativeCount(
            item,
            "replyCount",
            "repliesCount");

        return new YouTubeCommentDto(
            commentId,
            parentCommentId,
            author,
            text,
            ApifyYouTubeDatasetJson.GetUtcDateTime(
                item,
                "publishedAt",
                "publishedAtUtc",
                "date",
                "createdAt"),
            ApifyYouTubeDatasetJson.NormalizeText(
                ApifyYouTubeDatasetJson.GetString(
                    item,
                    "publishedTimeText",
                    "publishedText",
                    "publishedTime"),
                200),
            ApifyYouTubeDatasetJson.GetNonNegativeCount(
                item,
                "voteCount",
                "likeCount",
                "likes"),
            replyCount is null ? 0 : (int)Math.Min(replyCount.Value, int.MaxValue),
            parentCommentId is not null
            || string.Equals(type, "reply", StringComparison.OrdinalIgnoreCase),
            ApifyYouTubeDatasetJson.GetBoolean(
                item,
                false,
                "authorIsChannelOwner",
                "isChannelOwner"),
            ApifyYouTubeDatasetJson.GetBoolean(
                item,
                false,
                "hasCreatorHeart",
                "isHearted"),
            ApifyYouTubeDatasetJson.GetBoolean(
                item,
                false,
                "isPinned",
                "pinned"));
    }

    private int ResolveMaxComments(int? requested)
    {
        var configuredMaximum = Math.Clamp(_options.MaxCommentsPerRequest, 1, 10_000);
        var datasetMaximum = Math.Clamp(_options.MaxDatasetItems, 1, 10_000);
        var effectiveMaximum = Math.Min(configuredMaximum, datasetMaximum);
        var requestedValue = requested ?? _options.DefaultMaxComments;
        if (requestedValue < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requested),
                "YouTube 댓글 수집 개수는 1 이상이어야 합니다.");
        }

        return Math.Min(requestedValue, effectiveMaximum);
    }

    private static string ResolveSort(string? requested)
    {
        var normalized = string.IsNullOrWhiteSpace(requested)
            ? YouTubeCommentSortCodes.Top
            : requested.Trim().ToLowerInvariant();
        return normalized switch
        {
            YouTubeCommentSortCodes.Top => "TOP_COMMENTS",
            YouTubeCommentSortCodes.Newest => "NEWEST_FIRST",
            _ => throw new ArgumentException(
                $"YouTube 댓글 정렬은 {string.Join(", ", YouTubeCommentSortCodes.All)} 중 하나여야 합니다.",
                nameof(requested))
        };
    }

    private static string CreateDeterministicCommentId(
        string videoId,
        string? author,
        string text,
        string? parentCommentId)
    {
        var value = $"{videoId}\n{author}\n{text}\n{parentCommentId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return $"generated-{Convert.ToHexString(hash)[..24].ToLowerInvariant()}";
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("Apify YouTube 댓글 조회가 비활성화되어 있습니다.");
        }
    }
}
