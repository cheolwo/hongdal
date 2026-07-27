using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Services.Outbox;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Notifications;
using 살뜰.Services.Options;
using 살뜰.Services.Storage.Local;
using 살뜰.도메인.설정;

namespace Ssalddel.Tests.Services.Notifications;

public sealed class Command알림Outbox발송ServiceTests
{
    [Fact]
    public async Task 발송실패는_Pending으로돌아가고_다음실행에서재처리된다()
    {
        await using var db = CreateContext();
        var item = await SeedAsync(db);
        var fcm = new FailOnceFcmPushService();
        var service = CreateService(db, fcm);

        var firstProcessed = await service.대기알림발송Async();

        Assert.Equal(1, firstProcessed);
        Assert.Equal(OutboxProcessingStatuses.Pending, item.Status);
        Assert.Equal(1, item.RetryCount);

        item.UpdatedAt = DateTime.UtcNow - OutboxProcessingPolicy.RetryDelay - TimeSpan.FromSeconds(1);
        await db.SaveChangesAsync();

        var secondProcessed = await service.대기알림발송Async();

        Assert.Equal(1, secondProcessed);
        Assert.Equal(OutboxProcessingStatuses.Succeeded, item.Status);
        Assert.Equal(2, item.RetryCount);
        Assert.Equal(2, fcm.AttemptCount);
    }

    [Fact]
    public async Task 잘못된Payload는_Failed로종료한다()
    {
        await using var db = CreateContext();
        var item = await SeedAsync(db, "{not-json");
        var fcm = new FailOnceFcmPushService();
        var service = CreateService(db, fcm);

        var processed = await service.대기알림발송Async();

        Assert.Equal(0, processed);
        Assert.Equal(OutboxProcessingStatuses.Failed, item.Status);
        Assert.Equal(1, item.RetryCount);
        Assert.Equal(0, fcm.AttemptCount);
    }

    private static Command알림Outbox발송Service CreateService(
        SsalddelContext db,
        IFcmPushService fcm)
        => new(
            db,
            new StaticUserPushTokenStore("token-1"),
            fcm,
            new AlwaysSuccessfulKakaoAlimTalkService(),
            Options.Create(new KakaoAlimTalkOptions { Enabled = false }),
            NullLogger<Command알림Outbox발송Service>.Instance);

    private static async Task<Command알림Outbox> SeedAsync(
        SsalddelContext db,
        string? payloadJson = null)
    {
        var payload = new
        {
            notificationType = Command알림FeatureNames.배차수락,
            targetUserId = "user-1",
            title = "배차 알림",
            body = "배차가 접수되었습니다.",
            channels = new[] { "Push" }
        };
        var item = new Command알림Outbox
        {
            CommandName = "TestCommand",
            EventName = "TestEvent",
            FeatureName = Command알림FeatureNames.배차수락,
            Target = "Shipper",
            PayloadJson = payloadJson ?? JsonSerializer.Serialize(payload),
            Status = OutboxProcessingStatuses.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Command알림Outbox.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"command-notification-outbox-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class FailOnceFcmPushService : IFcmPushService
    {
        public int AttemptCount { get; private set; }

        public Task<bool> SendAsync(
            FcmPushMessage message,
            CancellationToken cancellationToken = default)
            => Send();

        public Task<bool> SendToTokenAsync(
            string token,
            string title,
            string body,
            IReadOnlyDictionary<string, string> data,
            CancellationToken cancellationToken = default)
            => Send();

        private Task<bool> Send()
        {
            AttemptCount++;
            return Task.FromResult(AttemptCount > 1);
        }
    }

    private sealed class AlwaysSuccessfulKakaoAlimTalkService : IKakaoAlimTalkService
    {
        public Task<bool> SendAsync(
            KakaoAlimTalkMessage message,
            CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class StaticUserPushTokenStore(string token) : I사용자PushTokenStore
    {
        public Task SetAsync(
            string userId,
            string pushToken,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<string?> GetAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(token);

        public Task ClearAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
