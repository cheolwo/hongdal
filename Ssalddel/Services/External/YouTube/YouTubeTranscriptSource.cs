using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Services.External.YouTube;

public interface IYouTubeTranscriptSource
{
    bool IsEnabled { get; }

    Task<YouTubeTranscriptResponse?> GetAsync(
        YouTubeTranscriptRequest request,
        CancellationToken cancellationToken);
}
