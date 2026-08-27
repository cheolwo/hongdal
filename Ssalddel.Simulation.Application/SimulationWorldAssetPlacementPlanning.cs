using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;
using Ssalddel.Interior.Domain;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "기존 LH 시나리오 지식에서 객체 선택 없는 지도구성 계획을 만든다.",
        StepKey = "application.world-map-composition",
        DependsOnStepKeys = new[] { "contract.world-map-composition" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 28,
        Boundary = "환경·건물·실내 자산과 Prefab을 선택하지 않는다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "H 결속·연결구·Anchor를 자산 선택 없는 지도 계획으로 조립한다.",
        Boundary = "지도 계획은 H 또는 WI의 실제 E5 발현을 대신하지 않는다.")]
    public sealed class SimulationScenario지도구성Engine
        : ISimulation지도구성Engine
    {
        private readonly SimulationLhSpatialKnowledgeProvider spatialKnowledge;

        public SimulationScenario지도구성Engine()
            : this(SimulationLhSpatialKnowledgeProvider.LoadEmbedded())
        {
        }

        internal SimulationScenario지도구성Engine(
            SimulationLhSpatialKnowledgeProvider provider)
        {
            spatialKnowledge = provider
                ?? throw new ArgumentNullException(nameof(provider));
        }

        public Simulation지도구성Plan Compose(
            Simulation지도구성Request request)
        {
            Validate(request);
            var plan = new Simulation지도구성Plan
            {
                GeneratorRevision = request.GeneratorRevision.Trim(),
                WorldSeed = request.WorldSeed.Trim(),
                CellStableId = request.CellStableId.Trim(),
                CellX = request.CellX,
                CellY = request.CellY,
                WindowRoleCode = request.WindowRoleCode.Trim(),
                SourceWorldRevision = request.WorldRevision,
                HBindings = CreateHBindings(request.CellX, request.CellY),
                Connectors = CreateConnectors(request.CellX, request.CellY),
                Anchors = CreateAnchors(request.CellStableId,
                    request.CellX, request.CellY),
                RequiredCapabilityCodes = request.RequiredCapabilityCodes
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };
            plan.MapPlanHashSha256 = Simulation세계자산CanonicalHash
                .ComputeMapPlanHash(plan);
            return plan;
        }

        private Simulation지도H결속Snapshot[] CreateHBindings(int x, int y)
        {
            var values = new List<Simulation지도H결속Snapshot>
            {
                Binding("H4", PyeongchangAreaSetStableIds.AreaSet,
                    SimulationLhWorldCodes.ApprovedReference),
                Binding("H3", PyeongchangAreaSetStableIds.FarmGraph,
                    SimulationLhWorldCodes.ApprovedReference),
                Binding("H2",
                    "landscape-block-candidate:sim:pyeongchang:daegwallyeong-harvest-day.v1",
                    SimulationLhWorldCodes.IdeaInventory),
            };
            if (Math.Abs(x - SimulationLhWorldService.CenterL3X) <= 1
                && Math.Abs(y - SimulationLhWorldService.CenterL3Y) <= 1)
            {
                foreach (var binding in spatialKnowledge.H1Bindings)
                    values.Add(Binding("H1", binding.InteractionH1Ref,
                        SimulationLhWorldCodes.ApprovedReference,
                        binding.WorldInteractionIds));
            }
            return values.ToArray();
        }

        private Simulation지도배치AnchorSnapshot[] CreateAnchors(
            string cellStableId, int x, int y)
        {
            var values = new List<Simulation지도배치AnchorSnapshot>();
            var packing = spatialKnowledge.GetSpace("packing-area");
            var production = spatialKnowledge.GetSpace("production-plot");
            var collection = spatialKnowledge.GetSpace("collection-area");
            var loading = spatialKnowledge.GetSpace("loading-area");
            AddAnchor(values, cellStableId, x, y,
                SimulationLhWorldService.CenterL3X,
                SimulationLhWorldService.CenterL3Y,
                "farmhouse", packing.PreferredCompositionKey,
                packing.InteractionH1Ref, -18d, 12d, 0d);
            AddAnchor(values, cellStableId, x, y,
                SimulationLhWorldService.CenterL3X + 1,
                SimulationLhWorldService.CenterL3Y,
                "potato-field", production.PreferredCompositionKey,
                production.InteractionH1Ref, 4d, -6d, 0d);
            AddAnchor(values, cellStableId, x, y,
                SimulationLhWorldService.CenterL3X,
                SimulationLhWorldService.CenterL3Y + 1,
                "work-yard", collection.PreferredCompositionKey,
                collection.InteractionH1Ref, 10d, 8d, 90d);
            AddAnchor(values, cellStableId, x, y,
                SimulationLhWorldService.CenterL3X + 1,
                SimulationLhWorldService.CenterL3Y + 1,
                "farm-gate", loading.PreferredCompositionKey,
                loading.InteractionH1Ref, -8d, -10d, 0d);
            return values.OrderBy(value => value.AnchorStableId,
                StringComparer.Ordinal).ToArray();
        }

        private static void AddAnchor(
            ICollection<Simulation지도배치AnchorSnapshot> values,
            string cellStableId,
            int x,
            int y,
            int expectedX,
            int expectedY,
            string roleCode,
            string compositionKey,
            string h1StableId,
            double localX,
            double localZ,
            double rotation)
        {
            if (x != expectedX || y != expectedY) return;
            var identity = Simulation세계자산CanonicalHash.Hash(
                cellStableId + "|H3Intent|" + roleCode);
            values.Add(new Simulation지도배치AnchorSnapshot
            {
                AnchorStableId = "lh-anchor:"
                    + identity.Substring(0, 24),
                AnchorRoleCode = roleCode,
                H1StableId = h1StableId,
                PreferredCompositionKey = compositionKey,
                LocalXMeters = localX,
                LocalZMeters = localZ,
                RotationDegrees = rotation,
                MaximumSlopeDegrees = 18d,
                FixedAnchor = true,
            });
        }

        private static Simulation지도연결구Snapshot[] CreateConnectors(
            int x, int y)
            => new[]
            {
                Connector(x, y, x, y + 1, "N"),
                Connector(x, y, x + 1, y, "E"),
                Connector(x, y, x, y - 1, "S"),
                Connector(x, y, x - 1, y, "W"),
            };

        private static Simulation지도연결구Snapshot Connector(
            int x, int y, int neighborX, int neighborY, string side)
        {
            var cell = SimulationLhWorldGrid.L3CellKey(x, y);
            var neighbor = SimulationLhWorldGrid.L3CellKey(
                neighborX, neighborY);
            var pair = new[] { cell, neighbor }
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var boundaryHash = Simulation세계자산CanonicalHash.Hash(
                pair[0] + "|" + pair[1] + "|connector.v1");
            return new Simulation지도연결구Snapshot
            {
                ConnectorStableId = "lh-connector:"
                    + boundaryHash.Substring(0, 24),
                SideCode = side,
                NeighborCellStableId = neighbor,
                BoundaryHashSha256 = boundaryHash,
                Passable = SimulationLhWorldGrid.IsInsideApprovedCoverage(
                    neighborX, neighborY),
            };
        }

        private static Simulation지도H결속Snapshot Binding(
            string level,
            string stableId,
            string state,
            string[]? worldInteractionIds = null)
            => new Simulation지도H결속Snapshot
            {
                HLevelCode = level,
                SpatialStableId = stableId,
                StateCode = state,
                WorldInteractionIds = worldInteractionIds
                    ?? Array.Empty<string>(),
            };

        private static void Validate(Simulation지도구성Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.WorldSeed)
                || string.IsNullOrWhiteSpace(request.GeneratorRevision)
                || string.IsNullOrWhiteSpace(request.CellStableId)
                || string.IsNullOrWhiteSpace(request.WindowRoleCode)
                || request.WorldRevision < 0
                || request.RequiredCapabilityCodes == null
                || !SimulationLhWorldGrid.TryParseL3CellKey(
                    request.CellStableId, out var parsedX, out var parsedY)
                || parsedX != request.CellX || parsedY != request.CellY)
                throw new ArgumentException(
                    "SimulationWorldMapCompositionRequestInvalid",
                    nameof(request));
        }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E4,
        "Nature 권위 결과를 WI·대상·H·revision이 있는 공간 변화 사실로 결속한다.",
        Boundary = "Projection은 원본 권위 상태를 변경하지 않는다.")]
    public sealed class SimulationNature공간변화ProjectionBuilder
    {
        public Simulation공간변화ProjectionSnapshot Build(
            SimulationNatureSurvivalStateSnapshot state,
            string cellStableId,
            long worldRevision)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!state.IsEnabled
                || string.IsNullOrWhiteSpace(state.AreaSetStableId)
                || string.IsNullOrWhiteSpace(cellStableId)
                || worldRevision < 0)
                throw new ArgumentException(
                    "SimulationNatureSpatialChangeProjectionInputInvalid");

            var facts = new List<Simulation공간변화FactSnapshot>();
            AddTier(facts, state, cellStableId, worldRevision,
                Simulation세계자산배치Codes.NoiseTier,
                NoiseTier(state.NoiseEventCount));
            AddTier(facts, state, cellStableId, worldRevision,
                Simulation세계자산배치Codes.SafetyTier,
                state.Cabin.DefenseAvailable ? 2
                    : state.Cabin.RecoveryAvailable ? 1 : 0);
            AddTier(facts, state, cellStableId, worldRevision,
                Simulation세계자산배치Codes.ResourcePressureTier,
                state.ResourceNodes.Count(value => string.Equals(
                    value.StateCode, SimulationNatureSurvivalCodes.Stump,
                    StringComparison.Ordinal)));
            AddTier(facts, state, cellStableId, worldRevision,
                Simulation세계자산배치Codes.StorageFillTier,
                StorageTier(state.StoredTimberQuantity,
                    state.Cabin.StorageCapacity),
                state.Cabin.CabinStableId,
                state.Cabin.H1StableId,
                state.Cabin.LocalX,
                state.Cabin.LocalZ,
                state.Cabin.YawDegrees,
                SimulationNatureSurvivalCodes.StoreAtCabinWorldInteractionId);

            foreach (var node in state.ResourceNodes.OrderBy(value =>
                         value.ResourceNodeStableId, StringComparer.Ordinal))
                facts.Add(Fact(state.AreaSetStableId, cellStableId,
                    worldRevision, "NatureResourceNode", node.StateCode,
                    node.ResourceNodeStableId, node.H1StableId,
                    node.LocalX, node.LocalZ, 0d, 1,
                    SimulationNatureSurvivalCodes
                        .BeginHarvestWorldInteractionId));
            foreach (var timber in state.DroppedTimber.OrderBy(value =>
                         value.DroppedTimberStableId,
                         StringComparer.Ordinal))
                facts.Add(Fact(state.AreaSetStableId, cellStableId,
                    worldRevision, "NatureDroppedTimber", timber.StateCode,
                    timber.DroppedTimberStableId, timber.H1StableId,
                    timber.LocalX, timber.LocalZ, 0d, timber.Quantity,
                    SimulationNatureSurvivalCodes
                        .CollectDroppedTimberWorldInteractionId));

            if (!string.IsNullOrWhiteSpace(state.Cabin.CabinStableId))
                facts.Add(Fact(state.AreaSetStableId, cellStableId,
                    worldRevision, "NatureCabin", state.Cabin.StateCode,
                    state.Cabin.CabinStableId, state.Cabin.H1StableId,
                    state.Cabin.LocalX, state.Cabin.LocalZ,
                    state.Cabin.YawDegrees, 1,
                    SimulationNatureSurvivalCodes
                        .BeginCabinBuildWorldInteractionId));

            var constructionTier = 0;
            foreach (var node in state.BuildingProgression?.Nodes
                         ?? Array.Empty<Simulation건물발전NodeSnapshot>())
            {
                if (string.Equals(node.StateCode,
                        Simulation영역건물발전Codes.Building,
                        StringComparison.Ordinal))
                    constructionTier = Math.Max(constructionTier, 1);
                else if (string.Equals(node.StateCode,
                             Simulation영역건물발전Codes.Operational,
                             StringComparison.Ordinal))
                    constructionTier = Math.Max(constructionTier, 2);
                facts.Add(Fact(state.AreaSetStableId, cellStableId,
                    worldRevision, "AreaBuilding", node.StateCode,
                    node.FacilityStableId, node.H1StableId,
                    node.LocalX, node.LocalZ, node.YawDegrees, 1,
                    Simulation영역건물발전Codes
                        .ConstructionWorldInteractionId,
                    node.BlueprintStableId));
            }
            AddTier(facts, state, cellStableId, worldRevision,
                Simulation세계자산배치Codes.ConstructionTier,
                constructionTier);

            if (state.Encounter != null)
                facts.Add(Fact(state.AreaSetStableId, cellStableId,
                    worldRevision, "NatureEncounter",
                    state.Encounter.StateCode,
                    state.Encounter.EncounterStableId,
                    state.CurrentH1StableId, 0d, 0d, 0d,
                    state.Encounter.HostileCount,
                    SimulationNatureSurvivalCodes
                        .ResolveEncounterWorldInteractionId));

            var projection = new Simulation공간변화ProjectionSnapshot
            {
                AreaSetStableId = state.AreaSetStableId.Trim(),
                CellStableId = cellStableId.Trim(),
                SourceWorldRevision = worldRevision,
                Facts = facts.OrderBy(value => value.ChangeStableId,
                    StringComparer.Ordinal).ToArray(),
            };
            projection.ProjectionHashSha256 =
                Simulation세계자산CanonicalHash
                    .ComputeChangeProjectionHash(projection);
            return projection;
        }

        public Simulation환경발생ContextSnapshot CreateEnvironmentContext(
            Simulation공간변화ProjectionSnapshot projection,
            int spawnEpoch)
        {
            if (projection == null)
                throw new ArgumentNullException(nameof(projection));
            var tierCodes = new[]
            {
                Simulation세계자산배치Codes.NoiseTier,
                Simulation세계자산배치Codes.SafetyTier,
                Simulation세계자산배치Codes.ResourcePressureTier,
                Simulation세계자산배치Codes.RecoveryTier,
                Simulation세계자산배치Codes.ConstructionTier,
                Simulation세계자산배치Codes.StorageFillTier,
                Simulation세계자산배치Codes.SettlementActivityTier,
            };
            return new Simulation환경발생ContextSnapshot
            {
                AreaSetStableId = projection.AreaSetStableId,
                CellStableId = projection.CellStableId,
                SpawnEpoch = spawnEpoch,
                SourceWorldRevision = projection.SourceWorldRevision,
                SourceChangeProjectionHashSha256 =
                    projection.ProjectionHashSha256,
                Values = tierCodes.Select(code =>
                    new Simulation환경발생ContextValueSnapshot
                    {
                        ContextCode = code,
                        Value = projection.Facts
                            .Where(value => value.ChangeCode == code)
                            .Select(value => value.ChangeValue)
                            .DefaultIfEmpty(0).Max(),
                    }).ToArray(),
            };
        }

        private static void AddTier(
            ICollection<Simulation공간변화FactSnapshot> facts,
            SimulationNatureSurvivalStateSnapshot state,
            string cellStableId,
            long revision,
            string code,
            int value,
            string targetStableId = "",
            string spatialStableId = "",
            double localX = 0d,
            double localZ = 0d,
            double rotation = 0d,
            string worldInteractionId = "")
            => facts.Add(Fact(state.AreaSetStableId, cellStableId,
                revision, code, value.ToString(CultureInfo.InvariantCulture),
                string.IsNullOrWhiteSpace(targetStableId)
                    ? state.AreaSetStableId : targetStableId,
                spatialStableId, localX, localZ, rotation, value,
                worldInteractionId, changeValue: value));

        private static Simulation공간변화FactSnapshot Fact(
            string areaSetStableId,
            string cellStableId,
            long revision,
            string changeCode,
            string stateCode,
            string targetStableId,
            string spatialStableId,
            double localX,
            double localZ,
            double rotation,
            int quantity,
            string worldInteractionId,
            string effectCode = "",
            int changeValue = 0)
        {
            var fingerprint = string.Join("|", new[]
            {
                areaSetStableId,
                cellStableId,
                revision.ToString(CultureInfo.InvariantCulture),
                changeCode,
                targetStableId,
                stateCode,
                changeValue.ToString(CultureInfo.InvariantCulture),
                quantity.ToString(CultureInfo.InvariantCulture),
            });
            return new Simulation공간변화FactSnapshot
            {
                ChangeStableId = "spatial-change:"
                    + Simulation세계자산CanonicalHash.Hash(fingerprint)
                        .Substring(0, 24),
                TriggerSourceCode = string.IsNullOrWhiteSpace(
                    worldInteractionId)
                    ? Simulation세계자산배치Codes.WorldDerived
                    : Simulation세계자산배치Codes.PlayerDriven,
                WorldInteractionId = worldInteractionId,
                EffectCode = effectCode,
                AreaSetStableId = areaSetStableId,
                SpatialStableId = spatialStableId,
                TargetStableId = targetStableId,
                ChangeCode = changeCode,
                ChangeValue = changeValue,
                Quantity = quantity,
                StateCode = stateCode,
                LocalXMeters = localX,
                LocalZMeters = localZ,
                RotationDegrees = rotation,
                FormationModeCode =
                    Simulation세계자산배치Codes.HybridEvolving,
                AppliedWorldRevision = revision,
            };
        }

        private static int StorageTier(int quantity, int capacity)
        {
            if (quantity <= 0 || capacity <= 0) return 0;
            return quantity >= capacity ? 3
                : quantity * 2 >= capacity ? 2 : 1;
        }

        private static int NoiseTier(int noiseEventCount)
        {
            if (noiseEventCount <= 0) return 0;
            if (noiseEventCount <= 2) return 1;
            return noiseEventCount <= 4 ? 2 : 3;
        }
    }

    public static class SimulationNature환경발생Catalog
    {
        public static Simulation환경발생RuleCatalog Create()
        {
            var catalog = new Simulation환경발생RuleCatalog
            {
                Candidates = new[]
                {
                    Candidate("forest-edge", "Vegetation",
                        "nature:숲 가장자리:A", 6500, 2,
                        Modifier(Simulation세계자산배치Codes
                            .ResourcePressureTier, 1, -1200)),
                    Candidate("open-grass", "Vegetation",
                        "nature:초지·야생화:A", 2200, 2,
                        Modifier(Simulation세계자산배치Codes
                            .ResourcePressureTier, 1, 900),
                        Modifier(Simulation세계자산배치Codes
                            .RecoveryTier, 1, 400)),
                    Candidate("stump-debris", "RecoveryTrace",
                        "nature:그루터기·가지:A", 250, 2,
                        Modifier(Simulation세계자산배치Codes
                            .ResourcePressureTier, 1, 2200)),
                    Candidate("hostile-trace", "ThreatTrace",
                        "nature:적대 흔적:A", 150, 2,
                        Modifier(Simulation세계자산배치Codes.NoiseTier,
                            1, 1700),
                        Modifier(Simulation세계자산배치Codes.SafetyTier,
                            1, -1400)),
                    Candidate("construction-trace", "ConstructionTrace",
                        "nature:공사 자재:A", 0, 2,
                        Modifier(Simulation세계자산배치Codes
                            .ConstructionTier, 1, 4000)),
                },
            };
            catalog.CatalogHashSha256 = Simulation세계자산CanonicalHash
                .ComputeSpawnCatalogHash(catalog);
            return catalog;
        }

        private static Simulation환경발생Candidate Candidate(
            string suffix,
            string category,
            string compositionKey,
            int baseWeight,
            int maximumInstances,
            params Simulation환경발생WeightModifier[] modifiers)
            => new Simulation환경발생Candidate
            {
                CandidateStableId = "environment-candidate:nature:"
                    + suffix,
                CategoryCode = category,
                CompositionKey = compositionKey,
                AuthorityKindCode =
                    Simulation세계자산배치Codes.AmbientPresentation,
                PersistenceKindCode =
                    Simulation세계자산배치Codes.Transient,
                BaseWeight = baseWeight,
                MaximumInstancesPerCell = maximumInstances,
                MinimumSpacingMeters = 8d,
                WeightModifiers = modifiers,
                PresentationOnly = true,
            };

        private static Simulation환경발생WeightModifier Modifier(
            string contextCode, int minimumValue, int delta)
            => new Simulation환경발생WeightModifier
            {
                ContextCode = contextCode,
                MinimumContextValue = minimumValue,
                WeightDeltaPerStep = delta,
            };
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "지도·공간 변화·결정적 Spawn에서 환경·건물·실내 계획을 조립한다.",
        StepKey = "application.world-asset-placement",
        DependsOnStepKeys = new[] {
            "application.world-map-composition",
            "domain.environment-spawn-decision",
            "domain.interior-layout-generate"
        },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.SimulationState
                    | SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 29,
        Boundary = "권위 Spawn과 건물 상태를 만들지 않고 LH 상세도와 Prefab을 결정하지 않는다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E5,
        "지도·권위 변화·환경 결정에서 야외·건물·실내 배치 계획을 발현한다.",
        Boundary = "계획 발현은 실제 Unity Scene·Game View E7 증거가 아니다.")]
    public sealed class Simulation결정적세계자산배치Engine
        : ISimulation세계자산배치Engine
    {
        private readonly ISimulation환경발생DecisionEngine spawnEngine;
        private readonly I실내공간조립Engine interiorEngine;

        public Simulation결정적세계자산배치Engine()
            : this(new Simulation결정적환경발생DecisionEngine(),
                new DeterministicInteriorLayoutEngine())
        {
        }

        public Simulation결정적세계자산배치Engine(
            ISimulation환경발생DecisionEngine environmentSpawnEngine,
            I실내공간조립Engine interiorLayoutEngine)
        {
            spawnEngine = environmentSpawnEngine
                ?? throw new ArgumentNullException(
                    nameof(environmentSpawnEngine));
            interiorEngine = interiorLayoutEngine
                ?? throw new ArgumentNullException(nameof(interiorLayoutEngine));
        }

        public Simulation세계자산배치Plan Compose(
            Simulation세계자산배치Request request)
        {
            Validate(request);
            var placements = request.MapPlan.Anchors.Select(anchor =>
                new Simulation세계자산PlacementSnapshot
                {
                    PlacementStableId = anchor.AnchorStableId,
                    OwnerCellStableId = request.MapPlan.CellStableId,
                    PlacementKindCode =
                        Simulation세계자산배치Codes.MapAnchor,
                    LayerCode = "H3Intent",
                    CategoryCode = anchor.AnchorRoleCode,
                    CompositionKey = anchor.PreferredCompositionKey,
                    H1StableId = anchor.H1StableId,
                    AuthorityKindCode =
                        Simulation세계자산배치Codes.DerivedWorldProp,
                    PersistenceKindCode =
                        Simulation세계자산배치Codes.DerivedPersistent,
                    LocalXMeters = anchor.LocalXMeters,
                    LocalZMeters = anchor.LocalZMeters,
                    RotationDegrees = anchor.RotationDegrees,
                    FixedAnchor = anchor.FixedAnchor,
                    PresentationOnly = true,
                }).ToList();

            Simulation환경발생DecisionPlan spawnPlan;
            if (request.PreserveLegacyProceduralLayout)
            {
                AddLegacyProceduralPlacements(request, placements);
                spawnPlan = EmptySpawnPlan(request);
            }
            else
            {
                spawnPlan = spawnEngine.Decide(request.MapPlan.WorldSeed,
                    request.SpawnRuleCatalog, request.SpawnContext);
                AddAdaptiveSpawnPlacements(request, spawnPlan, placements);
                AddAuthorityProjectionPlacements(request, placements);
                AddChangeOverlayPlacements(request, placements);
            }

            var handles = new List<SimulationInteriorPlanHandleSnapshot>();
            var bodies = new List<SimulationInteriorPlacementPlanBodySnapshot>();
            if (!request.PreserveLegacyProceduralLayout)
                AddInteriorPlans(request, placements, handles, bodies);

            var plan = new Simulation세계자산배치Plan
            {
                RuleRevision = request.AssetRuleRevision.Trim(),
                CellStableId = request.MapPlan.CellStableId,
                SourceWorldRevision = request.MapPlan.SourceWorldRevision,
                MapPlanHashSha256 = request.MapPlan.MapPlanHashSha256,
                ChangeProjectionHashSha256 = request.ChangeProjection
                    .ProjectionHashSha256,
                SpawnDecisionPlanHashSha256 =
                    spawnPlan.DecisionPlanHashSha256,
                Placements = placements.OrderBy(value =>
                    value.PlacementStableId, StringComparer.Ordinal).ToArray(),
                InteriorPlanHandles = handles.OrderBy(value =>
                    value.BuildingPlacementStableId,
                    StringComparer.Ordinal).ToArray(),
                InteriorPlanBodies = bodies.OrderBy(value =>
                    value.BuildingPlacementStableId,
                    StringComparer.Ordinal).ToArray(),
            };
            plan.AssetPlacementPlanHashSha256 =
                Simulation세계자산CanonicalHash
                    .ComputeAssetPlacementPlanHash(plan);
            return plan;
        }

        private static void AddLegacyProceduralPlacements(
            Simulation세계자산배치Request request,
            ICollection<Simulation세계자산PlacementSnapshot> placements)
        {
            var keys = request.CompatibilityCompositionKeys;
            if (keys.Length == 0)
                throw new ArgumentException(
                    "SimulationLegacyCompositionKeysMissing");
            var bytes = HashBytes(string.Join("|", new[]
            {
                request.MapPlan.WorldSeed,
                SimulationLhWorldCodes.GeneratorVersion,
                "L3Surface",
                request.MapPlan.CellStableId,
            }));
            var count = 3 + bytes[0] % 3;
            for (var index = 0; index < count; index++)
            {
                var offset = 1 + index * 5;
                var key = keys[bytes[offset] % keys.Length];
                var identity = Simulation세계자산CanonicalHash.Hash(
                    request.MapPlan.CellStableId + "|L3Surface|"
                    + index + "|" + key);
                placements.Add(new Simulation세계자산PlacementSnapshot
                {
                    PlacementStableId = "lh-object:"
                        + identity.Substring(0, 24),
                    OwnerCellStableId = request.MapPlan.CellStableId,
                    PlacementKindCode =
                        Simulation세계자산배치Codes.Environment,
                    LayerCode = "L3Surface",
                    CategoryCode = "LegacyProcedural",
                    CompositionKey = key,
                    H1StableId = key.StartsWith("farm:",
                        StringComparison.Ordinal)
                        ? "h1-stock:farm-production" : string.Empty,
                    AuthorityKindCode =
                        Simulation세계자산배치Codes.AmbientPresentation,
                    PersistenceKindCode =
                        Simulation세계자산배치Codes.Transient,
                    LocalXMeters = -50d + bytes[offset + 1] / 255d * 100d,
                    LocalZMeters = -50d + bytes[offset + 2] / 255d * 100d,
                    RotationDegrees = bytes[offset + 3] / 255d * 360d,
                    UniformScale = .85d + bytes[offset + 4] / 255d * .3d,
                    PresentationOnly = true,
                });
            }
        }

        private static void AddAdaptiveSpawnPlacements(
            Simulation세계자산배치Request request,
            Simulation환경발생DecisionPlan spawnPlan,
            ICollection<Simulation세계자산PlacementSnapshot> placements)
        {
            var candidates = request.SpawnRuleCatalog.Candidates.ToDictionary(
                value => value.CandidateStableId,
                StringComparer.Ordinal);
            foreach (var decision in spawnPlan.Decisions.Where(value =>
                         value.Selected && !value.RequiresWorldTickCommit))
            {
                var bytes = HashBytes(decision.DecisionStableId);
                var candidate = candidates[decision.CandidateStableId];
                var x = -50d + bytes[0] / 255d * 100d;
                var z = -50d + bytes[1] / 255d * 100d;
                if (placements.Any(value => Distance(value.LocalXMeters,
                            value.LocalZMeters, x, z)
                        < candidate.MinimumSpacingMeters))
                    continue;
                placements.Add(new Simulation세계자산PlacementSnapshot
                {
                    PlacementStableId = "world-asset:"
                        + Simulation세계자산CanonicalHash.Hash(
                            decision.DecisionStableId + "|placement")
                            .Substring(0, 24),
                    OwnerCellStableId = request.MapPlan.CellStableId,
                    PlacementKindCode =
                        Simulation세계자산배치Codes.Environment,
                    LayerCode = "EnvironmentOverlay",
                    CategoryCode = decision.CategoryCode,
                    CompositionKey = decision.CompositionKey,
                    AuthorityKindCode = decision.AuthorityKindCode,
                    PersistenceKindCode = decision.PersistenceKindCode,
                    LocalXMeters = x,
                    LocalZMeters = z,
                    RotationDegrees = bytes[2] / 255d * 360d,
                    UniformScale = .85d + bytes[3] / 255d * .3d,
                    PresentationOnly = decision.PresentationOnly,
                    SourceSpawnDecisionStableId =
                        decision.DecisionStableId,
                });
            }
        }

        private static void AddAuthorityProjectionPlacements(
            Simulation세계자산배치Request request,
            ICollection<Simulation세계자산PlacementSnapshot> placements)
        {
            foreach (var fact in request.ChangeProjection.Facts)
            {
                var compositionKey = fact.ChangeCode switch
                {
                    "NatureResourceNode" when string.Equals(fact.StateCode,
                        SimulationNatureSurvivalCodes.Standing,
                        StringComparison.Ordinal) =>
                        "nature:침엽수림 군집:A",
                    "NatureResourceNode" when string.Equals(fact.StateCode,
                        SimulationNatureSurvivalCodes.Stump,
                        StringComparison.Ordinal) =>
                        "nature:그루터기:A",
                    "NatureDroppedTimber" when string.Equals(fact.StateCode,
                        SimulationNatureSurvivalCodes
                            .DroppedTimberAvailable,
                        StringComparison.Ordinal) =>
                        "nature:통나무 묶음:A",
                    "NatureCabin" when string.Equals(fact.StateCode,
                        SimulationNatureSurvivalCodes.Completed,
                        StringComparison.Ordinal) =>
                        "nature:오두막:A",
                    "NatureCabin" when string.Equals(fact.StateCode,
                        SimulationNatureSurvivalCodes.Building,
                        StringComparison.Ordinal) =>
                        "nature:오두막 공사:A",
                    "AreaBuilding" when string.Equals(fact.StateCode,
                        Simulation영역건물발전Codes.Operational,
                        StringComparison.Ordinal) =>
                        "nature:운영 건물:A",
                    "AreaBuilding" when string.Equals(fact.StateCode,
                        Simulation영역건물발전Codes.Building,
                        StringComparison.Ordinal) =>
                        "nature:건설 중 건물:A",
                    _ => string.Empty,
                };
                if (compositionKey.Length == 0) continue;
                placements.Add(new Simulation세계자산PlacementSnapshot
                {
                    PlacementStableId = "authority-asset:"
                        + Simulation세계자산CanonicalHash.Hash(
                            fact.ChangeStableId + "|authority-placement")
                            .Substring(0, 24),
                    OwnerCellStableId = request.MapPlan.CellStableId,
                    PlacementKindCode = fact.ChangeCode == "NatureCabin"
                        || fact.ChangeCode == "AreaBuilding"
                        ? Simulation세계자산배치Codes.Building
                        : Simulation세계자산배치Codes.Environment,
                    LayerCode = "AuthoritativeWorldState",
                    CategoryCode = fact.ChangeCode,
                    CompositionKey = compositionKey,
                    H1StableId = fact.SpatialStableId,
                    AuthorityKindCode =
                        Simulation세계자산배치Codes.SimulationEntity,
                    PersistenceKindCode =
                        Simulation세계자산배치Codes.Persistent,
                    StateCode = fact.StateCode,
                    LocalXMeters = fact.LocalXMeters,
                    LocalZMeters = fact.LocalZMeters,
                    RotationDegrees = fact.RotationDegrees,
                    CollisionEligible = fact.ChangeCode == "NatureCabin"
                        || fact.ChangeCode == "AreaBuilding",
                    PresentationOnly = false,
                    SourceChangeStableIds =
                        new[] { fact.ChangeStableId },
                });
            }
        }

        private static void AddChangeOverlayPlacements(
            Simulation세계자산배치Request request,
            ICollection<Simulation세계자산PlacementSnapshot> placements)
        {
            var storage = request.ChangeProjection.Facts.SingleOrDefault(
                value => value.ChangeCode ==
                    Simulation세계자산배치Codes.StorageFillTier);
            if (storage != null && storage.ChangeValue > 0)
                placements.Add(Overlay(request, storage,
                    storage.ChangeValue >= 3
                        ? "nature:보관 통나무:full"
                        : storage.ChangeValue == 2
                            ? "nature:보관 통나무:half"
                            : "nature:보관 통나무:small",
                    "StoredMaterialOverlay", .8d, .4d));

            foreach (var building in request.ChangeProjection.Facts.Where(
                         value => value.ChangeCode == "AreaBuilding"))
            {
                if (string.Equals(building.StateCode,
                        Simulation영역건물발전Codes.Building,
                        StringComparison.Ordinal))
                    placements.Add(Overlay(request, building,
                        "nature:공사 자재:A", "ConstructionMaterialOverlay",
                        1.2d, -.6d));
                else if (string.Equals(building.StateCode,
                             Simulation영역건물발전Codes.Operational,
                             StringComparison.Ordinal))
                    placements.Add(Overlay(request, building,
                        "nature:작업 공구:A", "WorkToolOverlay",
                        .7d, -.4d));
            }
        }

        private static Simulation세계자산PlacementSnapshot Overlay(
            Simulation세계자산배치Request request,
            Simulation공간변화FactSnapshot fact,
            string compositionKey,
            string category,
            double offsetX,
            double offsetZ)
            => new Simulation세계자산PlacementSnapshot
            {
                PlacementStableId = "world-overlay:"
                    + Simulation세계자산CanonicalHash.Hash(
                        fact.ChangeStableId + "|" + compositionKey)
                        .Substring(0, 24),
                OwnerCellStableId = request.MapPlan.CellStableId,
                PlacementKindCode =
                    Simulation세계자산배치Codes.ExteriorOverlay,
                LayerCode = "ChangeOverlay",
                CategoryCode = category,
                CompositionKey = compositionKey,
                H1StableId = fact.SpatialStableId,
                AuthorityKindCode =
                    Simulation세계자산배치Codes.DerivedWorldProp,
                PersistenceKindCode =
                    Simulation세계자산배치Codes.DerivedPersistent,
                StateCode = fact.StateCode,
                LocalXMeters = fact.LocalXMeters + offsetX,
                LocalZMeters = fact.LocalZMeters + offsetZ,
                RotationDegrees = fact.RotationDegrees,
                PresentationOnly = true,
                SourceChangeStableIds = new[] { fact.ChangeStableId },
            };

        private void AddInteriorPlans(
            Simulation세계자산배치Request request,
            ICollection<Simulation세계자산PlacementSnapshot> placements,
            ICollection<SimulationInteriorPlanHandleSnapshot> handles,
            ICollection<SimulationInteriorPlacementPlanBodySnapshot> bodies)
        {
            var cabin = request.ChangeProjection.Facts.SingleOrDefault(value =>
                value.ChangeCode == "NatureCabin"
                && string.Equals(value.StateCode,
                    SimulationNatureSurvivalCodes.Completed,
                    StringComparison.Ordinal));
            if (cabin == null) return;
            var interiorPlan = interiorEngine.Generate(
                NatureCabinInteriorGrammar.CreateRequest(
                    request.MapPlan.WorldSeed,
                    cabin.TargetStableId,
                    cabin.SpatialStableId));
            var handle = new InteriorPlacementPlanCatalog().Pin(interiorPlan);
            handles.Add(ToSnapshot(handle));
            var body = ToBody(interiorPlan);
            bodies.Add(body);

            var storageTier = request.ChangeProjection.Facts.SingleOrDefault(
                value => value.ChangeCode ==
                    Simulation세계자산배치Codes.StorageFillTier);
            if (storageTier != null && storageTier.ChangeValue > 0)
            {
                var storageSurface = interiorPlan.Placements
                    .Where(value => value.ZoneStableId ==
                                    NatureCabinInteriorGrammar.StorageZone
                                    && value.PlacementLayerCode ==
                                    InteriorLayoutCodes.Surface)
                    .OrderBy(value => value.PlacementStableId,
                        StringComparer.Ordinal).FirstOrDefault();
                if (storageSurface != null)
                    placements.Add(InteriorOverlay(request, storageTier,
                        storageSurface.PlacementStableId,
                        storageTier.ChangeValue >= 3
                            ? "Nature.Storage.Timber.Full"
                            : storageTier.ChangeValue == 2
                                ? "Nature.Storage.Timber.Half"
                                : "Nature.Storage.Timber.Small",
                        "StoredMaterialOverlay"));
            }
        }

        private static Simulation세계자산PlacementSnapshot InteriorOverlay(
            Simulation세계자산배치Request request,
            Simulation공간변화FactSnapshot fact,
            string parentPlacementStableId,
            string visualKey,
            string category)
            => new Simulation세계자산PlacementSnapshot
            {
                PlacementStableId = "interior-overlay:"
                    + Simulation세계자산CanonicalHash.Hash(
                        parentPlacementStableId + "|" + fact.ChangeStableId
                        + "|" + visualKey).Substring(0, 24),
                ParentPlacementStableId = parentPlacementStableId,
                OwnerCellStableId = request.MapPlan.CellStableId,
                PlacementKindCode =
                    Simulation세계자산배치Codes.InteriorOverlay,
                LayerCode = InteriorLayoutCodes.LooseItem,
                CategoryCode = category,
                CompositionKey = visualKey,
                H1StableId = fact.SpatialStableId,
                AuthorityKindCode =
                    Simulation세계자산배치Codes.DerivedWorldProp,
                PersistenceKindCode =
                    Simulation세계자산배치Codes.DerivedPersistent,
                PresentationOnly = true,
                SourceChangeStableIds = new[] { fact.ChangeStableId },
            };

        private static SimulationInteriorPlanHandleSnapshot ToSnapshot(
            InteriorPlanHandle handle)
            => new SimulationInteriorPlanHandleSnapshot
            {
                SchemaVersion = handle.SchemaVersion,
                BuildingPlacementStableId =
                    handle.BuildingPlacementStableId,
                H1StableId = handle.H1StableId,
                InteriorDefinitionRevision =
                    handle.InteriorDefinitionRevision,
                ReferenceCatalogRevision = handle.ReferenceCatalogRevision,
                ReferenceCatalogHashSha256 =
                    handle.ReferenceCatalogHashSha256,
                PlacementControlRuleRevision =
                    handle.PlacementControlRuleRevision,
                VisualMetricCatalogRevision =
                    handle.VisualMetricCatalogRevision,
                VisualMetricCatalogHashSha256 =
                    handle.VisualMetricCatalogHashSha256,
                AdjustmentRevision = handle.AdjustmentRevision,
                InteriorPlacementPlanHashSha256 =
                    handle.InteriorPlacementPlanHashSha256,
            };

        private static SimulationInteriorPlacementPlanBodySnapshot ToBody(
            InteriorPlacementPlan plan)
        {
            var body = new SimulationInteriorPlacementPlanBodySnapshot
            {
                BuildingPlacementStableId = plan.BuildingPlacementStableId,
                H1StableId = plan.H1StableId,
                SourceInteriorPlanSchemaVersion = plan.SchemaVersion,
                InteriorPlacementPlanHashSha256 =
                    plan.InteriorPlacementPlanHashSha256,
                Placements = plan.Placements.OrderBy(value =>
                        value.PlacementStableId, StringComparer.Ordinal)
                    .Select(value =>
                        new SimulationInteriorPlacementBodyItemSnapshot
                        {
                            PlacementStableId = value.PlacementStableId,
                            ParentPlacementStableId =
                                value.ParentPlacementStableId,
                            ZoneStableId = value.ZoneStableId,
                            OwningH1StableId = value.OwningH1StableId,
                            PlacementLayerCode = value.PlacementLayerCode,
                            PlacementRoleCode = value.PlacementRoleCode,
                            VisualKey = value.VisualKey,
                            LocalX = value.LocalPosition.X,
                            LocalY = value.LocalPosition.Y,
                            LocalZ = value.LocalPosition.Z,
                            LocalRotationDegrees =
                                value.LocalRotationDegrees,
                            UniformScale = plan.SchemaVersion ==
                                           InteriorLayoutCodes.SchemaVersionV2
                                ? value.AppliedTransform.UniformScale : 1d,
                            ReferenceStableId = value.ReferenceStableId,
                            PresentationFlags = value.PresentationFlags,
                        }).ToArray(),
            };
            body.BodyHashSha256 = Simulation세계자산CanonicalHash
                .ComputeInteriorBodyHash(body);
            return body;
        }

        private static Simulation환경발생DecisionPlan EmptySpawnPlan(
            Simulation세계자산배치Request request)
        {
            var plan = new Simulation환경발생DecisionPlan
            {
                RuleRevision = "legacy-procedural-compatibility.r1",
                RuleCatalogHashSha256 = new string('0', 64),
                WorldSeed = request.MapPlan.WorldSeed,
                CellStableId = request.MapPlan.CellStableId,
                SpawnEpoch = request.SpawnContext.SpawnEpoch,
                SourceWorldRevision = request.MapPlan.SourceWorldRevision,
                SourceChangeProjectionHashSha256 = request.ChangeProjection
                    .ProjectionHashSha256,
            };
            plan.DecisionPlanHashSha256 = Simulation세계자산CanonicalHash
                .ComputeSpawnDecisionPlanHash(plan);
            return plan;
        }

        private static void Validate(Simulation세계자산배치Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.MapPlan == null || request.ChangeProjection == null
                || request.SpawnRuleCatalog == null
                || request.SpawnContext == null
                || string.IsNullOrWhiteSpace(request.AssetRuleRevision)
                || !string.Equals(request.MapPlan.MapPlanHashSha256,
                    Simulation세계자산CanonicalHash.ComputeMapPlanHash(
                        request.MapPlan), StringComparison.Ordinal)
                || !string.Equals(request.ChangeProjection
                        .ProjectionHashSha256,
                    Simulation세계자산CanonicalHash
                        .ComputeChangeProjectionHash(
                            request.ChangeProjection),
                    StringComparison.Ordinal)
                || !string.Equals(request.MapPlan.CellStableId,
                    request.ChangeProjection.CellStableId,
                    StringComparison.Ordinal)
                || request.MapPlan.SourceWorldRevision !=
                    request.ChangeProjection.SourceWorldRevision)
                throw new ArgumentException(
                    "SimulationWorldAssetPlacementRequestInvalid",
                    nameof(request));
        }

        private static byte[] HashBytes(string value)
        {
            var hex = Simulation세계자산CanonicalHash.Hash(value);
            var bytes = new byte[hex.Length / 2];
            for (var index = 0; index < bytes.Length; index++)
                bytes[index] = byte.Parse(hex.Substring(index * 2, 2),
                    NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return bytes;
        }

        private static double Distance(double ax, double az, double bx,
            double bz)
        {
            var x = ax - bx;
            var z = az - bz;
            return Math.Sqrt(x * x + z * z);
        }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "Nature 권위 상태를 지도·공간 변화 뒤 실외·실내 계획으로 분리하고 호환 상태 사본으로 조립한다.",
        StepKey = "application.nature-world-asset-placement-state",
        DependsOnStepKeys = new[] {
            "application.world-map-composition",
            "application.world-asset-placement"
        },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "플레이어 변화 정보를 읽어 표현 계획을 만들 뿐 Simulation 권위 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E5,
        "Nature 권위 상태에서 지도·변화·환경·실내를 하나의 결정적 배치 상태로 조립한다.",
        Boundary = "상태 사본은 LH 활성화와 Unity Prefab 표현을 완료하지 않는다.")]
    public sealed class SimulationNature세계자산배치Service
    {
        private readonly ISimulation지도구성Engine mapEngine;
        private readonly SimulationNature공간변화ProjectionBuilder
            changeProjectionBuilder;
        private readonly ISimulation환경발생DecisionEngine spawnEngine;
        private readonly Simulation분리세계자산배치Coordinator
            separatedPlacementCoordinator;

        public SimulationNature세계자산배치Service()
        {
            mapEngine = new SimulationScenario지도구성Engine();
            changeProjectionBuilder =
                new SimulationNature공간변화ProjectionBuilder();
            spawnEngine = new Simulation결정적환경발생DecisionEngine();
            separatedPlacementCoordinator =
                new Simulation분리세계자산배치Coordinator(
                    new Simulation결정적세계자산배치Engine(
                        spawnEngine, new DeterministicInteriorLayoutEngine()));
        }

        public SimulationWorldAssetPlacementStateSnapshot Compose(
            string worldSeed,
            string cellStableId,
            string windowRoleCode,
            long worldRevision,
            int spawnEpoch,
            SimulationNatureSurvivalStateSnapshot natureState,
            string[]? requiredCapabilityCodes = null)
        {
            if (!SimulationLhWorldGrid.TryParseL3CellKey(cellStableId,
                    out var cellX, out var cellY))
                throw new ArgumentException(
                    "SimulationNatureWorldAssetCellInvalid",
                    nameof(cellStableId));
            var map = mapEngine.Compose(new Simulation지도구성Request
            {
                WorldSeed = worldSeed,
                CellStableId = cellStableId,
                CellX = cellX,
                CellY = cellY,
                WindowRoleCode = windowRoleCode,
                WorldRevision = worldRevision,
                RequiredCapabilityCodes = requiredCapabilityCodes
                    ?? Array.Empty<string>(),
            });
            var changes = changeProjectionBuilder.Build(natureState,
                cellStableId, worldRevision);
            var context = changeProjectionBuilder.CreateEnvironmentContext(
                changes, spawnEpoch);
            var catalog = SimulationNature환경발생Catalog.Create();
            var decisions = spawnEngine.Decide(worldSeed, catalog, context);
            var separated = separatedPlacementCoordinator.Compose(
                new Simulation세계자산배치Request
                {
                    MapPlan = map,
                    ChangeProjection = changes,
                    SpawnRuleCatalog = catalog,
                    SpawnContext = context,
                    PreserveLegacyProceduralLayout = false,
                });
            var assets = separated.CompatibilityPlan;
            if (!string.Equals(assets.SpawnDecisionPlanHashSha256,
                    decisions.DecisionPlanHashSha256,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "SimulationNatureWorldAssetSpawnDecisionMismatch");

            var state = new SimulationWorldAssetPlacementStateSnapshot
            {
                SourceWorldRevision = worldRevision,
                MapPlans = new[] { map },
                ChangeProjections = new[] { changes },
                SpawnDecisionPlans = new[] { decisions },
                AssetPlacementPlans = new[] { assets },
                InteriorPlanBodies = assets.InteriorPlanBodies,
            };
            state.StateHashSha256 = Simulation세계자산CanonicalHash
                .ComputeStateHash(state);
            return state;
        }
    }
}
