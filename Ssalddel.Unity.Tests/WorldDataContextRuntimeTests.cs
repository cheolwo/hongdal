using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Tests.UnityData;

public sealed class WorldDataContextRuntimeTests
{
    [Fact]
    public void 같은ObjectId라도_World가다르면Cache와참조가충돌하지않는다()
    {
        var cache = new ContextScopedSnapshotCache<Snapshot>();
        var worldA = Context("session-a", "world-a", "scope-a");
        var worldB = Context("session-a", "world-b", "scope-a");
        var queryA = WorldDataQueryContext.ForAuthorizedUserWorld("warehouse:42", worldA);
        var queryB = WorldDataQueryContext.ForAuthorizedUserWorld("warehouse:42", worldB);

        cache.Store(queryA, new Snapshot("A"), "revision-a", Time());
        cache.Store(queryB, new Snapshot("B"), "revision-b", Time());

        Assert.True(cache.TryGet(queryA, out var cachedA));
        Assert.True(cache.TryGet(queryB, out var cachedB));
        Assert.Equal("A", cachedA!.Snapshot.Value);
        Assert.Equal("B", cachedB!.Snapshot.Value);
        Assert.NotEqual(
            new WorldObjectRef(worldA.World.WorldId, new WorldStableId("warehouse:42")),
            new WorldObjectRef(worldB.World.WorldId, new WorldStableId("warehouse:42")));
    }

    [Fact]
    public void Logout은Private와WorldCache를폐기하지만_GlobalPublicCache는유지한다()
    {
        var cache = new ContextScopedSnapshotCache<Snapshot>();
        var context = Context("session-a", "world-a", "scope-a");
        var global = WorldDataQueryContext.Global("population:kr", DataRuntimeMode.Operational);
        var world = WorldDataQueryContext.ForWorld("simulation-buildings", context.World);
        var authorized = WorldDataQueryContext.ForAuthorizedUserWorld("warehouse:42", context);
        cache.Store(global, new Snapshot("global"), "g1", Time());
        cache.Store(world, new Snapshot("world"), "w1", Time());
        cache.Store(authorized, new Snapshot("private"), "p1", Time());
        var runtime = new WorldDataContextRuntime(cache);
        runtime.Activate(context);

        runtime.Logout();

        Assert.True(cache.TryGet(global, out _));
        Assert.False(cache.TryGet(world, out _));
        Assert.False(cache.TryGet(authorized, out _));
    }

    [Fact]
    public void World전환은WorldScoped상태만폐기하고_UserScopedData는유지한다()
    {
        var cache = new ContextScopedSnapshotCache<Snapshot>();
        var first = Context("session-a", "world-a", "scope-a");
        var second = Context("session-a", "world-b", "scope-a");
        var user = WorldDataQueryContext.ForAuthorizedUser(
            "my-profile",
            DataRuntimeMode.Operational,
            first.Session,
            first.Authorization);
        var world = WorldDataQueryContext.ForAuthorizedUserWorld("warehouse:42", first);
        cache.Store(user, new Snapshot("user"), "u1", Time());
        cache.Store(world, new Snapshot("world"), "w1", Time());
        var runtime = new WorldDataContextRuntime(cache);
        runtime.Activate(first);

        var transition = runtime.Activate(second);

        Assert.Equal(WorldDataContextTransitionKind.WorldChanged, transition.Kind);
        Assert.True(cache.TryGet(user, out _));
        Assert.False(cache.TryGet(world, out _));
    }

    [Fact]
    public void Selection은_WorldDataContext전환과Logout에서해제된다()
    {
        var selection = new SelectionStateStore();
        var runtime = new WorldDataContextRuntime(selection);
        var first = Context("session-a", "world-a", "scope-a");
        runtime.Activate(first);
        selection.Select(new WorldStableId("warehouse:42"));

        runtime.Activate(Context("session-a", "world-b", "scope-a"));
        Assert.Null(selection.SelectedWorldId);

        selection.Select(new WorldStableId("warehouse:42"));
        runtime.Logout();
        Assert.Null(selection.SelectedWorldId);
        Assert.Equal(string.Empty, selection.AuthorizationScopeKey);
    }

