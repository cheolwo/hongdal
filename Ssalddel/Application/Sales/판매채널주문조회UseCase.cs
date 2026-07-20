using FluentResults;
using Microsoft.AspNetCore.Http;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Services.LogisticsProcessing.SalesOrders;

namespace Ssalddel.Application.Sales;

public interface I판매채널주문조회UseCase
{
    Task<Result<판매채널주문목록응답>> 목록Async(
        판매채널주문목록조회요청 request,
        CancellationToken cancellationToken);

    Task<Result<판매채널주문상세응답>> 상세Async(
        long orderId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.SalesChannelFulfillment)]
[SsalddelUseCase(
    "판매채널 주문 출고 후보 조회",
    Summary = "외부 채널을 호출하지 않고 이미 영속된 판매채널 주문 출고 후보의 목록과 상세를 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Seller)]
[SsalddelUseCaseActor(SsalddelActor.WarehouseManager, SsalddelUseCaseActorRole.Supporting)]
public sealed class 판매채널주문조회UseCase(
    ISalesChannelOrderReadService service) : I판매채널주문조회UseCase
{
    public async Task<Result<판매채널주문목록응답>> 목록Async(
        판매채널주문목록조회요청 request,
        CancellationToken cancellationToken)
        => await service.QueryAsync(request, cancellationToken);

    public async Task<Result<판매채널주문상세응답>> 상세Async(
        long orderId,
        CancellationToken cancellationToken)
    {
        if (orderId <= 0)
        {
            return Result.Fail<판매채널주문상세응답>("조회할 판매채널 주문 ID를 확인해 주세요.");
        }

        var order = await service.GetAsync(orderId, cancellationToken);
        return order is null
            ? Result.Fail<판매채널주문상세응답>(new Error("판매채널 주문 출고 후보를 찾을 수 없습니다.")
                .WithMetadata("StatusCode", StatusCodes.Status404NotFound))
            : Result.Ok(order);
    }
}
