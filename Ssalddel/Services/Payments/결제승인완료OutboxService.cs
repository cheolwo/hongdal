using System.Text.Json;
using Ssalddel.Application.Shipper.Payment.Events;
using Ssalddel.Services.Outbox;
using Microsoft.EntityFrameworkCore;
using 살뜰.도메인.설정;

namespace 살뜰.Services.Payments;

public interface I결제승인완료OutboxService
{
    Task<int> 대기이벤트발행Async(int take = 100, CancellationToken cancellationToken = default);
}

public sealed class 결제승인완료OutboxService : I결제승인완료OutboxService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SsalddelContext _db;
    private readonly IPublisher _publisher;
    private readonly ILogger<결제승인완료OutboxService> _logger;

    public 결제승인완료OutboxService(SsalddelContext db, IPublisher publisher, ILogger<결제승인완료OutboxService> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<int> 대기이벤트발행Async(int take = 100, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var retryCutoff = now - OutboxProcessingPolicy.RetryDelay;
        var leaseCutoff = now - OutboxProcessingPolicy.LeaseTimeout;
        var pendingItems = await _db.결제승인완료Outbox
            .Where(x =>
                (x.처리상태 == OutboxProcessingStatuses.Pending
                 && (x.시도횟수 == 0 || x.UpdatedAt <= retryCutoff))
                || (x.처리상태 == OutboxProcessingStatuses.Processing
                    && x.UpdatedAt <= leaseCutoff))
            .OrderBy(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return 0;
        }

        var processed = 0;
        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            item.처리상태 = OutboxProcessingStatuses.Processing;
            item.시도횟수 += 1;
            item.마지막시도시각 = now;
            item.UpdatedAt = now;
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _db.Entry(item).State = EntityState.Detached;
                continue;
            }

            processed++;

            try
            {
                var payload = JsonSerializer.Deserialize<결제승인완료Event>(item.PayloadJson, JsonOptions);
                if (payload is null)
                {
                    item.처리상태 = OutboxProcessingStatuses.Failed;
                    _logger.LogWarning("결제승인완료 Outbox payload 역직렬화 실패. OutboxId={OutboxId}", item.Id);
                }
                else
                {
                    await _publisher.Publish(payload, cancellationToken);
                    item.처리상태 = OutboxProcessingStatuses.Succeeded;
                }
            }
            catch (JsonException ex)
            {
                item.처리상태 = OutboxProcessingStatuses.Failed;
                _logger.LogWarning(
                    ex,
                    "결제승인완료 Outbox payload가 올바르지 않습니다. OutboxId={OutboxId}",
                    item.Id);
            }
            catch (Exception ex)
            {
                var retry = OutboxProcessingPolicy.CanRetry(item.시도횟수);
                item.처리상태 = retry
                    ? OutboxProcessingStatuses.Pending
                    : OutboxProcessingStatuses.Failed;
                _logger.LogWarning(
                    ex,
                    "결제승인완료 Outbox 발행 실패. OutboxId={OutboxId} Attempt={Attempt} WillRetry={WillRetry}",
                    item.Id,
                    item.시도횟수,
                    retry);
            }

            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }
}
