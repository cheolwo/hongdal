using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "지도구성·변화 기반 배치·실내 Overlay·v26 저장의 결정성과 호환을 자동 검증한다.",
    Boundary = "자동 시험은 SimulationWorldShell Play Mode·Game View E7 증거를 대신하지 않는다.")]
public sealed class SimulationWorldAssetPlacementTests
{
    [Fact]
    public void 분리된_LH공급자는_기존셀배치와Hash를보존한다()
    {
        var window = new SimulationLhWindowCell
        {
            CellKey = SimulationLhWorldService.L3CellKey(
                SimulationLhWorldService.CenterL3X,
                SimulationLhWorldService.CenterL3Y),
            CellX = SimulationLhWorldService.CenterL3X,
            CellY = SimulationLhWorldService.CenterL3Y,
            WindowRoleCode = SimulationLhWorldCodes.Detail,
            Priority = 1,
        };
        var context = new SimulationLhCellContentContext
        {
            Season = SimulationLhWorldService.CreateSeason(1),
            WorldRevision = 7,
        };

        var legacy = new ScenarioProceduralSimulationLhCellContentSource()
            .CreateCellPlan(window, context);
        var separated = new SimulationSeparatedLhCellContentSource()
            .CreateCellPlan(window, context);

        Assert.Equal(legacy.BasePlanHashSha256,
            separated.BasePlanHashSha256);
        Assert.Equal(legacy.PresentationHashSha256,
            separated.PresentationHashSha256);
        Assert.Equal(legacy.HBindings.Select(Canonical),
            separated.HBindings.Select(Canonical));
        Assert.Equal(legacy.Connectors.Select(Canonical),
            separated.Connectors.Select(Canonical));
        Assert.Equal(legacy.Placements.Select(Canonical),
            separated.Placements.Select(Canonical));
    }

    [Fact]
    public void 지도구성은_H와연결구와Anchor만소유하고_환경자산을선택하지않는다()
    {
        var cell = SimulationLhWorldService.L3CellKey(
            SimulationLhWorldService.CenterL3X,
            SimulationLhWorldService.CenterL3Y);

        var plan = new SimulationScenario지도구성Engine().Compose(
            new Simulation지도구성Request
            {
                WorldSeed = "world-seed:test",
                CellStableId = cell,
                CellX = SimulationLhWorldService.CenterL3X,
                CellY = SimulationLhWorldService.CenterL3Y,
                WindowRoleCode = SimulationLhWorldCodes.Detail,
                WorldRevision = 7,
            });

        Assert.NotEmpty(plan.HBindings);
        Assert.Equal(4, plan.Connectors.Length);
        Assert.Single(plan.Anchors);
        Assert.Equal(64, plan.MapPlanHashSha256.Length);
        Assert.DoesNotContain(plan.Anchors, value =>
            value.PreferredCompositionKey.StartsWith("nature:",
                StringComparison.Ordinal));
    }

    [Fact]
    public void 같은변화와Epoch는_같은환경발생과배치Hash를만든다()
    {
        var service = new SimulationNature세계자산배치Service();
        var state = NatureState(noiseEvents: 3, storedTimber: 4,
            cabinCompleted: false);

        var first = service.Compose("world-seed:nature", Cell,
            SimulationLhWorldCodes.Detail, 12, 2, state);
        var second = service.Compose("world-seed:nature", Cell,
            SimulationLhWorldCodes.Detail, 12, 2, state);

        Assert.Equal(first.StateHashSha256, second.StateHashSha256);
        Assert.Equal(first.SpawnDecisionPlans.Single()
                .DecisionPlanHashSha256,
            second.SpawnDecisionPlans.Single()
                .DecisionPlanHashSha256);
        Assert.Equal(first.AssetPlacementPlans.Single()
                .AssetPlacementPlanHashSha256,
            second.AssetPlacementPlans.Single()
                .AssetPlacementPlanHashSha256);
    }

