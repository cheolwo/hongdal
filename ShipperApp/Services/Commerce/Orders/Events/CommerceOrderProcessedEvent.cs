using ShipperApp.Services.Application;

namespace ShipperApp.Services.Commerce.Orders.Events;

public sealed record CommerceOrderProcessedEvent(
    string ChannelType,
    string ChannelOrderNo,
    string OrderScope,
    int NotificationCount,
    DateTime OccurredAt) : IAppEvent;
