using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Services.External.YouTube;

public interface IYouTubeTranscriptSource
{
    bool IsEnabled { get; }

    string Provider { get; }

    Task<YouTubeTranscriptResponse?> GetAsync(
        YouTubeTranscriptRequest request,
        CancellationToken cancellationToken);
}
