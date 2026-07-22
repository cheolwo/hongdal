namespace Ssalddel.Services.Community;

public sealed record CommunityTextTranslationResult(
    string Title,
    string Body,
    string Provider,
    string ProviderModelVersion);

public interface ICommunityTextTranslationProvider
{
    bool IsAvailable { get; }

    Task<CommunityTextTranslationResult> TranslateAsync(
        string title,
        string body,
        string sourceLanguageCode,
        string targetLanguageCode,
        CancellationToken cancellationToken);
}
