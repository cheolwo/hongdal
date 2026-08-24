using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class UrbanMarketManagerRuntimeTests
{
    [Fact]
    public async Task 첫Refresh는_Queue없이_진열작업계획Surface를Added로반환한다()
    {
        var fixture = await Fixture.CreateAsync();

        var result = await fixture.Runtime.RefreshAsync(DataContext());

        Assert.Equal(ZoneRuntimeStateCode.Ready, result.Status.StateCode);
        Assert.NotNull(result.Presentation);
        Assert.Equal(result.Presentation!.Shelves.Length, result.Changes!.Shelves.Added.Length);
        Assert.Equal(result.Presentation.TaskMarkers.Length, result.Changes.TaskMarkers.Added.Length);
        Assert.Equal(result.Presentation.SourcePlans.Length, result.Changes.SourcePlans.Added.Length);
        Assert.Empty(result.Changes.Details.Added);
    }

    [Fact]
    public async Task 같은PresentationRefresh는_SurfaceInstance를Unchanged로유지한다()
    {
        var fixture = await Fixture.CreateAsync();
        var first = await fixture.Runtime.RefreshAsync(DataContext());

        var second = await fixture.Runtime.RefreshAsync(DataContext());

        Assert.Empty(second.Changes!.Shelves.Updated);
        Assert.Equal(first.Presentation!.Shelves.Length, second.Changes.Shelves.Unchanged.Length);
        Assert.Same(first.Presentation.Shelves[0], second.Changes.Shelves.Unchanged[0]);
    }

    [Fact]
    public async Task WorldSelection은_관련SurfaceHighlight와Detail을증분생성한다()
    {
        var fixture = await Fixture.CreateAsync();
        await fixture.Runtime.RefreshAsync(DataContext());

        var selected = fixture.Runtime.Select(new WorldStableId("market-shelf:potato"));

        Assert.Equal("market-shelf:potato", selected.SelectedWorldId!.Value.Value);
        Assert.Contains(selected.Presentation!.Shelves, value =>
            value.Identity.SourceWorldIds.Any(id => id.Value == "market-shelf:potato")
            && value.IsHighlighted);
        Assert.Single(selected.Presentation.Details);
        Assert.Single(selected.Changes!.Details.Added);
    }

    [Fact]
    public async Task 선택대상이새Snapshot에서사라지면_Selection과Detail을해제한다()
    {
        var fixture = await Fixture.CreateAsync();
        await fixture.Runtime.RefreshAsync(DataContext());
        fixture.Runtime.Select(new WorldStableId("market-inventory:potato-backroom"));
        var next = await Fixture.SnapshotAsync();
        next.DataRevision = "simulation:market-operations:2";
        next.재고목록 = next.재고목록
            .Where(value => value.StableId != "market-inventory:potato-backroom")
            .ToArray();
        fixture.Query.Next = next;

        var refreshed = await fixture.Runtime.RefreshAsync(DataContext());

        Assert.Null(refreshed.SelectedWorldId);
        Assert.Empty(refreshed.Presentation!.Details);
        Assert.Single(refreshed.Changes!.Details.Removed);
    }

    [Fact]
    public async Task Refresh실패는_마지막성공Presentation과Selection을유지한다()
    {
        var fixture = await Fixture.CreateAsync();
        await fixture.Runtime.RefreshAsync(DataContext());
        var selected = fixture.Runtime.Select(new WorldStableId("market-shelf:potato"));
        fixture.Query.Error = new TimeoutException("offline");

        var failed = await fixture.Runtime.RefreshAsync(DataContext());

        Assert.Equal(ZoneRuntimeStateCode.RefreshError, failed.Status.StateCode);
        Assert.True(failed.Status.IsShowingLastSuccess);
        Assert.Equal("Timeout", failed.Status.SafeErrorCode);
        Assert.Same(selected.Presentation, failed.Presentation);
        Assert.Equal("market-shelf:potato", failed.SelectedWorldId!.Value.Value);
        Assert.Null(failed.Changes);
    }

    [Fact]
    public async Task AuthorizationScope변경은_Selection과이전LastSuccess를폐기한다()
    {
        var fixture = await Fixture.CreateAsync();
        await fixture.Runtime.RefreshAsync(DataContext());
        fixture.Runtime.Select(new WorldStableId("market-shelf:potato"));
        fixture.Query.Error = new InvalidOperationException("private snapshot unavailable");

        var failed = await fixture.Runtime.RefreshAsync(DataContext("session:other", "authorization:other"));

        Assert.Equal(ZoneRuntimeStateCode.InitialError, failed.Status.StateCode);
        Assert.False(failed.Status.IsShowingLastSuccess);
        Assert.Null(failed.Presentation);
        Assert.Null(failed.SelectedWorldId);
    }

    [Fact]
    public async Task ManagerRuntime은_승인된ManagerWorldContext만조회한다()
    {
        var fixture = await Fixture.CreateAsync();
        var unauthorized = WorldDataQueryContext.ForAuthorizedUserWorld(
            도심마트DataSetKeys.ManagerOperations,
            new WorldDataContext(
                new UserSessionContext(new SessionScopeId("session:observer"), "identity:observer"),
                new WorldContext(
                    new WorldContextId("world:urban-market-demo"),
                    "world-revision:1",
                    DataRuntimeMode.Simulation),
                new DataAuthorizationContext(
                    new AuthorizationScopeId("authorization:observer"),
                    new[] { "Observer" },
                    Array.Empty<string>(),
                    "authorization-revision:1")));

        var result = await fixture.Runtime.RefreshAsync(unauthorized);

        Assert.Equal(ZoneRuntimeStateCode.InitialError, result.Status.StateCode);
        Assert.Equal("UnexpectedError", result.Status.SafeErrorCode);
        Assert.Null(result.Presentation);
    }

    private sealed class Fixture
    {
        private Fixture(MutableQuery query, 도심마트ManagerRuntime runtime)
        {
            Query = query;
            Runtime = runtime;
        }

        public MutableQuery Query { get; }
        public 도심마트ManagerRuntime Runtime { get; }

        public static async Task<Fixture> CreateAsync()
        {
            var query = new MutableQuery { Next = await SnapshotAsync() };
            var runtime = new 도심마트ManagerRuntime(
                query,
                new 도심마트운영업무SharedWorldInterpreter(
                    new 도심마트운영SharedWorldInterpreter(),
                    new 도심마트진열보충Interpreter(),
                    도심마트ReplenishmentRuleSet.SimulationDefault()),
                new 마트관리자PerspectiveInterpreter(),
                new 도심마트PresentationProjector(new 도심마트ManagerVisualPolicy()),
                new 도심마트PresentationChangeSetCalculator(),
                new SelectionStateStore());
            return new Fixture(query, runtime);
        }

        public static Task<도심마트운영DataSnapshot> SnapshotAsync()
            => new Simulated도심마트운영DataQuery().조회Async();
    }

    private static WorldDataQueryContext DataContext(
        string sessionId = "session:market-manager",
        string authorizationId = "authorization:market-manager")
        => WorldDataQueryContext.ForAuthorizedUserWorld(
            도심마트DataSetKeys.ManagerOperations,
            new WorldDataContext(
                new UserSessionContext(new SessionScopeId(sessionId), "identity:market-manager"),
                new WorldContext(
                    new WorldContextId("world:urban-market-demo"),
                    "world-revision:1",
                    DataRuntimeMode.Simulation),
                new DataAuthorizationContext(
                    new AuthorizationScopeId(authorizationId),
                    new[] { 마트관리자PerspectiveCodes.Role },
                    Array.Empty<string>(),
                    "authorization-revision:1")));

    private sealed class MutableQuery : I도심마트운영DataQuery
    {
        public 도심마트운영DataSnapshot Next { get; set; } = null!;
        public Exception? Error { get; set; }

        public Task<도심마트운영DataSnapshot> 조회Async(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Error == null
                ? Task.FromResult(Next)
                : Task.FromException<도심마트운영DataSnapshot>(Error);
        }
    }
}
