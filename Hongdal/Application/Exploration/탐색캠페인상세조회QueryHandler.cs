using Hongdal.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Exploration;

public sealed class 탐색캠페인상세조회QueryHandler : IRequestHandler<탐색캠페인상세조회Query, 탐색캠페인상세응답?>
{
    private readonly HongdalContext _db;

    public 탐색캠페인상세조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<탐색캠페인상세응답?> Handle(탐색캠페인상세조회Query request, CancellationToken cancellationToken)
    {
        var campaign = await _db.탐색캠페인
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.탐색캠페인Id && x.개시자UserId == request.개시자UserId && x.개시자역할 == request.개시자역할, cancellationToken);

        if (campaign is null)
        {
            return null;
        }

        var responses = await _db.탐색캠페인응답
            .AsNoTracking()
            .Where(x => x.탐색캠페인Id == campaign.Id)
            .ToListAsync(cancellationToken);

        var targets = await _db.탐색캠페인대상자
            .AsNoTracking()
            .Where(x => x.탐색캠페인Id == campaign.Id)
            .OrderByDescending(x => x.마지막응답일시)
            .ThenByDescending(x => x.발송일시)
            .ToListAsync(cancellationToken);

        return new 탐색캠페인상세응답
        {
            Id = campaign.Id,
            개시자UserId = campaign.개시자UserId,
            개시자역할 = campaign.개시자역할,
            대상역할 = campaign.대상역할,
            탐색유형 = campaign.탐색유형,
            탐색명 = campaign.탐색명,
            운행예정일 = campaign.운행예정일,
            출발권역 = campaign.출발권역,
            희망도착권역 = campaign.희망도착권역,
            차량종류 = campaign.차량종류,
            최대적재중량Kg = campaign.최대적재중량Kg,
            최대적재부피Cbm = campaign.최대적재부피Cbm,
            모집대상수 = campaign.모집대상수,
            탐색상태 = campaign.탐색상태,
            메모 = campaign.메모,
            실행판단사유 = campaign.실행판단사유,
            응답수 = responses.Count,
            있음응답수 = responses.Count(x => x.응답유형 == 운행문의응답유형.있음),
            예상총중량Kg = responses.Where(x => x.예상중량Kg.HasValue).Sum(x => x.예상중량Kg),
            예상총부피Cbm = responses.Where(x => x.예상부피Cbm.HasValue).Sum(x => x.예상부피Cbm),
            대상자목록 = targets.Select(x => new 탐색캠페인대상자응답
            {
                대상UserId = x.대상UserId,
                대상역할 = x.대상역할,
                대상명 = x.대상UserId,
                관계점수Snapshot = x.관계점수Snapshot,
                대상상태 = x.대상상태,
                선정사유 = x.선정사유,
                마지막응답일시 = x.마지막응답일시,
                응답유형 = responses.FirstOrDefault(r => r.탐색캠페인Id == x.탐색캠페인Id && r.응답자UserId == x.대상UserId)?.응답유형.ToString(),
                예상중량Kg = responses.FirstOrDefault(r => r.탐색캠페인Id == x.탐색캠페인Id && r.응답자UserId == x.대상UserId)?.예상중량Kg,
                예상부피Cbm = responses.FirstOrDefault(r => r.탐색캠페인Id == x.탐색캠페인Id && r.응답자UserId == x.대상UserId)?.예상부피Cbm
            }).ToArray(),
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }
}
