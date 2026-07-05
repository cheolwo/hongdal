using ShipperApp.Services.Application;

namespace ShipperApp.Services.Commerce.Orders.Commands;

public sealed record ProcessCommerceOrderCommand(ExternalCommerceOrder Order)
    : IAppCommand<CommerceOrderFulfillmentResult>;
