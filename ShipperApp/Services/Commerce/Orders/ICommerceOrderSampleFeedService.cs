namespace ShipperApp.Services.Commerce.Orders;

public interface ICommerceOrderSampleFeedService
{
    IReadOnlyList<ExternalCommerceOrder> GetSampleOrders();
}
