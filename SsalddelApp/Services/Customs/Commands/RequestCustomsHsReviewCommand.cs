using SsalddelApp.Models.Shipper;
using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Customs.Commands;

public sealed record RequestCustomsHsReviewCommand(ShipperRequestItem Request, string ShipperUserId)
    : IAppCommand<CustomsHsReviewRequest?>;
