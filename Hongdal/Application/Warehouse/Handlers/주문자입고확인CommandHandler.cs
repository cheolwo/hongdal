using FluentResults;
using 홍달.도메인.창고;

namespace Hongdal.Application.Warehouse;

public sealed class 주문자입고확인CommandHandler : IRequestHandler<주문자입고확인Command, Result<Unit>>
{
    private readonly HongdalContext _db;
    private readonly IPublisher _publisher;

    public 주문자입고확인CommandHandler(HongdalContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<Result<Unit>> Handle(주문자입고확인Command request, CancellationToken cancellationToken)
    {
        var query = _db.입고요청
            .Where(x =>
                x.주문자UserId == request.주문자UserId &&
                (x.상태 == 입고상태.운송중 || x.상태 == 입고상태.예정));

        query = request.주문Id.HasValue
            ? query.Where(x => x.주문Id == request.주문Id)
            : query.Where(x => x.주문참조번호 == request.주문참조번호);

        var 입고목록 = await query.ToListAsync(cancellationToken);
        if (입고목록.Count == 0)
        {
            return Result.Fail<Unit>("입고 확인할 예정 항목이 없습니다.");
        }

        var now = DateTime.UtcNow;
        foreach (var 입고 in 입고목록)
        {
            var 출고 = 입고.출고예정Id is null
                ? null
                : await _db.출고예정.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 입고.출고예정Id, cancellationToken);

            입고.상태 = 입고상태.입고완료;
            입고.입고완료일시 = now;
            입고.UpdatedAt = now;

            var 입고상품 = new 입고상품
            {
                입고요청Id = 입고.Id,
                창고Id = 입고.창고Id,
                소유자UserId = 입고.주문자UserId,
                판매자UserId = 입고.판매자UserId,
                상품명 = 출고?.상품명 ?? "주문 입고 상품",
                SKU = 출고?.SKU ?? 입고.주문참조번호,
                입고수량 = 출고?.수량 ?? 1,
                가용수량 = 출고?.수량 ?? 1,
                상태 = "보관중",
                입고완료일시 = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.입고상품.Add(입고상품);
            await _db.SaveChangesAsync(cancellationToken);

            _db.재고이동.Add(new 재고이동
            {
                창고Id = 입고.창고Id,
                입고상품Id = 입고상품.Id,
                판매상품Id = 출고?.판매상품Id,
                상품명 = 입고상품.상품명,
                SKU = 입고상품.SKU,
                이동유형 = 재고이동유형.입고,
                수량 = 입고상품.가용수량,
                주문Id = 입고.주문Id,
                주문참조번호 = 입고.주문참조번호,
                출고예정Id = 입고.출고예정Id,
                입고요청Id = 입고.Id,
                운송의뢰Id = 입고.운송의뢰Id,
                처리UserId = request.주문자UserId,
                메모 = "주문자 입고 확인",
                발생일시 = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new 주문자상품입고완료됨Event(
                입고목록.First().주문Id,
                입고목록.First().주문참조번호,
                request.주문자UserId,
                입고목록.Select(x => x.Id).ToArray(),
                now,
                System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty),
            cancellationToken);

        return Result.Ok(Unit.Value);
    }
}
