namespace Ssalddel.Tests.Architecture;

public sealed class OrdererGroupPurchasePageCompositionTests
{
    [Fact]
    public void 공동구매_라우트_페이지는_업무_컴포넌트만_조립한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var pagePath = Path.Combine(
            repositoryRoot,
            "OrdererApp",
            "Components",
            "Pages",
            "GroupPurchaseIntent.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 100);
        Assert.Contains("<GroupPurchaseShipmentTrackingPanel", source);
        Assert.Contains("<GroupPurchaseProductCatalogPanel", source);
        Assert.Contains("<GroupPurchaseProductAnalysisPanel", source);
        Assert.Contains("<GroupPurchaseDemandPanel", source);
        Assert.DoesNotContain("IGroupPurchaseShipmentTrackingService", source);
        Assert.DoesNotContain("RegisterDemandAsync", source);
        Assert.DoesNotContain("ResolveDeliveryScopesAsync", source);
        Assert.DoesNotContain("SimulateImportUnitPriceAsync", source);
    }

    [Theory]
    [InlineData("GroupPurchaseShipmentTrackingPanel.razor")]
    [InlineData("GroupPurchaseProductCatalogPanel.razor")]
    [InlineData("GroupPurchaseProductAnalysisPanel.razor")]
    [InlineData("GroupPurchaseDemandPanel.razor")]
    [InlineData("GroupPurchaseDemandFlowSummary.razor")]
    [InlineData("GroupPurchasePaymentSchedule.razor")]
    public void 공동구매_하위_책임은_별도_컴포넌트로_존재한다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentPath = Path.Combine(
            repositoryRoot,
            "OrdererApp",
            "Components",
            "GroupPurchase",
            fileName);

        Assert.True(File.Exists(componentPath), $"공동구매 하위 컴포넌트가 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
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
