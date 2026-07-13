using Hongdal.Application.Community.Events;
using Hongdal.Services.Community;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hongdal.Application.Community.Handlers;

public sealed class 커뮤니티원장상태기록EventHandler : INotificationHandler<커뮤니티원장변경됨Event>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<커뮤니티원장상태기록EventHandler> _logger;

    public 커뮤니티원장상태기록EventHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<커뮤니티원장상태기록EventHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(커뮤니티원장변경됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<I커뮤니티원장상태이벤트Service>();

            if (notification.변경유형 == 커뮤니티원장변경유형.상태변경
                && notification.상태변경요청 is not null)
            {
                await service.상태변경이벤트기록Async(
                    notification.상태변경요청,
                    notification.원장,
                    notification.변경자,
                    cancellationToken);
                return;
            }

            await service.저장이벤트기록Async(
                notification.원장,
                notification.변경자,
                cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "커뮤니티 원장 변경 상태 기록에 실패했습니다. EventId={EventId}, 원장Id={원장Id}",
                notification.EventId,
                notification.원장.원장Id);
        }
    }
}
