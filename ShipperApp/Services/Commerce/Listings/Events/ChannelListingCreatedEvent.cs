using ShipperApp.Services.Application;

namespace ShipperApp.Services.Commerce.Listings.Events;

public sealed record ChannelListingCreatedEvent(
    long ListingId,
    long ProductId,
    long AccountId,
    string SyncStatus,
    DateTime OccurredAt) : IAppEvent;
