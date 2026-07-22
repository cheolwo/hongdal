using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.WebApp.Services;

public static class ShipperRoutes
{
    public const string Home = "/shipper";
    public const string Request = ShipperRequestPageRoutes.Root;
    public const string RequestBulk = ShipperRequestPageRoutes.Bulk;
    public const string RequestDetail = ShipperRequestPageRoutes.Root;
    public const string RequestDetailLookup = ShipperRequestDetailPageRoutes.LegacyLookup;
    public const string PaymentStatus = "/shipper/request/payment-status";
    public const string PublicCargo = "/shipper/public-cargo";
    public const string ExplorationInbox = "/shipper/exploration/inbox";
    public const string InboundDashboard = "/shipper/inbound/dashboard";
    public const string InboundRequests = InboundRequestPageRoutes.Root;
    public const string InboundRequestCreate = InboundRequestPageRoutes.Create;
    public const string WarehouseRegistration = InboundRequestPageRoutes.WarehouseRegistration;
    public const string WarehouseWorkspace = "/shipper/warehouse/workspace";
    public const string WarehouseInventory = "/shipper/warehouse/inventory";
    public const string WarehouseScan = "/shipper/warehouse/scan";
    public const string WarehouseInboundWorkStart = "/shipper/warehouse/work/inbound";
    public const string WarehouseOutboundWorkStart = "/shipper/warehouse/work/outbound";
    public const string WarehousePackingWorkStart = "/shipper/warehouse/work/packing";
    public const string MarketFulfillmentWorkStart = "/shipper/warehouse/work/market-fulfillment";
    public const string InternationalForwardingWorkStart = "/shipper/warehouse/work/international-forwarding";
    public const string DeliveryAgencyWorkStart = "/shipper/warehouse/work/delivery-agency";
    public const string ReconsignmentOrders = "/shipper/reconsignment/orders";
    public const string SalesChannels = "/shipper/sales/channels";
    public const string SalesPageComposer = "/shipper/sales/pages/new";
    public const string ProductListings = "/shipper/sales/listings";
    public const string OrderFulfillment = "/shipper/sales/orders";
    public const string CustomsHsReviews = "/shipper/customs/hs-reviews";
    public const string FclLclPlanner = "/shipper/international/fcl-lcl";
    public const string ViewSettings = "/shipper/settings/views";
    public const string ProfileSettings = "/shipper/settings/profile";
    public const string DispatchAddressForm = "/dispatch/address-form";

    public static string RequestDetailFor(string requestId)
        => ShipperRequestDetailPageRoutes.SummaryFor(requestId);

    public static string CreatedRequestDetailFor(string requestId)
        => new ShipperRequestDetailNavigationContext { Created = true }
            .PathFor(ShipperRequestDetailScreenKind.Summary, requestId);

    public static string RequestTimelineFor(string requestId)
        => ShipperRequestDetailPageRoutes.TimelineFor(requestId);

    public static string RequestPaymentFor(string requestId)
        => ShipperRequestDetailPageRoutes.PaymentFor(requestId);

    public static string RequestProofsFor(string requestId)
        => ShipperRequestDetailPageRoutes.ProofsFor(requestId);

    public static string InboundRequestDetailFor(long inboundId)
        => InboundRequestPageRoutes.DetailFor(inboundId);

    public static string InboundRequestCompleteFor(long inboundId)
        => InboundRequestPageRoutes.CompleteFor(inboundId);

    public static string ReconsignmentOrdersForInventory(long inventoryItemId)
        => $"{ReconsignmentOrders}?inventoryItemId={inventoryItemId}";
}
