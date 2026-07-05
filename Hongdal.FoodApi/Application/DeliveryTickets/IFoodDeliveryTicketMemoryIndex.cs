namespace Hongdal.FoodApi.Application.DeliveryTickets;

public interface IFoodDeliveryTicketMemoryIndex
{
    void AddOrUpdate(FoodDeliveryTicket ticket);
    FoodDeliveryTicket? GetById(string ticketId);
    IReadOnlyList<FoodDeliveryTicket> GetByRegion3(string region3Key, int take = 20);
    IReadOnlyList<FoodDeliveryTicket> GetByRegion2(string region2Key, int take = 20);
    IReadOnlyList<FoodDeliveryTicket> GetPendingByRegion(AddressRegionKey region, int take = 20);
}
