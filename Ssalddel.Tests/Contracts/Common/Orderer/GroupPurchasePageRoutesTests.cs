using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class GroupPurchasePageRoutesTests
{
    [Fact]
    public void 목록_상세_Action_후속조회_route가_서로_분리된다()
    {
        Assert.Equal("/group-purchase", GroupPurchasePageRoutes.Root);
        Assert.Equal("/group-purchase/practice", GroupPurchasePageRoutes.Practice);
        Assert.Equal("/group-purchase/products", GroupPurchasePageRoutes.ProductsRoot);
        Assert.Equal(
            "/group-purchase/products/hs-food-0203-pork-frozen",
            GroupPurchasePageRoutes.ProductDetailFor("hs-food-0203-pork-frozen"));
        Assert.Equal(
            "/group-purchase/demands/new/hs-food-0203-pork-frozen",
            GroupPurchasePageRoutes.DemandCreateFor("hs-food-0203-pork-frozen"));
        Assert.Equal("/group-purchase/wishes", GroupPurchasePageRoutes.WishesRoot);
        Assert.Equal("/group-purchase/wishes/new", GroupPurchasePageRoutes.WishCreate);
        Assert.Equal(
            "/group-purchase/wishes/wish-ledger-17",
            GroupPurchasePageRoutes.WishDetailFor("wish-ledger-17"));
        Assert.Equal(
            "/group-purchase/wishes/wish-ledger-17/edit",
            GroupPurchasePageRoutes.WishEditFor("wish-ledger-17"));
        Assert.Equal("/group-purchase/groups", GroupPurchasePageRoutes.GroupsRoot);
        Assert.Equal(
            "/group-purchase/groups/auto-group-onion",
            GroupPurchasePageRoutes.GroupDetailFor("auto-group-onion"));
        Assert.Equal(
            "/group-purchase/import-review/hs-food-0203-pork-frozen",
            GroupPurchasePageRoutes.ImportReviewFor("hs-food-0203-pork-frozen"));
        Assert.Equal("/group-purchase/shipments", GroupPurchasePageRoutes.Shipments);
    }

    [Fact]
    public void 공동수입_준비현황_route는_원장별_단일책임화면으로_분리된다()
    {
        Assert.Equal("/group-purchase/imports", GroupPurchasePageRoutes.ImportsRoot);
        Assert.Equal(
            "/group-purchase/imports/group-import-17",
            GroupPurchasePageRoutes.ImportOverviewFor("group-import-17"));
        Assert.Equal(
            "/group-purchase/imports/group-import-17/suppliers",
            GroupPurchasePageRoutes.ImportSuppliersFor("group-import-17"));
        Assert.Equal(
            "/group-purchase/imports/group-import-17/costs",
            GroupPurchasePageRoutes.ImportCostsFor("group-import-17"));
        Assert.Equal(
            "/group-purchase/imports/group-import-17/classification",
            GroupPurchasePageRoutes.ImportClassificationFor("group-import-17"));
        Assert.Equal(
            "/group-purchase/imports/group-import-17/handoff",
            GroupPurchasePageRoutes.ImportHandoffFor("group-import-17"));
        Assert.Equal(
            "/group-purchase/imports/group-import-17/consent",
            GroupPurchasePageRoutes.ImportConsentFor("group-import-17"));
    }

    [Fact]
    public void 원장_ID는_path_segment로_인코딩되고_빈값은_거절한다()
    {
        Assert.Equal(
            "/group-purchase/products/food%2F0203",
            GroupPurchasePageRoutes.ProductDetailFor("food/0203"));
        Assert.Equal(
            "/group-purchase/wishes/wish%2F17/edit",
            GroupPurchasePageRoutes.WishEditFor("wish/17"));
        Assert.Equal(
            "/group-purchase/groups/group%2Fonion",
            GroupPurchasePageRoutes.GroupDetailFor("group/onion"));
        Assert.Equal(
            "/group-purchase/imports/import%2F17/costs",
            GroupPurchasePageRoutes.ImportCostsFor("import/17"));
        Assert.Throws<ArgumentException>(() => GroupPurchasePageRoutes.DemandCreateFor(" "));
        Assert.Throws<ArgumentException>(() => GroupPurchasePageRoutes.WishDetailFor(" "));
        Assert.Throws<ArgumentException>(() => GroupPurchasePageRoutes.GroupDetailFor(" "));
        Assert.Throws<ArgumentException>(() => GroupPurchasePageRoutes.ImportOverviewFor(" "));
    }
}
