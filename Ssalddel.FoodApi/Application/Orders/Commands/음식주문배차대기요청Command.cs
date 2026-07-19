using Ssalddel.Contracts.Food;
using Ssalddel.FoodApi.Application;

namespace Ssalddel.FoodApi.Application.Orders.Commands;

public sealed record 음식주문배차대기요청Command(string 주문번호)
    : IFoodCommand<음식주문응답?>;
