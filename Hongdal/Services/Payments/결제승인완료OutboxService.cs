using System.Text.Json;
using Hongdal.Application.Shipper.Payment.Events;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.설정;

namespace 홍달.Services.Payments;

public interface I결제승인완료OutboxService
{
    Task<int> 대기이벤트발행Async(int take = 100, CancellationToken cancellationToken = default);
}

public sealed class 결제승인완료OutboxService : I결제승인완료OutboxService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string 상태_대기 = "Pending";
    private const string 상태_성공 = "Succeeded";
    private const string 상태_실패 = "Failed";

    private readonly HongdalContext _db;
    private readonly IPublisher _publisher;
    private readonly ILogger<결제승인완료OutboxService> _logger;

    public 결제승인완료OutboxService(HongdalContext db, IPublisher publisher, ILogger<결제승인완료OutboxService> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<int> 대기이벤트발행Async(int take = 100, CancellationToken cancellationToken = default)
    {
        var pendingItems = await _db.결제승인완료Outbox
            .Where(x => x.처리상태 == 상태_대기)
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return 0;
        }

        var processed = 0;
        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processed++;

            var now = DateTime.UtcNow;
            item.시도횟수 += 1;
            item.마지막시도시각 = now;
            item.UpdatedAt = now;

            try
            {
                var payload = JsonSerializer.Deserialize<결제승인완료Event>(item.PayloadJson, JsonOptions);
                if (payload is null)
                {
                    item.처리상태 = 상태_실패;
                    _logger.LogWarning("결제승인완료 Outbox payload 역직렬화 실패. OutboxId={OutboxId}", item.Id);
                    continue;
                }

                await _publisher.Publish(payload, cancellationToken);
                item.처리상태 = 상태_성공;
            }
            catch (Exception ex)
            {
                item.처리상태 = 상태_실패;
                _logger.LogWarning(ex, "결제승인완료 Outbox 발행 실패. OutboxId={OutboxId}", item.Id);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return processed;
    }
}
