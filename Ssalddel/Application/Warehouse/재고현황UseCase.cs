using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.ViewSettings;
using 살뜰.Data;
using 살뜰.도메인.창고;

namespace Ssalddel.Application.Warehouse;

public interface I재고현황UseCase
{
    Task<Result<창고재고현황목록페이지응답>> 목록Async(
        창고재고현황목록조회요청 request,
        CancellationToken cancellationToken);

    Task<Result<창고재고현황상세응답>> 상세Async(
        long inboundItemId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelUseCase(
    "창고 재고 현황",
    Summary = "현재 계정의 창고 작업 범위 안에서 최소 재고 목록과 명시한 입고상품 상세를 읽습니다.")]
[SsalddelUseCaseActor(SsalddelActor.WarehouseManager)]
public sealed class 재고현황UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : I재고현황UseCase
{
    public async Task<Result<창고재고현황목록페이지응답>> 목록Async(
        창고재고현황목록조회요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized<창고재고현황목록페이지응답>();
        }

        var page = Math.Max(0, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var status = 창고재고조회상태코드.Normalize(request.Status);
        var query = 접근가능재고Query(userId).AsNoTracking();

        if (request.WarehouseId is > 0)
        {
            query = query.Where(item => item.창고Id == request.WarehouseId.Value);
        }

        query = status switch
        {
            창고재고조회상태코드.가용 => query.Where(item => item.가용수량 > 0),
            창고재고조회상태코드.예약 => query.Where(item => item.예약수량 > 0),
            창고재고조회상태코드.위치미배정 => query.Where(item => item.보관위치 == string.Empty),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(item =>
                item.상품명.Contains(search)
                || item.SKU.Contains(search)
                || item.옵션명.Contains(search)
                || item.보관위치.Contains(search)
                || db.입고요청.Any(inbound =>
                    inbound.Id == item.입고요청Id
                    && (inbound.주문참조번호.Contains(search) || inbound.입고묶음바코드.Contains(search))));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalAvailable = await query.SumAsync(item => (int?)item.가용수량, cancellationToken) ?? 0;
        var totalReserved = await query.SumAsync(item => (int?)item.예약수량, cancellationToken) ?? 0;
        var unassignedLocations = await query.CountAsync(item => item.보관위치 == string.Empty, cancellationToken);

        var items = await (
                from item in query
                join warehouse in db.창고.AsNoTracking() on item.창고Id equals warehouse.Id
                orderby item.보관위치 == string.Empty descending, item.보관위치, item.상품명, item.Id
                select new 창고재고현황목록항목응답
                {
                    InboundItemId = item.Id,
                    WarehouseId = item.창고Id,
                    WarehouseName = warehouse.창고명,
                    ProductName = item.상품명,
                    Sku = item.SKU,
                    OptionName = item.옵션명,
                    AvailableQuantity = item.가용수량,
                    ReservedQuantity = item.예약수량,
                    StorageLocation = item.보관위치,
                    Status = item.상태,
                    HasCommunityLedger = item.커뮤니티원장Id != null && item.커뮤니티원장Id != string.Empty,
                    UpdatedAtUtc = AsUtc(item.UpdatedAt)
                })
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return Result.Ok(new 창고재고현황목록페이지응답
        {
            Items = items,
            TotalCount = totalCount,
            TotalAvailableQuantity = totalAvailable,
            TotalReservedQuantity = totalReserved,
            UnassignedLocationCount = unassignedLocations,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<창고재고현황상세응답>> 상세Async(
        long inboundItemId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized<창고재고현황상세응답>();
        }

        if (inboundItemId <= 0)
        {
            return NotFound<창고재고현황상세응답>();
        }

        var detail = await (
                from item in 접근가능재고Query(userId).AsNoTracking()
                join warehouse in db.창고.AsNoTracking() on item.창고Id equals warehouse.Id
                join inbound in db.입고요청.AsNoTracking() on item.입고요청Id equals inbound.Id
                where item.Id == inboundItemId
                select new 창고재고현황상세응답
                {
                    InboundItemId = item.Id,
                    InboundId = item.입고요청Id,
                    WarehouseId = item.창고Id,
                    WarehouseName = warehouse.창고명,
                    ProductName = item.상품명,
                    Sku = item.SKU,
                    OptionName = item.옵션명,
                    InboundQuantity = item.입고수량,
                    AvailableQuantity = item.가용수량,
                    ReservedQuantity = item.예약수량,
                    DefectiveQuantity = item.불량수량,
                    StorageLocation = item.보관위치,
                    StorageCondition = inbound.보관조건,
                    Status = item.상태,
                    OrderReference = inbound.주문참조번호,
                    InboundFlowType = inbound.입고흐름유형,
                    InboundPath = inbound.입고생성경로,
                    BundleBarcode = inbound.입고묶음바코드,
                    CommunityLedgerId = item.커뮤니티원장Id,
                    CommunityLedgerTemplateKey = item.커뮤니티원장템플릿Key,
                    CommunityLedgerState = item.커뮤니티원장상태,
                    ReceivedAtUtc = AsUtc(item.입고완료일시),
                    UpdatedAtUtc = AsUtc(item.UpdatedAt)
                })
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null ? NotFound<창고재고현황상세응답>() : Result.Ok(detail);
    }

    private IQueryable<입고상품> 접근가능재고Query(string userId)
    {
        var query = db.입고상품.AsQueryable();
        if (string.Equals(currentUserAccessor.Role, 역할명.서버관리자, StringComparison.OrdinalIgnoreCase))
        {
            return query;
        }

        return query.Where(item =>
            db.창고.Any(warehouse => warehouse.Id == item.창고Id && warehouse.소유자UserId == userId)
            || db.창고사용자.Any(warehouseUser =>
                warehouseUser.창고Id == item.창고Id && warehouseUser.UserId == userId));
    }

    private string? CurrentUserId()
    {
        var userId = currentUserAccessor.UserId?.Trim();
        return string.IsNullOrWhiteSpace(userId) ? null : userId;
    }

    private static DateTime AsUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static DateTime? AsUtc(DateTime? value)
        => value.HasValue ? AsUtc(value.Value) : null;

    private static Result<T> Unauthorized<T>()
        => Result.Fail<T>(new Error("로그인 사용자 인증 정보가 필요합니다.")
            .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));

    private static Result<T> NotFound<T>()
        => Result.Fail<T>(new Error("재고를 찾을 수 없거나 현재 계정의 창고 작업 범위에 없습니다.")
            .WithMetadata("StatusCode", StatusCodes.Status404NotFound));
}
