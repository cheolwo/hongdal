using Hongdal.Application.Community.Events;
using Hongdal.Services.Community;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hongdal.Application.Community.Handlers;

public sealed class 커뮤니티원장블록관계투영EventHandler : INotificationHandler<커뮤니티원장변경됨Event>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<커뮤니티원장블록관계투영EventHandler> _logger;

    public 커뮤니티원장블록관계투영EventHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<커뮤니티원장블록관계투영EventHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(커뮤니티원장변경됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<I커뮤니티원장블록관계투영Service>();
            await service.갱신Async(notification.원장, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "커뮤니티 원장 변경 블록 관계 투영에 실패했습니다. EventId={EventId}, 원장Id={원장Id}",
                notification.EventId,
                notification.원장.원장Id);
        }
    }
}
