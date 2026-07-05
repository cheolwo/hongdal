namespace Hongdal.FoodApi.Application.DeliveryTickets;

public sealed record FoodDeliveryTicketIndexEntry(
    string TicketId,
    DateTime PickupReadyAtUtc,
    DateTime CreatedAtUtc,
    decimal PriorityScore);

public sealed class FoodDeliveryTicketIndexEntryComparer : IComparer<FoodDeliveryTicketIndexEntry>
{
    public static readonly FoodDeliveryTicketIndexEntryComparer Instance = new();

    public int Compare(FoodDeliveryTicketIndexEntry? x, FoodDeliveryTicketIndexEntry? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var pickupCompare = x.PickupReadyAtUtc.CompareTo(y.PickupReadyAtUtc);
        if (pickupCompare != 0)
        {
            return pickupCompare;
        }

        var priorityCompare = y.PriorityScore.CompareTo(x.PriorityScore);
        if (priorityCompare != 0)
        {
            return priorityCompare;
        }

        var createdCompare = x.CreatedAtUtc.CompareTo(y.CreatedAtUtc);
        if (createdCompare != 0)
        {
            return createdCompare;
        }

        return string.CompareOrdinal(x.TicketId, y.TicketId);
    }
}
