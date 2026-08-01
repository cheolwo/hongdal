namespace Ssalddel.Contracts.Admin.BusinessPackages;

/// <summary>
/// Stable administration entry points for the three operational packages.
/// These are navigation boundaries only: identity, authorization, tenant context,
/// contracts and infrastructure remain shared platform concerns.
/// </summary>
public static class 업무패키지관리Routes
{
    public const string FoodDeliveryAdminRoot = "/admin/food-delivery";
    public const string FreightDeliveryAdminRoot = "/admin/freight-delivery";
    public const string OrderWarehouseAdminRoot = "/admin/order-warehouse";

    public const string FoodDeliveryOperations = FoodDeliveryAdminRoot + "/operations";
    public const string FoodDeliveryOrderTrace = FoodDeliveryAdminRoot + "/order-trace";
    public const string FoodDeliveryDispatchReview = FoodDeliveryAdminRoot + "/dispatch-ai-review";

    public const string FreightDeliveryRequests = FreightDeliveryAdminRoot + "/requests";
    public const string FreightDeliveryTransports = FreightDeliveryAdminRoot + "/transports";
    public const string FreightDeliveryDrivers = FreightDeliveryAdminRoot + "/drivers";
    public const string FreightDeliveryDispatchWait = FreightDeliveryAdminRoot + "/dispatch-wait";
    public const string FreightDeliveryDispatchReview = FreightDeliveryAdminRoot + "/dispatch-ai-review";
    public const string FreightDeliveryVehicles = FreightDeliveryAdminRoot + "/vehicles";

    public const string OrderWarehouseDashboard = OrderWarehouseAdminRoot + "/dashboard";
    public const string OrderWarehouseOutboundRequests = OrderWarehouseAdminRoot + "/outbound-requests";
    public const string OrderWarehouseOutboundTransports = OrderWarehouseAdminRoot + "/outbound-transports";
    public const string OrderWarehouseDocuments = OrderWarehouseAdminRoot + "/documents";
}
