using Ssalddel.Contracts.Common.Mart;

namespace Ssalddel.WebApp.Services;

public static class WebOrdererRoutes
{
    public const string Home = "/orderer";
    public const string Mart = MartProductPageRoutes.Root;
    public const string MartOrderRequest = MartProductPageRoutes.OrderRoot;
    public const string LegacyMart = MartProductPageRoutes.LegacyWebRoot;
    public const string LegacyMartOrderRequest = MartProductPageRoutes.LegacyWebOrderRoot;
}
