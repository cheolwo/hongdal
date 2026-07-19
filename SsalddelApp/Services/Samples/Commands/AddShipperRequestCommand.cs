using SsalddelApp.Models.Shipper;
using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Samples.Commands;

public sealed record AddShipperRequestCommand(ShipperRequestItem Request, string ShipperUserId)
    : IAppCommand<bool>;
