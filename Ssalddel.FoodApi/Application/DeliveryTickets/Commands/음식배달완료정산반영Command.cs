using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.Settlements;

namespace Ssalddel.FoodApi.Application.DeliveryTickets.Commands;

public sealed record 음식배달완료정산반영Command(
    string TicketId,
    string DriverId,
    int? ActualDistanceMeters,
    DateTime? CompletedAtUtc) : IFoodCommand<FoodDeliverySettlementApplyResult>;
