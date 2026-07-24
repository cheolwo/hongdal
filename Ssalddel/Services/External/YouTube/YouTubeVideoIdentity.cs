namespace Ssalddel.Services.External.YouTube;

internal static class YouTubeVideoIdentity
{
    public static string Normalize(string? value, string parameterName)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is < 1 or > 100
            || normalized.Any(character =>
                !(character is >= 'A' and <= 'Z')
                && !(character is >= 'a' and <= 'z')
                && !(character is >= '0' and <= '9')
                && character is not '_' and not '-'))
        {
            throw new ArgumentException(
                "YouTube VideoId는 영상 ID 형식이어야 합니다.",
                parameterName);
        }

        return normalized;
    }

    public static string BuildWatchUrl(string videoId)
        => $"https://www.youtube.com/watch?v={Normalize(videoId, nameof(videoId))}";
}
