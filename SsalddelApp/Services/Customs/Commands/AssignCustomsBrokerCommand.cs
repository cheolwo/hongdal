using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Customs.Commands;

public sealed record AssignCustomsBrokerCommand(long ReviewId, string BrokerId)
    : IAppCommand<bool>;
