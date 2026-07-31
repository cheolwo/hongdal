namespace Ssalddel.Tests.Architecture;

public sealed class CommunityDirectoryActivityBoardCompositionTests
{
    [Fact]
    public void 게시판모음은_주요네게시판만안내한다()
    {
        var root = FindRepositoryRoot();
        var page = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "CommunityDirectoryPage.razor"));

        var viewModel = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.WebApp",
            "ViewModels",
            "CommunityDirectoryPageViewModel.cs"));

        Assert.Contains("게시판 모음", page);
        Assert.Contains("서원, 자유·생활, 지역 문화, 농수산물 가격", page);
        Assert.Contains("CommunityBoardCatalog.FeaturedBoards", viewModel);
    }

    [Fact]
    public void 공용게시판모음도_주요네공간에집중한다()
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

        Assert.Contains("COMMUNITY BOARDS", component);
        Assert.Contains("서원, 자유·생활, 지역 문화, 농수산물 가격", component);
        Assert.DoesNotContain("CommunityActivityBoardCatalog.FindBundle", component);
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
