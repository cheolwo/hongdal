using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Customs.Events;

public sealed record CustomsBrokerAssignedEvent(
    long ReviewId,
    string BrokerName,
    DateTime OccurredAt) : IAppEvent;
