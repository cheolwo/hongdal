using Ssalddel.Contracts.Common.Mart;

namespace Ssalddel.Tests.Architecture;

public sealed class MartProductRouteCompositionTests
{
    [Fact]
    public void 복합CatalogWorkspace는제거되고_책임별공용Screen이존재한다()
    {
        var directory = FindCommonComponentDirectory();

        Assert.False(File.Exists(Path.Combine(directory, "OrdererMartCatalogWorkspace.razor")));
        Assert.False(File.Exists(Path.Combine(directory, "OrdererMartCatalogWorkspace.razor.cs")));
        Assert.False(File.Exists(Path.Combine(directory, "OrdererMartCatalogWorkspace.razor.css")));

        foreach (var fileName in new[]
                 {
                     "MartProductAccessFrame.razor",
                     "MartProductNavigation.razor",
                     "MartProductListScreen.razor",
                     "MartProductDetailScreen.razor",
                     "MartProductReviewScreen.razor"
                 })
        {
            Assert.True(File.Exists(Path.Combine(directory, fileName)), $"마트 상품 공용 Screen이 없습니다: {fileName}");
        }
    }

    [Fact]
    public void 목록상세후기Screen은_서로의상태전이책임을소유하지않는다()
    {
        var directory = FindCommonComponentDirectory();
        var list = File.ReadAllText(Path.Combine(directory, "MartProductListScreen.razor"));
        var detail = File.ReadAllText(Path.Combine(directory, "MartProductDetailScreen.razor"));
        var review = File.ReadAllText(Path.Combine(directory, "MartProductReviewScreen.razor"));

        Assert.Contains("마트공개상품목록ViewModel", list);
        Assert.DoesNotContain("마트공개상품상세ViewModel", list);
        Assert.DoesNotContain("마트공개상품후기작성ViewModel", list);

        Assert.Contains("마트공개상품상세ViewModel", detail);
        Assert.DoesNotContain("마트공개상품후기작성ViewModel", detail);
        Assert.DoesNotContain("작성Async", detail);

        Assert.Contains("마트공개상품후기PageViewModel", review);
        Assert.Contains("작성후같은상품재조회Async", review);
        Assert.DoesNotContain("마트공개상품목록ViewModel", review);
    }

    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages", "OrdererMartCatalogPage.razor", "<MartProductListScreen")]
    [InlineData("Ssalddel.WebApp", "Pages", "OrdererMartProductDetailPage.razor", "<MartProductDetailScreen")]
    [InlineData("Ssalddel.WebApp", "Pages", "OrdererMartProductReviewPage.razor", "<MartProductReviewScreen")]
    [InlineData("OrdererApp", "Components/Pages", "MartOrder.razor", "<MartProductListScreen")]
    [InlineData("OrdererApp", "Components/Pages", "MartProductDetail.razor", "<MartProductDetailScreen")]
    [InlineData("OrdererApp", "Components/Pages", "MartProductReview.razor", "<MartProductReviewScreen")]
    public void Web과주문자앱RoutePage는_한공용Screen만주콘텐츠로사용한다(
        string project,
        string directory,
        string fileName,
        string expectedScreen)
    {
        var path = Path.Combine(FindRepositoryRoot(), project, directory, fileName);
        var source = File.ReadAllText(path);

        Assert.Contains("<MartProductAccessFrame", source);
        Assert.Contains(expectedScreen, source);
        Assert.Equal(1, CountOccurrences(source, "<MartProductListScreen")
                        + CountOccurrences(source, "<MartProductDetailScreen")
                        + CountOccurrences(source, "<MartProductReviewScreen"));
    }

    [Fact]
    public void Web과주문자앱은_같은canonicalRoute의미와stableIdAction을사용한다()
    {
        var webList = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Ssalddel.WebApp", "Pages", "OrdererMartCatalogPage.razor"));
        var appList = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "OrdererApp", "Components", "Pages", "MartOrder.razor"));
        var webOrder = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Ssalddel.WebApp", "Pages", "OrdererMartOrderRequestPage.razor"));
        var appOrder = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "OrdererApp", "Components", "Pages", "MartOrderRequest.razor"));

        Assert.Contains($"@page \"{MartProductPageRoutes.Root}\"", webList);
        Assert.Contains($"@page \"{MartProductPageRoutes.Root}\"", appList);
        Assert.Contains($"@page \"{MartProductPageRoutes.OrderTemplate}\"", webOrder);
        Assert.Contains($"@page \"{MartProductPageRoutes.OrderTemplate}\"", appOrder);
        Assert.Contains(MartProductPageRoutes.LegacyWebRoot, webList);
    }

    [Fact]
    public void MobileNavigation과각Screen은_좁은폭단일열규칙을가진다()
    {
        var directory = FindCommonComponentDirectory();
        var navigationCss = File.ReadAllText(Path.Combine(directory, "MartProductNavigation.razor.css"));
        var listCss = File.ReadAllText(Path.Combine(directory, "MartProductListScreen.razor.css"));
        var detailCss = File.ReadAllText(Path.Combine(directory, "MartProductDetailScreen.razor.css"));
        var reviewCss = File.ReadAllText(Path.Combine(directory, "MartProductReviewScreen.razor.css"));

        Assert.Contains("grid-template-columns: repeat(2, minmax(0, 1fr))", navigationCss);
        Assert.Contains("min-height: 58px", navigationCss);
        Assert.Contains("@media (max-width: 680px)", listCss);
        Assert.Contains("grid-template-columns: 1fr", listCss);
        Assert.Contains("@media (max-width: 680px)", detailCss);
        Assert.Contains("@media (max-width: 680px)", reviewCss);
    }

    [Fact]
    public void 공용Screen은_플랫폼namespace와마트Route문자열을소유하지않는다()
    {
        var directory = FindCommonComponentDirectory();
        var fileNames = new[]
        {
            "MartProductAccessFrame.razor",
            "MartProductNavigation.razor",
            "MartProductListScreen.razor",
            "MartProductDetailScreen.razor",
            "MartProductReviewScreen.razor",
            "OrdererMartOrderRequestWorkspace.razor",
            "OrdererMartOrderRequestWorkspace.razor.cs"
        };

        foreach (var fileName in fileNames)
        {
            var source = File.ReadAllText(Path.Combine(directory, fileName));
            Assert.DoesNotContain("Ssalddel.WebApp", source);
            Assert.DoesNotContain("OrdererApp", source);
            Assert.DoesNotContain("\"/food/mart", source);
            Assert.DoesNotContain("\"/orderer/mart", source);
        }
    }

    private static int CountOccurrences(string source, string value)
        => source.Split(value, StringSplitOptions.None).Length - 1;

    private static string FindCommonComponentDirectory()
        => Path.Combine(FindRepositoryRoot(), "Ssalddel.Ui.Common", "Areas", "App", "Components", "Mart");

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
