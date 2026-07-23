using Ssalddel.Contracts.Common.Mart;
using Ssalddel.Contracts.Common.Orderer;

namespace OrdererApp.Services;

public static class OrdererRoutes
{
    public const string Home = "/";
    public const string Food = "/food";
    public const string Mart = MartProductPageRoutes.Root;
    public const string MartOrderRequest = MartProductPageRoutes.OrderRoot;
    public const string Restaurants = "/food/restaurants";
    public const string Cargo = "/cargo";
    public const string GroupPurchase = GroupPurchasePageRoutes.Root;
    public const string GroupPurchaseProducts = GroupPurchasePageRoutes.ProductsRoot;
    public const string GroupPurchaseWishes = GroupPurchasePageRoutes.WishesRoot;
    public const string GroupPurchaseWishCreate = GroupPurchasePageRoutes.WishCreate;
    public const string GroupPurchaseGroups = GroupPurchasePageRoutes.GroupsRoot;
    public const string GroupPurchaseShipments = GroupPurchasePageRoutes.Shipments;
    public const string Orders = "/orders";
}
