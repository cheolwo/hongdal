namespace 홍달.Services
{
    public interface IDriverPushTokenStore
    {
        Task SetAsync(string driverId, string pushToken, CancellationToken cancellationToken = default);
        Task<string?> GetAsync(string driverId, CancellationToken cancellationToken = default);
        Task ClearAsync(string driverId, CancellationToken cancellationToken = default);
    }
}
