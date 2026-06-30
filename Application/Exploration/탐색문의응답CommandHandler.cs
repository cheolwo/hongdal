using FluentResults;
using Hongdal.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;
using 탐색캠페인응답엔터티 = 홍달.도메인.탐색캠페인.탐색캠페인응답;

namespace Hongdal.Application.Exploration;

public sealed class 탐색문의응답CommandHandler : IRequestHandler<탐색문의응답Command, Result>
{
    private readonly HongdalContext _db;
    private readonly I탐색캠페인이벤트저장소 _eventStore;

    public 탐색문의응답CommandHandler(HongdalContext db, I탐색캠페인이벤트저장소 eventStore)
    {
        _db = db;
        _eventStore = eventStore;
    }

    public async Task<Result> Handle(탐색문의응답Command request, CancellationToken cancellationToken)
    {
        var campaign = await _db.탐색캠페인.FirstOrDefaultAsync(x => x.Id == request.탐색캠페인Id, cancellationToken);
        if (campaign is null)
        {
            return Result.Fail("탐색캠페인을 찾을 수 없습니다.");
        }

        var target = await _db.탐색캠페인대상자.FirstOrDefaultAsync(x => x.탐색캠페인Id == request.탐색캠페인Id && x.대상UserId == request.대상UserId && x.대상역할 == request.대상역할, cancellationToken);
        if (target is null)
        {
            return Result.Fail("탐색문의 대상을 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        var existing = await _db.탐색캠페인응답.FirstOrDefaultAsync(x => x.탐색캠페인Id == request.탐색캠페인Id && x.응답자UserId == request.대상UserId, cancellationToken);
        if (existing is null)
        {
            existing = new 탐색캠페인응답엔터티
            {
                탐색캠페인Id = request.탐색캠페인Id,
                응답자UserId = request.대상UserId,
                응답자역할 = request.대상역할,
                CreatedAt = now
            };
            _db.탐색캠페인응답.Add(existing);
        }

        existing.응답유형 = request.요청.응답유형;
        existing.희망상차일시 = request.요청.희망상차일시;
        existing.출발지요약 = request.요청.출발지요약 ?? string.Empty;
        existing.도착지요약 = request.요청.도착지요약 ?? string.Empty;
        existing.예상중량Kg = request.요청.예상중량Kg;
        existing.예상부피Cbm = request.요청.예상부피Cbm;
        existing.예상팔레트개수 = request.요청.예상팔레트개수;
        existing.메모요약 = request.요청.메모 ?? string.Empty;
        existing.응답일시 = now;
        existing.UpdatedAt = now;

        target.대상상태 = request.요청.응답유형 switch
        {
            운행문의응답유형.있음 => 상태값.탐색캠페인대상상태.있음응답,
            운행문의응답유형.없음 => 상태값.탐색캠페인대상상태.없음응답,
            운행문의응답유형.미정 => 상태값.탐색캠페인대상상태.미정응답,
            _ => 상태값.탐색캠페인대상상태.나중응답
        };
        target.마지막응답일시 = now;
        target.예상정보요약 = $"중량:{request.요청.예상중량Kg?.ToString() ?? "-"},부피:{request.요청.예상부피Cbm?.ToString() ?? "-"},팔레트:{request.요청.예상팔레트개수?.ToString() ?? "-"}";
        target.UpdatedAt = now;

        campaign.응답요약 = await BuildSummaryAsync(request.탐색캠페인Id, cancellationToken);
        campaign.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
        await _eventStore.AppendAsync(탐색캠페인이벤트.Create(request.탐색캠페인Id, "campaign_replied", request.대상UserId, request.대상역할, request.요청), cancellationToken);

        return Result.Ok();
    }

    private async Task<string> BuildSummaryAsync(long campaignId, CancellationToken cancellationToken)
    {
        var total = await _db.탐색캠페인응답.CountAsync(x => x.탐색캠페인Id == campaignId, cancellationToken);
        var positive = await _db.탐색캠페인응답.CountAsync(x => x.탐색캠페인Id == campaignId && x.응답유형 == 운행문의응답유형.있음, cancellationToken);
        return $"응답 {total}건 / 가능 {positive}건";
    }
}
