using Ssalddel.Application.Community.Events;
using Ssalddel.Services.Community;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ssalddel.Application.Community.Handlers;

public sealed class 커뮤니티원장업무투영EventHandler : INotificationHandler<커뮤니티원장변경됨Event>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<커뮤니티원장업무투영EventHandler> _logger;

    public 커뮤니티원장업무투영EventHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<커뮤니티원장업무투영EventHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(커뮤니티원장변경됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<I커뮤니티원장업무투영동기화Service>();
            await service.갱신Async(notification.원장, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "커뮤니티 원장 변경 업무 투영에 실패했습니다. EventId={EventId}, 원장Id={원장Id}",
                notification.EventId,
                notification.원장.원장Id);
            throw;
        }
    }
}
