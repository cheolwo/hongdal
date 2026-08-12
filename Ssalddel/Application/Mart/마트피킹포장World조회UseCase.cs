using FluentResults;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Mart;
using Ssalddel.Application.Warehouse;

namespace Ssalddel.Application.Mart;

public interface I마트피킹포장World조회UseCase
{
    Task<Result<MarketPickingPackingWorldSnapshotResponse>> 조회Async(
        long? warehouseId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelUseCase(
    "마트 피킹·포장 World 조회",
    Summary = "권한 범위 안의 마트 주문 작업을 상품 위치, 피킹 이동과 포장 인계용 공간 상태로 해석합니다.")]
[SsalddelUseCaseActor(SsalddelActor.WarehouseManager)]
public sealed class 마트피킹포장World조회UseCase(
    I마트피킹조회UseCase martPickingReader,
    I창고WorldSnapshot조회UseCase warehouseWorldReader,
    마트피킹포장WorldProjector projector) : I마트피킹포장World조회UseCase
{
    private const int MaximumOrders = 50;

    public async Task<Result<MarketPickingPackingWorldSnapshotResponse>> 조회Async(
        long? warehouseId,
        CancellationToken cancellationToken)
    {
        if (warehouseId is null or <= 0)
        {
            return Result.Fail<MarketPickingPackingWorldSnapshotResponse>("WarehouseIdInvalid");
        }

        var warehouseResult = await warehouseWorldReader.조회Async(
            warehouseId,
            cancellationToken);
        if (warehouseResult.IsFailed)
        {
            return Result.Fail<MarketPickingPackingWorldSnapshotResponse>(warehouseResult.Errors);
        }

        var listResult = await martPickingReader.목록Async(new 마트피킹주문목록조회요청
        {
            창고Id = warehouseId,
            Page = 1,
            PageSize = MaximumOrders
        }, cancellationToken);
        if (listResult.IsFailed)
        {
            return Result.Fail<MarketPickingPackingWorldSnapshotResponse>(listResult.Errors);
        }

        var details = new List<마트피킹주문상세응답>(listResult.Value.Items.Count);
        foreach (var order in listResult.Value.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var detailResult = await martPickingReader.상세Async(
                order.주문Id,
                cancellationToken);
            if (detailResult.IsFailed)
            {
                return Result.Fail<MarketPickingPackingWorldSnapshotResponse>(detailResult.Errors);
            }

            details.Add(detailResult.Value);
        }

        return Result.Ok(projector.Project(
            warehouseId.Value,
            listResult.Value.TotalCount,
            MaximumOrders,
            details,
            warehouseResult.Value,
            DateTimeOffset.UtcNow));
    }
}
