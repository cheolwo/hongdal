namespace Ssalddel.Tests.Architecture;

public sealed class CommunityLedgerDiagramDetailCompositionTests
{
    [Fact]
    public void 원장_상세는_증빙과_Command_실행을_직접_소유하지_않는다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentDirectory = Path.Combine(
            repositoryRoot,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community");
        var detailSource = File.ReadAllText(Path.Combine(
            componentDirectory,
            "CommunityLedgerDiagramDetail.razor"));
        var inspectorSource = File.ReadAllText(Path.Combine(
            componentDirectory,
            "CommunityLedgerBlockInspector.razor"));
        var actionSource = File.ReadAllText(Path.Combine(
            componentDirectory,
            "CommunityLedgerNodeActionPanel.razor"));

        Assert.DoesNotContain("<CommunityLedgerNodeActionPanel", detailSource);
        Assert.DoesNotContain("CommunityLedgerNodeActionService", detailSource);
        Assert.DoesNotContain("IBrowserFile", detailSource);
        Assert.DoesNotContain("ExecutePendingActionAsync", detailSource);

        Assert.Contains("<CommunityLedgerNodeActionPanel", inspectorSource);
        Assert.Contains("MvvmComponentBase<CommunityLedgerNodeActionViewModel>", actionSource);
        Assert.Contains("<InputFile", actionSource);
        Assert.Contains("ViewModel.ExecuteAsync()", actionSource);
    }

    [Fact]
    public void 원장_상세는_다이어그램과_inspector와_실시간_session만_조립한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentDirectory = Path.Combine(
            repositoryRoot,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community");
        var detailPath = Path.Combine(componentDirectory, "CommunityLedgerDiagramDetail.razor");
        var detailSource = File.ReadAllText(detailPath);

        Assert.True(File.ReadLines(detailPath).Count() <= 170);
        Assert.Contains("MvvmComponentBase<CommunityLedgerDiagramDetailViewModel>", detailSource);
        Assert.Contains("<CommunityLedgerDiagramCanvas", detailSource);
        Assert.Contains("<CommunityLedgerBlockInspector", detailSource);
        Assert.DoesNotContain("IDiagramCollaborationClientService", detailSource);
        Assert.DoesNotContain("BuildEdgePath", detailSource);
        Assert.DoesNotContain("OrganizationReferences", detailSource);
        Assert.DoesNotContain("<CommunityLedgerNodeActionPanel", detailSource);

        Assert.True(File.Exists(Path.Combine(componentDirectory, "CommunityLedgerDiagramCanvas.razor")));
        Assert.True(File.Exists(Path.Combine(componentDirectory, "CommunityLedgerBlockInspector.razor")));
        Assert.True(File.Exists(Path.Combine(componentDirectory, "CommunityLedgerDiagramPresentation.cs")));
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
