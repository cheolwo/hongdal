namespace SsalddelApp.Services.Customs;

public interface ICustomsBrokerDirectory
{
    IReadOnlyList<CustomsBrokerProfile> GetAvailableBrokers();
}
