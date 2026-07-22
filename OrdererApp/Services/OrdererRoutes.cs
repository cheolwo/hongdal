using Ssalddel.Contracts.Common.Mart;

namespace OrdererApp.Services;

public static class OrdererRoutes
{
    public const string Home = "/";
    public const string Food = "/food";
    public const string Mart = MartProductPageRoutes.Root;
    public const string MartOrderRequest = MartProductPageRoutes.OrderRoot;
    public const string Restaurants = "/food/restaurants";
    public const string Cargo = "/cargo";
    public const string GroupPurchase = "/group-purchase";
    public const string Orders = "/orders";
}
