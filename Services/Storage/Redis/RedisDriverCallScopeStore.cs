using StackExchange.Redis;

namespace 홍달.Services.Storage.Redis
{
    public sealed class RedisDriverCallScopeStore : IDriverCallScopeStore
    {
        private const string KeyPrefix = "hongdal:driver-call-scope:";
        private readonly IDatabase _database;

        public RedisDriverCallScopeStore(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public Task SetNationwideEnabledAsync(string driverId, bool enabled, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return Task.CompletedTask;
            }

            return _database.StringSetAsync(BuildKey(driverId), enabled ? "1" : "0");
        }

        public async Task<bool> IsNationwideEnabledAsync(string driverId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return false;
            }

            var value = await _database.StringGetAsync(BuildKey(driverId)).ConfigureAwait(false);
            if (value.IsNullOrEmpty)
            {
                return false;
            }

            var text = value.ToString();
            return string.Equals(text, "1", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildKey(string driverId) => $"{KeyPrefix}{driverId.Trim()}";
    }
}



