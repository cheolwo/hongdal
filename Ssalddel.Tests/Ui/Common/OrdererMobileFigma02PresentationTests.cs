namespace Ssalddel.Tests.Ui.Common;

public sealed class OrdererMobileFigma02PresentationTests
{
    [Theory]
    [InlineData("Home.razor", "02.01")]
    [InlineData("ProducePriceComparison.razor", "02.02A")]
    [InlineData("GroupPurchaseProducts.razor", "02.02")]
    [InlineData("GroupPurchaseWishCreate.razor", "02.03")]
    [InlineData("GroupPurchaseWishDetail.razor", "02.04")]
    [InlineData("GroupPurchaseWishEdit.razor", "02.05")]
    [InlineData("GroupPurchaseGroups.razor", "02.06")]
    [InlineData("GroupPurchaseGroupDetail.razor", "02.07")]
    [InlineData("GroupPurchaseImportReview.razor", "02.08")]
    [InlineData("GroupImportReadinessOverview.razor", "02.09")]
    [InlineData("GroupImportReadinessCosts.razor", "02.10")]
    [InlineData("OrderDetail.razor", "02.11")]
    [InlineData("IndividualImportLedger.razor", "02.12")]
    [InlineData("IndividualExportLedger.razor", "02.13")]
    [InlineData("GroupExportLedger.razor", "02.14")]
    public void Figma02화면은_기존MauiRoute에책임코드를고정한다(
        string fileName,
        string screenCode)
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "OrdererApp",
            "Components",
            "Pages",
            fileName));

        Assert.Contains(screenCode, source);
    }

    [Fact]
    public void 주문자MauiShell은_FigmaAppBar와네개하단Navigation을제공한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "OrdererApp",
            "Components",
            "Layout",
            "MainLayout.razor"));

        Assert.Contains("orderer-mobile-shell__appbar", source);
        Assert.Contains("orderer-mobile-shell__bottom-nav", source);
        Assert.Contains("개별주문 → 공동주문", source);
        Assert.Contains(">홈</span>", source);
        Assert.Contains(">재료</span>", source);
        Assert.Contains(">내 주문</span>", source);
        Assert.Contains(">원장</span>", source);
    }

    [Fact]
    public void 주문자홈은_Figma바로가기와비구속경계를기존Route로연결한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "OrdererApp",
            "Components",
            "Orderer",
            "OrdererMobileHomeScreen.razor"));

        Assert.Contains("OrdererRoutes.Food", source);
        Assert.Contains("OrdererRoutes.Mart", source);
        Assert.Contains("OrdererRoutes.ProducePriceComparison", source);
        Assert.Contains("OrdererRoutes.GroupPurchaseProducts", source);
        Assert.Contains("OrdererRoutes.Orders", source);
        Assert.Contains("개별주문과 공동 실행은 분리됩니다.", source);
    }

    [Fact]
    public void 가격비교는_공개Route를보존하고_주문자흐름에서재료와원함으로이어진다()
    {
        var root = FindRepositoryRoot();
        var ordererPage = File.ReadAllText(Path.Combine(
            root,
            "OrdererApp",
            "Components",
            "Pages",
            "ProducePriceComparison.razor"));
        var publicPage = File.ReadAllText(Path.Combine(
            root,
            "SsalddelApp",
            "Components",
            "Pages",
            "ProduceRegionalPriceComparison.razor"));

        Assert.Contains("OrdererRoutes.GroupPurchaseProducts", ordererPage);
        Assert.Contains("OrdererRoutes.GroupPurchaseWishCreate", ordererPage);
        Assert.Contains("OrdererRoutes.GroupPurchaseGroups", ordererPage);
        Assert.Contains("비구속 수요", ordererPage);
        Assert.Contains("@page \"/information/produce-price-comparison\"", publicPage);
    }

    [Theory]
    [InlineData("IndividualImportLedgerScreen.razor")]
    [InlineData("IndividualExportLedgerScreen.razor")]
    [InlineData("GroupExportLedgerScreen.razor")]
    public void 공용무역원장은_기존소비자를위한Heading기본값을유지한다(string fileName)
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Orderer",
            fileName));

        Assert.Contains("ShowHeading { get; set; } = true", source);
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
