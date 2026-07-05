using MediatR;
using System.Text.Json;

namespace Hongdal.Application.Warehouse;

public sealed class 관세사통관알림EventHandler : INotificationHandler<통관절차생성됨Event>
{
    private readonly HongdalContext _db;
    private readonly ILogger<관세사통관알림EventHandler> _logger;

    public 관세사통관알림EventHandler(HongdalContext db, ILogger<관세사통관알림EventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(통관절차생성됨Event notification, CancellationToken cancellationToken)
    {
        var 대상관세사 = await _db.관세사프로필
            .AsNoTracking()
            .Where(x => x.관리자승인여부 && x.수임가능여부)
            .Where(x => notification.물류거래방향 != 홍달.도메인.통관.물류거래방향.수입 || x.수입전문여부)
            .Where(x => notification.물류거래방향 != 홍달.도메인.통관.물류거래방향.수출 || x.수출전문여부)
            .ToListAsync(cancellationToken);

        if (대상관세사.Count == 0)
        {
            _logger.LogInformation("통관절차 알림 대상 관세사가 없습니다. 통관절차Id={통관절차Id}", notification.통관절차Id);
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var 관세사 in 대상관세사)
        {
            var payload = JsonSerializer.Serialize(new
            {
                참여자Id = 관세사.참여자Id,
                통관절차Id = notification.통관절차Id,
                주문Id = notification.주문Id,
                주문참조번호 = notification.주문참조번호,
                거래방향 = notification.물류거래방향.ToString(),
                대표상품명 = notification.대표상품명,
                발생시각Utc = notification.발생시각Utc,
                알림유형 = "통관검토요청"
            });

            _db.Command알림Outbox.Add(new 홍달.도메인.설정.Command알림Outbox
            {
                CommandName = "관세사통관알림EventHandler",
                EventName = nameof(통관절차생성됨Event),
                FeatureName = "CustomsClearance",
                Target = "CustomsBroker",
                PayloadJson = payload,
                Status = "Pending",
                TraceId = notification.TraceId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "통관절차 알림 의도 적재 완료: 통관절차Id={통관절차Id}, 대상수={대상수}",
            notification.통관절차Id,
            대상관세사.Count);
    }
}
