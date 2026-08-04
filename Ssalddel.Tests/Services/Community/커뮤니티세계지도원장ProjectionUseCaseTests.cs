using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Extensions;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도원장ProjectionUseCaseTests
{
    [Fact]
    public void Evaluator_builds_owner_draft_actions_without_exposing_ledger_id()
    {
        var ledger = Ledger(
            CommunityLedgerTemplateKeys.Order,
            "private-ledger-123",
            "owner-1",
            maturityCode: 커뮤니티세계지도원장성숙도Codes.Provisional,
            state: 커뮤니티원장상태.초안);

        var projection = 커뮤니티세계지도원장ProjectionEvaluator.Evaluate(
            Input(ledger, 커뮤니티세계지도원장ViewerScopeCodes.Owner));

        Assert.NotNull(projection);
        Assert.Equal(커뮤니티세계지도원장공개상태Codes.ProvisionalDraft, projection.PublicStatusCode);
        Assert.Contains(커뮤니티세계지도원장ActionCodes.ViewLedger, projection.AvailableActionCodes);
        Assert.Contains(커뮤니티세계지도원장ActionCodes.ContinueDraft, projection.AvailableActionCodes);
        Assert.Contains(커뮤니티세계지도원장ActionCodes.Submit, projection.AvailableActionCodes);
        Assert.Contains(커뮤니티세계지도원장ActionCodes.Withdraw, projection.AvailableActionCodes);
        Assert.DoesNotContain(ledger.원장Id, projection.ProjectionId, StringComparison.Ordinal);
        Assert.Null(projection.PublicAggregateCount);
    }

    [Fact]
    public void Evaluator_fails_closed_when_maturity_or_freshness_is_unknown()
    {
        var ledger = Ledger(
            CommunityLedgerTemplateKeys.GroupPurchase,
            "ledger-1",
            "owner-1",
            maturityCode: "unknown-maturity",
            state: 커뮤니티원장상태.진행중);

        Assert.Null(커뮤니티세계지도원장ProjectionEvaluator.Evaluate(
            Input(ledger, 커뮤니티세계지도원장ViewerScopeCodes.Owner)));

        ledger.확장속성 = Maturity(커뮤니티세계지도원장성숙도Codes.Established);
        Assert.Null(커뮤니티세계지도원장ProjectionEvaluator.Evaluate(
            Input(ledger, 커뮤니티세계지도원장ViewerScopeCodes.Owner) with
            {
                EvidenceFreshnessCode = "unknown-code"
            }));
    }

    [Fact]
    public async Task Anonymous_projection_is_suppressed_below_template_threshold()
    {
        var store = new RecordingLedgerStore(
            Enumerable.Range(1, 4)
                .Select(index => Ledger(
                    CommunityLedgerTemplateKeys.GroupPurchase,
                    $"ledger-{index}",
                    $"owner-{index}",
                    maturityCode: 커뮤니티세계지도원장성숙도Codes.Established,
                    state: 커뮤니티원장상태.진행중)));
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(store);

        var projections = await useCase.조회Async(Query(CommunityLedgerTemplateKeys.GroupPurchase));

        Assert.Empty(projections);
        Assert.Null(store.LastQuery!.접근UserId);
    }

    [Fact]
    public async Task Anonymous_projection_returns_only_thresholded_administrative_region_aggregate()
    {
        var store = new RecordingLedgerStore(
            Enumerable.Range(1, 5)
                .Select(index => Ledger(
                    CommunityLedgerTemplateKeys.GroupPurchase,
                    $"ledger-{index}",
                    $"owner-{index}",
                    maturityCode: 커뮤니티세계지도원장성숙도Codes.Established,
                    state: 커뮤니티원장상태.진행중)));
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(store);

        var projection = Assert.Single(await useCase.조회Async(
            Query(CommunityLedgerTemplateKeys.GroupPurchase) with
            {
                EvidenceFreshnessCode = 커뮤니티세계지도FreshnessCodes.Stale,
                EvidenceSnapshotVersion = "source-v2"
            }));

        Assert.Equal(커뮤니티세계지도원장ViewerScopeCodes.Public, projection.ViewerScopeCode);
        Assert.Equal(5, projection.PublicAggregateCount);
        Assert.Equal("region:seoul", projection.AdministrativeRegionKey);
        Assert.Null(projection.MapMarkerId);
        Assert.Null(projection.CountryCode);
        Assert.Equal(커뮤니티세계지도FreshnessCodes.Stale, projection.EvidenceFreshnessCode);
        Assert.Equal([커뮤니티세계지도원장ActionCodes.ViewEvidence], projection.AvailableActionCodes);
    }

    [Fact]
    public async Task Country_policy_removes_marker_and_region_from_public_aggregate()
    {
        var store = new RecordingLedgerStore(
            Enumerable.Range(1, 5)
                .Select(index => Ledger(
                    CommunityLedgerTemplateKeys.GroupImport,
                    $"import-{index}",
                    $"owner-{index}",
                    maturityCode: 커뮤니티세계지도원장성숙도Codes.Established,
                    state: 커뮤니티원장상태.진행중)));
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(store);

        var projection = Assert.Single(await useCase.조회Async(
            Query(CommunityLedgerTemplateKeys.GroupImport) with { CountryCode = "us" }));

        Assert.Equal("US", projection.CountryCode);
        Assert.Null(projection.MapMarkerId);
        Assert.Null(projection.AdministrativeRegionKey);
    }

    [Fact]
    public async Task Owner_receives_private_projection_and_store_query_is_access_scoped()
    {
        var ledger = Ledger(
            CommunityLedgerTemplateKeys.Order,
            "order-1",
            "owner-1",
            maturityCode: 커뮤니티세계지도원장성숙도Codes.Provisional,
            state: 커뮤니티원장상태.초안);
        var store = new RecordingLedgerStore([ledger]);
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(store);

        var projection = Assert.Single(await useCase.조회Async(
            Query(CommunityLedgerTemplateKeys.Order) with { ViewerUserId = "owner-1" }));

        Assert.Equal(커뮤니티세계지도원장ViewerScopeCodes.Owner, projection.ViewerScopeCode);
        Assert.Equal("owner-1", store.LastQuery!.접근UserId);
        Assert.Equal("marker-1", projection.MapMarkerId);
        Assert.Null(projection.PublicAggregateCount);
    }

    [Fact]
    public async Task Signed_in_user_gets_the_same_public_aggregate_as_anonymous_user()
    {
        var ledgers = Enumerable.Range(1, 5)
            .Select(index => Ledger(
                CommunityLedgerTemplateKeys.GroupPurchase,
                $"ledger-{index}",
                $"owner-{index}",
                maturityCode: 커뮤니티세계지도원장성숙도Codes.Established,
                state: 커뮤니티원장상태.진행중))
            .ToArray();
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(new RecordingLedgerStore(ledgers));

        var anonymous = await useCase.조회Async(Query(CommunityLedgerTemplateKeys.GroupPurchase));
        var signedIn = await useCase.조회Async(
            Query(CommunityLedgerTemplateKeys.GroupPurchase) with { ViewerUserId = "unrelated-user" });

        Assert.Equal(
            Assert.Single(anonymous).PublicAggregateCount,
            Assert.Single(signedIn).PublicAggregateCount);
        Assert.Equal(
            Assert.Single(anonymous).ProjectionId,
            Assert.Single(signedIn).ProjectionId);
    }

    [Fact]
    public async Task Participant_receives_participant_actions_but_not_owner_mutation_actions()
    {
        var ledger = Ledger(
            CommunityLedgerTemplateKeys.CargoTransport,
            "cargo-1",
            "owner-1",
            maturityCode: 커뮤니티세계지도원장성숙도Codes.Established,
            state: 커뮤니티원장상태.진행중,
            participantUserId: "participant-1");
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(new RecordingLedgerStore([ledger]));

        var projection = Assert.Single(await useCase.조회Async(
            Query(CommunityLedgerTemplateKeys.CargoTransport) with { ViewerUserId = "participant-1" }));

        Assert.Equal(커뮤니티세계지도원장ViewerScopeCodes.Participant, projection.ViewerScopeCode);
        Assert.Contains(커뮤니티세계지도원장ActionCodes.ViewLedger, projection.AvailableActionCodes);
        Assert.DoesNotContain(커뮤니티세계지도원장ActionCodes.Submit, projection.AvailableActionCodes);
        Assert.DoesNotContain(커뮤니티세계지도원장ActionCodes.Withdraw, projection.AvailableActionCodes);
    }

    [Fact]
    public async Task Anonymous_user_never_receives_private_order_projection()
    {
        var ledger = Ledger(
            CommunityLedgerTemplateKeys.Order,
            "order-1",
            "owner-1",
            maturityCode: 커뮤니티세계지도원장성숙도Codes.Established,
            state: 커뮤니티원장상태.진행중);
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(new RecordingLedgerStore([ledger]));

        Assert.Empty(await useCase.조회Async(Query(CommunityLedgerTemplateKeys.Order)));
    }

    [Fact]
    public async Task Reviewer_authorization_is_resolved_by_server_use_case_not_public_contract()
    {
        var ledger = Ledger(
            CommunityLedgerTemplateKeys.Order,
            "order-1",
            "owner-1",
            maturityCode: 커뮤니티세계지도원장성숙도Codes.Established,
            state: 커뮤니티원장상태.보류);
        var store = new RecordingLedgerStore([ledger]);
        var useCase = new 커뮤니티세계지도원장ProjectionUseCase(store);

        var projection = Assert.Single(await useCase.조회Async(
            Query(CommunityLedgerTemplateKeys.Order) with
            {
                ViewerUserId = "reviewer-1",
                ReviewerAuthorized = true
            }));

        Assert.Equal(커뮤니티세계지도원장ViewerScopeCodes.Reviewer, projection.ViewerScopeCode);
        Assert.Null(store.LastQuery!.접근UserId);
        Assert.Contains(커뮤니티세계지도원장ActionCodes.ViewLedger, projection.AvailableActionCodes);
    }

    [Fact]
    public void Projection_use_case_is_registered_in_community_domain_services()
    {
        var services = new ServiceCollection();

        services.AddSsalddelDomainServices();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(I커뮤니티세계지도원장ProjectionUseCase)
            && descriptor.ImplementationType == typeof(커뮤니티세계지도원장ProjectionUseCase));
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(I커뮤니티세계지도원장ProjectionCache)
            && descriptor.ImplementationType == typeof(커뮤니티세계지도원장ProjectionCache)
            && descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    private static 커뮤니티세계지도원장ProjectionEvaluationInput Input(
        커뮤니티원장Dto ledger,
        string viewerScopeCode)
        => new(
            ledger,
            viewerScopeCode,
            $"ledger:{ledger.원장Id}:{viewerScopeCode}",
            "marker-1",
            "region:seoul",
            "KR",
            PublicAggregateCount: null,
            PublicAggregateMayBeTruncated: false,
            커뮤니티세계지도FreshnessCodes.Fresh,
            "source-v1",
            new DateTimeOffset(2026, 8, 4, 9, 0, 0, TimeSpan.Zero),
            "event-1");

    private static 커뮤니티세계지도원장ProjectionQuery Query(string templateKey)
        => new(
            templateKey,
            "marker-1",
            AdministrativeRegionKey: "region:seoul",
            CountryCode: "KR",
            EvidenceFreshnessCode: 커뮤니티세계지도FreshnessCodes.Fresh,
            EvidenceSnapshotVersion: "source-v1");

    private static 커뮤니티원장Dto Ledger(
        string templateKey,
        string ledgerId,
        string ownerUserId,
        string maturityCode,
        string state,
        string? participantUserId = null)
        => new()
        {
            원장Id = ledgerId,
            Revision = 3,
            투영EventId = $"event:{ledgerId}",
            커뮤니티Id = "platform",
            원장템플릿Key = templateKey,
            제목 = "지도 원장",
            상태 = state,
            현재단계Key = state == 커뮤니티원장상태.진행중 ? "active" : null,
            생성자UserId = ownerUserId,
            참여자목록 = participantUserId is null
                ? []
                : [new 커뮤니티원장참여자Dto { UserId = participantUserId }],
            외부참조 = new Dictionary<string, string>
            {
                ["MapMarkerId"] = "marker-1"
            },
            확장속성 = Maturity(maturityCode),
            수정시각Utc = new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc)
        };

    private static IReadOnlyDictionary<string, string> Maturity(string maturityCode)
        => new Dictionary<string, string>
        {
            [CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey] = maturityCode
        };

    private sealed class RecordingLedgerStore(IEnumerable<커뮤니티원장Dto> ledgers)
        : I커뮤니티원장저장소
    {
        private readonly IReadOnlyList<커뮤니티원장Dto> values = ledgers.ToArray();

        public 커뮤니티원장조회조건? LastQuery { get; private set; }
        public List<커뮤니티원장조회조건> Queries { get; } = [];

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(values.FirstOrDefault(ledger => ledger.원장Id == 원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            Queries.Add(query);
            var result = values.Where(ledger =>
                query.접근UserId is null
                || string.Equals(ledger.생성자UserId, query.접근UserId, StringComparison.Ordinal)
                || ledger.참여자목록.Any(participant =>
                    string.Equals(participant.UserId, query.접근UserId, StringComparison.Ordinal)));
            return Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(result.ToArray());
        }

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
