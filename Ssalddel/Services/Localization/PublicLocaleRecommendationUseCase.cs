using Ssalddel.Contracts.Common.Localization;

namespace Ssalddel.Services.Localization;

public interface IPublicLocaleRecommendationUseCase
{
    PublicLocaleRecommendationResponse Recommend(string? acceptLanguage, string? trustedCountryCode);
}

public sealed class PublicLocaleRecommendationUseCase : IPublicLocaleRecommendationUseCase
{
    public PublicLocaleRecommendationResponse Recommend(
        string? acceptLanguage,
        string? trustedCountryCode)
    {
        var countryCode = PublicCountryLanguageRecommendation.NormalizeCountryCode(trustedCountryCode);
        var browserLanguage = DisplayLanguageCodes.TryResolveAcceptLanguage(
            acceptLanguage,
            out var resolvedBrowserLanguage)
            ? resolvedBrowserLanguage
            : null;
        var countryLanguage = PublicCountryLanguageRecommendation.Recommend(countryCode);

        return new PublicLocaleRecommendationResponse
        {
            CountryCode = countryCode,
            BrowserLanguageCode = browserLanguage,
            RecommendedLanguageCode = browserLanguage
                                      ?? countryLanguage
                                      ?? DisplayLanguageCodes.Korean,
            CountryRecommendationAvailable = countryLanguage is not null
        };
    }
}
