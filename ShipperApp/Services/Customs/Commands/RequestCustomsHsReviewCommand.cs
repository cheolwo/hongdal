using ShipperApp.Models.Shipper;
using ShipperApp.Services.Application;

namespace ShipperApp.Services.Customs.Commands;

public sealed record RequestCustomsHsReviewCommand(ShipperRequestItem Request, string ShipperUserId)
    : IAppCommand<CustomsHsReviewRequest?>;
