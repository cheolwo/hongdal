using ShipperApp.Services.Application;

namespace ShipperApp.Services.Customs.Commands;

public sealed record CompleteCustomsHsReviewCommand(long ReviewId, string HsCode, string Comment)
    : IAppCommand<bool>;
