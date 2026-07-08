using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Inventory;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Services.LogisticsProcessing.Warehouse;
using 홍달.Services.Audit;

namespace Hongdal.Application.Warehouse;

public interface I창고작업UseCase
{
    Task<Result<창고목록응답>> 창고목록Async(CancellationToken cancellationToken);
    Task<Result<창고요약응답>> 창고생성Async(창고저장요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
    Task<Result<창고사용자목록응답>> 창고사용자목록Async(long warehouseId, CancellationToken cancellationToken);
    Task<Result<창고사용자항목응답>> 창고사용자추가Async(long warehouseId, 창고사용자저장요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
    Task<Result<입고요청목록응답>> 입고목록Async(CancellationToken cancellationToken);
    Task<Result<입고요청항목응답>> 입고생성Async(입고요청저장요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
    Task<Result<입고상품목록응답>> 입고완료Async(long inboundId, 입고완료요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
    Task<Result<재고목록응답>> 재고목록Async(CancellationToken cancellationToken);
    Task<Result<창고작업결과응답>> 입고검수Async(long inboundItemId, 입고검수요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
    Task<Result<창고작업결과응답>> 적재위치배정Async(long inboundItemId, 적재위치배정요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
    Task<Result<창고작업결과응답>> 포장작업Async(long inboundItemId, 포장작업요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
    Task<Result<화주운송의뢰응답>> 재위탁운송생성Async(재고운송의뢰생성요청 request, 창고작업요청Context context, CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.WarehouseFulfillment)]
[HongdalUseCase("창고 입출고 작업", Summary = "창고 생성, 입고, 검수, 적재, 포장, 재위탁 운송 생성을 처리합니다.")]
[HongdalUseCaseActor(HongdalActor.WarehouseManager)]
[HongdalUseCaseActor(HongdalActor.ShipperOrSeller, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.Shipper, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "화주운송의뢰UseCase",
    Condition = "창고 출고 또는 재위탁 운송이 필요한 경우",
    Summary = "창고 작업 결과를 국내 화물 운송 의뢰로 확장합니다.")]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "판매채널UseCase",
    Condition = "창고 재고가 판매채널 주문 이행에 쓰이는 경우",
    Summary = "창고 재고와 출고 작업을 판매채널 주문 이행 흐름으로 확장합니다.")]
public sealed class 창고작업UseCase : I창고작업UseCase
{
    private readonly IWarehouseOperationService _warehouseOperationService;
    private readonly I사용자행위로그Service _activityLogService;

    public 창고작업UseCase(
        IWarehouseOperationService warehouseOperationService,
        I사용자행위로그Service activityLogService)
    {
        _warehouseOperationService = warehouseOperationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<창고목록응답>> 창고목록Async(CancellationToken cancellationToken)
        => await _warehouseOperationService.GetWarehousesAsync(cancellationToken);

    public async Task<Result<창고요약응답>> 창고생성Async(창고저장요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateWarehouseAsync(request, cancellationToken);
        await 로그Async("Warehouse", "Created", context, cancellationToken, entityId: result.Id);
        return result;
    }

    public async Task<Result<창고사용자목록응답>> 창고사용자목록Async(long warehouseId, CancellationToken cancellationToken)
        => await _warehouseOperationService.GetWarehouseUsersAsync(warehouseId, cancellationToken);

    public async Task<Result<창고사용자항목응답>> 창고사용자추가Async(long warehouseId, 창고사용자저장요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.AddWarehouseUserAsync(warehouseId, request, cancellationToken);
        await 로그Async("WarehouseUser", "Added", context, cancellationToken, metadataJson: $"{{\"warehouseId\":{warehouseId},\"userId\":\"{result.UserId}\"}}");
        return result;
    }

    public async Task<Result<입고요청목록응답>> 입고목록Async(CancellationToken cancellationToken)
        => await _warehouseOperationService.GetInboundsAsync(cancellationToken);

    public async Task<Result<입고요청항목응답>> 입고생성Async(입고요청저장요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateInboundAsync(request, cancellationToken);
        await 로그Async("Inbound", "Created", context, cancellationToken, entityId: result.Id, metadataJson: $"{{\"warehouseId\":{result.창고Id}}}");
        return result;
    }

    public async Task<Result<입고상품목록응답>> 입고완료Async(long inboundId, 입고완료요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CompleteInboundAsync(inboundId, request, cancellationToken);
        await 로그Async("Inbound", "Completed", context, cancellationToken, entityId: inboundId, metadataJson: $"{{\"createdItems\":{result.Items.Count}}}");
        return result;
    }

    public async Task<Result<재고목록응답>> 재고목록Async(CancellationToken cancellationToken)
        => await _warehouseOperationService.GetInventoryAsync(cancellationToken);

    public async Task<Result<창고작업결과응답>> 입고검수Async(long inboundItemId, 입고검수요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.InspectInboundItemAsync(inboundItemId, request, cancellationToken);
        await 로그Async("WarehouseWork", "InboundInspected", context, cancellationToken, entityId: inboundItemId, metadataJson: $"{{\"available\":{result.가용수량},\"defect\":{result.불량수량}}}");
        return result;
    }

    public async Task<Result<창고작업결과응답>> 적재위치배정Async(long inboundItemId, 적재위치배정요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.PutAwayInventoryItemAsync(inboundItemId, request, cancellationToken);
        await 로그Async("WarehouseWork", "PutAwayCompleted", context, cancellationToken, entityId: inboundItemId, metadataJson: $"{{\"location\":\"{result.보관위치}\"}}");
        return result;
    }

    public async Task<Result<창고작업결과응답>> 포장작업Async(long inboundItemId, 포장작업요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.PackInventoryItemAsync(inboundItemId, request, cancellationToken);
        await 로그Async("WarehouseWork", "Packed", context, cancellationToken, entityId: inboundItemId, metadataJson: $"{{\"quantity\":{request.포장수량}}}");
        return result;
    }

    public async Task<Result<화주운송의뢰응답>> 재위탁운송생성Async(재고운송의뢰생성요청 request, 창고작업요청Context context, CancellationToken cancellationToken)
    {
        var result = await _warehouseOperationService.CreateReconsignmentRequestAsync(request, cancellationToken);
        await 로그Async("Reconsignment", "Created", context, cancellationToken, metadataJson: $"{{\"requestId\":\"{result.의뢰Id}\",\"inventoryItemId\":{request.입고상품Id},\"quantity\":{request.요청수량}}}");
        return result;
    }

    private async Task 로그Async(
        string actionType,
        string actionName,
        창고작업요청Context context,
        CancellationToken cancellationToken,
        long entityId = 0,
        string? metadataJson = null)
    {
        await _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = context.AppKey,
            UserId = context.UserId,
            UserName = context.UserName,
            RoleName = context.RoleName,
            ActionType = actionType,
            ActionName = actionName,
            Route = context.Route,
            TraceId = context.TraceId,
            IsSuccess = true,
            ClientIp = context.ClientIp,
            UserAgent = context.UserAgent,
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = metadataJson ?? $"{{\"entityId\":{entityId}}}"
        }, cancellationToken);
    }
}

public sealed record 창고작업요청Context(
    string AppKey,
    string UserId,
    string UserName,
    string RoleName,
    string Route,
    string TraceId,
    string ClientIp,
    string UserAgent);
