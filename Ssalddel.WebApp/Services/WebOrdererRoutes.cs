using Ssalddel.Contracts.Common.Mart;

namespace Ssalddel.WebApp.Services;

public static class WebOrdererRoutes
{
    public const string Home = "/orderer";
    public const string GroupPurchase = "/community/group-purchase";
    public const string GroupPurchaseDemand = "/community/group-purchase/demand";
    public const string IndividualImportLedger = "/orderer/ledgers/individual-import";
    public const string IndividualExportLedger = "/orderer/ledgers/individual-export";
    public const string GroupExportLedger = "/orderer/ledgers/group-export";
    public const string Mart = MartProductPageRoutes.Root;
    public const string MartOrderRequest = MartProductPageRoutes.OrderRoot;
    public const string LegacyMart = MartProductPageRoutes.LegacyWebRoot;
    public const string LegacyMartOrderRequest = MartProductPageRoutes.LegacyWebOrderRoot;
}
