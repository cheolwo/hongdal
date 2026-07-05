namespace WarehouseManagerApp.Services;

public static class WarehouseManagerRoutes
{
    public const string Home = "/";
    public const string InboundWorkStart = "/work/inbound";
    public const string OutboundWorkStart = "/work/outbound";
    public const string PackingWorkStart = "/work/packing";
    public const string MarketFulfillmentWorkStart = "/work/market-fulfillment";
    public const string InternationalForwardingWorkStart = "/work/international-forwarding";
    public const string DeliveryAgencyWorkStart = "/work/delivery-agency";
    public const string Scan = "/scan";
    public const string WorkBoard = "/work-board";
    public const string InboundProductScan = "/work/inbound/products";
    public const string InboundInspection = "/work/inbound/inspection";
    public const string MartHome = "/mart";
    public const string MartInboundWorkStart = "/mart/work/mart-inbound";
    public const string MartReplenishmentWorkStart = "/mart/work/mart-replenishment";
    public const string MartPickingWorkStart = "/mart/work/mart-picking";
    public const string MartDeliveryPickupWorkStart = "/mart/work/mart-delivery-pickup";
    public const string MartWorkBoard = "/mart/work-board";

    public static string WorkbenchScan(string processCode) => $"/work/{processCode}/workbench";
}
