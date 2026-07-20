using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Mart;
using 살뜰.Data;
using 살뜰.도메인.창고;

namespace Ssalddel.Application.Mart;

public interface I마트피킹조회UseCase
{
    Task<Result<마트피킹주문목록응답>> 목록Async(
        마트피킹주문목록조회요청 request,
        CancellationToken cancellationToken);

    Task<Result<마트피킹주문상세응답>> 상세Async(
        long orderId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelUseCase(
    "마트 피킹 작업 조회",
    Summary = "로그인 사용자가 담당하거나 접근할 수 있는 창고의 마트 주문과 피킹·포장 작업만 읽습니다.")]
[SsalddelUseCaseActor(SsalddelActor.WarehouseManager)]
public sealed class 마트피킹조회UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : I마트피킹조회UseCase
{
    public async Task<Result<마트피킹주문목록응답>> 목록Async(
        마트피킹주문목록조회요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized<마트피킹주문목록응답>();
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var scopedTasks = 접근가능작업Query(userId);
        if (request.창고Id is > 0)
        {
            scopedTasks = scopedTasks.Where(task => task.창고Id == request.창고Id.Value);
        }

        var candidateTasks = scopedTasks;
        if (!string.IsNullOrWhiteSpace(request.작업상태))
        {
            var status = request.작업상태.Trim();
            candidateTasks = candidateTasks.Where(task => task.상태 == status);
        }

        var orders = db.마트주문
            .AsNoTracking()
            .Where(order => candidateTasks.Any(task => task.주문참조번호 == order.주문참조번호));

        if (!string.IsNullOrWhiteSpace(request.검색어))
        {
            var search = request.검색어.Trim();
            orders = orders.Where(order =>
                order.주문참조번호.Contains(search)
                || order.상태.Contains(search)
                || (order.현재단계 != null && order.현재단계.Contains(search))
                || order.상품목록.Any(item => item.상품명.Contains(search) || item.SKU.Contains(search))
                || candidateTasks.Any(task =>
                    task.주문참조번호 == order.주문참조번호
                    && (task.상품명.Contains(search) || task.SKU.Contains(search))));
        }

        var totalCount = await orders.CountAsync(cancellationToken);
        var pageOrders = await orders
            .Include(order => order.상품목록)
            .OrderByDescending(order => order.UpdatedAt)
            .ThenByDescending(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        var orderReferences = pageOrders
            .Select(order => order.주문참조번호)
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        피킹포장작업[] taskRows;
        if (orderReferences.Count == 0)
        {
            taskRows = [];
        }
        else
        {
            taskRows = await scopedTasks
                .Where(task => orderReferences.Contains(task.주문참조번호))
                .OrderBy(task => task.작업유형)
                .ThenBy(task => task.Id)
                .ToArrayAsync(cancellationToken);
        }
        var tasksByOrder = taskRows
            .GroupBy(task => task.주문참조번호, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        return Result.Ok(new 마트피킹주문목록응답
        {
            Items = pageOrders.Select(order => 요약생성(
                order,
                tasksByOrder.GetValueOrDefault(order.주문참조번호) ?? [])).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<마트피킹주문상세응답>> 상세Async(
        long orderId,
        CancellationToken cancellationToken)
    {
        if (orderId <= 0)
        {
            return Result.Fail<마트피킹주문상세응답>("조회할 마트 주문 ID를 확인해 주세요.");
        }

        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized<마트피킹주문상세응답>();
        }

        var scopedTasks = 접근가능작업Query(userId);
        var order = await db.마트주문
            .AsNoTracking()
            .Include(item => item.상품목록)
            .FirstOrDefaultAsync(item =>
                item.Id == orderId
                && scopedTasks.Any(task => task.주문참조번호 == item.주문참조번호),
                cancellationToken);
        if (order is null)
        {
            return NotFound<마트피킹주문상세응답>();
        }

        var tasks = await scopedTasks
            .Where(task => task.주문참조번호 == order.주문참조번호)
            .OrderBy(task => task.작업유형)
            .ThenBy(task => task.라인Key)
            .ThenBy(task => task.Id)
            .Select(task => new 마트피킹작업응답
            {
                작업Id = task.Id,
                작업Key = task.작업Key,
                작업유형 = task.작업유형,
                처리방식 = task.처리방식,
                상태 = task.상태,
                창고Id = task.창고Id,
                창고명 = task.창고명,
                작업자표시명 = task.작업자표시명,
                라인Key = task.라인Key,
                상품명 = task.상품명,
                SKU = task.SKU,
                수량 = task.수량,
                적재대코드 = task.적재대코드,
                보관위치코드 = task.보관위치코드,
                시작일시Utc = task.시작일시Utc,
                완료일시Utc = task.완료일시Utc,
                수정일시Utc = task.UpdatedAt
            })
            .ToArrayAsync(cancellationToken);

        return Result.Ok(new 마트피킹주문상세응답
        {
            주문Id = order.Id,
            주문참조번호 = order.주문참조번호,
            주문상태 = order.상태,
            현재단계 = order.현재단계,
            생성일시Utc = order.CreatedAt,
            수정일시Utc = order.UpdatedAt,
            상품목록 = order.상품목록
                .OrderBy(item => item.Id)
                .Select(item => new 마트피킹주문상품응답
                {
                    상품라인Id = item.Id,
                    상품명 = item.상품명,
                    SKU = item.SKU,
                    수량 = item.수량,
                    상태 = item.상태
                })
                .ToArray(),
            작업목록 = tasks
        });
    }

    private IQueryable<피킹포장작업> 접근가능작업Query(string userId)
    {
        var query = db.피킹포장작업.AsNoTracking();
        if (string.Equals(currentUserAccessor.Role, 역할명.서버관리자, StringComparison.OrdinalIgnoreCase))
        {
            return query;
        }

        return query.Where(task =>
            task.작업자UserId == userId
            || task.상대작업자UserId == userId
            || db.창고.Any(warehouse =>
                warehouse.Id == task.창고Id && warehouse.소유자UserId == userId)
            || db.창고사용자.Any(warehouseUser =>
                warehouseUser.창고Id == task.창고Id && warehouseUser.UserId == userId));
    }

    private static 마트피킹주문요약응답 요약생성(
        살뜰.도메인.마트.마트주문 order,
        IReadOnlyCollection<피킹포장작업> tasks)
    {
        var completedTasks = tasks
            .Where(task => string.Equals(task.상태, 피킹포장작업상태.완료, StringComparison.Ordinal))
            .ToArray();
        var latestTaskUpdate = tasks.Count == 0
            ? order.UpdatedAt
            : tasks.Max(task => task.UpdatedAt);

        return new 마트피킹주문요약응답
        {
            주문Id = order.Id,
            주문참조번호 = order.주문참조번호,
            주문상태 = order.상태,
            현재단계 = order.현재단계,
            상품종류수 = order.상품목록.Count,
            주문수량 = order.상품목록.Sum(item => item.수량),
            작업수 = tasks.Count,
            완료작업수 = completedTasks.Length,
            작업수량 = tasks.Sum(task => task.수량),
            완료작업수량 = completedTasks.Sum(task => task.수량),
            창고목록 = tasks
                .Select(task => string.IsNullOrWhiteSpace(task.창고명) ? $"창고 #{task.창고Id}" : task.창고명.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray(),
            최근수정일시Utc = latestTaskUpdate > order.UpdatedAt ? latestTaskUpdate : order.UpdatedAt
        };
    }

    private static Result<T> Unauthorized<T>()
        => Result.Fail<T>(new Error("로그인 사용자 인증 정보가 필요합니다.")
            .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));

    private static Result<T> NotFound<T>()
        => Result.Fail<T>(new Error("마트 주문을 찾을 수 없거나 현재 계정의 창고 조회 범위에 없습니다.")
            .WithMetadata("StatusCode", StatusCodes.Status404NotFound));
}
