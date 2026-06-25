using StackExchange.Redis;

namespace 홍달.Services
{
    public sealed class RedisDriverWorkQueueStore : IDriverWorkQueueStore
    {
        private const string QueueIndexKey = "hongdal:driver-work-queue:index";
        private const string ItemKeyPrefix = "hongdal:driver-work-queue:item:";

        private readonly IDatabase _database;

        public RedisDriverWorkQueueStore(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task UpsertAsync(DriverWorkQueueEntry entry, CancellationToken cancellationToken = default)
        {
            var score = entry.StartedAtUtc.ToUniversalTime().Ticks;

            await _database.SortedSetAddAsync(QueueIndexKey, entry.DriverId, score).ConfigureAwait(false);
            await _database.HashSetAsync(BuildItemKey(entry.DriverId),
                [
                    new HashEntry(nameof(DriverWorkQueueEntry.DriverId), entry.DriverId),
                    new HashEntry(nameof(DriverWorkQueueEntry.ShiftId), entry.ShiftId),
                    new HashEntry(nameof(DriverWorkQueueEntry.StartedAtUtc), entry.StartedAtUtc.ToUniversalTime().ToString("O")),
                    new HashEntry(nameof(DriverWorkQueueEntry.StartMode), entry.StartMode),
                    new HashEntry(nameof(DriverWorkQueueEntry.StartLocation), entry.StartLocation),
                    new HashEntry(nameof(DriverWorkQueueEntry.ReturnDestination), entry.ReturnDestination ?? string.Empty)
                ]).ConfigureAwait(false);
        }

        public async Task RemoveAsync(string driverId, CancellationToken cancellationToken = default)
        {
            driverId = driverId.Trim();
            if (string.IsNullOrWhiteSpace(driverId))
            {
                return;
            }

            await _database.SortedSetRemoveAsync(QueueIndexKey, driverId).ConfigureAwait(false);
            await _database.KeyDeleteAsync(BuildItemKey(driverId)).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<DriverWorkQueueEntry>> SnapshotAsync(CancellationToken cancellationToken = default)
        {
            var driverIds = await _database.SortedSetRangeByRankAsync(QueueIndexKey, order: Order.Ascending).ConfigureAwait(false);
            if (driverIds.Length == 0)
            {
                return [];
            }

            var items = new List<DriverWorkQueueEntry>(driverIds.Length);
            foreach (var driverIdValue in driverIds)
            {
                var driverId = driverIdValue.ToString();
                var hash = await _database.HashGetAllAsync(BuildItemKey(driverId)).ConfigureAwait(false);
                if (hash.Length == 0)
                {
                    continue;
                }

                try
                {
                    var values = hash.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

                    if (!values.TryGetValue(nameof(DriverWorkQueueEntry.DriverId), out var storedDriverId) ||
                        !values.TryGetValue(nameof(DriverWorkQueueEntry.ShiftId), out var shiftIdValue) ||
                        !values.TryGetValue(nameof(DriverWorkQueueEntry.StartedAtUtc), out var startedAtValue) ||
                        !values.TryGetValue(nameof(DriverWorkQueueEntry.StartMode), out var startMode) ||
                        !values.TryGetValue(nameof(DriverWorkQueueEntry.StartLocation), out var startLocation))
                    {
                        continue;
                    }

                    var returnDestination = values.TryGetValue(nameof(DriverWorkQueueEntry.ReturnDestination), out var returnDestinationValue) && !string.IsNullOrWhiteSpace(returnDestinationValue)
                        ? returnDestinationValue
                        : null;

                    var item = new DriverWorkQueueEntry(
                        storedDriverId,
                        long.Parse(shiftIdValue),
                        DateTime.Parse(startedAtValue, null, System.Globalization.DateTimeStyles.RoundtripKind),
                        startMode,
                        startLocation,
                        returnDestination);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
                catch
                {
                }
            }

            return items;
        }

        private static string BuildItemKey(string driverId) => $"{ItemKeyPrefix}{driverId.Trim()}";
    }
}
