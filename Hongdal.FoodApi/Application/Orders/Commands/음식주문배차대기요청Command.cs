using Hongdal.Contracts.Food;
using Hongdal.FoodApi.Application;

namespace Hongdal.FoodApi.Application.Orders.Commands;

public sealed record 음식주문배차대기요청Command(string 주문번호)
    : IFoodCommand<음식주문응답?>;
