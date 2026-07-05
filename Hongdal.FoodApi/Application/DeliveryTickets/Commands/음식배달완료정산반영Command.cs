using Hongdal.FoodApi.Application;
using Hongdal.FoodApi.Application.Settlements;

namespace Hongdal.FoodApi.Application.DeliveryTickets.Commands;

public sealed record 음식배달완료정산반영Command(
    string TicketId,
    string DriverId,
    int? ActualDistanceMeters,
    DateTime? CompletedAtUtc) : IFoodCommand<FoodDeliverySettlementApplyResult>;
