namespace Ssalddel.Tests.Architecture;

public sealed class ShipperBulkImportPageResponsibilityTests
{
    [Fact]
    public void 대량_의뢰_page는_명시적_CSV_미리보기만_제공하고_등록을_가장하지_않는다()
    {
        var root = FindRepositoryRoot();
        var pagePath = Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "ShipperBulkImportPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.Contains("CSV 일괄등록 미리보기", source);
        Assert.Contains("원장에는 저장하지 않습니다", source);
        Assert.Contains("형식 통과", source);
        Assert.DoesNotContain("OnInitialized", source);
        Assert.DoesNotContain("등록가능", source);
        Assert.DoesNotContain("AddRequestAsync", source);
        Assert.DoesNotContain("HttpClient", source);
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
