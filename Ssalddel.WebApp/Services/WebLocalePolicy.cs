using Ssalddel.Contracts.Common.Localization;

namespace Ssalddel.WebApp.Services;

public static class WebLocalePolicy
{
    public static string ResolveLanguage(
        string? accountLanguageCode,
        string? cookieLanguageCode,
        IEnumerable<string>? browserLanguageCodes,
        string? countryCode,
        string? serverRecommendation = null)
    {
        if (DisplayLanguageCodes.TryNormalize(accountLanguageCode, out var accountLanguage))
        {
            return accountLanguage;
        }

        if (DisplayLanguageCodes.TryNormalize(cookieLanguageCode, out var cookieLanguage))
        {
            return cookieLanguage;
        }

        foreach (var browserLanguageCode in browserLanguageCodes ?? [])
        {
            if (DisplayLanguageCodes.TryNormalize(browserLanguageCode, out var browserLanguage))
            {
                return browserLanguage;
            }
        }

        var countryLanguage = PublicCountryLanguageRecommendation.Recommend(countryCode);
        if (countryLanguage is not null)
        {
            return countryLanguage;
        }

        return DisplayLanguageCodes.Normalize(serverRecommendation);
    }

    public static string? LanguageFromPath(string? relativePath)
    {
        var segment = NormalizePath(relativePath)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        return DisplayLanguageCodes.TryFromPathSegment(segment, out var languageCode)
            ? languageCode
            : null;
    }

    public static string LanguageSegment(string? languageCode)
        => DisplayLanguageCodes.ToPathSegment(languageCode);

    public static bool IsCommunityPath(string? relativePath)
    {
        var segments = NormalizePath(relativePath)
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return false;
        }

        var communityIndex = DisplayLanguageCodes.TryFromPathSegment(segments[0], out _)
            ? 1
            : 0;
        return segments.Length > communityIndex
               && segments[communityIndex].Equals(
                   "community",
                   StringComparison.OrdinalIgnoreCase);
    }

    public static string LocalizedCommunityHome(string? languageCode)
        => $"/{LanguageSegment(languageCode)}/community";

    private static string NormalizePath(string? relativePath)
        => (relativePath ?? string.Empty)
            .Split('?', '#')[0]
            .Trim('/');
}