    [Fact]
    public void 플레이어소음과거점방어는_적대흔적발생가중치를바꾼다()
    {
        var projection = new SimulationNature공간변화ProjectionBuilder();
        var spawn = new Ssalddel.Simulation.Domain
            .Simulation결정적환경발생DecisionEngine();
        var catalog = SimulationNature환경발생Catalog.Create();
        var quiet = projection.Build(NatureState(0, 0, false), Cell, 8);
        var noisy = projection.Build(NatureState(5, 0, false), Cell, 8);
        var defended = projection.Build(NatureState(5, 0, true), Cell, 8);

        var quietDecision = Hostile(spawn.Decide("seed", catalog,
            projection.CreateEnvironmentContext(quiet, 1)));
        var noisyDecision = Hostile(spawn.Decide("seed", catalog,
            projection.CreateEnvironmentContext(noisy, 1)));
        var defendedDecision = Hostile(spawn.Decide("seed", catalog,
            projection.CreateEnvironmentContext(defended, 1)));

        Assert.True(noisyDecision.EffectiveWeight
                    > quietDecision.EffectiveWeight);
        Assert.True(defendedDecision.EffectiveWeight
                    < noisyDecision.EffectiveWeight);
    }

    [Fact]
    public void 실내기본Plan은_보관량변화와분리되고_Overlay만달라진다()
    {
        var service = new SimulationNature세계자산배치Service();
        var empty = service.Compose("world-seed:interior", Cell,
            SimulationLhWorldCodes.Detail, 21, 3,
            NatureState(2, 0, true));
        var stored = service.Compose("world-seed:interior", Cell,
            SimulationLhWorldCodes.Detail, 21, 3,
            NatureState(2, 10, true));

        Assert.Single(empty.InteriorPlanBodies);
        Assert.Equal(empty.InteriorPlanBodies.Single().BodyHashSha256,
            stored.InteriorPlanBodies.Single().BodyHashSha256);
        Assert.DoesNotContain(empty.AssetPlacementPlans.Single().Placements,
            value => value.PlacementKindCode ==
                     Simulation세계자산배치Codes.InteriorOverlay);
        Assert.Contains(stored.AssetPlacementPlans.Single().Placements,
            value => value.PlacementKindCode ==
                     Simulation세계자산배치Codes.InteriorOverlay);
        Assert.NotEqual(empty.StateHashSha256, stored.StateHashSha256);
    }

    [Fact]
    public void 통합배치계획은_실외와실내실행계획으로_결정적으로분리된다()
    {
        var request = PlacementRequest(NatureState(3, 10, true));

        var result = new Simulation분리세계자산배치Coordinator()
            .Compose(request);

        Assert.NotEmpty(result.ExteriorPlan.Placements);
        Assert.Single(result.InteriorPlan.InteriorPlanBodies);
        Assert.DoesNotContain(result.ExteriorPlan.Placements, value =>
            value.PlacementKindCode ==
            Simulation세계자산배치Codes.InteriorOverlay);
        Assert.Contains(result.InteriorPlan.OverlayPlacements, value =>
            value.PlacementKindCode ==
            Simulation세계자산배치Codes.InteriorOverlay);
        Assert.Equal(result.CompatibilityPlan.AssetPlacementPlanHashSha256,
            result.ExteriorPlan.SourceCombinedPlanHashSha256);
        Assert.Equal(result.CompatibilityPlan.AssetPlacementPlanHashSha256,
            result.InteriorPlan.SourceCombinedPlanHashSha256);
        Assert.Equal(64, result.ExteriorPlan.ExteriorPlacementPlanHashSha256.Length);
        Assert.Equal(64, result.InteriorPlan.InteriorPlacementPlanHashSha256.Length);
    }

