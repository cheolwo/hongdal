using FluentResults;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Services.LogisticsProcessing.SalesOrders;
using 살뜰.Services;

namespace Ssalddel.Application.Sales;

public interface I판매채널주문동기화UseCase
{
    Task<Result<판매채널주문동기화응답>> 실행Async(
        판매채널주문동기화요청? request,
        CancellationToken cancellationToken = default);
}

public sealed class 판매채널주문동기화UseCase(
    ISalesChannelOrderSyncService syncService,
    ISsalddelExecutionModePolicy executionMode) : I판매채널주문동기화UseCase
{
    public async Task<Result<판매채널주문동기화응답>> 실행Async(
        판매채널주문동기화요청? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Result.Fail<판매채널주문동기화응답>("request body is required");
        }

        if (!executionMode.IsOperational)
        {
            return Result.Fail<판매채널주문동기화응답>(
                "판매채널 주문 동기화는 Operational 실행 모드에서만 허용됩니다.");
        }

        var result = await syncService.SyncAsync(request.SyncScope, cancellationToken);
        return Result.Ok(new 판매채널주문동기화응답
        {
            SyncScope = result.SyncScope,
            AccountCount = result.AccountCount,
            FetchedOrderCount = result.FetchedOrderCount,
            CreatedOutboundCount = result.CreatedOutboundCount,
            SkippedOrderCount = result.SkippedOrderCount
        });
    }
}
