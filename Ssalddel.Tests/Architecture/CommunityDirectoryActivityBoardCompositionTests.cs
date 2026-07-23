namespace Ssalddel.Tests.Architecture;

public sealed class CommunityDirectoryActivityBoardCompositionTests
{
    [Fact]
    public void 게시판모음은_간괘산과_CommandEvent페이지연결을카드에서보여준다()
    {
        var root = FindRepositoryRoot();
        var page = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "CommunityDirectoryPage.razor"));

        Assert.Contains("☶", page);
        Assert.Contains("게시판 산맥", page);
        Assert.Contains("CommunityActivityBoardCatalog.FindBundle", page);
        Assert.Contains("Command·Event·페이지 연결 보기", page);
        Assert.Contains("relatedPage.CanNavigateFromCommunityWeb", page);
    }

    [Fact]
    public void 공용게시판모음도_활동게시판을간괘카드로표현한다()
    {
        var root = FindRepositoryRoot();
        var component = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            "PlatformCommunityBoardIndex.razor"));

        Assert.Contains("GEN ☶", component);
        Assert.Contains("CommunityActivityBoardCatalog.FindBundle", component);
        Assert.Contains("Command @activityBundle.CommandCount", component);
        Assert.Contains("페이지 @activityBundle.Pages.Count", component);
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
