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

        Assert.Equal(DisplayLanguageCodes.Japanese, result);
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
    [InlineData("ja/community", DisplayLanguageCodes.Japanese)]
    [InlineData("community", null)]
    public void LanguageFromPath_RecognizesOnlyExplicitLanguageSegment(
        string path,
        string? expected)
        => Assert.Equal(expected, WebLocalePolicy.LanguageFromPath(path));

    [Theory]
    [InlineData("community", true)]
    [InlineData("ko/community/posts/1", true)]
    [InlineData("en/community", true)]
    [InlineData("ja/community?board=food", true)]
    [InlineData("fr/community", false)]
    [InlineData("ja/work", false)]
    public void IsCommunityPath_UsesLanguageCatalog(string path, bool expected)
        => Assert.Equal(expected, WebLocalePolicy.IsCommunityPath(path));

    [Fact]
    public void JapaneseCommunityHome_UsesJapanesePathSegment()
        => Assert.Equal(
            "/ja/community",
            WebLocalePolicy.LocalizedCommunityHome(DisplayLanguageCodes.Japanese));
}
