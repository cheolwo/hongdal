namespace Ssalddel.Tests.Architecture;

public sealed class CommunityWorkspacePageCompositionTests
{
    [Theory]
    [InlineData("CommunityWorkspacePage.razor", "/community/workspace", "<CommunityWorkspaceScreen")]
    [InlineData("CommunityBoardManagementPage.razor", "/community/boards/manage", "<CommunityBoardManagementScreen")]
    [InlineData("CommunityLedgerDraftPage.razor", "/community/ledgers/new", "<CommunityLedgerDraftScreen")]
    [InlineData("CommunityPostComposePage.razor", "/community/write", "<CommunityPostComposeScreen")]
    [InlineData("CommunityRecommendedPostsPage.razor", "/community/posts/recommended", "<CommunityRecommendedPostListScreen")]
    [InlineData("CommunityRecommendedPostDetailPage.razor", "/community/posts/recommended/detail", "<CommunityRecommendedPostDetailScreen")]
    [InlineData("CommunityPostDetailPage.razor", "/community/posts/{PostId:long}", "<CommunityPostDetailScreen")]
    public void 커뮤니티route는_사용자목표별_공용screen하나를조립한다(
        string pageFileName,
        string route,
        string screenMarkup)
    {
        var pagePath = Path.Combine(FindRepositoryRoot(), "Ssalddel.WebApp", "Pages", pageFileName);
        var source = File.ReadAllText(pagePath);
        var routeDirectives = File.ReadLines(pagePath)
            .Where(line => line.TrimStart().StartsWith("@page ", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(routeDirectives);
        Assert.Equal($"@page \"{route}\"", routeDirectives[0].Trim());
        Assert.Contains(screenMarkup, source);
        Assert.Contains("<CommunityRoutePageFrame", source);
        Assert.DoesNotContain("<PlatformCommunityHome", source);
        Assert.DoesNotContain("ComposeOnly=", source);
        Assert.DoesNotContain("ListOnly=", source);
        Assert.DoesNotContain("PostDetailOnly=", source);
    }

    [Theory]
    [InlineData("CommunityWorkspaceScreen.razor", "WorkspaceOnly=\"@WorkspaceOnly\"")]
    [InlineData("CommunityBoardManagementScreen.razor", "CommunityWorkspaceSurfaceKind.BoardManagement")]
    [InlineData("CommunityLedgerDraftScreen.razor", "CommunityWorkspaceSurfaceKind.LedgerDraft")]
    [InlineData("CommunityPostComposeScreen.razor", "ComposeOnly=\"true\"")]
    [InlineData("CommunityRecommendedPostListScreen.razor", "ListOnly=\"true\"")]
    [InlineData("CommunityRecommendedPostDetailScreen.razor", "PostDetailOnly=\"true\"")]
    [InlineData("CommunityPostDetailScreen.razor", "PostDetailOnly=\"true\"")]
    public void 공용screen은_route없이_하나의home표면을고정한다(
        string screenFileName,
        string responsibilityMarker)
    {
        var screenPath = Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            screenFileName);
        var source = File.ReadAllText(screenPath);

        Assert.DoesNotContain("@page ", source);
        Assert.Contains("<PlatformCommunityHome", source);
        Assert.Contains(responsibilityMarker, source);
        Assert.DoesNotContain("Ssalddel.WebApp", source);
        Assert.DoesNotContain("SsalddelApp", source);
    }

    [Fact]
    public void 과거추천글query는_새상세route로만_호환이동한다()
    {
        var pagePath = Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.WebApp",
            "Pages",
            "CommunityRecommendedPostsPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.Contains("CommunityPageRoutes.RecommendedPostDetailFor", source);
        Assert.Contains("replace: true", source);
        Assert.DoesNotContain("<CommunityRecommendedPostDetailScreen", source);
    }

    [Theory]
    [InlineData("CommunityHomePage.razor", "/community", "<CommunityMobileBoardDirectoryScreen")]
    [InlineData("CommunityBoardPage.razor", "/community/boards", "<CommunityBoardListScreen")]
    [InlineData("CommunityWorkspacePage.razor", "/community/workspace", "<CommunityWorkspaceScreen")]
    [InlineData("CommunityBoardManagementPage.razor", "/community/boards/manage", "<CommunityBoardManagementScreen")]
    [InlineData("CommunityLedgerDraftPage.razor", "/community/ledgers/new", "<CommunityLedgerDraftScreen")]
    [InlineData("CommunityPostComposePage.razor", "/community/write", "<CommunityPostComposeScreen")]
    [InlineData("CommunityRecommendedPostsPage.razor", "/community/posts/recommended", "<CommunityRecommendedPostListScreen")]
    [InlineData("CommunityRecommendedPostDetailPage.razor", "/community/posts/recommended/detail", "<CommunityRecommendedPostDetailScreen")]
    [InlineData("CommunityPostDetailPage.razor", "/community/posts/{PostId:long}", "<CommunityPostDetailScreen")]
    public void 모바일앱은_Web과같은커뮤니티route와공용Screen을조립한다(
        string pageFileName,
        string route,
        string screenMarkup)
    {
        var pagePath = Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            "Components",
            "Pages",
            pageFileName);
        var source = File.ReadAllText(pagePath);
        var routeDirectives = File.ReadLines(pagePath)
            .Where(line => line.TrimStart().StartsWith("@page ", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(routeDirectives);
        Assert.Equal($"@page \"{route}\"", routeDirectives[0].Trim());
        Assert.Contains(screenMarkup, source);
    }

    [Fact]
    public void 게시판route는_공용Screen에query문맥만전달한다()
    {
        var webPage = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.WebApp",
            "Pages",
            "CommunityBoardPage.razor"));
        var commonScreen = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            "CommunityBoardListScreen.razor"));

        Assert.True(File.ReadLines(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.WebApp",
            "Pages",
            "CommunityBoardPage.razor")).Count() <= 55);
        Assert.Contains("CommunityBoardNavigationQueryNames.Search", webPage);
        Assert.Contains("CommunityBoardNavigationQueryNames.ListFilter", webPage);
        Assert.Contains("PageNavigationQueryNames.FocusTarget", webPage);
        Assert.Contains("CommunityPageRoutes.PostDetailFor", commonScreen);
        Assert.Contains("CurrentBoardPath", commonScreen);
        Assert.Contains("scrollToFocusTarget", commonScreen);
        Assert.Contains("ShowsImportedFoodCountryFilters", commonScreen);
        Assert.Contains("CommunityImportedFoodCountryFilterCatalog.All", commonScreen);
        Assert.Contains("data-country-code=\"@country.CountryCode\"", commonScreen);
        Assert.DoesNotContain("Ssalddel.WebApp", commonScreen);
        Assert.DoesNotContain("SsalddelApp", commonScreen);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
