namespace Ssalddel.Tests.Architecture;

public sealed class CommunityDiagramRouteCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/DiagramWorkbenchPage.razor")]
    [InlineData("SsalddelApp", "Components/Pages/DiagramWorkbench.razor")]
    public void Web과모바일은_같은DiagramRoute와공용Screen을조립한다(
        string project,
        string relativePath)
    {
        var path = ProjectFile(project, relativePath);
        var source = File.ReadAllText(path);
        var routes = File.ReadLines(path)
            .Where(line => line.TrimStart().StartsWith("@page ", StringComparison.Ordinal))
            .ToArray();

        Assert.Single(routes);
        Assert.Equal("@page \"/diagram\"", routes[0].Trim());
        Assert.Contains("<CommunityDiagramWorkbenchScreen", source);
        Assert.Contains("CommunityDiagramNavigationQueryNames.LedgerTemplate", source);
        Assert.Contains("CommunityDiagramNavigationQueryNames.SelectedNode", source);
        Assert.Contains("CommunityDiagramNavigationQueryNames.Zoom", source);
        Assert.Contains("CommunityDiagramNavigationQueryNames.Filter", source);
        Assert.Contains("CommunityDiagramNavigationQueryNames.ReturnPath", source);
        Assert.DoesNotContain("PlatformCommunityHome", source);
        Assert.DoesNotContain("PlatformDiagramPaletteStateService", source);
        Assert.DoesNotContain("WorkflowPresets", source);
        Assert.True(File.ReadLines(path).Count() <= 40);
    }

    [Fact]
    public void 공용DiagramScreen은_desktopSidebar와_mobileBottomSheet를같은목표로조립한다()
    {
        var source = File.ReadAllText(ComponentFile("CommunityDiagramWorkbenchScreen.razor"));

        Assert.DoesNotContain("@page ", source);
        Assert.DoesNotContain("Ssalddel.WebApp", source);
        Assert.DoesNotContain("SsalddelApp", source);
        Assert.Contains("<PlatformCommunityHome", source);
        Assert.Contains("community-diagram-palette", source);
        Assert.Contains("community-diagram-mobile-palette-trigger", source);
        Assert.Contains("community-diagram-palette--open", source);
        Assert.Contains("role=", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("QueryDiagramSelectedNode", source);
        Assert.Contains("QueryDiagramZoomPercent", source);
        Assert.Contains("DiagramCloseHref", source);
        Assert.Contains("DiagramSelectedNodeChanged", source);
        Assert.Contains("CommunityPageRoutes.DiagramFor", source);
    }

    [Fact]
    public void Diagram문맥은_선택node와zoom을_canvas상태로복원한다()
    {
        var lifecycle = File.ReadAllText(ComponentFile("PlatformCommunityHome.Lifecycle.razor.cs"));
        var presentation = File.ReadAllText(ComponentFile("PlatformCommunityHome.DiagramPresentation.razor.cs"));
        var interactions = File.ReadAllText(ComponentFile("PlatformCommunityHome.DiagramNodeInteractions.razor.cs"));

        Assert.Contains("QueryDiagramSelectedNode", lifecycle);
        Assert.Contains("선택원장블록노드제목 = requestedNode.Title", lifecycle);
        Assert.Contains("DiagramCanvas.ZoomPercent = normalizedZoom", lifecycle);
        Assert.Contains("BuildZoomedDiagramCanvasStyle", presentation);
        Assert.Contains("DiagramSelectedNodeChanged.InvokeAsync", interactions);
        Assert.Contains("Navigation.NavigateTo(action.Url)", interactions);
    }

    [Fact]
    public void 과거WorkspaceDiagramQuery는_공용DiagramRoute로호환이동한다()
    {
        var source = File.ReadAllText(ProjectFile(
            "Ssalddel.WebApp",
            "Pages/CommunityWorkspacePage.razor"));

        Assert.Contains("IsLegacyDiagramRequest", source);
        Assert.Contains("CommunityPageRoutes.DiagramFor", source);
        Assert.Contains("returnPath: CommunityPageRoutes.Workspace", source);
        Assert.Contains("replace: true", source);
    }

    [Fact]
    public void DiagramJS는_공용정적asset으로Web과앱에서재사용한다()
    {
        var canvas = File.ReadAllText(ComponentFile("PlatformCommunityDiagramDesktopCanvas.razor"));
        var chat = File.ReadAllText(ComponentFile("PlatformCommunityDiagramChatPanel.razor"));
        var scriptPath = Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "wwwroot",
            "js",
            "platformDiagram.js");

        Assert.Contains("/_content/Ssalddel.Ui.Common/js/platformDiagram.js", canvas);
        Assert.Contains("/_content/Ssalddel.Ui.Common/js/platformDiagram.js", chat);
        Assert.True(File.Exists(scriptPath));
        Assert.Contains("logicalHeight", File.ReadAllText(scriptPath));
    }

    private static string ComponentFile(string fileName)
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            fileName);

    private static string ProjectFile(string project, string relativePath)
        => Path.Combine(
            FindRepositoryRoot(),
            project,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

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
