using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Commerce.Listings.Events;

public sealed record ChannelListingCreatedEvent(
    long ListingId,
    long ProductId,
    long AccountId,
    string SyncStatus,
    DateTime OccurredAt) : IAppEvent;
