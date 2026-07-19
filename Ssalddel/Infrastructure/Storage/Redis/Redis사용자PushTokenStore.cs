using StackExchange.Redis;
using 살뜰.Services.Storage.Local;

namespace 살뜰.Infrastructure.Storage.Redis;

public sealed class Redis사용자PushTokenStore : I사용자PushTokenStore
{
    private const string KeyPrefix = "ssalddel:user-push-token:";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(30);

    private readonly IDatabase _database;

    public Redis사용자PushTokenStore(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public Task SetAsync(string userId, string pushToken, CancellationToken cancellationToken = default)
    {
        userId = userId.Trim();
        pushToken = pushToken.Trim();
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(pushToken))
        {
            return Task.CompletedTask;
        }

        return _database.StringSetAsync(BuildKey(userId), pushToken, DefaultTtl);
    }

    public async Task<string?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        userId = userId.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var token = await _database.StringGetAsync(BuildKey(userId)).ConfigureAwait(false);
        return token.IsNullOrEmpty ? null : token.ToString();
    }

    public Task ClearAsync(string userId, CancellationToken cancellationToken = default)
    {
        userId = userId.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        return _database.KeyDeleteAsync(BuildKey(userId));
    }

    private static string BuildKey(string userId) => $"{KeyPrefix}{userId.Trim()}";
}
