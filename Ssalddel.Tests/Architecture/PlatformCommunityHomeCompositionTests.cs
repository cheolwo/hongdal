namespace Ssalddel.Tests.Architecture;

public sealed class PlatformCommunityHomeCompositionTests
{
    [Fact]
    public void 커뮤니티_홈_루트는_경로와_화면_mode만_조립한다()
    {
        var componentDirectory = FindComponentDirectory();
        var pagePath = Path.Combine(componentDirectory, "PlatformCommunityHome.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 340);
        Assert.Contains("<PlatformCommunityHomeHero", source);
        Assert.Contains("<PlatformCommunityHomeDiagramStage", source);
        Assert.Contains("<PlatformCommunityHomeFeed", source);
        Assert.Contains("<PlatformCommunityHomeWorkspace", source);
        Assert.DoesNotContain("<PlatformCommunityDiagramToolbar", source);
        Assert.DoesNotContain("<PlatformCommunityPostComposer", source);
        Assert.DoesNotContain("<PlatformCommunityBoardManagementPanel", source);
        Assert.DoesNotContain("platform-ledger-diagram-workbench", source);
    }

    [Theory]
    [InlineData("PlatformCommunityHomeHero.razor")]
    [InlineData("PlatformCommunityHomeFeed.razor")]
    [InlineData("PlatformCommunityHomeDiagramStage.razor")]
    [InlineData("PlatformCommunityHome.DiagramStageSurface.razor.cs")]
    [InlineData("PlatformCommunityHomeLedgerDraft.razor")]
    [InlineData("PlatformCommunityHome.LedgerDraftSurface.razor.cs")]
    [InlineData("PlatformCommunityHomeWorkspace.razor")]
    [InlineData("PlatformCommunityHome.WorkspaceSurface.razor.cs")]
    public void 커뮤니티_홈_화면_책임은_전용_파일로_존재한다(string fileName)
    {
        var componentPath = Path.Combine(FindComponentDirectory(), fileName);

        Assert.True(File.Exists(componentPath), $"커뮤니티 홈 전용 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
    }

    private static string FindComponentDirectory()
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community");

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
