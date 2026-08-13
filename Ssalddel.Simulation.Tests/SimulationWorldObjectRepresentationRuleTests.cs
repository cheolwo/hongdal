using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldObjectRepresentationRuleTests
{
    [Fact]
    public void Simulation규칙이미정이면_활성공간규칙으로기본외형만해석한다()
    {
        var catalog = Catalog(simulationActive: false);
        var request = Request(withSimulationState: false);

        var ledger = SimulationWorld객체표현해석기.Interpret(request, catalog);

        var result = Assert.Single(ledger.Results);
        Assert.Equal(SimulationWorld객체표현해석Codes.공간규칙적용, result.ResolutionCode);
        Assert.Equal("composition.logistics-warehouse.observed.v1", result.DefaultCompositionKey);
        Assert.Null(result.DynamicIntentBundleKey);
        Assert.Null(result.AppliedSimulationRuleStableId);
    }

    [Fact]
    public void 활성Simulation상태가만나면_더높은우선순위의동적표현을선택한다()
    {
        var catalog = Catalog(simulationActive: true);
        var request = Request(withSimulationState: true);

        var ledger = SimulationWorld객체표현해석기.Interpret(request, catalog);

        var result = Assert.Single(ledger.Results);
        Assert.Equal(SimulationWorld객체표현해석Codes.공간Simulation규칙적용, result.ResolutionCode);
        Assert.Equal("simulation-rule:warehouse-loading", result.AppliedSimulationRuleStableId);
        Assert.Equal("intent-bundle.warehouse.loading-active.v1", result.DynamicIntentBundleKey);
    }

    [Fact]
    public void 초안Simulation규칙은저장할수있지만_활성표현으로선택되지않는다()
    {
        var catalog = Catalog(simulationActive: false);
        var request = Request(withSimulationState: true);
        request.RuleCatalogRevision = catalog.CatalogRevision;

        var ledger = SimulationWorld객체표현해석기.Interpret(request, catalog);

        Assert.Equal(
            SimulationWorld객체표현해석Codes.공간규칙적용,
            Assert.Single(ledger.Results).ResolutionCode);
    }

    [Fact]
    public void 같은입력은대상순서와무관하게같은해시를만든다()
    {
        var catalog = Catalog(simulationActive: true);
        var request = Request(withSimulationState: true);
        request.Targets = new[] { request.Targets[0], SecondTarget() };
        var reversed = Request(withSimulationState: true);
        reversed.Targets = new[] { SecondTarget(), reversed.Targets[0] };

        var first = SimulationWorld객체표현해석기.Interpret(request, catalog);
        var second = SimulationWorld객체표현해석기.Interpret(reversed, catalog);

        Assert.Equal(first.InputFingerprintSha256, second.InputFingerprintSha256);
        Assert.Equal(first.OutputHashSha256, second.OutputHashSha256);
    }

    [Fact]
    public void Prefab경로를기본구성키로저장하면거부한다()
    {
        var catalog = Catalog(simulationActive: false);
        catalog.BindingRules[0].DefaultCompositionKey = "Assets/Synty/Warehouse.prefab";

        var error = Assert.Throws<InvalidOperationException>(() =>
            SimulationWorld객체표현규칙Validator.Validate(catalog));

        Assert.StartsWith(SimulationWorld객체표현규칙Validator.InvalidCode, error.Message);
    }

    [Fact]
    public void 관측근거가필요한규칙에파생근거만있으면선택하지않는다()
    {
        var catalog = Catalog(simulationActive: false);
        var request = Request(withSimulationState: false);
        request.Targets[0].EvidenceKindCode = "Derived";

        var ledger = SimulationWorld객체표현해석기.Interpret(request, catalog);

        Assert.Equal(
            SimulationWorld객체표현해석Codes.일치규칙없음,
            Assert.Single(ledger.Results).ResolutionCode);
    }

    [Fact]
    public async Task 규칙대장과해석결과는멱등불변실행본으로저장된다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        await AddSpatialRun(db);
        var store = new SimulationWorld객체표현규칙Store(db);
        var catalog = Catalog(simulationActive: true);
        var ledger = SimulationWorld객체표현해석기.Interpret(Request(withSimulationState: true), catalog);

        var catalogFirst = await store.규칙대장저장Async(catalog, CancellationToken.None);
        db.ChangeTracker.Clear();
        var catalogSecond = await store.규칙대장저장Async(catalog, CancellationToken.None);
        var resultFirst = await store.해석결과저장Async(ledger, CancellationToken.None);
        db.ChangeTracker.Clear();
        var resultSecond = await store.해석결과저장Async(ledger, CancellationToken.None);

        Assert.True(catalogFirst.Inserted);
        Assert.False(catalogSecond.Inserted);
        Assert.True(resultFirst.Inserted);
        Assert.False(resultSecond.Inserted);
        Assert.Equal(1, await db.ObjectRepresentationRuleCatalogs.CountAsync());
        Assert.Equal(1, await db.SpatialRuleMetadata.CountAsync());
        Assert.Equal(1, await db.SimulationRuleMetadata.CountAsync());
        Assert.Equal(2, await db.ObjectRepresentationBindingRules.CountAsync());
        Assert.Equal(1, await db.ObjectRepresentationInterpretationRuns.CountAsync());
        Assert.Equal(1, await db.ObjectRepresentationInterpretationResults.CountAsync());
    }

    [Fact]
    public async Task 공간출력Hash가실행본과다르면해석결과를저장하지않는다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        await AddSpatialRun(db);
        var store = new SimulationWorld객체표현규칙Store(db);
        var catalog = Catalog(simulationActive: false);
        await store.규칙대장저장Async(catalog, CancellationToken.None);
        var request = Request(withSimulationState: false);
        request.SpatialOutputHashSha256 = Hash('b');
        var ledger = SimulationWorld객체표현해석기.Interpret(request, catalog);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.해석결과저장Async(ledger, CancellationToken.None));

        Assert.Equal(SimulationWorld객체표현규칙Store.SpatialOutputMismatchCode, error.Message);
        Assert.Equal(0, await db.ObjectRepresentationInterpretationRuns.CountAsync());
    }

    [Fact]
    public async Task 저장결과가규칙대장의표현키와다르면거부한다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        await AddSpatialRun(db);
        var store = new SimulationWorld객체표현규칙Store(db);
        var catalog = Catalog(simulationActive: false);
        await store.규칙대장저장Async(catalog, CancellationToken.None);
        var ledger = SimulationWorld객체표현해석기.Interpret(Request(withSimulationState: false), catalog);
        ledger.Results[0].DefaultCompositionKey = "composition.forged.v1";
        ledger.OutputHashSha256 = SimulationWorld객체표현해석기.ComputeOutputHash(ledger);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.해석결과저장Async(ledger, CancellationToken.None));

        Assert.Equal(SimulationWorld객체표현규칙Store.InterpretationRuleMismatchCode, error.Message);
    }

    [Fact]
    public async Task 해석JobShell은공간실행의실제Node와규칙대장을만나게한다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        await AddSpatialRun(db);
        var store = new SimulationWorld객체표현규칙Store(db);
        var catalog = Catalog(simulationActive: false);
        await store.규칙대장저장Async(catalog, CancellationToken.None);
        var shell = new SimulationWorld객체표현해석JobShell(
            new SimulationWorld공간실행Reader(db), store);

        var result = await shell.실행Async(
            Request(withSimulationState: false), catalog, CancellationToken.None);

        Assert.True(result.Inserted);
        Assert.Equal(1, result.ResultCount);
        Assert.Equal(1, await db.ObjectRepresentationInterpretationResults.CountAsync());
    }

    [Fact]
    public void 새물리표는한글업무의미로분리된다()
    {
        using var db = CreateDb();

        Assert.Equal("시뮬레이션월드_객체표현규칙대장", db.Model.FindEntityType(typeof(SimulationWorld객체표현규칙CatalogEntity))!.GetTableName());
        Assert.Equal("시뮬레이션월드_공간규칙Metadata", db.Model.FindEntityType(typeof(SimulationWorld공간규칙MetadataEntity))!.GetTableName());
        Assert.Equal("시뮬레이션월드_Simulation규칙Metadata", db.Model.FindEntityType(typeof(SimulationWorldSimulation규칙MetadataEntity))!.GetTableName());
        Assert.Equal("시뮬레이션월드_객체표현결합규칙", db.Model.FindEntityType(typeof(SimulationWorld객체표현결합규칙Entity))!.GetTableName());
        Assert.Equal("시뮬레이션월드_객체표현해석실행", db.Model.FindEntityType(typeof(SimulationWorld객체표현해석RunEntity))!.GetTableName());
        Assert.Equal("시뮬레이션월드_객체표현해석결과", db.Model.FindEntityType(typeof(SimulationWorld객체표현해석ResultEntity))!.GetTableName());
    }

    [Fact]
    public void DI는객체표현규칙Store를등록한다()
    {
        var services = new ServiceCollection();
        services.AddSimulationWorldDerivationPersistence("Server=localhost;Database=test;User=test;Password=test");

        Assert.Contains(services, item =>
            item.ServiceType == typeof(ISimulationWorld객체표현규칙Store)
            && item.ImplementationType == typeof(SimulationWorld객체표현규칙Store));
    }

    private static SimulationWorld객체표현규칙대장 Catalog(bool simulationActive)
    {
        var simulationStatus = simulationActive ? SimulationWorld규칙상태Codes.활성 : SimulationWorld규칙상태Codes.초안;
        return new SimulationWorld객체표현규칙대장
        {
            CatalogRevision = simulationActive ? "object-presentation-rules.v2" : "object-presentation-rules.v1",
            CreatedAtUtc = DateTimeOffset.Parse("2026-08-13T04:00:00Z"),
            SpatialRules = new[]
            {
                new SimulationWorld공간규칙Metadata
                {
                    StableId = "spatial-rule:observed-logistics-warehouse", Revision = "r1",
                    StatusCode = SimulationWorld규칙상태Codes.활성, SpatialFactKindCode = "BuildingUse",
                    OperatorCode = "Equals", ExpectedValueCode = "LogisticsWarehouse",
                    RequiredEvidenceKindCode = "Observed", Description = "관측된 물류 창고 건물이다.",
                },
            },
            SimulationRules = new[]
            {
                new SimulationWorldSimulation규칙Metadata
                {
                    StableId = "simulation-rule:warehouse-loading", Revision = "r1",
                    StatusCode = simulationStatus, StateTypeCode = "WarehouseTaskState",
                    ExpectedStateCode = "Loading", Description = "창고 상차 작업 상태는 아직 규칙 확정 전에도 초안으로 보존할 수 있다.",
                },
            },
            BindingRules = new[]
            {
                new SimulationWorld객체표현결합규칙
                {
                    StableId = "binding-rule:warehouse-spatial-base", Revision = "r1",
                    StatusCode = SimulationWorld규칙상태Codes.활성, ObjectSemanticCode = "LogisticsWarehouse",
                    ScopeCode = SimulationWorld객체표현적용범위Codes.건물,
                    SpatialRuleStableId = "spatial-rule:observed-logistics-warehouse", SpatialRuleRevision = "r1",
                    MinimumEvidenceKindCode = "Observed", DefaultCompositionKey = "composition.logistics-warehouse.observed.v1",
                    UnmetRuleHandlingCode = SimulationWorld규칙미충족처리Codes.공간표현만, Priority = 10,
                },
                new SimulationWorld객체표현결합규칙
                {
                    StableId = "binding-rule:warehouse-loading", Revision = "r1", StatusCode = simulationStatus,
                    ObjectSemanticCode = "LogisticsWarehouse", ScopeCode = SimulationWorld객체표현적용범위Codes.건물,
                    SpatialRuleStableId = "spatial-rule:observed-logistics-warehouse", SpatialRuleRevision = "r1",
                    SimulationRuleStableId = "simulation-rule:warehouse-loading", SimulationRuleRevision = "r1",
                    SimulationRuleRequired = true, MinimumEvidenceKindCode = "Observed",
                    DefaultCompositionKey = "composition.logistics-warehouse.observed.v1",
                    DynamicIntentBundleKey = "intent-bundle.warehouse.loading-active.v1",
                    UnmetRuleHandlingCode = SimulationWorld규칙미충족처리Codes.공간표현만, Priority = 20,
                },
            },
        };
    }

    private static SimulationWorld객체표현해석요청 Request(bool withSimulationState) => new()
    {
        InterpretationStableId = "object-presentation-interpretation:test-v1",
        SpatialBuildStableId = "world-build:test-v1",
        SpatialOutputHashSha256 = Hash('a'),
        SimulationSessionStableId = withSimulationState ? "simulation-session:test-v1" : null,
        SimulationSessionRevision = withSimulationState ? 7 : null,
        WorldTick = withSimulationState ? 12 : null,
        RuleCatalogRevision = withSimulationState ? "object-presentation-rules.v2" : "object-presentation-rules.v1",
        InterpretedAtUtc = DateTimeOffset.Parse("2026-08-13T04:30:00Z"),
        Targets = new[]
        {
            new SimulationWorld객체표현대상사실
            {
                TargetNodeStableId = "building:test:warehouse-1", ObjectSemanticCode = "LogisticsWarehouse",
                ScopeCode = SimulationWorld객체표현적용범위Codes.건물, EvidenceKindCode = "Observed",
                MatchedSpatialRuleStableIds = new[] { "spatial-rule:observed-logistics-warehouse" },
                MatchedSimulationRuleStableIds = withSimulationState
                    ? new[] { "simulation-rule:warehouse-loading" } : Array.Empty<string>(),
            },
        },
    };

    private static SimulationWorld객체표현대상사실 SecondTarget() => new()
    {
        TargetNodeStableId = "building:test:warehouse-2", ObjectSemanticCode = "LogisticsWarehouse",
        ScopeCode = SimulationWorld객체표현적용범위Codes.건물, EvidenceKindCode = "Observed",
        MatchedSpatialRuleStableIds = new[] { "spatial-rule:observed-logistics-warehouse" },
        MatchedSimulationRuleStableIds = new[] { "simulation-rule:warehouse-loading" },
    };

    private static SimulationWorld파생DbContext CreateDb() => new(
        new DbContextOptionsBuilder<SimulationWorld파생DbContext>()
            .UseInMemoryDatabase("object-representation-rules-" + Guid.NewGuid().ToString("N")).Options);

    private static async Task AddSpatialRun(SimulationWorld파생DbContext db)
    {
        db.Runs.Add(new SimulationWorld파생RunEntity
        {
            SchemaVersion = 2, BuildStableId = "world-build:test-v1", AreaSetStableId = "area-set:test-v1",
            RecipeRevision = "recipe.r1", RuleRevision = "spatial.r1", Seed = 1,
            InputFingerprintSha256 = Hash('f'), OutputHashSha256 = Hash('a'),
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-13T03:00:00Z"), StoredAtUtc = DateTimeOffset.UtcNow,
        });
        db.Nodes.Add(new SimulationWorld파생NodeEntity
        {
            Run = db.Runs.Local.Single(), StableId = "building:test:warehouse-1", NodeKindCode = "Building",
            EvidenceKindCode = "Observed", DisplayName = "시험 물류 창고",
        });
        await db.SaveChangesAsync();
    }

    private static string Hash(char value) => new(value, 64);
}
