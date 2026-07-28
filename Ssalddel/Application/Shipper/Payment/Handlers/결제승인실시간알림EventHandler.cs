using Ssalddel.Application.Shipper.Payment.Events;
using 살뜰.Services.Transport;

namespace Ssalddel.Application.Shipper.Payment;

public sealed class 결제승인실시간알림EventHandler
    : INotificationHandler<결제승인완료Event>
{
    private readonly ITransportRequestLedgerRealtimeService _realtimeService;

    public 결제승인실시간알림EventHandler(
        ITransportRequestLedgerRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public Task Handle(결제승인완료Event notification, CancellationToken cancellationToken)
        => _realtimeService.PublishAsync(
            notification.대상Id,
            nameof(결제승인완료Event),
            cancellationToken);
}
