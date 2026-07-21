namespace Ssalddel.Contracts.Common.Localization;

public static class SsalddelDisplayLanguageClaimTypes
{
    public const string PreferredLanguage = "ssalddel:preferred-language";
}

public sealed class PublicLocaleRecommendationResponse
{
    public string? CountryCode { get; set; }
    public string? BrowserLanguageCode { get; set; }
    public string RecommendedLanguageCode { get; set; } = DisplayLanguageCodes.Korean;
    public bool CountryRecommendationAvailable { get; set; }
}

public static class PublicCountryLanguageRecommendation
{
    public static string? NormalizeCountryCode(string? value)
    {
        var candidate = value?.Trim();
        return candidate is { Length: 2 }
               && candidate.All(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
            ? candidate.ToUpperInvariant()
            : null;
    }

    public static string? Recommend(string? countryCode)
        => NormalizeCountryCode(countryCode) switch
        {
            "KR" => DisplayLanguageCodes.Korean,
            "US" => DisplayLanguageCodes.English,
            _ => null
        };
}
