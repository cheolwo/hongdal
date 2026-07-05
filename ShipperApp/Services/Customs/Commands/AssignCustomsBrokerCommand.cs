using ShipperApp.Services.Application;

namespace ShipperApp.Services.Customs.Commands;

public sealed record AssignCustomsBrokerCommand(long ReviewId, string BrokerId)
    : IAppCommand<bool>;
