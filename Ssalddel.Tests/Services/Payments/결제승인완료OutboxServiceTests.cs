using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.Shipper.Payment.Events;
using Ssalddel.Services.Outbox;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Payments;
using 살뜰.도메인.설정;

namespace Ssalddel.Tests.Services.Payments;

public sealed class 결제승인완료OutboxServiceTests
{
    [Fact]
    public async Task 일시실패는_Pending으로돌아가고_다음실행에서재처리된다()
    {
        await using var db = CreateContext();
        var item = await SeedAsync(db);
        var publisher = new FailOncePublisher();
        var service = new 결제승인완료OutboxService(
            db,
            publisher,
            NullLogger<결제승인완료OutboxService>.Instance);

        var firstProcessed = await service.대기이벤트발행Async();

        Assert.Equal(1, firstProcessed);
        Assert.Equal(OutboxProcessingStatuses.Pending, item.처리상태);
        Assert.Equal(1, item.시도횟수);

        item.UpdatedAt = DateTime.UtcNow - OutboxProcessingPolicy.RetryDelay - TimeSpan.FromSeconds(1);
        await db.SaveChangesAsync();

        var secondProcessed = await service.대기이벤트발행Async();

        Assert.Equal(1, secondProcessed);
        Assert.Equal(OutboxProcessingStatuses.Succeeded, item.처리상태);
        Assert.Equal(2, item.시도횟수);
        Assert.Equal(2, publisher.AttemptCount);
    }

    [Fact]
    public async Task 잘못된Payload는_재시도하지않고_Failed로종료한다()
    {
        await using var db = CreateContext();
        var item = await SeedAsync(db, payloadJson: "{not-json");
        var publisher = new FailOncePublisher();
        var service = new 결제승인완료OutboxService(
            db,
            publisher,
            NullLogger<결제승인완료OutboxService>.Instance);

        var processed = await service.대기이벤트발행Async();

        Assert.Equal(1, processed);
        Assert.Equal(OutboxProcessingStatuses.Failed, item.처리상태);
        Assert.Equal(1, item.시도횟수);
        Assert.Equal(0, publisher.AttemptCount);
    }

    [Fact]
    public async Task 최대시도횟수에도실패하면_Failed로종료한다()
    {
        await using var db = CreateContext();
        var item = await SeedAsync(
            db,
            attemptCount: OutboxProcessingPolicy.MaximumAttempts - 1,
            updatedAt: DateTime.UtcNow - TimeSpan.FromMinutes(1));
        var publisher = new AlwaysFailPublisher();
        var service = new 결제승인완료OutboxService(
            db,
            publisher,
            NullLogger<결제승인완료OutboxService>.Instance);

        await service.대기이벤트발행Async();

        Assert.Equal(OutboxProcessingStatuses.Failed, item.처리상태);
        Assert.Equal(OutboxProcessingPolicy.MaximumAttempts, item.시도횟수);
    }

    private static async Task<결제승인완료Outbox> SeedAsync(
        SsalddelContext db,
        string? payloadJson = null,
        int attemptCount = 0,
        DateTime? updatedAt = null)
    {
        var paymentEvent = new 결제승인완료Event(
            10,
            "payment-10",
            1,
            "target-10",
            1,
            42000,
            "KRW",
            DateTime.UtcNow);
        var item = new 결제승인완료Outbox
        {
            결제레코드Id = 10,
            결제Id = paymentEvent.결제Id,
            결제대상유형 = paymentEvent.결제대상유형,
            대상Id = paymentEvent.대상Id,
            결제제공자 = paymentEvent.결제제공자,
            결제금액 = paymentEvent.결제금액,
            통화 = paymentEvent.통화,
            승인일시Utc = paymentEvent.승인일시Utc,
            PayloadJson = payloadJson ?? JsonSerializer.Serialize(paymentEvent),
            처리상태 = OutboxProcessingStatuses.Pending,
            시도횟수 = attemptCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
        db.결제승인완료Outbox.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"payment-approved-outbox-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class FailOncePublisher : IPublisher
    {
        public int AttemptCount { get; private set; }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => PublishCore();

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => PublishCore();

        private Task PublishCore()
        {
            AttemptCount++;
            return AttemptCount == 1
                ? Task.FromException(new InvalidOperationException("temporary"))
                : Task.CompletedTask;
        }
    }

    private sealed class AlwaysFailPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("temporary"));

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.FromException(new InvalidOperationException("temporary"));
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
