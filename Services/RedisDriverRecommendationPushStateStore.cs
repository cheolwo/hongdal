using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;

namespace 홍달.Services
{
    public sealed class RedisDriverRecommendationPushStateStore : IDriverRecommendationPushStateStore
    {
        private const string KeyPrefix = "hongdal:driver-recommendation-push-sig:";

        private readonly IDatabase _database;

        public RedisDriverRecommendationPushStateStore(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<bool> HasChangedAsync(string driverId, IReadOnlyList<string> recommendationIds, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return false;
            }

            var currentSignature = BuildSignature(recommendationIds);
            var key = BuildKey(driverId);
            var previousSignature = await _database.StringGetAsync(key).ConfigureAwait(false);
            if (!previousSignature.IsNullOrEmpty && string.Equals(previousSignature.ToString(), currentSignature, StringComparison.Ordinal))
            {
                return false;
            }

            await _database.StringSetAsync(key, currentSignature).ConfigureAwait(false);
            return true;
        }

        public async Task<string?> GetSignatureAsync(string driverId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return null;
            }

            var signature = await _database.StringGetAsync(BuildKey(driverId)).ConfigureAwait(false);
            return signature.IsNullOrEmpty ? null : signature.ToString();
        }

        private static string BuildSignature(IReadOnlyList<string> recommendationIds)
        {
            var normalized = recommendationIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();

            var input = string.Join('|', normalized);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }

        private static string BuildKey(string driverId) => $"{KeyPrefix}{driverId.Trim()}";
    }
}
