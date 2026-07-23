using MediatR;
using Ssalddel.Application.Community.Events;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Application.Community.Handlers;

/// <summary>개별 원함 원장의 자동집단 투영 한 관심사만 처리합니다.</summary>
public sealed class 개별원함자동집단투영EventHandler(
    I공동구매개별원함자동집단투영Service projection,
    ILogger<개별원함자동집단투영EventHandler> logger)
    : INotificationHandler<커뮤니티원장변경됨Event>
{
    public async Task Handle(
        커뮤니티원장변경됨Event notification,
        CancellationToken cancellationToken)
    {
        if (!projection.투영대상(notification.원장))
        {
            return;
        }

        try
        {
            await projection.투영Async(notification.원장, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(
                exception,
                "개별 원함 자동집단 투영에 실패했습니다. EventId={EventId}, LedgerId={LedgerId}, Revision={Revision}",
                notification.EventId,
                notification.원장.원장Id,
                notification.원장.Revision);
            throw;
        }
    }
}
