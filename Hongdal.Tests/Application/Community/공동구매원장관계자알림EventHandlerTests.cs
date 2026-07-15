using System.Text.Json;
using Hongdal.Application.Community.Events;
using Hongdal.Application.Community.Handlers;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using 홍달.Services.Notifications;

namespace Hongdal.Tests.Application.Community;

public sealed class 공동구매원장관계자알림EventHandlerTests
{
    [Fact]
    public async Task 생성이후_공동구매원장_저장과_상태변경만_관계자알림을_적재한다()
    {
        var service = new RecordingNotificationService();
        using var services = new ServiceCollection()
            .AddSingleton<I공동구매원장관계자알림Service>(service)
            .BuildServiceProvider();
        var handler = new 공동구매원장관계자알림EventHandler(
            services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<공동구매원장관계자알림EventHandler>.Instance);

        await handler.Handle(CreateEvent(CommunityLedgerTemplateKeys.GroupPurchase, revision: 1, "저장"), default);
        await handler.Handle(CreateEvent(CommunityLedgerTemplateKeys.GroupPurchase, revision: 2, "저장"), default);
        await handler.Handle(CreateEvent(CommunityLedgerTemplateKeys.GroupPurchase, revision: 3, "상태변경"), default);
        await handler.Handle(CreateEvent(CommunityLedgerTemplateKeys.GroupImport, revision: 2, "저장"), default);

        Assert.Equal(2, service.Events.Count);
        Assert.Equal(["저장", "상태변경"], service.Events.Select(item => item.ChangeType));
    }

    [Fact]
    public void 알림대상은_생성자와_참여자를_합치고_변경자와_중복을_제외한다()
    {
        var ledger = CreateLedger(CommunityLedgerTemplateKeys.GroupPurchase, revision: 2);
        ledger.생성자UserId = "owner";
        ledger.참여자목록 =
        [
            Participant("owner"),
            Participant("actor"),
            Participant("member-1"),
            Participant("MEMBER-1"),
            Participant(null)
        ];

        var recipients = 공동구매원장관계자알림Policy.ResolveRecipientUserIds(ledger, "actor");

        Assert.Equal(["owner", "member-1"], recipients);
    }

    [Fact]
    public void 알림Payload는_대상과_변경요약과_원장링크를_포함한다()
    {
        var ledger = CreateLedger(CommunityLedgerTemplateKeys.GroupPurchase, revision: 4);
        ledger.제목 = "아파트 감자 공동구매";
        ledger.상태 = 커뮤니티원장상태.진행중;
        ledger.현재단계Key = "구매 조건 합의";

        var json = 공동구매원장관계자알림Policy.BuildPayload(ledger, "member-1", "상태변경");
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(Command알림FeatureNames.공동구매원장변경, root.GetProperty("notificationType").GetString());
        Assert.Equal("member-1", root.GetProperty("targetUserId").GetString());
        Assert.Equal("group-purchase-ledger-1", root.GetProperty("ledgerId").GetString());
        Assert.Equal(4, root.GetProperty("ledgerRevision").GetInt64());
        Assert.Contains("구매 조건 합의", root.GetProperty("body").GetString(), StringComparison.Ordinal);
        Assert.Equal(
            "/community/group-purchase?ledgerId=group-purchase-ledger-1",
            root.GetProperty("deepLink").GetString());
        Assert.Equal("Push", root.GetProperty("channels")[0].GetString());
    }

    [Fact]
    public void 동일이벤트는_항상_같은_중복방지키를_만든다()
    {
        var first = 공동구매원장관계자알림Policy.BuildTraceId("event-1", "fallback-1");
        var second = 공동구매원장관계자알림Policy.BuildTraceId("event-1", "fallback-2");

        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
    }

    private static 커뮤니티원장변경됨Event CreateEvent(string templateKey, long revision, string changeType)
        => new(
            CreateLedger(templateKey, revision),
            changeType,
            "actor",
            null,
            new DateTime(2026, 7, 15, 1, 2, 3, DateTimeKind.Utc),
            $"event-{revision}-{changeType}");

    private static 커뮤니티원장Dto CreateLedger(string templateKey, long revision)
        => new()
        {
            원장Id = "group-purchase-ledger-1",
            Revision = revision,
            커뮤니티Id = "platform",
            원장템플릿Key = templateKey,
            제목 = "감자 공동구매",
            상태 = 커뮤니티원장상태.초안,
            생성자UserId = "owner",
            참여자목록 = [Participant("member-1"), Participant("actor")]
        };

    private static 커뮤니티원장참여자Dto Participant(string? userId)
        => new() { UserId = userId, DisplayName = userId ?? "익명" };

    private sealed class RecordingNotificationService : I공동구매원장관계자알림Service
    {
        public List<(string ChangeType, string Actor)> Events { get; } = [];

        public Task<int> 변경알림적재Async(
            커뮤니티원장Dto 원장,
            string 변경유형,
            string 변경자UserId,
            string eventId,
            DateTime occurredAtUtc,
            CancellationToken cancellationToken = default)
        {
            Events.Add((변경유형, 변경자UserId));
            return Task.FromResult(1);
        }
    }
}
