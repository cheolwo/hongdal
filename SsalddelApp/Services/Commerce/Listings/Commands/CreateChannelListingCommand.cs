using Ssalddel.Contracts.Common.Sales;
using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Commerce.Listings.Commands;

public sealed record CreateChannelListingCommand(채널출품저장요청 Payload)
    : IAppCommand<채널출품항목응답?>;
