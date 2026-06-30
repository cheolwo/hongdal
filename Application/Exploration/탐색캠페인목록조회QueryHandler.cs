using Hongdal.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Exploration;

public sealed class 탐색캠페인목록조회QueryHandler : IRequestHandler<탐색캠페인목록조회Query, IReadOnlyList<탐색캠페인목록항목응답>>
{
    private readonly HongdalContext _db;

    public 탐색캠페인목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<탐색캠페인목록항목응답>> Handle(탐색캠페인목록조회Query request, CancellationToken cancellationToken)
    {
        return await _db.탐색캠페인
            .AsNoTracking()
            .Where(x => x.개시자UserId == request.개시자UserId && x.개시자역할 == request.개시자역할)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new 탐색캠페인목록항목응답
            {
                Id = x.Id,
                개시자역할 = x.개시자역할,
                대상역할 = x.대상역할,
                탐색유형 = x.탐색유형,
                탐색명 = x.탐색명,
                운행예정일 = x.운행예정일,
                출발권역 = x.출발권역,
                희망도착권역 = x.희망도착권역,
                차량종류 = x.차량종류,
                탐색상태 = x.탐색상태,
                모집대상수 = x.모집대상수,
                응답수 = _db.탐색캠페인응답.Count(r => r.탐색캠페인Id == x.Id),
                있음응답수 = _db.탐색캠페인응답.Count(r => r.탐색캠페인Id == x.Id && r.응답유형 == 운행문의응답유형.있음),
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
