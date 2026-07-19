using Ssalddel.Application.Food.Queries;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;
using MediatR;

namespace Ssalddel.Application.Food.Handlers;

public sealed class 음식주문목록조회QueryHandler(ISsalddelFoodOrderStore orderStore)
    : IRequestHandler<음식주문목록조회Query, 음식주문목록응답>
{
    public Task<음식주문목록응답> Handle(음식주문목록조회Query request, CancellationToken cancellationToken)
    {
        return Task.FromResult(orderStore.GetOrders());
    }
}

public sealed class 음식주문상세조회QueryHandler(ISsalddelFoodOrderStore orderStore)
    : IRequestHandler<음식주문상세조회Query, 음식주문응답?>
{
    public Task<음식주문응답?> Handle(음식주문상세조회Query request, CancellationToken cancellationToken)
    {
        return Task.FromResult(orderStore.GetOrder(request.주문번호));
    }
}
