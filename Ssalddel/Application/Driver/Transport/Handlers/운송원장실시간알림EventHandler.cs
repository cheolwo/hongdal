using Microsoft.EntityFrameworkCore;
using 살뜰.Services.Transport;

namespace Ssalddel.Application.Driver.Transport;

public sealed class 운송원장실시간알림EventHandler :
    INotificationHandler<운송상차지도착됨Event>,
    INotificationHandler<운송상차완료됨Event>,
    INotificationHandler<운송하차지도착됨Event>,
    INotificationHandler<운송인수완료됨Event>
{
    private readonly SsalddelContext _db;
    private readonly ITransportRequestLedgerRealtimeService _realtimeService;

    public 운송원장실시간알림EventHandler(
        SsalddelContext db,
        ITransportRequestLedgerRealtimeService realtimeService)
    {
        _db = db;
        _realtimeService = realtimeService;
    }

    public Task Handle(운송상차지도착됨Event notification, CancellationToken cancellationToken)
        => PublishByTransportIdAsync(notification.운송Id, nameof(운송상차지도착됨Event), cancellationToken);

    public Task Handle(운송상차완료됨Event notification, CancellationToken cancellationToken)
        => PublishByTransportIdAsync(notification.운송Id, nameof(운송상차완료됨Event), cancellationToken);

    public Task Handle(운송하차지도착됨Event notification, CancellationToken cancellationToken)
        => PublishByTransportIdAsync(notification.운송Id, nameof(운송하차지도착됨Event), cancellationToken);

    public Task Handle(운송인수완료됨Event notification, CancellationToken cancellationToken)
        => PublishByTransportIdAsync(notification.운송Id, nameof(운송인수완료됨Event), cancellationToken);

    private async Task PublishByTransportIdAsync(
        long transportId,
        string eventType,
        CancellationToken cancellationToken)
    {
        var requestId = await _db.운송원장
            .AsNoTracking()
            .Where(x => x.Id == transportId)
            .Select(x => x.의뢰Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            await _realtimeService.PublishAsync(requestId, eventType, cancellationToken);
        }
    }
}