    [Fact]
    public void Nature셀조립은_LH지면과독립된_실외실내계획을같은Revision으로묶는다()
    {
        var state = NatureState(3, 10, true);
        state.ResourceNodes = new[]
        {
            new SimulationNatureResourceNodeSnapshot
            {
                ResourceNodeStableId = "resource:nature:tree:01",
                H1StableId = SimulationNatureSurvivalCodes.SafeClearingH1StableId,
                LocalX = -3,
                LocalZ = 4,
                StateCode = SimulationNatureSurvivalCodes.Standing,
            },
        };
        state.DroppedTimber = new[]
        {
            new SimulationNatureDroppedTimberSnapshot
            {
                DroppedTimberStableId = "drop:nature:timber:01",
                H1StableId = SimulationNatureSurvivalCodes.SafeClearingH1StableId,
                LocalX = -2,
                LocalZ = 4,
                Quantity = 3,
            },
        };
        var lh = LhCell(worldRevision: 37);
        var engine = new SimulationNatureWorldCellAssemblyEngine();

        var first = engine.Compose(lh, state, 37, true,
            "DeterministicValueNoise", new string('c', 64));
        var repeat = engine.Compose(lh, state, 37, true,
            "DeterministicValueNoise", new string('c', 64));
        var offset = engine.Compose(lh, state, 37, true,
            "DeterministicValueNoise", new string('c', 64),
            10d, -5d);

        Assert.True(first.IsAvailable);
        Assert.Equal(lh.BasePlanHashSha256,
            first.SourceLhBasePlanHashSha256);
        Assert.Equal(37, first.SourceWorldRevision);
        Assert.Equal(first.AssemblyHashSha256, repeat.AssemblyHashSha256);
        Assert.Contains(first.ExteriorPlacement.Placements, value =>
            value.PlacementStableId ==
            "nature-resource:resource:nature:tree:01"
            && value.CompositionKey == "Nature.Tree.Standing");
        Assert.Contains(first.ExteriorPlacement.Placements, value =>
            value.PlacementStableId ==
            "nature-dropped-timber:drop:nature:timber:01");
        Assert.Single(first.InteriorPlacement.InteriorPlanBodies);
        Assert.Contains(first.InteriorPlacement.InteriorPlanBodies[0].Placements,
            value => value.VisualKey == "Nature.Shelter.Bedroll");
        Assert.Contains(first.InteriorPlacement.OverlayPlacements,
            value => value.CompositionKey ==
                     "Nature.Storage.Timber.Half");
        Assert.Equal(64, first.AssemblyHashSha256.Length);
        var offsetTree = offset.ExteriorPlacement.Placements.Single(value =>
            value.CategoryCode == "NatureResourceNode");
        Assert.Equal(7d, offsetTree.LocalXMeters);
        Assert.Equal(-1d, offsetTree.LocalZMeters);
        Assert.NotEqual(first.AssemblyHashSha256,
            offset.AssemblyHashSha256);
    }

    [Fact]
    public void 벌목상태가바뀌어도_자원배치StableId는유지되고_표현과Hash만바뀐다()
    {
        var standing = NatureState(0, 0, false);
        standing.ResourceNodes = new[]
        {
            new SimulationNatureResourceNodeSnapshot
            {
                ResourceNodeStableId = "resource:nature:tree:stable",
                H1StableId = SimulationNatureSurvivalCodes.SafeClearingH1StableId,
                StateCode = SimulationNatureSurvivalCodes.Standing,
            },
        };
        var stump = NatureState(1, 0, false);
        stump.ResourceNodes = new[]
        {
            new SimulationNatureResourceNodeSnapshot
            {
                ResourceNodeStableId = "resource:nature:tree:stable",
                H1StableId = SimulationNatureSurvivalCodes.SafeClearingH1StableId,
                StateCode = SimulationNatureSurvivalCodes.Stump,
            },
        };
        var lh = LhCell(worldRevision: 41);
        var engine = new SimulationNatureWorldCellAssemblyEngine();

        var before = engine.Compose(lh, standing, 41, true);
        var after = engine.Compose(lh, stump, 41, true);
        var beforeTree = before.ExteriorPlacement.Placements.Single(value =>
            value.CategoryCode == "NatureResourceNode");
        var afterTree = after.ExteriorPlacement.Placements.Single(value =>
            value.CategoryCode == "NatureResourceNode");

        Assert.Equal(beforeTree.PlacementStableId,
            afterTree.PlacementStableId);
        Assert.Equal("Nature.Tree.Standing", beforeTree.CompositionKey);
        Assert.Equal("Nature.Tree.Stump", afterTree.CompositionKey);
        Assert.NotEqual(before.AssemblyHashSha256, after.AssemblyHashSha256);
    }