    [Fact]
    public async Task ContextualQuery는_승인된DataContext를받고_World전환시LastSuccess를격리한다()
    {
        var query = new ContextualQuery();
        var runtime = new WorldReadRuntime<
            string,
            RuntimeData,
            string,
            RuntimeShared,
            string,
            RuntimePerspective,
            string,
            RuntimePresentation,
            string>(
                query,
                new SharedInterpreter(),
                new PerspectiveInterpreter(),
                new Projector(),
                new ChangeSetCalculator());
        var firstContext = WorldDataQueryContext.ForAuthorizedUserWorld(
            "warehouse:42",
            Context("session-a", "world-a", "scope-a"));

        var success = await runtime.RefreshDataAsync(
            "request", "rule", "manager", "desktop", firstContext);

        Assert.Equal(firstContext, query.LastContext);
        Assert.Equal(ZoneRuntimeStateCode.Ready, success.Status.StateCode);
        query.Error = new InvalidOperationException("private server detail");
        var failed = await runtime.RefreshDataAsync(
            "request",
            "rule",
            "manager",
            "desktop",
            WorldDataQueryContext.ForAuthorizedUserWorld(
                "warehouse:42",
                Context("session-a", "world-b", "scope-a")));

        Assert.Equal(ZoneRuntimeStateCode.InitialError, failed.Status.StateCode);
        Assert.False(failed.Status.IsShowingLastSuccess);
        Assert.Null(failed.Presentation);
    }

    [Fact]
    public void AuthorizationContext는_서버승인Role과Capability만질의한다()
    {
        var authorization = Authorization("scope-a", "auth-1");

        Assert.True(authorization.HasRole("WarehouseManager"));
        Assert.True(authorization.HasCapability("Warehouse.Read"));
        Assert.False(authorization.HasCapability("Warehouse.Command"));
    }

    private static WorldDataContext Context(string session, string world, string scope)
        => new(
            new UserSessionContext(new SessionScopeId(session), "authorized-subject"),
            new WorldContext(new WorldContextId(world), "world-revision-1", DataRuntimeMode.Operational),
            Authorization(scope, "auth-1"));

    private static DataAuthorizationContext Authorization(string scope, string revision)
        => new(
            new AuthorizationScopeId(scope),
            new[] { "WarehouseManager" },
            new[] { "Warehouse.Read" },
            revision);

    private static DateTimeOffset Time() => DateTimeOffset.Parse("2026-08-08T09:00:00Z");

    private sealed record Snapshot(string Value);

    private sealed class RuntimeData
    {
        public RuntimeData(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class RuntimeShared
    {
        public RuntimeShared(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class RuntimePerspective
    {
        public RuntimePerspective(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class RuntimePresentation
    {
        public RuntimePresentation(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class ContextualQuery : IContextualWorldDataQuery<string, RuntimeData>
    {
        public WorldDataQueryContext? LastContext { get; private set; }
        public Exception? Error { get; set; }

        public Task<RuntimeData> QueryAsync(
            string query,
            WorldDataQueryContext context,
            CancellationToken cancellationToken = default)
        {
            LastContext = context;
            return Error == null
                ? Task.FromResult(new RuntimeData("data"))
                : Task.FromException<RuntimeData>(Error);
        }
    }

    private sealed class SharedInterpreter : ISharedWorldInterpreter<RuntimeData, string, RuntimeShared>
    {
        public RuntimeShared Interpret(RuntimeData data, string context)
            => new(data.Value + "|" + context);
    }

    private sealed class PerspectiveInterpreter : IPerspectiveInterpreter<RuntimeShared, string, RuntimePerspective>
    {
        public RuntimePerspective Interpret(RuntimeShared world, string context)
            => new(world.Value + "|" + context);
    }

    private sealed class Projector : IPresentationProjector<RuntimePerspective, string, RuntimePresentation>
    {
        public RuntimePresentation Project(RuntimePerspective world, string context)
            => new(world.Value + "|" + context);
    }

    private sealed class ChangeSetCalculator : IPresentationChangeSetCalculator<RuntimePresentation, string>
    {
        public string Calculate(RuntimePresentation? current, RuntimePresentation incoming)
            => current == null ? "added" : "updated";
    }
}
