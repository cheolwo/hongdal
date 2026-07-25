namespace Ssalddel.Tests.Architecture;

public sealed class CommunityPageViewModelCompositionTests
{
    [Theory]
    [InlineData(
        "Community/Community전체FeedScreen.razor",
        "Community전체FeedViewModel")]
    [InlineData(
        "Community/CommunityMobileBoardDirectoryScreen.razor",
        "CommunityMobileBoardDirectoryViewModel")]
    [InlineData(
        "Community/CommunityMobileWorkBoardScreen.razor",
        "CommunityMobileBoardDirectoryViewModel")]
    [InlineData(
        "Community/CommunityBoardListScreen.razor",
        "CommunityBoardPageViewModel")]
    [InlineData(
        "Information/RegionalCultureSpecialtyBrowse.razor",
        "지역문화특산물목록PageViewModel")]
    [InlineData(
        "Information/RegionalCultureSpecialtyDetail.razor",
        "지역문화특산물상세PageViewModel")]
    public void Community화면은_DI_PageViewModel패턴을사용한다(
        string relativePath,
        string viewModelName)
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

        Assert.Contains($"@inherits MvvmComponentBase<{viewModelName}>", source);
        Assert.DoesNotContain($"new {viewModelName}", source);
    }

    [Fact]
    public void CommunityPageViewModel은_공통DI_module에등록된다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Services",
            "CommunityPlatformUiModule.cs"));

        Assert.Contains("TryAddTransient<Community전체FeedViewModel>()", source);
        Assert.Contains("TryAddTransient<CommunityMobileBoardDirectoryViewModel>()", source);
        Assert.Contains("TryAddTransient<CommunityBoardPageViewModel>()", source);
        Assert.Contains("TryAddTransient<지역문화특산물목록PageViewModel>()", source);
        Assert.Contains("TryAddTransient<지역문화특산물상세PageViewModel>()", source);
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
