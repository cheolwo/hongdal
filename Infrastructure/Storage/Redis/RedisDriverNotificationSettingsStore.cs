using System.Text.Json;
using StackExchange.Redis;
using 홍달.Services.Storage.Local;

namespace 홍달.Infrastructure.Storage.Redis
{
    public sealed class RedisDriverNotificationSettingsStore : IDriverNotificationSettingsStore
    {
        private const string KeyPrefix = "hongdal:driver-notification-settings:";
        private static readonly DriverNotificationSettings DefaultSettings = new(false, false, true, true, false, false);

        private readonly IDatabase _database;

        public RedisDriverNotificationSettingsStore(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<DriverNotificationSettings> GetAsync(string driverId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return DefaultSettings;
            }

            var json = await _database.StringGetAsync(BuildKey(driverId)).ConfigureAwait(false);
            if (json.IsNullOrEmpty)
            {
                return DefaultSettings;
            }

            try
            {
                var settings = JsonSerializer.Deserialize<DriverNotificationSettings>(json.ToString());
                return settings ?? DefaultSettings;
            }
            catch
            {
                return DefaultSettings;
            }
        }

        public Task SetAsync(string driverId, DriverNotificationSettings settings, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return Task.CompletedTask;
            }

            var json = JsonSerializer.Serialize(settings);
            return _database.StringSetAsync(BuildKey(driverId), json, TimeSpan.FromDays(365));
        }

        private static string BuildKey(string driverId) => $"{KeyPrefix}{driverId.Trim()}";
    }
}
