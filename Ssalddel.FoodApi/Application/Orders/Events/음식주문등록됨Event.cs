using Ssalddel.Contracts.Food;

namespace Ssalddel.FoodApi.Application.Orders.Events;

public sealed record 음식주문등록됨Event(
    음식주문응답 주문,
    DateTime 발생시각Utc,
    string EventId);