    [Fact]
    public void 하늘과지면상태는_실외배치Identity가아닌_표현Hash만바꾼다()
    {
        var separated = new Simulation분리세계자산배치Coordinator()
            .Compose(PlacementRequest(NatureState(3, 10, true)));
        var engine = new Simulation결정적실외환경표현Engine();
        var surfaceHash = new string('b', 64);

        var clear = engine.ComposePresentation(new Simulation실외환경표현Request
        {
            ExteriorPlacementPlan = separated.ExteriorPlan,
            SurfaceModeCode = "DeterministicValueNoise",
            SurfaceStateHashSha256 = surfaceHash,
            Atmosphere = new SimulationAtmosphereStateSnapshot
            {
                IsEnabled = true,
                RuleRevision = "world-atmosphere.r1",
                WeatherCode = "Clear",
            },
        });
        var rain = engine.ComposePresentation(new Simulation실외환경표현Request
        {
            ExteriorPlacementPlan = separated.ExteriorPlan,
            SurfaceModeCode = "DeterministicValueNoise",
            SurfaceStateHashSha256 = surfaceHash,
            Atmosphere = new SimulationAtmosphereStateSnapshot
            {
                IsEnabled = true,
                RuleRevision = "world-atmosphere.r1",
                WeatherCode = "Rain",
                PrecipitationPermille = 800,
                WindIntensityPermille = 400,
            },
        });

        Assert.Equal(clear.SourceExteriorPlacementPlanHashSha256,
            rain.SourceExteriorPlacementPlanHashSha256);
        Assert.Equal(clear.Placements.Select(value => value.PlacementStableId),
            rain.Placements.Select(value => value.PlacementStableId));
        Assert.NotEqual(clear.PresentationPlanHashSha256,
            rain.PresentationPlanHashSha256);
        Assert.All(clear.Placements, value =>
            Assert.Equal("Dry", value.SurfaceAppearanceCode));
        Assert.All(rain.Placements, value =>
            Assert.Equal("Wet", value.SurfaceAppearanceCode));
    }

    [Fact]
    public void SaveV26은_세계자산배치상태를봉인하고_복원한다()
    {
        var aggregate = new 경영SimulationSessionAggregate(
            CreateSessionRequest());
        var placement = new SimulationNature세계자산배치Service().Compose(
            "world-seed:save-v26", Cell,
            SimulationLhWorldCodes.Detail, aggregate.Revision, 0,
            NatureState(3, 10, true));

        var package = aggregate.CreateSavePackage(
            new SimulationSessionSaveRequest
            {
                SaveStableId = "simulation-save:test:world-assets-v26",
                ExpectedRevision = aggregate.Revision,
                WorldAssetPlacementState = placement,
            });
        placement.MapPlans[0].WorldSeed = "mutated-after-save";

        Assert.Equal(SimulationSaveSchemaVersions.V26,
            package.SchemaVersion);
        Assert.NotNull(package.WorldAssetPlacement);
        Assert.Equal("world-seed:save-v26",
            package.WorldAssetPlacement!.MapPlans.Single().WorldSeed);
        Assert.Equal(64, package.ReplayHash.Length);

        var restored = SimulationSessionReplay.Restore(package);
        var replayed = restored.CreateSavePackage(
            new SimulationSessionSaveRequest
            {
                SaveStableId = package.SaveStableId,
                ExpectedRevision = restored.Revision,
            });
        Assert.Equal(package.ReplayHash, replayed.ReplayHash);
        Assert.Equal(package.WorldAssetPlacement.StateHashSha256,
            replayed.WorldAssetPlacement!.StateHashSha256);
    }

    private const string Cell = "kr5186:l3:2801:4581";

    private static SimulationNatureSurvivalStateSnapshot NatureState(
        int noiseEvents, int storedTimber, bool cabinCompleted)
        => new()
        {
            IsEnabled = true,
            AreaSetStableId = SimulationNatureSurvivalCodes.AreaSetStableId,
            H3StableId = SimulationNatureSurvivalCodes.HomeH3StableId,
            CurrentH2StableId = SimulationNatureSurvivalCodes.HomeH2StableId,
            CurrentH1StableId =
                SimulationNatureSurvivalCodes.CabinSiteH1StableId,
            NoiseEventCount = noiseEvents,
            RawThreatTier = noiseEvents == 0 ? 0 : 1,
            EffectiveThreatTier = noiseEvents == 0 ? 0 : 1,
            StoredTimberQuantity = storedTimber,
            Cabin = new SimulationNatureCabinSnapshot
            {
                CabinStableId = "facility:nature-cabin",
                H1StableId =
                    SimulationNatureSurvivalCodes.CabinSiteH1StableId,
                StateCode = cabinCompleted
                    ? SimulationNatureSurvivalCodes.Completed
                    : SimulationNatureSurvivalCodes.Planned,
                LocalX = 2,
                LocalZ = 4,
                StorageCapacity = cabinCompleted ? 20 : 0,
                DefenseAvailable = cabinCompleted,
                RecoveryAvailable = cabinCompleted,
            },
        };

