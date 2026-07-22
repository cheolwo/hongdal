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
    public const string PickingBatch = Ssalddel.Contracts.Common.Warehouse.PickingTaskPageRoutes.Root;
    public static string PickingBatchDetail(string taskKey)
        => Ssalddel.Contracts.Common.Warehouse.PickingTaskPageRoutes.DetailFor(taskKey);
    public static string PickingBatchExecute(string taskKey)
        => Ssalddel.Contracts.Common.Warehouse.PickingTaskPageRoutes.ExecuteFor(taskKey);
    public const string InboundProductScan = "/warehouse/work/inbound/products";
    public const string InboundInspection = Ssalddel.Contracts.Common.Inventory.InboundInspectionPageRoutes.Root;

    public static string InboundInspectionDetail(long inboundItemId)
        => Ssalddel.Contracts.Common.Inventory.InboundInspectionPageRoutes.DetailFor(inboundItemId);

    public static string InboundInspectionRecord(long inboundItemId)
        => Ssalddel.Contracts.Common.Inventory.InboundInspectionPageRoutes.RecordFor(inboundItemId);
    public const string GeneralInventory = "/warehouse/general/inventory";
    public const string PutAwayTask = "/warehouse/work/inbound/put-away";
    public const string PackingTask = "/warehouse/work/outbound/packing";
    public const string GeneralTransportHandoff = "/warehouse/general/transport-handoff";
    public const string OutboundPlanReview = "/warehouse/general/outbound-plan-review";
    public const string TransportRequestDraft = "/warehouse/general/transport-request-draft";
    public const string MartHome = "/warehouse/mart";
    public const string MartInboundWorkStart = "/warehouse/mart/work/mart-inbound";
    public const string MartReplenishmentWorkStart = "/warehouse/mart/work/mart-replenishment";
    public const string MartPickingWorkStart = "/warehouse/mart/work/mart-picking";
    public const string MartPickingPacking = "/warehouse/mart/picking";
    public const string MartDeliveryPickupWorkStart = "/warehouse/mart/work/mart-delivery-pickup";
    public const string MartWorkBoard = "/warehouse/mart/work-board";

    public static string WorkbenchScan(string processCode) => $"/warehouse/work/{processCode}/workbench";
}
