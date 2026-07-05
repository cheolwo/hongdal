namespace ShipperApp.Services.Application;

public interface IAppEvent
{
    DateTime OccurredAt { get; }
}
