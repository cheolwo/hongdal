using FluentResults;
using Hongdal.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Application.Exploration;

public sealed class 탐색캠페인발송CommandHandler : IRequestHandler<탐색캠페인발송Command, Result<탐색캠페인상세응답>>
{
    private readonly HongdalContext _db;
    private readonly I탐색캠페인상태전이Service _stateTransitionService;
    private readonly I탐색캠페인이벤트저장소 _eventStore;

    public 탐색캠페인발송CommandHandler(HongdalContext db, I탐색캠페인상태전이Service stateTransitionService, I탐색캠페인이벤트저장소 eventStore)
    {
        _db = db;
        _stateTransitionService = stateTransitionService;
        _eventStore = eventStore;
    }

    public async Task<Result<탐색캠페인상세응답>> Handle(탐색캠페인발송Command request, CancellationToken cancellationToken)
    {
        var campaign = await _db.탐색캠페인.FirstOrDefaultAsync(x => x.Id == request.탐색캠페인Id && x.개시자UserId == request.개시자UserId && x.개시자역할 == request.개시자역할, cancellationToken);
        if (campaign is null)
        {
            return Result.Fail<탐색캠페인상세응답>("탐색캠페인을 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        var selectedIds = request.요청.대상UserIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray();
        if (selectedIds.Length == 0)
        {
            return Result.Fail<탐색캠페인상세응답>("발송 대상이 비어 있습니다.");
        }

        var existing = await _db.탐색캠페인대상자
            .Where(x => x.탐색캠페인Id == campaign.Id && selectedIds.Contains(x.대상UserId))
            .ToDictionaryAsync(x => x.대상UserId, StringComparer.Ordinal, cancellationToken);

        foreach (var targetUserId in selectedIds)
        {
            if (!existing.TryGetValue(targetUserId, out var target))
            {
                target = new 탐색캠페인대상자
                {
                    탐색캠페인Id = campaign.Id,
                    대상UserId = targetUserId,
                    대상역할 = campaign.대상역할,
                    선정사유 = "수동 발송 대상",
                    대상상태 = 상태값.탐색캠페인대상상태.발송됨,
                    발송메시지 = request.요청.발송메시지 ?? string.Empty,
                    발송일시 = now,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _db.탐색캠페인대상자.Add(target);
            }
            else
            {
                target.대상상태 = 상태값.탐색캠페인대상상태.발송됨;
                target.발송메시지 = request.요청.발송메시지 ?? string.Empty;
                target.발송일시 = now;
                target.UpdatedAt = now;
            }
        }

        if (string.Equals(campaign.탐색상태, 상태값.탐색캠페인상태.초안, StringComparison.Ordinal))
        {
            _stateTransitionService.전이(campaign, 상태값.탐색캠페인상태.탐색중);
        }

        if (string.Equals(campaign.탐색상태, 상태값.탐색캠페인상태.탐색중, StringComparison.Ordinal))
        {
            _stateTransitionService.전이(campaign, 상태값.탐색캠페인상태.응답수집중);
        }

        campaign.응답요약 = $"발송대상 {selectedIds.Length}건";
        campaign.UpdatedAt = now;
        await _db.SaveChangesAsync(cancellationToken);

        await _eventStore.AppendAsync(탐색캠페인이벤트.Create(campaign.Id, "campaign_sent", request.개시자UserId, request.개시자역할, new { request.요청.대상UserIds, request.요청.발송메시지 }), cancellationToken);

        var detail = await new 탐색캠페인상세조회QueryHandler(_db).Handle(new 탐색캠페인상세조회Query(request.개시자UserId, request.개시자역할, campaign.Id), cancellationToken);
        return Result.Ok(detail!);
    }
}
