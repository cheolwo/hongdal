using Hongdal.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Exploration;

public sealed class 탐색문의상세조회QueryHandler : IRequestHandler<탐색문의상세조회Query, 탐색문의상세응답?>
{
    private readonly HongdalContext _db;

    public 탐색문의상세조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<탐색문의상세응답?> Handle(탐색문의상세조회Query request, CancellationToken cancellationToken)
    {
        var item = await (
            from target in _db.탐색캠페인대상자.AsNoTracking()
            join campaign in _db.탐색캠페인.AsNoTracking() on target.탐색캠페인Id equals campaign.Id
            where campaign.Id == request.탐색캠페인Id && target.대상UserId == request.대상UserId && target.대상역할 == request.대상역할
            select new { target, campaign }
        ).FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var response = await _db.탐색캠페인응답
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.탐색캠페인Id == item.campaign.Id && x.응답자UserId == request.대상UserId, cancellationToken);

        return new 탐색문의상세응답
        {
            탐색캠페인Id = item.campaign.Id,
            탐색명 = item.campaign.탐색명,
            개시자UserId = item.campaign.개시자UserId,
            개시자명 = item.campaign.개시자UserId,
            개시자역할 = item.campaign.개시자역할,
            운행예정일 = item.campaign.운행예정일,
            출발권역 = item.campaign.출발권역,
            희망도착권역 = item.campaign.희망도착권역,
            차량종류 = item.campaign.차량종류,
            대상상태 = item.target.대상상태,
            발송일시 = item.target.발송일시,
            발송메시지 = item.target.발송메시지,
            메모 = item.campaign.메모,
            최대적재중량Kg = item.campaign.최대적재중량Kg,
            최대적재부피Cbm = item.campaign.최대적재부피Cbm,
            기존응답유형 = response?.응답유형,
            기존응답일시 = response?.응답일시
        };
    }
}
