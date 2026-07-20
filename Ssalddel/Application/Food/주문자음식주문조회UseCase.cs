using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Participants;
using Ssalddel.Contracts.Food;
using 살뜰.Data;
using 살뜰.도메인.음식;

namespace Ssalddel.Application.Food;

public interface I주문자음식주문조회UseCase
{
    Task<Result<주문자음식주문목록응답>> 목록Async(
        주문자음식주문목록조회요청 request,
        string? ordererUserId,
        CancellationToken cancellationToken);

    Task<Result<주문자음식주문상세응답>> 상세Async(
        string orderNo,
        string? ordererUserId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.FoodDelivery)]
[SsalddelUseCase(
    "주문자 음식 주문 조회",
    Summary = "로그인한 주문자가 소유한 영속 음식 주문의 목록과 정확한 주문번호 상세만 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
public sealed class 주문자음식주문조회UseCase(
    SsalddelContext db) : I주문자음식주문조회UseCase
{
    public async Task<Result<주문자음식주문목록응답>> 목록Async(
        주문자음식주문목록조회요청 request,
        string? ordererUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = Clean(ordererUserId);
        if (userId is null)
        {
            return Unauthorized<주문자음식주문목록응답>();
        }

        var status = Clean(request.상태);
        if (status is not null && !음식주문상태코드.지원여부(status))
        {
            return Result.Fail<주문자음식주문목록응답>("조회할 음식 주문 상태를 확인해 주세요.");
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = db.음식주문
            .AsNoTracking()
            .Where(item => item.주문자UserId == userId);

        if (status is not null)
        {
            query = query.Where(item => item.상태 == status);
        }

        var search = Clean(request.검색어);
        if (search is not null)
        {
            query = query.Where(item =>
                item.주문번호.Contains(search)
                || item.음식점명.Contains(search)
                || item.상품목록.Any(product => product.상품명.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(item => item.상품목록)
            .ToArrayAsync(cancellationToken);

        return Result.Ok(new 주문자음식주문목록응답
        {
            Items = orders.Select(ToSummary).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<주문자음식주문상세응답>> 상세Async(
        string orderNo,
        string? ordererUserId,
        CancellationToken cancellationToken)
    {
        var userId = Clean(ordererUserId);
        if (userId is null)
        {
            return Unauthorized<주문자음식주문상세응답>();
        }

        var cleanOrderNo = Clean(orderNo);
        if (cleanOrderNo is null)
        {
            return Result.Fail<주문자음식주문상세응답>("조회할 음식 주문번호를 확인해 주세요.");
        }

        var order = await db.음식주문
            .AsNoTracking()
            .Include(item => item.상품목록)
            .Include(item => item.상태이력)
            .FirstOrDefaultAsync(
                item => item.주문번호 == cleanOrderNo && item.주문자UserId == userId,
                cancellationToken);
        if (order is null)
        {
            return NotFound<주문자음식주문상세응답>();
        }

        return Result.Ok(ToDetail(order));
    }

    private static 주문자음식주문요약응답 ToSummary(음식주문 order)
    {
        var products = order.상품목록.OrderBy(item => item.Id).ToArray();
        return new 주문자음식주문요약응답
        {
            주문번호 = order.주문번호,
            음식점Id = order.음식점Id,
            음식점명 = order.음식점명,
            상품요약 = BuildProductSummary(products),
            상품종류수 = products.Length,
            총수량 = products.Sum(item => item.수량),
            총주문금액 = order.총주문금액,
            상태 = 음식주문상태코드.Normalize(order.상태),
            배차상태 = order.배차상태,
            조리예상완료시각Utc = order.조리예상완료시각Utc,
            CreatedAtUtc = order.CreatedAt
        };
    }

    private static 주문자음식주문상세응답 ToDetail(음식주문 order)
        => new()
        {
            주문 = ToSummary(order),
            음식점주소 = order.음식점주소,
            음식점상세주소 = order.음식점상세주소,
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = order.수령인명,
                연락처 = order.수령인연락처,
                주소 = order.수령지주소,
                상세주소 = order.수령지상세주소,
                요청사항 = order.수령요청사항,
                주문자본인수령여부 = order.주문자본인수령여부
            },
            상품목록 = order.상품목록
                .OrderBy(item => item.Id)
                .Select(item => new 음식주문상품Dto
                {
                    상품명 = item.상품명,
                    수량 = item.수량,
                    단가 = item.단가
                })
                .ToArray(),
            결제수단 = order.결제수단,
            음식점수락시각Utc = order.음식점수락시각Utc,
            배차요청시각Utc = order.배차요청시각Utc,
            수락메모 = order.수락메모,
            상태이력 = order.상태이력
                .OrderBy(item => item.전이시각Utc)
                .ThenBy(item => item.Id)
                .Select(item => new 음식주문상태전이기록Dto
                {
                    이전상태 = item.이전상태,
                    다음상태 = item.다음상태,
                    사유 = item.사유,
                    전이시각Utc = item.전이시각Utc
                })
                .ToArray()
        };

    private static string BuildProductSummary(IReadOnlyList<음식주문상품> products)
    {
        if (products.Count == 0)
        {
            return "상품 정보 없음";
        }

        var names = products
            .Select(item => item.상품명)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Take(2)
            .ToArray();
        var summary = names.Length == 0 ? "상품 정보 없음" : string.Join(", ", names);
        return products.Count > 2 ? $"{summary} 외 {products.Count - 2}종" : summary;
    }

    private static Result<T> Unauthorized<T>()
        => Result.Fail<T>(new Error("음식 주문 내역을 보려면 로그인해 주세요.")
            .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));

    private static Result<T> NotFound<T>()
        => Result.Fail<T>(new Error("음식 주문을 찾을 수 없습니다.")
            .WithMetadata("StatusCode", StatusCodes.Status404NotFound));

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
