using ShipperApp.Services.Application;

namespace ShipperApp.Services.Customs.Events;

public sealed record CustomsHsReviewCompletedEvent(
    long ReviewId,
    string HsCode,
    DateTime OccurredAt) : IAppEvent;
