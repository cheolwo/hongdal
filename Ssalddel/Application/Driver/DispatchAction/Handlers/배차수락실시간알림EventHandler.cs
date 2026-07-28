using 살뜰.Services.Transport;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed class 배차수락실시간알림EventHandler
    : INotificationHandler<배차수락됨Event>
{
    private readonly ITransportRequestLedgerRealtimeService _realtimeService;

    public 배차수락실시간알림EventHandler(
        ITransportRequestLedgerRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public Task Handle(배차수락됨Event notification, CancellationToken cancellationToken)
        => _realtimeService.PublishAsync(
            notification.의뢰Id,
            nameof(배차수락됨Event),
            cancellationToken);
}
