using Ssalddel.WebApp.Models;

namespace Ssalddel.Tests.WebApp;

public sealed class CommunityPersonalRouteContextTests
{
    [Fact]
    public void BaseRoute_UsesOverviewSection()
    {
        var context = CommunityPersonalRouteContext.Resolve(
            "community/me",
            sectionKey: null,
            productKey: null);

        Assert.Equal("overview", context.SectionKey);
        Assert.Equal("내 정보 · 살뜰 커뮤니티", context.PageTitle);
    }

    [Theory]
    [InlineData("posts", "내 글")]
    [InlineData("actions", "참여 중")]
    [InlineData("ledgers", "내 원장")]
    [InlineData("notifications", "알림")]
    [InlineData("settings", "사용 설정")]
    public void KnownSection_UsesDedicatedPageContext(string sectionKey, string title)
    {
        var context = CommunityPersonalRouteContext.Resolve(
            $"community/me/{sectionKey}",
            sectionKey,
            productKey: null);

        Assert.Equal(sectionKey, context.SectionKey);
        Assert.Equal($"{title} · 살뜰 커뮤니티", context.PageTitle);
    }

    [Theory]
    [InlineData("community/decorations", null, null)]
    [InlineData("community/decorations/theme-pack", null, "theme-pack")]
    [InlineData("community/me/decorations", "decorations", null)]
    public void DecorationRoute_UsesDecorationSection(
        string path,
        string? sectionKey,
        string? productKey)
    {
        var context = CommunityPersonalRouteContext.Resolve(path, sectionKey, productKey);

        Assert.Equal("decorations", context.SectionKey);
        Assert.Equal(productKey, context.ProductKey);
    }

    [Fact]
    public void UnknownSection_FallsBackToOverview()
    {
        var context = CommunityPersonalRouteContext.Resolve(
            "community/me/unknown",
            "unknown",
            productKey: null);

        Assert.Equal("overview", context.SectionKey);
    }
}
