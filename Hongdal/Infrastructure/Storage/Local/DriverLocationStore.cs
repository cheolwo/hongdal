using System.Text.Json;
using StackExchange.Redis;
using 홍달.Services.Storage.Local;

namespace 홍달.Infrastructure.Storage.Local
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
                    root.GetProperty(nameof(DriverLocationSnapshot.Latitude)).GetDecimal(),
                    root.GetProperty(nameof(DriverLocationSnapshot.Longitude)).GetDecimal(),
                    root.TryGetProperty(nameof(DriverLocationSnapshot.AccuracyM), out var accuracyElement) && accuracyElement.ValueKind != JsonValueKind.Null
                        ? accuracyElement.GetDecimal()
                        : null,
                    root.GetProperty(nameof(DriverLocationSnapshot.DrivingStatus)).GetString() ?? string.Empty,
                    root.GetProperty(nameof(DriverLocationSnapshot.RecordedAtUtc)).GetDateTime(),
                    root.GetProperty(nameof(DriverLocationSnapshot.ReceivedAtUtc)).GetDateTime());
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
