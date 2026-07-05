namespace ShipperApp.Services.Customs;

public interface ICustomsBrokerDirectory
{
    IReadOnlyList<CustomsBrokerProfile> GetAvailableBrokers();
}
