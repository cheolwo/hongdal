namespace Ssalddel.FoodApi.Application.Settlements;

public sealed class FoodDeliverySettlementMemoryStore : IFoodDeliverySettlementStore
{
    private readonly object _sync = new();
    private readonly Dictionary<string, FoodDeliverySettlementEntry> _entriesByTicketId = new(StringComparer.Ordinal);

    public FoodDeliverySettlementEntry AddOrReplace(FoodDeliverySettlementEntry entry)
    {
        lock (_sync)
        {
            _entriesByTicketId[entry.TicketId] = entry;
            return entry;
        }
    }

    public FoodDeliverySettlementSummary GetDaily(string driverId, DateOnly date)
    {
        return BuildSummary(driverId, date, date);
    }

    public FoodDeliverySettlementSummary GetWeekly(string driverId, DateOnly anyDateInWeek)
    {
        var start = anyDateInWeek.AddDays(-GetMondayOffset(anyDateInWeek));
        var end = start.AddDays(6);
        return BuildSummary(driverId, start, end);
    }

    private FoodDeliverySettlementSummary BuildSummary(string driverId, DateOnly fromDate, DateOnly toDate)
    {
        lock (_sync)
        {
            var entries = _entriesByTicketId.Values
                .Where(x => string.Equals(x.DriverId, driverId, StringComparison.Ordinal))
                .Where(x => x.BusinessDate >= fromDate && x.BusinessDate <= toDate)
                .OrderBy(x => x.CompletedAtUtc)
                .ToArray();

            return new FoodDeliverySettlementSummary
            {
                DriverId = driverId,
                FromDate = fromDate,
                ToDate = toDate,
                DeliveryCount = entries.Length,
                TotalPlatformDeliveryFee = entries.Sum(x => x.PlatformDeliveryFee),
                TotalDriverPayout = entries.Sum(x => x.DriverPayout),
                TotalPlatformMargin = entries.Sum(x => x.PlatformMargin),
                Entries = entries
            };
        }
    }

    private static int GetMondayOffset(DateOnly date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
            _ => 0
        };
    }
}
