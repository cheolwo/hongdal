using ShipperApp.Services.Application;

namespace ShipperApp.Services.Customs.Events;

public sealed record CustomsHsReviewRequestedEvent(
    long ReviewId,
    string TransportRequestId,
    string FlowDirection,
    string CargoName,
    DateTime OccurredAt) : IAppEvent;
