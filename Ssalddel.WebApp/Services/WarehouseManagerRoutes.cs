namespace Ssalddel.WebApp.Services;

public static class WarehouseManagerRoutes
{
    public const string Home = "/warehouse";
    public const string InboundWorkStart = "/warehouse/work/inbound";
    public const string OutboundWorkStart = "/warehouse/work/outbound";
    public const string PackingWorkStart = "/warehouse/work/packing";
    public const string MarketFulfillmentWorkStart = "/warehouse/work/market-fulfillment";
    public const string InternationalForwardingWorkStart = "/warehouse/work/international-forwarding";
    public const string DeliveryAgencyWorkStart = "/warehouse/work/delivery-agency";
    public const string Scan = "/warehouse/scan";
    public const string WorkBoard = "/warehouse/work-board";
    public const string PickingBatch = "/warehouse/work/picking-batch";
    public const string InboundProductScan = "/warehouse/work/inbound/products";
    public const string InboundInspection = "/warehouse/work/inbound/inspection";
    public const string MartHome = "/warehouse/mart";
    public const string MartInboundWorkStart = "/warehouse/mart/work/mart-inbound";
    public const string MartReplenishmentWorkStart = "/warehouse/mart/work/mart-replenishment";
    public const string MartPickingWorkStart = "/warehouse/mart/work/mart-picking";
    public const string MartPickingPacking = "/warehouse/mart/picking";
    public const string MartDeliveryPickupWorkStart = "/warehouse/mart/work/mart-delivery-pickup";
    public const string MartWorkBoard = "/warehouse/mart/work-board";

    public static string WorkbenchScan(string processCode) => $"/warehouse/work/{processCode}/workbench";
}
