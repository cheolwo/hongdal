namespace 홍달.Services.Storage.Local
{
    public interface IDriverRecommendationPushStateStore
    {
        Task<bool> HasChangedAsync(string driverId, IReadOnlyList<string> recommendationIds, CancellationToken cancellationToken = default);
        Task<string?> GetSignatureAsync(string driverId, CancellationToken cancellationToken = default);
    }
}



