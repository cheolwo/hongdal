namespace Ssalddel.Tests.Architecture;

public sealed class WarehousePlatformHomeSrpCompositionTests
{
    [Fact]
    public void 창고_루트는_실제운영홈으로위임하고_업무화면을조립하지않는다()
    {
        var homePath = WarehousePage("Home.razor");
        var source = File.ReadAllText(homePath);

        Assert.True(File.ReadLines(homePath).Count() <= 20);
        Assert.Contains("@page \"/\"", source);
        Assert.Contains("@inject NavigationManager Navigation", source);
        Assert.Contains("Navigation.NavigateTo(WarehouseManagerRoutes.Warehouse, replace: true)", source);
        Assert.DoesNotContain("<CommunityWorkspaceScreen", source);
        Assert.DoesNotContain("<PlatformCommunityHome", source);
        Assert.DoesNotContain("<CommunityBoardListScreen", source);
        Assert.DoesNotContain("<CommunityBoardManagementScreen", source);
        Assert.DoesNotContain("<PlatformCommunityBoardManagementPanel", source);
    }

    [Theory]
    [InlineData("CommunityHomePage.razor", "/community", "<PlatformCommunityHome", "CommunityFeedOnly=\"true\"")]
    [InlineData("CommunityBoardPage.razor", "/community/boards", "<CommunityBoardListScreen", "AppKey=\"warehouse-manager\"")]
    [InlineData("CommunityBoardManagementPage.razor", "/community/boards/manage", "<CommunityBoardManagementScreen", "AppKey=\"warehouse-manager\"")]
    [InlineData("CommunityWorkspacePage.razor", "/community/workspace", "<CommunityWorkspaceScreen", "WorkspaceOnly=\"@RouteContext.IsWorkspaceLandingRoute\"")]
    [InlineData("CommunityLedgerDraftPage.razor", "/community/ledgers/new", "<CommunityLedgerDraftScreen", "AppKey=\"warehouse-manager\"")]
    [InlineData("CommunityPostComposePage.razor", "/community/write", "<CommunityPostComposeScreen", "AppKey=\"warehouse-manager\"")]
    [InlineData("CommunityPostDetailPage.razor", "/community/posts/{PostId:long}", "<CommunityPostDetailScreen", "AppKey=\"warehouse-manager\"")]
    [InlineData("CommunityRecommendedPostsPage.razor", "/community/posts/recommended", "<CommunityRecommendedPostListScreen", "AppKey=\"warehouse-manager\"")]
    [InlineData("CommunityRecommendedPostDetailPage.razor", "/community/posts/recommended/detail", "<CommunityRecommendedPostDetailScreen", "AppKey=\"warehouse-manager\"")]
    [InlineData("DiagramWorkbench.razor", "/diagram", "<CommunityDiagramWorkbenchScreen", "AppKey=\"warehouse-manager\"")]
    public void 창고_커뮤니티route는_사용자목표별_공용screen을_조립한다(
        string fileName,
        string route,
        string screenMarker,
        string responsibilityMarker)
    {
        var pagePath = WarehousePage(fileName);
        var source = File.ReadAllText(pagePath);
        var routeDirectives = File.ReadLines(pagePath)
            .Where(line => line.TrimStart().StartsWith("@page ", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(routeDirectives);
        Assert.Equal($"@page \"{route}\"", routeDirectives[0].Trim());
        Assert.Contains(screenMarker, source);
        Assert.Contains(responsibilityMarker, source);
    }

    [Fact]
    public void 게시판_개설신청은_글목록과_다른route에서만_화면을_조립한다()
    {
        var home = File.ReadAllText(WarehousePage("Home.razor"));
        var boardList = File.ReadAllText(WarehousePage("CommunityBoardPage.razor"));
        var management = File.ReadAllText(WarehousePage("CommunityBoardManagementPage.razor"));
        var navigationCatalog = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "WarehouseManagerApp",
            "Services",
            "WarehousePlatformHomeNavigationCatalog.cs"));

        Assert.DoesNotContain("CommunityBoardManagementScreen", home);
        Assert.DoesNotContain("CommunityBoardManagementScreen", boardList);
        Assert.Contains("CommunityBoardManagementScreen", management);
        Assert.Contains("CommunityPageRoutes.BoardManagement", navigationCatalog);
        Assert.Contains("게시판 개설 신청", navigationCatalog);
    }

    [Fact]
    public void 공용workspace화면은_선택적으로_사방탐색을_보여준다()
    {
        var componentDirectory = Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community");
        var screen = File.ReadAllText(Path.Combine(componentDirectory, "CommunityWorkspaceScreen.razor"));
        var cardinalNavigation = File.ReadAllText(Path.Combine(
            componentDirectory,
            "PlatformWorkspaceCardinalNavigation.razor"));

        Assert.Contains("@if (ShowWorkspaceNavigation)", screen);
        Assert.Contains("<PlatformWorkspaceCardinalNavigation", screen);
        Assert.Contains("NavigationItems=\"@CardinalNavigationOptions\"", screen);
        Assert.Contains("<SsalddelLaterHeavenBaguaNavigator", cardinalNavigation);
        Assert.Contains("PageNavigationContext.NormalizeReturnPath", cardinalNavigation);
    }

    [Fact]
    public void 공용게시판_바로가기는_canonical_route를_사용한다()
    {
        var hero = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            "PlatformCommunityHomeHero.razor"));

        Assert.Contains("Href=\"@CommunityPageRoutes.BoardDirectory\"", hero);
        Assert.DoesNotContain("/community/categories", hero);
    }

    private static string WarehousePage(string fileName)
        => Path.Combine(
            FindRepositoryRoot(),
            "WarehouseManagerApp",
            "Components",
            "Pages",
            fileName);

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
