namespace Ssalddel.Contracts.Common.Orderer;

public enum GroupPurchaseScreenKind
{
    Overview,
    ProductList,
    ProductDetail,
    DemandCreate,
    ImportReview,
    ShipmentTracking
}

/// <summary>Web과 주문자 앱이 공유하는 공동구매 List·Detail·Action route입니다.</summary>
public static class GroupPurchasePageRoutes
{
    public const string Root = "/group-purchase";
    public const string ProductsRoot = $"{Root}/products";
    public const string ProductDetailTemplate = $"{ProductsRoot}/{{ProductId}}";
    public const string DemandCreateRoot = $"{Root}/demands/new";
    public const string DemandCreateTemplate = $"{DemandCreateRoot}/{{ProductId}}";
    public const string ImportReviewRoot = $"{Root}/import-review";
    public const string ImportReviewTemplate = $"{ImportReviewRoot}/{{ProductId}}";
    public const string Shipments = $"{Root}/shipments";

    public static string ProductDetailFor(string productId)
        => $"{ProductsRoot}/{RequireProductId(productId)}";

    public static string DemandCreateFor(string productId)
        => $"{DemandCreateRoot}/{RequireProductId(productId)}";

    public static string ImportReviewFor(string productId)
        => $"{ImportReviewRoot}/{RequireProductId(productId)}";

    private static string RequireProductId(string productId)
        => !string.IsNullOrWhiteSpace(productId)
            ? Uri.EscapeDataString(productId.Trim())
            : throw new ArgumentException("공동구매 상품 ID가 필요합니다.", nameof(productId));
}
