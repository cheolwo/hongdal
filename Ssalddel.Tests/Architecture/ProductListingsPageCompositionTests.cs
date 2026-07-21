namespace Ssalddel.Tests.Architecture;

public sealed class ProductListingsPageCompositionTests
{
    [Fact]
    public void 판매출품_라우트는_상태와업무영역만_조립한다()
    {
        var pagePath = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "Components", "Pages", "ProductListings.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 65);
        Assert.Contains("<ProductListingsHeader", source);
        Assert.Contains("<ProductListingsLoadState", source);
        Assert.Contains("<ProductListingFeedback", source);
        Assert.Contains("<ProductListingSummary", source);
        Assert.Contains("<ProductListingProductPanel", source);
        Assert.Contains("<ProductListingLedgerPanel", source);
        Assert.Contains("<ProductListingDraftPanel", source);
        Assert.Contains("<ProductListingResultPanel", source);
        Assert.DoesNotContain("IShipperSalesService", source);
        Assert.DoesNotContain("@inject", source);
        Assert.DoesNotContain("<MudTable", source);
        Assert.DoesNotContain("@code", source);
    }

    [Fact]
    public void 판매출품_조회선택생성은_독립_ViewModel로_분리한다()
    {
        var path = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "ViewModels", "Shipper", "ProductListingsPageViewModels.cs");
        var source = File.ReadAllText(path);

        Assert.Contains("class ProductListingReadViewModel", source);
        Assert.Contains("class ProductListingDraftViewModel", source);
        Assert.Contains("class ProductListingCreateViewModel", source);
        Assert.Contains("class ProductListingsPageViewModel", source);
        Assert.Contains("Task.WhenAll", source);
        Assert.Contains("계정상세조회Async(accountId.Value", source);
        Assert.Contains("재조회결과적용", source);
        Assert.DoesNotContain("선택상품Id ??=", source);
        Assert.DoesNotContain("선택계정Id ??=", source);
    }

    [Theory]
    [InlineData("ProductListingsHeader.razor")]
    [InlineData("ProductListingsHeader.razor.css")]
    [InlineData("ProductListingsLoadState.razor")]
    [InlineData("ProductListingsLoadState.razor.css")]
    [InlineData("ProductListingFeedback.razor")]
    [InlineData("ProductListingSummary.razor")]
    [InlineData("ProductListingSummary.razor.css")]
    [InlineData("ProductListingProductPanel.razor")]
    [InlineData("ProductListingProductPanel.razor.css")]
    [InlineData("ProductListingLedgerPanel.razor")]
    [InlineData("ProductListingLedgerPanel.razor.css")]
    [InlineData("ProductListingDraftPanel.razor")]
    [InlineData("ProductListingDraftPanel.razor.css")]
    [InlineData("ProductListingResultPanel.razor")]
    [InlineData("ProductListingResultPanel.razor.css")]
    [InlineData("ProductListingPresentation.cs")]
    public void 판매출품_화면책임은_전용파일로_존재한다(string fileName)
    {
        var path = Path.Combine(FindComponentDirectory(), fileName);

        Assert.True(File.Exists(path), $"판매 출품 전용 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(path));
    }

    [Fact]
    public void 판매출품_화면은_Simulation과_외부효과없음_경계를고정한다()
    {
        var directory = FindComponentDirectory();
        var header = File.ReadAllText(Path.Combine(directory, "ProductListingsHeader.razor"));
        var draft = File.ReadAllText(Path.Combine(directory, "ProductListingDraftPanel.razor"));
        var result = File.ReadAllText(Path.Combine(directory, "ProductListingResultPanel.razor"));

        Assert.Contains("로컬 메모리 Simulation 출품 원장", header);
        Assert.Contains("외부 상품 생성·수정·발행", header);
        Assert.Contains("OAuth·API 자격증명", header);
        Assert.Contains("외부 상품 API를 호출하지 않고", draft);
        Assert.Contains("Simulation 출품 원장 생성", draft);
        Assert.Contains("외부 API 호출", result);
        Assert.Contains("0건", result);
    }

    [Fact]
    public void 판매출품_화면은_좁은폭에서_단일열로_전환한다()
    {
        var pages = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "Components", "Pages");
        var rootCss = File.ReadAllText(Path.Combine(pages, "ProductListings.razor.css"));
        var productCss = File.ReadAllText(Path.Combine(FindComponentDirectory(), "ProductListingProductPanel.razor.css"));
        var draftCss = File.ReadAllText(Path.Combine(FindComponentDirectory(), "ProductListingDraftPanel.razor.css"));

        Assert.Contains("grid-template-columns: minmax(0, 1fr)", rootCss);
        Assert.Contains(".product-listings-shell > *", rootCss);
        Assert.Contains("@media (max-width: 720px)", rootCss);
        Assert.Contains("@media (max-width: 720px)", productCss);
        Assert.Contains("grid-template-columns: minmax(0, 1fr)", productCss);
        Assert.Contains("@media (max-width: 720px)", draftCss);
        Assert.Contains("grid-template-columns: minmax(0, 1fr)", draftCss);
    }

    [Fact]
    public void 판매출품_PageViewModel과_정확한계정읽기는_DI에_등록한다()
    {
        var modulePath = Path.Combine(FindRepositoryRoot(), "SsalddelApp", "Services", "ShipperSalesModule.cs");
        var source = File.ReadAllText(modulePath);

        Assert.Contains("AddScoped<I판매채널계정읽기Service>", source);
        Assert.Contains("AddTransient<ProductListingReadViewModel>", source);
        Assert.Contains("AddTransient<ProductListingDraftViewModel>", source);
        Assert.Contains("AddTransient<ProductListingCreateViewModel>", source);
        Assert.Contains("AddTransient<ProductListingsPageViewModel>", source);
    }

    private static string FindComponentDirectory()
        => Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            "Components",
            "Pages",
            "ProductListingComponents");

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
