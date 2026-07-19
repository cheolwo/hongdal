namespace 살뜰.Services.Storage.Local
{
    public interface IDriverCallScopeStore
    {
        Task SetNationwideEnabledAsync(string driverId, bool enabled, CancellationToken cancellationToken = default);
        Task<bool> IsNationwideEnabledAsync(string driverId, CancellationToken cancellationToken = default);
    }
}



