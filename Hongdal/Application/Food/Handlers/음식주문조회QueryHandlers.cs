using Hongdal.Application.Food.Queries;
using Hongdal.Contracts.Food;
using Hongdal.Services.Food;
using MediatR;

namespace Hongdal.Application.Food.Handlers;

public sealed class 음식주문목록조회QueryHandler(IHongdalFoodOrderStore orderStore)
    : IRequestHandler<음식주문목록조회Query, 음식주문목록응답>
{
    public Task<음식주문목록응답> Handle(음식주문목록조회Query request, CancellationToken cancellationToken)
    {
        return Task.FromResult(orderStore.GetOrders());
    }
}

public sealed class 음식주문상세조회QueryHandler(IHongdalFoodOrderStore orderStore)
    : IRequestHandler<음식주문상세조회Query, 음식주문응답?>
{
    public Task<음식주문응답?> Handle(음식주문상세조회Query request, CancellationToken cancellationToken)
    {
        return Task.FromResult(orderStore.GetOrder(request.주문번호));
    }
}
