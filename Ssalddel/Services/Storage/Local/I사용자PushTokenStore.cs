namespace 살뜰.Services.Storage.Local;

public interface I사용자PushTokenStore
{
    Task SetAsync(string userId, string pushToken, CancellationToken cancellationToken = default);

    Task<string?> GetAsync(string userId, CancellationToken cancellationToken = default);

    Task ClearAsync(string userId, CancellationToken cancellationToken = default);
}
