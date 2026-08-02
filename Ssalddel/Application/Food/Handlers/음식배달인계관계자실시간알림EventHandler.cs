using MediatR;
using Ssalddel.Application.Food.Events;
using 살뜰.Services.Transport;

namespace Ssalddel.Application.Food.Handlers;

/// <summary>
/// 음식 배달의 기사 배정·픽업·인계가 저장된 뒤, 인증된 주문자·음식점·확정 기사에게만
/// 해당 운송 원장을 다시 조회하도록 알립니다. 공개 활동 집계에는 식별자를 노출하지 않습니다.
/// </summary>
public sealed class 음식배달인계관계자실시간알림EventHandler(
    ITransportRequestLedgerRealtimeService realtimeService,
    ILogger<음식배달인계관계자실시간알림EventHandler> logger)
    : INotificationHandler<음식배달인계상태변경됨Event>
{
    public async Task Handle(
        음식배달인계상태변경됨Event notification,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.주문번호))
        {
            return;
        }

        try
        {
            await realtimeService.PublishAsync(
                notification.주문번호,
                notification.상태,
                cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "음식 배달 관계자 원장 재조회 신호 발송에 실패했습니다. OrderNo={OrderNo}, Status={Status}, EventId={EventId}",
                notification.주문번호,
                notification.상태,
                notification.EventId);
        }
    }
}
