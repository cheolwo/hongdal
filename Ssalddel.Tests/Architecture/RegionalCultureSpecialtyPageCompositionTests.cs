namespace Ssalddel.Tests.Architecture;

public sealed class RegionalCultureSpecialtyPageCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/RegionalCultureSpecialtyPage.razor")]
    [InlineData("SsalddelApp", "Components/Pages/RegionalCultureSpecialtyPage.razor")]
    public void 지역문화특산물route는_공용탐색화면을조립한다(string project, string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

        Assert.Contains("@page \"/community/regions\"", source);
        Assert.Contains("<RegionalCultureSpecialtyBrowse", source);
    }

    [Fact]
    public void 공용탐색화면은_문화_특산물_근거경계와_후속탐색을함께보여준다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "RegionalCultureSpecialtyBrowse.razor"));

        Assert.Contains("문화를 알아가는 질문", source);
        Assert.Contains("특산물 탐색", source);
        Assert.Contains("EvidenceBoundary", source);
        Assert.Contains("공식 음식·재료 탐색으로 이동", source);
        Assert.Contains("주문·참여·수입이 만들어지지 않습니다", source);
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
