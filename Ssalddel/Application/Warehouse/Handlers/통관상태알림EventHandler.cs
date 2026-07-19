using MediatR;
using System.Text.Json;
using 살뜰.도메인.통관;

namespace Ssalddel.Application.Warehouse;

public sealed class 통관상태알림EventHandler : INotificationHandler<통관상태변경감지됨Event>
{
    private readonly SsalddelContext _db;
    private readonly ILogger<통관상태알림EventHandler> _logger;

    public 통관상태알림EventHandler(SsalddelContext db, ILogger<통관상태알림EventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(통관상태변경감지됨Event notification, CancellationToken cancellationToken)
    {
        var 절차 = await _db.통관절차
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == notification.통관절차Id, cancellationToken);

        if (절차 is null)
        {
            return;
        }

        var 출고 = 절차.출고예정Id is null
            ? null
            : await _db.출고예정.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 절차.출고예정Id.Value, cancellationToken);

        var 입고 = 절차.입고요청Id is null
            ? null
            : await _db.입고요청.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 절차.입고요청Id.Value, cancellationToken);

        var 수신자 = new HashSet<string>(StringComparer.Ordinal)
        {
            출고?.주문자UserId ?? string.Empty,
            출고?.판매자UserId ?? string.Empty,
            입고?.주문자UserId ?? string.Empty,
            입고?.판매자UserId ?? string.Empty
        };

        수신자.RemoveWhere(string.IsNullOrWhiteSpace);

        var 관세사참여자목록 = await _db.통관수임
            .AsNoTracking()
            .Where(x => x.통관절차Id == notification.통관절차Id)
            .Select(x => x.관세사참여자Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var 관세사참여자Id in 관세사참여자목록)
        {
            수신자.Add(관세사참여자Id);
        }

        var now = DateTime.UtcNow;
        foreach (var 대상 in 수신자)
        {
            var payload = JsonSerializer.Serialize(new
            {
                TargetUserId = 대상,
                주문Id = notification.주문Id,
                통관절차Id = notification.통관절차Id,
                이전단계 = notification.이전단계.ToString(),
                현재단계 = notification.현재단계.ToString(),
                notification.처리단계명,
                알림유형 = "통관상태변경"
            });

            _db.Command알림Outbox.Add(new 살뜰.도메인.설정.Command알림Outbox
            {
                CommandName = nameof(통관상태알림EventHandler),
                EventName = nameof(통관상태변경감지됨Event),
                FeatureName = "CustomsTracking",
                Target = "User",
                PayloadJson = payload,
                Status = "Pending",
                TraceId = notification.TraceId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (notification.현재단계 is 통관진행단계.보류 or 통관진행단계.검사대상)
        {
            _db.Command알림Outbox.Add(new 살뜰.도메인.설정.Command알림Outbox
            {
                CommandName = nameof(통관상태알림EventHandler),
                EventName = nameof(통관상태변경감지됨Event),
                FeatureName = "CustomsTracking",
                Target = "Operations",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    notification.주문Id,
                    notification.통관절차Id,
                    단계 = notification.현재단계.ToString(),
                    notification.처리단계명,
                    긴급 = true
                }),
                Status = "Pending",
                TraceId = notification.TraceId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "통관상태 알림 적재 완료: 통관절차Id={통관절차Id}, 수신자수={수신자수}, 현재단계={현재단계}",
            notification.통관절차Id,
            수신자.Count,
            notification.현재단계);
    }
}
