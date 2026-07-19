using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Customs.Commands;

public sealed record CompleteCustomsHsReviewCommand(long ReviewId, string HsCode, string Comment)
    : IAppCommand<bool>;
