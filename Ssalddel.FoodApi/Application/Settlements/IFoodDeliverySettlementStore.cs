namespace Ssalddel.FoodApi.Application.Settlements;

public interface IFoodDeliverySettlementStore
{
    FoodDeliverySettlementEntry AddOrReplace(FoodDeliverySettlementEntry entry);
    FoodDeliverySettlementSummary GetDaily(string driverId, DateOnly date);
    FoodDeliverySettlementSummary GetWeekly(string driverId, DateOnly anyDateInWeek);
}
