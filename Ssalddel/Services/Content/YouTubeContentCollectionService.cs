using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.External.YouTube;

namespace Ssalddel.Services.Content;

public interface IYouTubeContentCollectionService
{
    Task<YouTubeContentCollectionResponse?> CollectAsync(
        string videoId,
        YouTubeContentCollectionRequest request,
        CancellationToken cancellationToken);
}

public sealed class YouTubeContentCollectionService : IYouTubeContentCollectionService
{
    private const string VideoProvider = "YouTube 감시 저장소";

    private readonly IYouTubeSocialContextVideoSource _videoSource;
    private readonly IYouTubeTranscriptSource _transcriptSource;
    private readonly IYouTubeCommentSource _commentSource;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<YouTubeContentCollectionService> _logger;

    public YouTubeContentCollectionService(
        IYouTubeSocialContextVideoSource videoSource,
        IYouTubeTranscriptSource transcriptSource,
        IYouTubeCommentSource commentSource,
        TimeProvider timeProvider,
        ILogger<YouTubeContentCollectionService> logger)
    {
        _videoSource = videoSource;
        _transcriptSource = transcriptSource;
        _commentSource = commentSource;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<YouTubeContentCollectionResponse?> CollectAsync(
        string videoId,
        YouTubeContentCollectionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalizedVideoId = YouTubeVideoIdentity.Normalize(videoId, nameof(videoId));
        var video = await _videoSource.GetAsync(normalizedVideoId, cancellationToken);
        if (video is null)
        {
            return null;
        }

        var targetLanguage = string.IsNullOrWhiteSpace(request.TargetLanguage)
                             && !string.Equals(video.LanguageCode, "und", StringComparison.OrdinalIgnoreCase)
            ? video.LanguageCode
            : request.TargetLanguage;
        var transcriptTask = CollectSourceAsync(
            normalizedVideoId,
            YouTubeContentCollectionSourceKeys.Transcript,
            _transcriptSource.Provider,
            _transcriptSource.IsEnabled,
            () => _transcriptSource.GetAsync(
                new YouTubeTranscriptRequest(normalizedVideoId, targetLanguage),
                cancellationToken),
            transcript => transcript.Segments.Count,
            "공개 자막을 찾지 못했습니다.",
            cancellationToken);
        var commentsTask = CollectSourceAsync(
            normalizedVideoId,
            YouTubeContentCollectionSourceKeys.Comments,
            _commentSource.Provider,
            _commentSource.IsEnabled,
            async () => (YouTubeCommentCollectionResponse?)await _commentSource.GetAsync(
                new YouTubeCommentSourceRequest(
                    normalizedVideoId,
                    request.MaxComments,
                    request.CommentSort),
                cancellationToken),
            comments => comments.Comments.Count,
            "공개 댓글을 찾지 못했습니다.",
            cancellationToken);

        await Task.WhenAll(transcriptTask, commentsTask);
        var transcriptAttempt = await transcriptTask;
        var commentsAttempt = await commentsTask;
        return new YouTubeContentCollectionResponse(
            _timeProvider.GetUtcNow().UtcDateTime,
            video,
            transcriptAttempt.Value,
            commentsAttempt.Value,
            [
                new YouTubeContentCollectionSourceStatusDto(
                    YouTubeContentCollectionSourceKeys.Video,
                    VideoProvider,
                    true,
                    true,
                    1,
                    null),
                transcriptAttempt.Status,
                commentsAttempt.Status
            ]);
    }

    private async Task<CollectionAttempt<T>> CollectSourceAsync<T>(
        string videoId,
        string sourceKey,
        string provider,
        bool enabled,
        Func<Task<T?>> collect,
        Func<T, int> count,
        string emptyMessage,
        CancellationToken cancellationToken)
        where T : class
    {
        if (!enabled)
        {
            return new CollectionAttempt<T>(
                null,
                new YouTubeContentCollectionSourceStatusDto(
                    sourceKey,
                    provider,
                    false,
                    false,
                    0,
                    "설정에서 수집 모듈이 비활성화되어 있습니다."));
        }

        try
        {
            var value = await collect();
            return new CollectionAttempt<T>(
                value,
                new YouTubeContentCollectionSourceStatusDto(
                    sourceKey,
                    provider,
                    true,
                    true,
                    value is null ? 0 : count(value),
                    value is null || count(value) == 0 ? emptyMessage : null));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ArgumentException exception)
        {
            return Failed<T>(sourceKey, provider, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Failed<T>(sourceKey, provider, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "YouTube 통합 콘텐츠 수집 원천 호출에 실패했습니다. VideoId={VideoId}, SourceKey={SourceKey}",
                videoId,
                sourceKey);
            return Failed<T>(
                sourceKey,
                provider,
                "외부 수집 원천을 조회하지 못했습니다.");
        }
    }

    private static CollectionAttempt<T> Failed<T>(
        string sourceKey,
        string provider,
        string message)
        where T : class
        => new(
            null,
            new YouTubeContentCollectionSourceStatusDto(
                sourceKey,
                provider,
                true,
                false,
                0,
                message));

    private sealed record CollectionAttempt<T>(
        T? Value,
        YouTubeContentCollectionSourceStatusDto Status)
        where T : class;
}
