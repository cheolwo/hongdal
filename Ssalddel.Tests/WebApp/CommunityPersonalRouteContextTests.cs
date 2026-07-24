using Ssalddel.WebApp.Models;

namespace Ssalddel.Tests.WebApp;

public sealed class CommunityPersonalRouteContextTests
{
    [Fact]
    public void BaseRoute_UsesOverviewSection()
    {
        var context = CommunityPersonalRouteContext.Resolve(sectionKey: null);

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
        var context = CommunityPersonalRouteContext.Resolve(sectionKey);

        Assert.Equal(sectionKey, context.SectionKey);
        Assert.Equal($"{title} · 살뜰 커뮤니티", context.PageTitle);
    }

    [Fact]
    public void PersonalDecorationSection_UsesManagementRoute()
    {
        var context = CommunityPersonalRouteContext.Resolve("decorations");

        Assert.Equal("decorations", context.SectionKey);
        Assert.Equal("/community/me/decorations", context.Section.Href);
    }

    [Fact]
    public void VisibleNavigationSections_ExposeOnlyProfileAndPosts()
    {
        Assert.Equal(
        [
            ("overview", "내 정보", "/community/me"),
            ("posts", "내 글", "/community/me/posts")
        ],
            CommunityPersonalRouteContext.VisibleNavigationSections
                .Select(section => (section.Key, section.Title, section.Href)));

        Assert.Contains(
            CommunityPersonalRouteContext.Sections,
            section => section.Key == "settings");
        Assert.DoesNotContain(
            CommunityPersonalRouteContext.VisibleNavigationSections,
            section => section.Key == "settings");
    }

    [Fact]
    public void UnknownSection_FallsBackToOverview()
    {
        var context = CommunityPersonalRouteContext.Resolve("unknown");

        Assert.Equal("overview", context.SectionKey);
    }
}
