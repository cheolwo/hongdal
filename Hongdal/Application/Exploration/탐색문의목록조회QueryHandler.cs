using Hongdal.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Exploration;

public sealed class 탐색문의목록조회QueryHandler : IRequestHandler<탐색문의목록조회Query, IReadOnlyList<탐색문의목록항목응답>>
{
    private readonly HongdalContext _db;

    public 탐색문의목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<탐색문의목록항목응답>> Handle(탐색문의목록조회Query request, CancellationToken cancellationToken)
    {
        return await (
            from target in _db.탐색캠페인대상자.AsNoTracking()
            join campaign in _db.탐색캠페인.AsNoTracking() on target.탐색캠페인Id equals campaign.Id
            where target.대상UserId == request.대상UserId && target.대상역할 == request.대상역할
            orderby target.UpdatedAt descending
            select new 탐색문의목록항목응답
            {
                탐색캠페인Id = campaign.Id,
                탐색명 = campaign.탐색명,
                개시자UserId = campaign.개시자UserId,
                개시자명 = campaign.개시자UserId,
                개시자역할 = campaign.개시자역할,
                운행예정일 = campaign.운행예정일,
                출발권역 = campaign.출발권역,
                희망도착권역 = campaign.희망도착권역,
                차량종류 = campaign.차량종류,
                대상상태 = target.대상상태,
                발송일시 = target.발송일시
            })
            .ToListAsync(cancellationToken);
    }
}
