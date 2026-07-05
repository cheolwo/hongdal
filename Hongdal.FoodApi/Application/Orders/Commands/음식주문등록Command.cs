using Hongdal.Contracts.Food;
using Hongdal.FoodApi.Application;

namespace Hongdal.FoodApi.Application.Orders.Commands;

public sealed record 음식주문등록Command(음식주문등록요청 Payload)
    : IFoodCommand<음식주문응답>;
