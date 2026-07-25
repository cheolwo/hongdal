using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Services.Localization;

namespace Ssalddel.Tests.Services.Localization;

public sealed class PublicLocaleRecommendationUseCaseTests
{
    private readonly PublicLocaleRecommendationUseCase _useCase = new();

    [Fact]
    public void BrowserLanguage_WinsWithoutChangingCountryContext()
    {
        var result = _useCase.Recommend("en-US,en;q=0.9", "KR");

        Assert.Equal("KR", result.CountryCode);
        Assert.Equal(DisplayLanguageCodes.English, result.BrowserLanguageCode);
        Assert.Equal(DisplayLanguageCodes.English, result.RecommendedLanguageCode);
        Assert.True(result.CountryRecommendationAvailable);
    }

    [Fact]
    public void TrustedCountry_IsOnlyFallbackForUnsupportedBrowserLanguages()
    {
        var result = _useCase.Recommend("fr-FR,de;q=0.8", "US");

        Assert.Equal("US", result.CountryCode);
        Assert.Null(result.BrowserLanguageCode);
        Assert.Equal(DisplayLanguageCodes.English, result.RecommendedLanguageCode);
        Assert.True(result.CountryRecommendationAvailable);
    }

    [Fact]
    public void JapaneseBrowserLanguage_WinsWithoutChangingCountryContext()
    {
        var result = _useCase.Recommend("ja-JP,en;q=0.8", "US");

        Assert.Equal("US", result.CountryCode);
        Assert.Equal(DisplayLanguageCodes.Japanese, result.BrowserLanguageCode);
        Assert.Equal(DisplayLanguageCodes.Japanese, result.RecommendedLanguageCode);
        Assert.True(result.CountryRecommendationAvailable);
    }

    [Fact]
    public void JapanCountry_RecommendsJapaneseWhenBrowserLanguageIsUnsupported()
    {
        var result = _useCase.Recommend("fr-FR", "JP");

        Assert.Equal("JP", result.CountryCode);
        Assert.Null(result.BrowserLanguageCode);
        Assert.Equal(DisplayLanguageCodes.Japanese, result.RecommendedLanguageCode);
        Assert.True(result.CountryRecommendationAvailable);
    }

    [Fact]
    public void MissingSignals_FallBackToKoreanWithoutInventingCountry()
    {
        var result = _useCase.Recommend(null, null);

        Assert.Null(result.CountryCode);
        Assert.Null(result.BrowserLanguageCode);
        Assert.Equal(DisplayLanguageCodes.Korean, result.RecommendedLanguageCode);
        Assert.False(result.CountryRecommendationAvailable);
    }
}
