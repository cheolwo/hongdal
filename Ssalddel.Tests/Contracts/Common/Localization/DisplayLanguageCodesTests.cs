using Ssalddel.Contracts.Common.Localization;

namespace Ssalddel.Tests.Contracts.Common.Localization;

public sealed class DisplayLanguageCodesTests
{
    [Theory]
    [InlineData("ko", DisplayLanguageCodes.Korean)]
    [InlineData("ko-KR", DisplayLanguageCodes.Korean)]
    [InlineData("en", DisplayLanguageCodes.English)]
    [InlineData("en-US", DisplayLanguageCodes.English)]
    [InlineData("EN-gb", DisplayLanguageCodes.English)]
    public void Normalize_UsesSupportedDisplayLanguage(string source, string expected)
        => Assert.Equal(expected, DisplayLanguageCodes.Normalize(source));

    [Fact]
    public void TryNormalize_UnsupportedLanguage_DoesNotReportSuccess()
    {
        var found = DisplayLanguageCodes.TryNormalize("ja-JP", out var languageCode);

        Assert.False(found);
        Assert.Equal(DisplayLanguageCodes.Korean, languageCode);
    }

    [Theory]
    [InlineData("fr-FR, en-US;q=0.8, ko-KR;q=0.6", DisplayLanguageCodes.English)]
    [InlineData("en-US;q=0.4, ko-KR;q=0.9", DisplayLanguageCodes.Korean)]
    public void TryResolveAcceptLanguage_UsesSupportedLanguageWithHighestQuality(
        string source,
        string expected)
    {
        var found = DisplayLanguageCodes.TryResolveAcceptLanguage(source, out var languageCode);

        Assert.True(found);
        Assert.Equal(expected, languageCode);
    }

    [Theory]
    [InlineData("kr", "KR", DisplayLanguageCodes.Korean)]
    [InlineData("US", "US", DisplayLanguageCodes.English)]
    [InlineData("KOR", null, null)]
    public void CountryRecommendation_KeepsCountrySeparateFromLanguage(
        string source,
        string? expectedCountry,
        string? expectedLanguage)
    {
        Assert.Equal(expectedCountry, PublicCountryLanguageRecommendation.NormalizeCountryCode(source));
        Assert.Equal(expectedLanguage, PublicCountryLanguageRecommendation.Recommend(source));
    }
}
