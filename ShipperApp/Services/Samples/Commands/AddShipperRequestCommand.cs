using ShipperApp.Models.Shipper;
using ShipperApp.Services.Application;

namespace ShipperApp.Services.Samples.Commands;

public sealed record AddShipperRequestCommand(ShipperRequestItem Request, string ShipperUserId)
    : IAppCommand<bool>;
