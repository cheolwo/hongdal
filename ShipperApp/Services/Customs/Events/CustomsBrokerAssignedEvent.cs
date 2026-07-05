using ShipperApp.Services.Application;

namespace ShipperApp.Services.Customs.Events;

public sealed record CustomsBrokerAssignedEvent(
    long ReviewId,
    string BrokerName,
    DateTime OccurredAt) : IAppEvent;
