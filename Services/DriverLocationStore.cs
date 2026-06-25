using System.Text.Json;
using StackExchange.Redis;

namespace 홍달.Services
{
    public sealed class DriverLocationStore : IDriverLocationStore
    {
        private const string KeyPrefix = "hongdal:driver-location:";
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromDays(7);

        private readonly IDatabase _database;

        public DriverLocationStore(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public void Upsert(DriverLocationSnapshot snapshot)
        {
            var json = JsonSerializer.Serialize(snapshot);
            _database.StringSet(BuildKey(snapshot.DriverId), json, DefaultTtl);
        }

        public bool TryGetLatest(string driverId, out DriverLocationSnapshot snapshot)
        {
            snapshot = default!;

            var json = _database.StringGet(BuildKey(driverId));
            if (json.IsNullOrEmpty)
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(json.ToString());
                var root = document.RootElement;

                var value = new DriverLocationSnapshot(
                    root.GetProperty(nameof(DriverLocationSnapshot.DriverId)).GetString() ?? string.Empty,
                    root.GetProperty(nameof(DriverLocationSnapshot.위도)).GetDecimal(),
                    root.GetProperty(nameof(DriverLocationSnapshot.경도)).GetDecimal(),
                    root.TryGetProperty(nameof(DriverLocationSnapshot.정확도_m), out var accuracyElement) && accuracyElement.ValueKind != JsonValueKind.Null
                        ? accuracyElement.GetDecimal()
                        : null,
                    root.GetProperty(nameof(DriverLocationSnapshot.운행상태)).GetString() ?? string.Empty,
                    root.GetProperty(nameof(DriverLocationSnapshot.기록시각)).GetDateTime(),
                    root.GetProperty(nameof(DriverLocationSnapshot.수신시각)).GetDateTime());
                if (value == null)
                {
                    return false;
                }

                snapshot = value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildKey(string driverId) => $"{KeyPrefix}{driverId.Trim()}";
    }
}
