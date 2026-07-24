using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Services.External.YouTube;

public sealed record YouTubeCommentSourceRequest(
    string VideoId,
    int? MaxComments = null,
    string? Sort = null);

public interface IYouTubeCommentSource
{
    bool IsEnabled { get; }

    string Provider { get; }

    Task<YouTubeCommentCollectionResponse> GetAsync(
        YouTubeCommentSourceRequest request,
        CancellationToken cancellationToken);
}
