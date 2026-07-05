using System.Text.Json;
using MediatR;
using 홍달.도메인.설정;
using 홍달.도메인.통관;

namespace Hongdal.Application.Warehouse;

public sealed class 화주통관의뢰알림EventHandler : INotificationHandler<화주통관의뢰등록됨Event>
{
    private readonly HongdalContext _db;
    private readonly ILogger<화주통관의뢰알림EventHandler> _logger;

    public 화주통관의뢰알림EventHandler(HongdalContext db, ILogger<화주통관의뢰알림EventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(화주통관의뢰등록됨Event notification, CancellationToken cancellationToken)
    {
        var 대상관세사목록 = await ResolveTargetBrokersAsync(notification, cancellationToken);
        if (대상관세사목록.Count == 0)
        {
            _logger.LogInformation(
                "화주 통관 의뢰 알림 대상 관세사가 없습니다. 통관절차Id={통관절차Id}, 의뢰유형={의뢰유형}",
                notification.통관절차Id,
                notification.의뢰유형);
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var 관세사 in 대상관세사목록)
        {
            var payload = JsonSerializer.Serialize(new
            {
                TargetBrokerParticipantId = 관세사.참여자Id,
                notification.통관절차Id,
                notification.화주UserId,
                notification.의뢰유형,
                거래방향 = notification.물류거래방향.ToString(),
                notification.대표상품명,
                notification.발생시각Utc,
                알림유형 = ResolveNotificationType(notification.의뢰유형)
            });

            _db.Command알림Outbox.Add(new Command알림Outbox
            {
                CommandName = nameof(화주통관의뢰등록Command),
                EventName = nameof(화주통관의뢰등록됨Event),
                FeatureName = "CustomsRequest",
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
            "화주 통관 의뢰 알림 의도 적재 완료: 통관절차Id={통관절차Id}, 의뢰유형={의뢰유형}, 대상수={대상수}",
            notification.통관절차Id,
            notification.의뢰유형,
            대상관세사목록.Count);
    }

    private async Task<IReadOnlyList<관세사프로필>> ResolveTargetBrokersAsync(
        화주통관의뢰등록됨Event notification,
        CancellationToken cancellationToken)
    {
        var query = _db.관세사프로필
            .AsNoTracking()
            .Where(x => x.관리자승인여부 && x.수임가능여부);

        if (!string.IsNullOrWhiteSpace(notification.대상관세사참여자Id))
        {
            return await query
                .Where(x => x.참여자Id == notification.대상관세사참여자Id)
                .ToListAsync(cancellationToken);
        }

        query = notification.물류거래방향 switch
        {
            물류거래방향.수입 => query.Where(x => x.수입전문여부),
            물류거래방향.수출 => query.Where(x => x.수출전문여부),
            _ => query
        };

        return await query.ToListAsync(cancellationToken);
    }

    private static string ResolveNotificationType(string requestType)
    {
        return string.Equals(requestType, "HS_CODE_REVIEW", StringComparison.OrdinalIgnoreCase)
            ? "HS코드검토요청"
            : "통관대행요청";
    }
}
