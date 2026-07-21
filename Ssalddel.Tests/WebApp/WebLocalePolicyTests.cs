using Ssalddel.Contracts.Common.Localization;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class WebLocalePolicyTests
{
    [Fact]
    public void AccountLanguage_WinsOverCookieBrowserAndCountry()
    {
        var result = WebLocalePolicy.ResolveLanguage(
            DisplayLanguageCodes.English,
            DisplayLanguageCodes.Korean,
            [DisplayLanguageCodes.Korean],
            "KR");

        Assert.Equal(DisplayLanguageCodes.English, result);
    }

    [Fact]
    public void CookieLanguage_WinsOverBrowserAndCountry()
    {
        var result = WebLocalePolicy.ResolveLanguage(
            null,
            DisplayLanguageCodes.English,
            [DisplayLanguageCodes.Korean],
            "KR");

        Assert.Equal(DisplayLanguageCodes.English, result);
    }

    [Fact]
    public void BrowserLanguage_WinsOverCountryRecommendation()
    {
        var result = WebLocalePolicy.ResolveLanguage(
            null,
            null,
            ["ja-JP", DisplayLanguageCodes.English],
            "KR");

        Assert.Equal(DisplayLanguageCodes.English, result);
    }

    [Fact]
    public void CountryIsOnlyUsedWhenNoUserOrBrowserChoiceExists()
    {
        var result = WebLocalePolicy.ResolveLanguage(null, null, [], "US");

        Assert.Equal(DisplayLanguageCodes.English, result);
    }

    [Theory]
    [InlineData("ko/community", DisplayLanguageCodes.Korean)]
    [InlineData("en/community?board=free", DisplayLanguageCodes.English)]
    [InlineData("community", null)]
    public void LanguageFromPath_RecognizesOnlyExplicitLanguageSegment(
        string path,
        string? expected)
        => Assert.Equal(expected, WebLocalePolicy.LanguageFromPath(path));
}
