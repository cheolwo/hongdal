using StackExchange.Redis;

namespace 홍달.Services.Storage.Redis
{
    public sealed class RedisDriverPushTokenStore : IDriverPushTokenStore
    {
        private const string KeyPrefix = "hongdal:driver-push-token:";
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(30);

        private readonly IDatabase _database;

        public RedisDriverPushTokenStore(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public Task SetAsync(string driverId, string pushToken, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            pushToken = pushToken.Trim();
            if (string.IsNullOrWhiteSpace(driverId) || string.IsNullOrWhiteSpace(pushToken))
            {
                return Task.CompletedTask;
            }

            return _database.StringSetAsync(BuildKey(driverId), pushToken, DefaultTtl);
        }

        public async Task<string?> GetAsync(string driverId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return null;
            }

            var token = await _database.StringGetAsync(BuildKey(driverId)).ConfigureAwait(false);
            return token.IsNullOrEmpty ? null : token.ToString();
        }

        public Task ClearAsync(string driverId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return Task.CompletedTask;
            }

            return _database.KeyDeleteAsync(BuildKey(driverId));
        }

        private static string BuildKey(string driverId) => $"{KeyPrefix}{driverId.Trim()}";
    }
}



