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
        var actionSource = File.ReadAllText(Path.Combine(
            componentDirectory,
            "CommunityLedgerNodeActionPanel.razor"));

        Assert.Contains("<CommunityLedgerNodeActionPanel", detailSource);
        Assert.DoesNotContain("CommunityLedgerNodeActionService", detailSource);
        Assert.DoesNotContain("IBrowserFile", detailSource);
        Assert.DoesNotContain("ExecutePendingActionAsync", detailSource);

        Assert.Contains("MvvmComponentBase<CommunityLedgerNodeActionViewModel>", actionSource);
        Assert.Contains("<InputFile", actionSource);
        Assert.Contains("ViewModel.ExecuteAsync()", actionSource);
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
