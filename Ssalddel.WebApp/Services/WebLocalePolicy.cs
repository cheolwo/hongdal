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
        return segment?.ToLowerInvariant() switch
        {
            "ko" => DisplayLanguageCodes.Korean,
            "en" => DisplayLanguageCodes.English,
            _ => null
        };
    }

    public static string LanguageSegment(string? languageCode)
        => DisplayLanguageCodes.Normalize(languageCode) == DisplayLanguageCodes.English
            ? "en"
            : "ko";

    public static bool IsCommunityPath(string? relativePath)
    {
        var path = NormalizePath(relativePath);
        return path.Equals("community", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("community/", StringComparison.OrdinalIgnoreCase)
               || path.Equals("ko/community", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("ko/community/", StringComparison.OrdinalIgnoreCase)
               || path.Equals("en/community", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("en/community/", StringComparison.OrdinalIgnoreCase);
    }

    public static string LocalizedCommunityHome(string? languageCode)
        => $"/{LanguageSegment(languageCode)}/community";

    private static string NormalizePath(string? relativePath)
        => (relativePath ?? string.Empty)
            .Split('?', '#')[0]
            .Trim('/');
}
