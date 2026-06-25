namespace 홍달.Services
{
    public interface IDriverRejectedRequestStore
    {
        Task RejectAsync(string driverId, string requestId, CancellationToken cancellationToken = default);
        Task<bool> IsRejectedAsync(string driverId, string requestId, CancellationToken cancellationToken = default);
        Task<IReadOnlySet<string>> GetRejectedRequestIdsAsync(string driverId, CancellationToken cancellationToken = default);
    }
}
