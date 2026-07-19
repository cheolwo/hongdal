namespace SsalddelApp.Services.Commerce.Orders;

public interface ICommerceOrderSampleFeedService
{
    IReadOnlyList<ExternalCommerceOrder> GetSampleOrders();
}
