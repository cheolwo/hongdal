using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Warehouse;

public sealed class 공개상품이력감사대상조회QueryHandler : IRequestHandler<공개상품이력감사대상조회Query, IReadOnlyList<감사대상응답>>
{
    private readonly HongdalContext _db;

    public 공개상품이력감사대상조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<감사대상응답>> Handle(공개상품이력감사대상조회Query request, CancellationToken cancellationToken)
    {
        var results = new List<감사대상응답>();

        var 출고 = request.주문Id is null
            ? null
            : await _db.출고예정.AsNoTracking().FirstOrDefaultAsync(x => x.주문Id == request.주문Id, cancellationToken);

        var 입고 = request.주문Id is null
            ? null
            : await _db.입고요청.AsNoTracking().FirstOrDefaultAsync(x => x.주문Id == request.주문Id, cancellationToken);

        if (출고 is not null)
        {
            results.Add(new 감사대상응답
            {
                대상역할 = "판매자",
                대상참여자Id = 출고.판매자UserId,
                대상표시명 = "판매 파트너",
                역할설명 = "상품을 소개하고 판매를 책임졌어요.",
                처리일시 = 출고.출고처리일시 is null ? null : new DateTimeOffset(DateTime.SpecifyKind(출고.출고처리일시.Value, DateTimeKind.Utc))
            });

            results.Add(new 감사대상응답
            {
                대상역할 = "입고/포장 담당자",
                대상참여자Id = 출고.주문자UserId,
                대상표시명 = "홍달 물류 파트너",
                역할설명 = "상품이 안전하게 보관되도록 처리했어요.",
                처리일시 = 출고.UpdatedAt == default ? null : new DateTimeOffset(DateTime.SpecifyKind(출고.UpdatedAt, DateTimeKind.Utc))
            });
        }

        if (입고 is not null)
        {
            results.Add(new 감사대상응답
            {
                대상역할 = "입고 담당자",
                대상참여자Id = 입고.판매자UserId,
                대상표시명 = "홍달 입고 파트너",
                역할설명 = "입고 처리와 상태 확인을 담당했어요.",
                처리일시 = 입고.입고완료일시 is null ? null : new DateTimeOffset(DateTime.SpecifyKind(입고.입고완료일시.Value, DateTimeKind.Utc))
            });
        }

        if (request.통관절차Id is not null)
        {
            var 통관수임목록 = await _db.통관수임.AsNoTracking()
                .Where(x => x.통관절차Id == request.통관절차Id.Value)
                .Where(x => x.관세사참여자Id != null)
                .OrderByDescending(x => x.확정시각 ?? x.요청시각)
                .ToListAsync(cancellationToken);

            foreach (var item in 통관수임목록)
            {
                results.Add(new 감사대상응답
                {
                    대상역할 = "관세사",
                    대상참여자Id = item.관세사참여자Id,
                    대상표시명 = "홍달 통관 파트너",
                    역할설명 = "통관 절차 검토와 진행을 도왔어요.",
                    처리일시 = item.확정시각 ?? item.요청시각
                });
            }
        }

        results.Add(new 감사대상응답
        {
            대상역할 = "전체관계자",
            대상표시명 = "상품 준비 관계자 전체",
            역할설명 = "이 상품이 오기까지 함께한 모든 관계자에게 전달돼요.",
            처리일시 = DateTimeOffset.UtcNow
        });

        return results
            .GroupBy(x => new { x.대상역할, x.대상참여자Id, x.대상표시명 })
            .Select(x => x.OrderByDescending(y => y.처리일시).First())
            .ToArray();
    }
}
