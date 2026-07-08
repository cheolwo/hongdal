using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using 홍달.도메인.설정;
using 홍달.도메인.운송;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차수락사후처리EventHandler : INotificationHandler<배차수락됨Event>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HongdalContext _db;
    private readonly IDispatchAcceptanceLogStore _acceptanceLogStore;
    private readonly 홍달.Services.Dispatch.Queue.I배차큐전환Service _queueTransitionService;
    private readonly ILogger<배차수락사후처리EventHandler> _logger;

    public 배차수락사후처리EventHandler(
        HongdalContext db,
        IDispatchAcceptanceLogStore acceptanceLogStore,
        홍달.Services.Dispatch.Queue.I배차큐전환Service queueTransitionService,
        ILogger<배차수락사후처리EventHandler> logger)
    {
        _db = db;
        _acceptanceLogStore = acceptanceLogStore;
        _queueTransitionService = queueTransitionService;
        _logger = logger;
    }

    public async Task Handle(배차수락됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await _acceptanceLogStore.AppendAsync(new DispatchAcceptanceLogEntry(
                notification.기사Id,
                notification.화주Id,
                notification.의뢰Id,
                notification.발생시각Utc,
                notification.배차대기상태,
                notification.의뢰배차상태,
                notification.의뢰결제상태), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락 사후처리 중 수락로그 적재 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        try
        {
            await _queueTransitionService.배차확정처리Async(notification.의뢰Id, notification.기사Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락 사후처리 중 큐전환 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        try
        {
            await 운송진행건생성또는보정Async(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락 사후처리 중 운송 진행 건 생성 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        try
        {
            await 화주수락알림의도적재Async(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차수락 사후처리 중 화주 알림 의도 적재 예외가 발생했습니다. RequestId={RequestId}", notification.의뢰Id);
        }

        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} RequestId={RequestId} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "DispatchAccepted",
            notification.기사Id,
            notification.의뢰Id,
            notification.배차대기상태,
            "Success",
            notification.TraceId,
            notification.발생시각Utc);
    }

    private async Task 운송진행건생성또는보정Async(배차수락됨Event notification, CancellationToken cancellationToken)
    {
        var dispatchRequest = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.의뢰Id == notification.의뢰Id, cancellationToken);
        if (dispatchRequest is null)
        {
            return;
        }

        var existing = await _db.배송_운송
            .FirstOrDefaultAsync(x => x.운송번호 == notification.의뢰Id, cancellationToken);

        var now = notification.발생시각Utc;
        if (existing is null)
        {
            _db.배송_운송.Add(new 배송_운송
            {
                운송번호 = notification.의뢰Id,
                상태 = "매칭중",
                기사_운송자 = notification.기사Id,
                출발지 = dispatchRequest.픽업_도로명주소,
                도착지 = dispatchRequest.하차_도로명주소,
                운임 = dispatchRequest.최종운임,
                첨부_json = "[]",
                메모 = "배차 수락으로 생성된 기사 운송 진행 건",
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            existing.기사_운송자 = notification.기사Id;
            existing.출발지 = dispatchRequest.픽업_도로명주소;
            existing.도착지 = dispatchRequest.하차_도로명주소;
            existing.운임 = dispatchRequest.최종운임;
            if (string.Equals(existing.상태, "배차대기", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(existing.상태))
            {
                existing.상태 = "매칭중";
            }

            existing.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task 화주수락알림의도적재Async(배차수락됨Event notification, CancellationToken cancellationToken)
    {
        var dispatchRequest = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.의뢰Id == notification.의뢰Id, cancellationToken);
        if (dispatchRequest is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var payloadJson = JsonSerializer.Serialize(new
        {
            알림유형 = "배차수락",
            TargetUserId = notification.화주Id,
            ShipperUserId = notification.화주Id,
            DriverId = notification.기사Id,
            RequestId = notification.의뢰Id,
            CargoType = dispatchRequest.화물종류,
            PickupAddress = dispatchRequest.픽업_도로명주소,
            PickupAddressDetail = dispatchRequest.픽업_상세주소,
            PickupContactName = dispatchRequest.픽업_연락처_이름,
            PickupContactPhone = dispatchRequest.픽업_연락처_전화번호,
            PickupWindowStartUtc = dispatchRequest.픽업_시간창_시작일시,
            PickupWindowEndUtc = dispatchRequest.픽업_시간창_종료일시,
            AcceptedAtUtc = notification.발생시각Utc,
            Title = "기사님이 운송 의뢰를 수락했습니다.",
            Body = $"{dispatchRequest.화물종류} 운송 의뢰가 수락되었습니다. 상차 준비를 확인해 주세요.",
            Channels = new[] { "Push", "AlimTalk" }
        }, JsonOptions);

        _db.Command알림Outbox.Add(new Command알림Outbox
        {
            CommandName = nameof(배차수락Command),
            EventName = nameof(배차수락됨Event),
            FeatureName = "DispatchAccepted",
            Target = "Shipper",
            PayloadJson = payloadJson,
            Status = "Pending",
            TraceId = notification.TraceId,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
