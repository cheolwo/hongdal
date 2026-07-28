namespace Ssalddel.Contracts.Common.Orderer;

public enum GroupPurchaseScreenKind
{
    Overview,
    Practice,
    ProductList,
    ProductDetail,
    RecipeUse,
    OrderModeComparison,
    DeliveryScopeFinder,
    DeliveryScopeDetail,
    TogetherOrderList,
    TogetherOrderDetail,
    SupplierRelationshipDetail,
    SupplierMembership,
    UrgentHarvestOffer,
    UrgentHarvestReview,
    DemandCreate,
    WishCreate,
    WishList,
    WishDetail,
    WishEdit,
    GroupList,
    GroupDetail,
    ImportOverview,
    ImportSuppliers,
    ImportCosts,
    ImportClassification,
    ImportLogisticsReview,
    ImportHandoff,
    ImportConsent,
    ImportReview,
    ShipmentTracking
}

/// <summary>Web과 주문자 앱이 공유하는 같이 주문 List·Detail·Action route입니다.</summary>
public static class GroupPurchasePageRoutes
{
    public const string Root = "/group-purchase";
    public const string Practice = $"{Root}/practice";
    public const string ProductsRoot = $"{Root}/products";
    public const string ProductDetailTemplate = $"{ProductsRoot}/{{ProductId}}";
    public const string RecipeUsesRoot = $"{Root}/recipe-uses";
    public const string RecipeUseTemplate = $"{RecipeUsesRoot}/{{ProductId}}";
    public const string OrderModeComparisonRoot = $"{Root}/compare";
    public const string OrderModeComparisonTemplate = $"{OrderModeComparisonRoot}/{{ProductId}}";
    public const string DeliveryScopesRoot = $"{Root}/delivery-scopes";
    public const string DeliveryScopeDetailTemplate = $"{DeliveryScopesRoot}/{{DeliveryScopeKey}}";
    public const string TogetherOrdersRoot = $"{Root}/together-orders";
    public const string TogetherOrderDetailTemplate = $"{TogetherOrdersRoot}/{{AutoGroupId}}";
    public const string SupplierRelationshipsRoot = $"{Root}/supplier-relationships";
    public const string SupplierRelationshipDetailTemplate =
        $"{SupplierRelationshipsRoot}/{{SupplierKey}}";
    public const string SupplierMembershipTemplate =
        $"{SupplierRelationshipDetailTemplate}/membership";
    public const string UrgentHarvestOffersRoot =
        $"{Root}/urgent-harvest-offers";
    public const string UrgentHarvestOfferTemplate =
        $"{UrgentHarvestOffersRoot}/{{SupplyOfferDraftId}}";
    public const string UrgentHarvestReviewTemplate =
        $"{UrgentHarvestOfferTemplate}/review";
    public const string DemandCreateRoot = $"{Root}/demands/new";
    public const string DemandCreateTemplate = $"{DemandCreateRoot}/{{ProductId}}";
    public const string WishesRoot = $"{Root}/wishes";
    public const string WishCreate = $"{WishesRoot}/new";
    public const string WishDetailTemplate = $"{WishesRoot}/{{WishLedgerId}}";
    public const string WishEditTemplate = $"{WishDetailTemplate}/edit";
    public const string GroupsRoot = $"{Root}/groups";
    public const string GroupDetailTemplate = $"{GroupsRoot}/{{AutoGroupId}}";
    public const string ImportsRoot = $"{Root}/imports";
    public const string ImportOverviewTemplate = $"{ImportsRoot}/{{GroupImportLedgerId}}";
    public const string ImportSuppliersTemplate = $"{ImportOverviewTemplate}/suppliers";
    public const string ImportCostsTemplate = $"{ImportOverviewTemplate}/costs";
    public const string ImportClassificationTemplate = $"{ImportOverviewTemplate}/classification";
    public const string ImportLogisticsReviewTemplate = $"{ImportOverviewTemplate}/logistics-review";
    public const string ImportHandoffTemplate = $"{ImportOverviewTemplate}/handoff";
    public const string ImportConsentTemplate = $"{ImportOverviewTemplate}/consent";
    public const string ImportReviewRoot = $"{Root}/import-review";
    public const string ImportReviewTemplate = $"{ImportReviewRoot}/{{ProductId}}";
    public const string Shipments = $"{Root}/shipments";

    public static string ProductDetailFor(string productId)
        => $"{ProductsRoot}/{RequireProductId(productId)}";

    public static string RecipeUseFor(string productId)
        => $"{RecipeUsesRoot}/{RequireProductId(productId)}";

    public static string OrderModeComparisonFor(string productId)
        => $"{OrderModeComparisonRoot}/{RequireProductId(productId)}";

    public static string DeliveryScopeFor(string deliveryScopeKey)
        => $"{DeliveryScopesRoot}/{RequireSegment(deliveryScopeKey, nameof(deliveryScopeKey), "배송권 Key")}";

    public static string TogetherOrderDetailFor(string autoGroupId)
        => $"{TogetherOrdersRoot}/{RequireSegment(autoGroupId, nameof(autoGroupId), "같이 주문 ID")}";

    public static string SupplierRelationshipFor(string supplierKey)
        => $"{SupplierRelationshipsRoot}/{RequireSegment(supplierKey, nameof(supplierKey), "공급자 Key")}";

    public static string SupplierMembershipFor(string supplierKey)
        => $"{SupplierRelationshipFor(supplierKey)}/membership";

    public static string UrgentHarvestOfferFor(string supplyOfferDraftId)
        => $"{UrgentHarvestOffersRoot}/{RequireSegment(supplyOfferDraftId, nameof(supplyOfferDraftId), "긴급 수확 제안 초안 ID")}";

    public static string UrgentHarvestReviewFor(string supplyOfferDraftId)
        => $"{UrgentHarvestOfferFor(supplyOfferDraftId)}/review";

    public static string DemandCreateFor(string productId)
        => $"{DemandCreateRoot}/{RequireProductId(productId)}";

    public static string WishDetailFor(string wishLedgerId)
        => $"{WishesRoot}/{RequireSegment(wishLedgerId, nameof(wishLedgerId), "개별 원함 원장 ID")}";

    public static string WishEditFor(string wishLedgerId)
        => $"{WishDetailFor(wishLedgerId)}/edit";

    public static string GroupDetailFor(string autoGroupId)
        => $"{GroupsRoot}/{RequireSegment(autoGroupId, nameof(autoGroupId), "자동집단 ID")}";

    public static string ImportOverviewFor(string groupImportLedgerId)
        => $"{ImportsRoot}/{RequireSegment(groupImportLedgerId, nameof(groupImportLedgerId), "같이 수입 원장 ID")}";

    public static string ImportSuppliersFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/suppliers";

    public static string ImportCostsFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/costs";

    public static string ImportClassificationFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/classification";

    public static string ImportLogisticsReviewFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/logistics-review";

    public static string ImportHandoffFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/handoff";

    public static string ImportConsentFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/consent";

    public static string ImportReviewFor(string productId)
        => $"{ImportReviewRoot}/{RequireProductId(productId)}";

    private static string RequireProductId(string productId)
        => RequireSegment(productId, nameof(productId), "공동구매 상품 ID");

    private static string RequireSegment(string value, string parameterName, string displayName)
        => !string.IsNullOrWhiteSpace(value)
            ? Uri.EscapeDataString(value.Trim())
            : throw new ArgumentException($"{displayName}가 필요합니다.", parameterName);
}
