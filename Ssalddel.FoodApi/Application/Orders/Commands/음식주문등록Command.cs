using Ssalddel.Contracts.Food;
using Ssalddel.FoodApi.Application;

namespace Ssalddel.FoodApi.Application.Orders.Commands;

public sealed record 음식주문등록Command(음식주문등록요청 Payload)
    : IFoodCommand<음식주문응답>;
