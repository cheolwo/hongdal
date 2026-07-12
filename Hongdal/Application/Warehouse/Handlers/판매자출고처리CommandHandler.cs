using FluentResults;
using Hongdal.Services.Community;
using 홍달.도메인.창고;

namespace Hongdal.Application.Warehouse;

public sealed class 판매자출고처리CommandHandler : IRequestHandler<판매자출고처리Command, Result<Unit>>
{
    private readonly HongdalContext _db;
    private readonly IPublisher _publisher;
    private readonly I음식마트원장Mongo동기화Service _ledgerSync;

    public 판매자출고처리CommandHandler(
        HongdalContext db,
        IPublisher publisher,
        I음식마트원장Mongo동기화Service ledgerSync)
    {
        _db = db;
        _publisher = publisher;
        _ledgerSync = ledgerSync;
    }

    public async Task<Result<Unit>> Handle(판매자출고처리Command request, CancellationToken cancellationToken)
    {
        var query = _db.출고예정
            .Where(x =>
                x.판매자UserId == request.판매자UserId &&
                x.상태 == 출고상태.예정);

        query = request.주문Id.HasValue
            ? query.Where(x => x.주문Id == request.주문Id)
            : query.Where(x => x.주문참조번호 == request.주문참조번호);

        var 출고목록 = await query.ToListAsync(cancellationToken);
        if (출고목록.Count == 0)
        {
            return Result.Fail<Unit>("출고 처리할 예정 항목이 없습니다.");
        }

        var now = DateTime.UtcNow;
        foreach (var 출고 in 출고목록)
        {
            출고.상태 = 출고상태.출고완료;
            출고.출고처리일시 = now;
            출고.UpdatedAt = now;

            _db.재고이동.Add(new 재고이동
            {
                창고Id = 출고.출고창고Id,
                입고상품Id = 출고.입고상품Id,
                판매상품Id = 출고.판매상품Id,
                상품명 = 출고.상품명,
                SKU = 출고.SKU,
                이동유형 = 재고이동유형.출고,
                수량 = 출고.수량,
                주문Id = 출고.주문Id,
                주문참조번호 = 출고.주문참조번호,
                출고예정Id = 출고.Id,
                입고요청Id = 출고.입고요청Id,
                운송의뢰Id = 출고.운송의뢰Id,
                처리UserId = request.판매자UserId,
                메모 = "판매자 출고 처리",
                발생일시 = now
            });

            if (출고.입고요청Id is not null)
            {
                var 입고 = await _db.입고요청.FirstOrDefaultAsync(x => x.Id == 출고.입고요청Id, cancellationToken);
                if (입고 is not null && 입고.상태 == 입고상태.예정)
                {
                    입고.상태 = 입고상태.운송중;
                    입고.UpdatedAt = now;
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new 판매자상품출고됨Event(
                출고목록.First().주문Id,
                출고목록.First().주문참조번호,
                request.판매자UserId,
                출고목록.Select(x => x.Id).ToArray(),
                now,
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty),
            cancellationToken);

        var 입고Ids = 출고목록
            .Select(x => x.입고요청Id)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();
        List<입고요청> 입고목록 = 입고Ids.Length == 0
            ? []
            : await _db.입고요청.Where(x => 입고Ids.Contains(x.Id)).ToListAsync(cancellationToken);

        await _ledgerSync.출고원장동기화Async(
            출고목록,
            입고목록,
            request.판매자UserId,
            "출고 완료",
            cancellationToken: cancellationToken);

        return Result.Ok(Unit.Value);
    }
}
