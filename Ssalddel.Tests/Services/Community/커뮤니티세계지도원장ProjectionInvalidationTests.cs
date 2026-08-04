using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Ssalddel.Application.Community.Events;
using Ssalddel.Application.Community.Handlers;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도원장ProjectionInvalidationTests
{
    [Fact]
    public void Cache_requires_evidence_version_and_never_stores_personal_projection()
    {
        var cache = new 커뮤니티세계지도원장ProjectionCache();
        var noVersion = Query() with { EvidenceSnapshotVersion = null };
        var ownerProjection = Projection();
        ownerProjection.ViewerScopeCode = 커뮤니티세계지도원장ViewerScopeCodes.Owner;

        cache.SetPublic(noVersion, [Projection()]);
        cache.SetPublic(Query(), [ownerProjection]);

        Assert.False(cache.TryGetPublic(noVersion, out _));
        Assert.False(cache.TryGetPublic(Query(), out _));
    }

    [Fact]
    public void Evidence_version_change_is_a_cache_miss_without_serving_old_snapshot()
    {
        var cache = new 커뮤니티세계지도원장ProjectionCache();
        var versionOne = Query() with { EvidenceSnapshotVersion = "source-v1" };
        var versionTwo = Query() with { EvidenceSnapshotVersion = "source-v2" };

        cache.SetPublic(versionOne, [Projection()]);

        Assert.True(cache.TryGetPublic(versionOne, out _));
        Assert.False(cache.TryGetPublic(versionTwo, out _));
    }

    [Fact]
    public void Invalidation_removes_every_evidence_version_for_only_matching_template_and_marker()
    {
        var cache = new 커뮤니티세계지도원장ProjectionCache();
        var first = Query() with { EvidenceSnapshotVersion = "source-v1" };
        var second = Query() with { EvidenceSnapshotVersion = "source-v2" };
        var otherMarker = Query() with { MapMarkerId = "marker-2" };
        cache.SetPublic(first, [Projection()]);
        cache.SetPublic(second, [Projection()]);
        cache.SetPublic(otherMarker, [Projection()]);

        cache.Invalidate(CommunityLedgerTemplateKeys.GroupPurchase, "marker-1");
        cache.Invalidate(CommunityLedgerTemplateKeys.GroupPurchase, "marker-1");

        Assert.False(cache.TryGetPublic(first, out _));
        Assert.False(cache.TryGetPublic(second, out _));
        Assert.True(cache.TryGetPublic(otherMarker, out _));
    }

    [Fact]
    public void Cache_entry_expires_and_fails_safe()
    {
        var time = new TestTimeProvider(new DateTimeOffset(2026, 8, 4, 9, 0, 0, TimeSpan.Zero));
        var cache = new 커뮤니티세계지도원장ProjectionCache(time);
        cache.SetPublic(Query(), [Projection()]);

        time.Advance(TimeSpan.FromMinutes(6));

        Assert.False(cache.TryGetPublic(Query(), out _));
    }

    [Fact]
    public async Task Ledger_change_handler_invalidates_cached_public_projection_idempotently()
    {
        var cache = new 커뮤니티세계지도원장ProjectionCache();
        cache.SetPublic(Query(), [Projection()]);
        var handler = new 커뮤니티세계지도원장Projection무효화EventHandler(
            cache,
            NullLogger<커뮤니티세계지도원장Projection무효화EventHandler>.Instance);
        var notification = Event(Ledger());

        await handler.Handle(notification, CancellationToken.None);
        await handler.Handle(notification, CancellationToken.None);

        Assert.False(cache.TryGetPublic(Query(), out _));
    }

    [Fact]
    public void Invalidation_reason_distinguishes_state_consent_and_cancellation()
    {
        var stateChanged = Event(Ledger(), 커뮤니티원장변경유형.상태변경);
        var consentLedger = Ledger();
        consentLedger.확장속성 = Attributes(
            (CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey, 커뮤니티세계지도원장성숙도Codes.Established),
            (지도신청가원장정책.개인정보동의철회Key, bool.TrueString));
        var cancelledLedger = Ledger();
        cancelledLedger.현재단계Key = 지도신청가원장정책.운영신청취소단계;

        Assert.Equal(
            커뮤니티세계지도원장Projection무효화ReasonCodes.StateChanged,
            커뮤니티세계지도원장Projection무효화EventHandler.ResolveReasonCode(stateChanged));
        Assert.Equal(
            커뮤니티세계지도원장Projection무효화ReasonCodes.ConsentWithdrawn,
            커뮤니티세계지도원장Projection무효화EventHandler.ResolveReasonCode(Event(consentLedger)));
        Assert.Equal(
            커뮤니티세계지도원장Projection무효화ReasonCodes.Cancelled,
            커뮤니티세계지도원장Projection무효화EventHandler.ResolveReasonCode(Event(cancelledLedger)));
    }

    [Fact]
    public async Task Use_case_reuses_public_cache_then_reloads_after_ledger_event()
    {
        var store = new CountingLedgerStore(
            Enumerable.Range(1, 5).Select(index => Ledger($"ledger-{index}")));
        var cache = new 커뮤니티세계지도원장ProjectionCache();
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(store, cache);
        var handler = new 커뮤니티세계지도원장Projection무효화EventHandler(
            cache,
            NullLogger<커뮤니티세계지도원장Projection무효화EventHandler>.Instance);

        Assert.Single(await useCase.조회Async(Query()));
        Assert.Single(await useCase.조회Async(Query()));
        Assert.Equal(1, store.QueryCount);

        await handler.Handle(Event(store.Ledgers[0]), CancellationToken.None);

        Assert.Single(await useCase.조회Async(Query()));
        Assert.Equal(2, store.QueryCount);
    }

    [Fact]
    public void Invalidation_handler_is_discovered_by_the_application_mediatr_scan()
    {
        var services = new ServiceCollection();

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(
            typeof(커뮤니티세계지도원장Projection무효화EventHandler).Assembly));

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(INotificationHandler<커뮤니티원장변경됨Event>)
            && descriptor.ImplementationType == typeof(커뮤니티세계지도원장Projection무효화EventHandler));
    }

    private static 커뮤니티세계지도원장ProjectionQuery Query()
        => new(
            CommunityLedgerTemplateKeys.GroupPurchase,
            "marker-1",
            AdministrativeRegionKey: "region:seoul",
            CountryCode: "KR",
            EvidenceFreshnessCode: 커뮤니티세계지도FreshnessCodes.Fresh,
            EvidenceSnapshotVersion: "source-v1");

    private static 커뮤니티세계지도원장ProjectionDto Projection()
        => new()
        {
            ProjectionId = "projection-1",
            LedgerTemplateKey = CommunityLedgerTemplateKeys.GroupPurchase,
            ViewerScopeCode = 커뮤니티세계지도원장ViewerScopeCodes.Public,
            EvidenceSnapshotVersion = "source-v1"
        };

    private static 커뮤니티원장Dto Ledger(string ledgerId = "ledger-1")
        => new()
        {
            원장Id = ledgerId,
            Revision = 3,
            투영EventId = $"event:{ledgerId}",
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupPurchase,
            제목 = "공동구매 원장",
            상태 = 커뮤니티원장상태.진행중,
            현재단계Key = "active",
            생성자UserId = $"owner:{ledgerId}",
            외부참조 = new Dictionary<string, string> { ["MapMarkerId"] = "marker-1" },
            확장속성 = Attributes(
                (CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey, 커뮤니티세계지도원장성숙도Codes.Established)),
            수정시각Utc = new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc)
        };

    private static 커뮤니티원장변경됨Event Event(
        커뮤니티원장Dto ledger,
        string changeType = 커뮤니티원장변경유형.저장)
        => new(
            ledger,
            changeType,
            "tester",
            상태변경요청: null,
            new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc),
            ledger.투영EventId!);

    private static IReadOnlyDictionary<string, string> Attributes(
        params (string Key, string Value)[] values)
        => values.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

    private sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset current = now;

        public override DateTimeOffset GetUtcNow() => current;

        public void Advance(TimeSpan duration) => current = current.Add(duration);
    }

    private sealed class CountingLedgerStore(IEnumerable<커뮤니티원장Dto> ledgers)
        : I커뮤니티원장저장소
    {
        public IReadOnlyList<커뮤니티원장Dto> Ledgers { get; } = ledgers.ToArray();
        public int QueryCount { get; private set; }

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Ledgers.FirstOrDefault(ledger => ledger.원장Id == 원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
        {
            QueryCount++;
            return Task.FromResult(Ledgers);
        }

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
