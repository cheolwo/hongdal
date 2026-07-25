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
    [InlineData("ja", DisplayLanguageCodes.Japanese)]
    [InlineData("ja-JP", DisplayLanguageCodes.Japanese)]
    public void Normalize_UsesSupportedDisplayLanguage(string source, string expected)
        => Assert.Equal(expected, DisplayLanguageCodes.Normalize(source));

    [Fact]
    public void TryNormalize_UnsupportedLanguage_DoesNotReportSuccess()
    {
        var found = DisplayLanguageCodes.TryNormalize("fr-FR", out var languageCode);

        Assert.False(found);
        Assert.Equal(DisplayLanguageCodes.Korean, languageCode);
    }

    [Theory]
    [InlineData("fr-FR, en-US;q=0.8, ko-KR;q=0.6", DisplayLanguageCodes.English)]
    [InlineData("en-US;q=0.4, ko-KR;q=0.9", DisplayLanguageCodes.Korean)]
    [InlineData("ja-JP, en-US;q=0.8", DisplayLanguageCodes.Japanese)]
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
    [InlineData("jp", "JP", DisplayLanguageCodes.Japanese)]
    [InlineData("KOR", null, null)]
    public void CountryRecommendation_KeepsCountrySeparateFromLanguage(
        string source,
        string? expectedCountry,
        string? expectedLanguage)
    {
        Assert.Equal(expectedCountry, PublicCountryLanguageRecommendation.NormalizeCountryCode(source));
        Assert.Equal(expectedLanguage, PublicCountryLanguageRecommendation.Recommend(source));
    }

    [Theory]
    [InlineData(DisplayLanguageCodes.Korean, "ko", "ko", "한국어")]
    [InlineData(DisplayLanguageCodes.English, "en", "en", "English")]
    [InlineData(DisplayLanguageCodes.Japanese, "ja", "ja", "日本語")]
    public void Profile_ProvidesNeutralCodePathAndNativeName(
        string languageCode,
        string neutralCode,
        string pathSegment,
        string nativeName)
    {
        Assert.Equal(neutralCode, DisplayLanguageCodes.ToNeutralCode(languageCode));
        Assert.Equal(pathSegment, DisplayLanguageCodes.ToPathSegment(languageCode));
        Assert.Equal(nativeName, DisplayLanguageCodes.NativeName(languageCode));
        Assert.True(DisplayLanguageCodes.TryFromPathSegment(pathSegment, out var fromPath));
        Assert.Equal(languageCode, fromPath);
    }

    [Fact]
    public void Select_UsesEnglishAsFallbackWhenJapaneseCopyIsNotAvailable()
    {
        Assert.Equal(
            "English copy",
            DisplayLanguageCodes.Select(
                DisplayLanguageCodes.Japanese,
                "한국어 문구",
                "English copy"));
        Assert.Equal(
            "日本語",
            DisplayLanguageCodes.Select(
                DisplayLanguageCodes.Japanese,
                "한국어 문구",
                "English copy",
                "日本語"));
    }
}
