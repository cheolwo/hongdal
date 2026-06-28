using StackExchange.Redis;

namespace 홍달.Services.Storage.Redis
{
    public sealed class RedisDriverRejectedRequestStore : IDriverRejectedRequestStore
    {
        private const string KeyPrefix = "hongdal:driver-rejected-requests:";
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(14);

        private readonly IDatabase _database;

        public RedisDriverRejectedRequestStore(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task RejectAsync(string driverId, string requestId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            requestId = requestId.Trim();
            if (string.IsNullOrWhiteSpace(driverId) || string.IsNullOrWhiteSpace(requestId))
            {
                return;
            }

            var key = BuildKey(driverId);
            await _database.SetAddAsync(key, requestId).ConfigureAwait(false);
            await _database.KeyExpireAsync(key, DefaultTtl).ConfigureAwait(false);
        }

        public async Task<bool> IsRejectedAsync(string driverId, string requestId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            requestId = requestId.Trim();
            if (string.IsNullOrWhiteSpace(driverId) || string.IsNullOrWhiteSpace(requestId))
            {
                return false;
            }

            return await _database.SetContainsAsync(BuildKey(driverId), requestId).ConfigureAwait(false);
        }

        public async Task<IReadOnlySet<string>> GetRejectedRequestIdsAsync(string driverId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return new HashSet<string>(StringComparer.Ordinal);
            }

            var values = await _database.SetMembersAsync(BuildKey(driverId)).ConfigureAwait(false);
            if (values.Length == 0)
            {
                return new HashSet<string>(StringComparer.Ordinal);
            }

            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty)
                {
                    set.Add(value.ToString());
                }
            }

            return set;
        }

        private static string BuildKey(string driverId) => $"{KeyPrefix}{driverId.Trim()}";
    }
}



