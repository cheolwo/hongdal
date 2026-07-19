using SsalddelApp.Services.Application;

namespace SsalddelApp.Services.Samples.Events;

public sealed record ShipperRequestAddedEvent(
    string TransportRequestId,
    string CargoName,
    string PickupLocation,
    string DropoffLocation,
    DateTime OccurredAt) : IAppEvent;
