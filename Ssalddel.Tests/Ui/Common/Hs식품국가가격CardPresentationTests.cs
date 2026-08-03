namespace Ssalddel.Tests.Ui.Common;

public sealed class Hs식품국가가격CardPresentationTests
{
    [Fact]
    public void HS후보Panel은_사용자요청형국가가격Card와_Loading표시를제공한다()
    {
        var componentDirectory = Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information");
        var panel = File.ReadAllText(Path.Combine(componentDirectory, "OfficialFoodIngredientHsPanel.razor"));
        var card = File.ReadAllText(Path.Combine(componentDirectory, "Hs식품국가가격Card.razor"));

        Assert.Contains("<데이터조회Button", panel, StringComparison.Ordinal);
        Assert.Contains("RepresentativeHsCodes", panel, StringComparison.Ordinal);
        Assert.Contains("<Hs식품국가가격Card", panel, StringComparison.Ordinal);
        Assert.Contains("Hs식품국가가격Card조회Async", card, StringComparison.Ordinal);
        Assert.Contains("국가 가격 불러오는 중", card, StringComparison.Ordinal);
        Assert.Contains("ComparisonBoundaries", card, StringComparison.Ordinal);
        Assert.DoesNotContain("@onclick=\"LoadAsync\"", card, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("저장소 루트를 찾지 못했습니다.");
    }
}
