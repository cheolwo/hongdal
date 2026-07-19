namespace Ssalddel.FoodApi.Application.DeliveryTickets;

public sealed class FoodDeliveryTicketMemoryIndex : IFoodDeliveryTicketMemoryIndex
{
    private readonly object _sync = new();
    private readonly Dictionary<string, FoodDeliveryTicket> _ticketsById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SortedSet<FoodDeliveryTicketIndexEntry>> _ticketsByRegion2 = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SortedSet<FoodDeliveryTicketIndexEntry>> _ticketsByRegion3 = new(StringComparer.Ordinal);

    public void AddOrUpdate(FoodDeliveryTicket ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket.TicketId))
        {
            throw new ArgumentException("TicketId is required.", nameof(ticket));
        }

        lock (_sync)
        {
            if (_ticketsById.TryGetValue(ticket.TicketId, out var existing))
            {
                RemoveFromIndexes(existing);
            }

            _ticketsById[ticket.TicketId] = ticket;
            AddToIndexes(ticket);
        }
    }

    public FoodDeliveryTicket? GetById(string ticketId)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            return null;
        }

        lock (_sync)
        {
            return _ticketsById.GetValueOrDefault(ticketId);
        }
    }

    public IReadOnlyList<FoodDeliveryTicket> GetByRegion3(string region3Key, int take = 20)
    {
        return GetFromIndex(_ticketsByRegion3, region3Key, take);
    }

    public IReadOnlyList<FoodDeliveryTicket> GetByRegion2(string region2Key, int take = 20)
    {
        return GetFromIndex(_ticketsByRegion2, region2Key, take);
    }

    public IReadOnlyList<FoodDeliveryTicket> GetPendingByRegion(AddressRegionKey region, int take = 20)
    {
        var result = new List<FoodDeliveryTicket>();

        if (!string.IsNullOrWhiteSpace(region.Region3Key))
        {
            result.AddRange(GetByRegion3(region.Region3Key, take));
        }

        if (result.Count < take && !string.IsNullOrWhiteSpace(region.Region2Key))
        {
            var existingIds = result.Select(x => x.TicketId).ToHashSet(StringComparer.Ordinal);
            result.AddRange(GetByRegion2(region.Region2Key, take)
                .Where(x => existingIds.Add(x.TicketId)));
        }

        return result
            .Where(x => string.Equals(x.Status, FoodDeliveryTicketStatus.Pending, StringComparison.Ordinal))
            .Take(take)
            .ToArray();
    }

    private IReadOnlyList<FoodDeliveryTicket> GetFromIndex(
        Dictionary<string, SortedSet<FoodDeliveryTicketIndexEntry>> index,
        string key,
        int take)
    {
        if (string.IsNullOrWhiteSpace(key) || take <= 0)
        {
            return [];
        }

        lock (_sync)
        {
            if (!index.TryGetValue(key, out var entries))
            {
                return [];
            }

            return entries
                .Select(x => _ticketsById.GetValueOrDefault(x.TicketId))
                .Where(x => x is not null)
                .Cast<FoodDeliveryTicket>()
                .Where(x => string.Equals(x.Status, FoodDeliveryTicketStatus.Pending, StringComparison.Ordinal))
                .Take(take)
                .ToArray();
        }
    }

    private void AddToIndexes(FoodDeliveryTicket ticket)
    {
        var entry = CreateEntry(ticket);
        AddToIndex(_ticketsByRegion2, ticket.PickupRegion.Region2Key, entry);
        AddToIndex(_ticketsByRegion3, ticket.PickupRegion.Region3Key, entry);
    }

    private void RemoveFromIndexes(FoodDeliveryTicket ticket)
    {
        var entry = CreateEntry(ticket);
        RemoveFromIndex(_ticketsByRegion2, ticket.PickupRegion.Region2Key, entry);
        RemoveFromIndex(_ticketsByRegion3, ticket.PickupRegion.Region3Key, entry);
    }

    private static FoodDeliveryTicketIndexEntry CreateEntry(FoodDeliveryTicket ticket)
    {
        return new FoodDeliveryTicketIndexEntry(
            ticket.TicketId,
            ticket.PickupReadyAtUtc,
            ticket.CreatedAtUtc,
            ticket.PriorityScore);
    }

    private static void AddToIndex(
        Dictionary<string, SortedSet<FoodDeliveryTicketIndexEntry>> index,
        string key,
        FoodDeliveryTicketIndexEntry entry)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!index.TryGetValue(key, out var entries))
        {
            entries = new SortedSet<FoodDeliveryTicketIndexEntry>(FoodDeliveryTicketIndexEntryComparer.Instance);
            index[key] = entries;
        }

        entries.Add(entry);
    }

    private static void RemoveFromIndex(
        Dictionary<string, SortedSet<FoodDeliveryTicketIndexEntry>> index,
        string key,
        FoodDeliveryTicketIndexEntry entry)
    {
        if (string.IsNullOrWhiteSpace(key) || !index.TryGetValue(key, out var entries))
        {
            return;
        }

        entries.Remove(entry);
        if (entries.Count == 0)
        {
            index.Remove(key);
        }
    }
}