    private static Simulation세계자산배치Request PlacementRequest(
        SimulationNatureSurvivalStateSnapshot state)
    {
        var map = new SimulationScenario지도구성Engine().Compose(
            new Simulation지도구성Request
            {
                WorldSeed = "world-seed:separated-placement",
                CellStableId = Cell,
                CellX = SimulationLhWorldService.CenterL3X,
                CellY = SimulationLhWorldService.CenterL3Y,
                WindowRoleCode = SimulationLhWorldCodes.Detail,
                WorldRevision = 31,
            });
        var projection = new SimulationNature공간변화ProjectionBuilder();
        var changes = projection.Build(state, Cell, 31);
        return new Simulation세계자산배치Request
        {
            MapPlan = map,
            ChangeProjection = changes,
            SpawnRuleCatalog = SimulationNature환경발생Catalog.Create(),
            SpawnContext = projection.CreateEnvironmentContext(changes, 4),
            PreserveLegacyProceduralLayout = false,
        };
    }

    private static SimulationLhCellPlanResponse LhCell(long worldRevision)
    {
        var source = new SimulationSeparatedLhCellContentSource()
            .CreateCellPlan(new SimulationLhWindowCell
            {
                CellKey = Cell,
                CellX = SimulationLhWorldService.CenterL3X,
                CellY = SimulationLhWorldService.CenterL3Y,
                WindowRoleCode = SimulationLhWorldCodes.Detail,
                Priority = 0,
            }, new SimulationLhCellContentContext
            {
                WorldRevision = worldRevision,
                Season = new SimulationLhSeasonSnapshot
                {
                    SeasonCode = SimulationLhWorldCodes.Spring,
                    SeasonRuleVersion = "simulation-season.test.r1",
                },
            });
        Assert.Equal(64, source.BasePlanHashSha256.Length);
        return source;
    }

    private static 경영SimulationSession생성Request CreateSessionRequest()
        => new()
        {
            ClientRequestId = Guid.Parse(
                "ef43a393-621f-4b10-bb7e-d103f4f45ae1"),
            ScenarioStableId = "scenario:world-assets-v26",
            ScenarioDataRevision = "scenario-data:world-assets-r1",
            ScenarioSeed = 20260826,
            RuleRevision = "rule:world-assets-r1",
            DurationTicks = 112,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim:nature-v26",
                TerritoryStableId = "territory:sim:nature-v26",
                SettlementStableId = "settlement:sim:nature-v26",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 8, 26, 0, 0, 0, TimeSpan.Zero),
            },
        };

    private static Simulation환경발생Decision Hostile(
        Simulation환경발생DecisionPlan plan)
        => plan.Decisions.First(value => value.CandidateStableId.EndsWith(
            ":hostile-trace", StringComparison.Ordinal));

    private static string Canonical(SimulationLhHBindingResponse value)
        => string.Join("|", value.HLevelCode, value.SpatialStableId,
            value.StateCode, string.Join(",", value.WorldInteractionIds));

    private static string Canonical(SimulationLhConnectorResponse value)
        => string.Join("|", value.ConnectorStableId, value.SideCode,
            value.NeighborCellKey, value.BoundaryHashSha256,
            value.Passable);

    private static string Canonical(SimulationLhPlacementResponse value)
        => string.Join("|", value.GeneratedStableId, value.OwnerCellKey,
            value.LayerCode, value.CompositionKey, value.H1StableId,
            value.LocalXMeters, value.LocalZMeters, value.RotationDegrees,
            value.UniformScale, value.FixedAnchor,
            value.CollisionEligible, value.PresentationOnly);
}
