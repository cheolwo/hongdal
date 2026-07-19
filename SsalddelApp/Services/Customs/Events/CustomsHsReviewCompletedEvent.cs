using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Customs.Events;

public sealed record CustomsHsReviewCompletedEvent(
    long ReviewId,
    string HsCode,
    DateTime OccurredAt) : IAppEvent;
