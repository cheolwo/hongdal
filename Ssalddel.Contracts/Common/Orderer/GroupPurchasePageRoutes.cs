namespace Ssalddel.Contracts.Common.Orderer;

public enum GroupPurchaseScreenKind
{
    Overview,
    Practice,
    ProductList,
    ProductDetail,
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
    ImportHandoff,
    ImportConsent,
    ImportReview,
    ShipmentTracking
}

/// <summary>Web과 주문자 앱이 공유하는 공동구매 List·Detail·Action route입니다.</summary>
public static class GroupPurchasePageRoutes
{
    public const string Root = "/group-purchase";
    public const string Practice = $"{Root}/practice";
    public const string ProductsRoot = $"{Root}/products";
    public const string ProductDetailTemplate = $"{ProductsRoot}/{{ProductId}}";
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
    public const string ImportHandoffTemplate = $"{ImportOverviewTemplate}/handoff";
    public const string ImportConsentTemplate = $"{ImportOverviewTemplate}/consent";
    public const string ImportReviewRoot = $"{Root}/import-review";
    public const string ImportReviewTemplate = $"{ImportReviewRoot}/{{ProductId}}";
    public const string Shipments = $"{Root}/shipments";

    public static string ProductDetailFor(string productId)
        => $"{ProductsRoot}/{RequireProductId(productId)}";

    public static string DemandCreateFor(string productId)
        => $"{DemandCreateRoot}/{RequireProductId(productId)}";

    public static string WishDetailFor(string wishLedgerId)
        => $"{WishesRoot}/{RequireSegment(wishLedgerId, nameof(wishLedgerId), "개별 원함 원장 ID")}";

    public static string WishEditFor(string wishLedgerId)
        => $"{WishDetailFor(wishLedgerId)}/edit";

    public static string GroupDetailFor(string autoGroupId)
        => $"{GroupsRoot}/{RequireSegment(autoGroupId, nameof(autoGroupId), "자동집단 ID")}";

    public static string ImportOverviewFor(string groupImportLedgerId)
        => $"{ImportsRoot}/{RequireSegment(groupImportLedgerId, nameof(groupImportLedgerId), "공동수입 원장 ID")}";

    public static string ImportSuppliersFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/suppliers";

    public static string ImportCostsFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/costs";

    public static string ImportClassificationFor(string groupImportLedgerId)
        => $"{ImportOverviewFor(groupImportLedgerId)}/classification";

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
