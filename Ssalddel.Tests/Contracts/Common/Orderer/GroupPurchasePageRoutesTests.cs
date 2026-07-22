using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class GroupPurchasePageRoutesTests
{
    [Fact]
    public void 목록_상세_Action_후속조회_route가_서로_분리된다()
    {
        Assert.Equal("/group-purchase", GroupPurchasePageRoutes.Root);
        Assert.Equal("/group-purchase/products", GroupPurchasePageRoutes.ProductsRoot);
        Assert.Equal(
            "/group-purchase/products/hs-food-0203-pork-frozen",
            GroupPurchasePageRoutes.ProductDetailFor("hs-food-0203-pork-frozen"));
        Assert.Equal(
            "/group-purchase/demands/new/hs-food-0203-pork-frozen",
            GroupPurchasePageRoutes.DemandCreateFor("hs-food-0203-pork-frozen"));
        Assert.Equal(
            "/group-purchase/import-review/hs-food-0203-pork-frozen",
            GroupPurchasePageRoutes.ImportReviewFor("hs-food-0203-pork-frozen"));
        Assert.Equal("/group-purchase/shipments", GroupPurchasePageRoutes.Shipments);
    }

    [Fact]
    public void 상품_ID는_path_segment로_인코딩되고_빈값은_거절한다()
    {
        Assert.Equal(
            "/group-purchase/products/food%2F0203",
            GroupPurchasePageRoutes.ProductDetailFor("food/0203"));
        Assert.Throws<ArgumentException>(() => GroupPurchasePageRoutes.DemandCreateFor(" "));
    }
}
