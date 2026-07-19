using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Customs.Events;

public sealed record CustomsHsReviewRequestedEvent(
    long ReviewId,
    string TransportRequestId,
    string FlowDirection,
    string CargoName,
    DateTime OccurredAt) : IAppEvent;
