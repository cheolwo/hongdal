using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Transport;
using Ssalddel.Hubs;

namespace 살뜰.Services.Transport;

public interface ITransportRequestLedgerRealtimeService
{
    Task PublishAsync(
        string requestId,
        string eventType,
        CancellationToken cancellationToken = default);
}

public sealed class TransportRequestLedgerRealtimeService : ITransportRequestLedgerRealtimeService
{
    private readonly SsalddelContext _db;
    private readonly IHubContext<TransportRequestLedgerHub> _hubContext;
    private readonly ILogger<TransportRequestLedgerRealtimeService> _logger;

    public TransportRequestLedgerRealtimeService(
        SsalddelContext db,
        IHubContext<TransportRequestLedgerHub> hubContext,
        ILogger<TransportRequestLedgerRealtimeService> logger)
    {
        _db = db;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(
        string requestId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return;
        }

        var normalizedRequestId = requestId.Trim();
        var request = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.의뢰Id == normalizedRequestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        var transport = await _db.운송원장
            .AsNoTracking()
            .Where(x => x.의뢰Id == normalizedRequestId)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddRecipient(recipients, request.주문자UserId);
        AddRecipient(recipients, request.화주Id);
        AddRecipient(recipients, transport?.확정기사Id);
        var changed = new TransportRequestLedgerChangedResponse(
            normalizedRequestId,
            request.상태,
            request.결제상태,
            request.배차상태,
            request.정산상태,
            transport?.상태,
            DateTimeOffset.UtcNow,
            "TransportLedger",
            string.IsNullOrWhiteSpace(eventType) ? null : eventType.Trim());

        try
        {
            var groups = recipients
                .Select(TransportRequestLedgerHub.UserGroup)
                .Append(TransportRequestLedgerHub.AdminGroup)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            await _hubContext.Clients
                .Groups(groups)
                .SendAsync(
                    TransportRequestLedgerRealtime.ChangedMethod,
                    changed,
                    cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "운송 원장 실시간 알림 전송에 실패했습니다. RequestId={RequestId} EventType={EventType}",
                normalizedRequestId,
                eventType);
        }
    }

    private static void AddRecipient(ISet<string> recipients, string? userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            recipients.Add(userId.Trim());
        }
    }
}
