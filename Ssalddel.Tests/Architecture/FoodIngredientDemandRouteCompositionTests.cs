namespace Ssalddel.Tests.Architecture;

public sealed class FoodIngredientDemandRouteCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/CommunityGroupPurchaseDemandPage.razor")]
    [InlineData("SsalddelApp", "Components/Pages/CommunityGroupPurchaseDemandPage.razor")]
    public void 음식재료_비구속수요route는_공용screen하나만_조립한다(
        string project,
        string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

        Assert.Contains("@page \"/community/group-purchase/demand\"", source);
        if (project == "SsalddelApp")
        {
            Assert.Contains("@page \"/community/orders/new\"", source);
            Assert.Contains("@layout CommunityMobileLayout", source);
        }
        Assert.Contains("<OfficialFoodIngredientDemandScreen", source);
        Assert.Contains("CommunityGroupPurchaseIngredientSeed.Create", source);
        Assert.Contains("DecodeMaterialBundle", source);
        Assert.Contains("IngredientSeeds", source);
        Assert.DoesNotContain("I비구속공동구매수요Service", source);
        Assert.DoesNotContain("비구속수요저장Async", source);
        Assert.DoesNotContain("수요배치미리보기Async", source);
    }

    [Fact]
    public void 음식재료탐색은_제안초안이아닌_비구속수요route로_문맥을전달한다()
    {
        var root = FindRepositoryRoot();
        var journey = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "OfficialFoodIngredientJourney.razor"));
        var seed = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "ViewModels",
            "CommunityGroupPurchaseIngredientSeed.cs"));
        var routes = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel.Contracts",
            "Common",
            "Community",
            "CommunityPageRoutes.cs"));

        Assert.Contains("ToDemandNavigationUri(_selectedPurchaseSeeds)", journey);
        Assert.Contains("ToDemandNavigationUri", seed);
        Assert.Contains("MaterialBundleQueryName", seed);
        Assert.Contains("CommunityPageRoutes.GroupPurchaseDemand", seed);
        Assert.Contains("GroupPurchaseDemand = \"/community/group-purchase/demand\"", routes);
        Assert.Contains("IndividualOrderStart = \"/community/orders/new\"", routes);
        Assert.Contains("IndividualOrders = \"/community/orders\"", routes);
    }

    [Fact]
    public void 공용수요screen은_미리보기_저장_철회와_실행금지경계를_표시한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            "OfficialFoodIngredientDemandScreen.razor"));

        Assert.Contains("공동 할인 후보 미리보기", source);
        Assert.Contains("개별주문 의향 저장", source);
        Assert.Contains("내 개별주문 철회", source);
        Assert.Contains("공동주문후보참여동의", source);
        Assert.Contains("IngredientLines", source);
        Assert.Contains("결제·계약·수입 신고·공급자 선정·운송 의뢰·창고 입고", source);
        Assert.Contains("상세 주소는 받지 않으며", source);
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
