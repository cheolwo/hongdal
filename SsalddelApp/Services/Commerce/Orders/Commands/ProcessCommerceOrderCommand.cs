using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Commerce.Orders.Commands;

public sealed record ProcessCommerceOrderCommand(ExternalCommerceOrder Order)
    : IAppCommand<CommerceOrderFulfillmentResult>;
