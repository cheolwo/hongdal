using Ssalddel.Contracts.Common.Sales;

namespace SellerApp.Services;

public static class SellerRoutes
{
    public const string Home = "/";
    public const string Login = "/login";
    public const string Inventory = "/seller/inventory";
    public const string SalesChannels = "/shipper/sales/channels";
    public const string SalesPageComposer = "/shipper/sales/pages/new";
    public const string Products = "/shipper/sales/products";
    public const string ProductCreate = "/shipper/sales/products/new";
    public const string Listings = "/shipper/sales/listings";
    public const string ListingCreate = "/shipper/sales/listings/new";
    public const string Orders = SalesOrderPageRoutes.Root;
    public const string OrdererDemand = "/seller/orderer-demand";
    public const string ForeignFoodFacilities = "/seller/foreign-food-facilities";

    public static string ProductCreateForInventory(long inventoryItemId)
        => $"{ProductCreate}?inventoryItemId={inventoryItemId}";

    public static string ListingCreateForProduct(long productId)
        => $"{ListingCreate}?productId={productId}";
}
