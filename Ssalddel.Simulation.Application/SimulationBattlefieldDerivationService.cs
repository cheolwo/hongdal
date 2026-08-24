using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public interface ISimulationBattlefieldDerivationService
    {
        SimulationBattlefieldDerivationSnapshot Derive(
            string sessionStableId,
            string encounterStableId,
            string areaStableId,
            long capturedWorldRevision,
            bool natureEncounter);
    }

    /// <summary>
    /// H5 생활세계를 전투 좌표로 확대하지 않고, 조우 주변의 지역 사실을
    /// 불변 문맥으로 채취해 독립 BattleLocalMeters 전장 계획을 만든다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class SimulationBattlefieldDerivationService
        : ISimulationBattlefieldDerivationService
    {
        private const double ContextSizeMeters = 1000d;
        private const double BattlefieldSizeMeters = 500d;
        private const double GridCellSizeMeters = 4d;
        private const double ContextHalf = ContextSizeMeters / 2d;
        private const double BattlefieldHalf = BattlefieldSizeMeters / 2d;
        private readonly ISimulationWorldLayoutCatalogReader layoutReader;
        private readonly ISimulationWorldActualE5SpatialCatalogReader spatialReader;
        private readonly ISimulationBattleRuntimeProjectionProvider? runtimeProjectionProvider;

        public SimulationBattlefieldDerivationService(
            ISimulationWorldLayoutCatalogReader worldLayoutReader,
            ISimulationWorldActualE5SpatialCatalogReader actualE5SpatialReader,
            ISimulationBattleRuntimeProjectionProvider? battleRuntimeProjectionProvider = null)
        {
            layoutReader = worldLayoutReader ?? throw new ArgumentNullException(nameof(worldLayoutReader));
            spatialReader = actualE5SpatialReader ?? throw new ArgumentNullException(nameof(actualE5SpatialReader));
            runtimeProjectionProvider = battleRuntimeProjectionProvider;
        }

        public SimulationBattlefieldDerivationSnapshot Derive(
            string sessionStableId,
            string encounterStableId,
            string areaStableId,
            long capturedWorldRevision,
            bool natureEncounter)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId)
                || string.IsNullOrWhiteSpace(encounterStableId)
                || string.IsNullOrWhiteSpace(areaStableId)
                || capturedWorldRevision < 0)
                throw new ArgumentException("SimulationBattlefieldDerivationInputInvalid");

            var blocks = new List<string>();
            if (!layoutReader.TryRead(out var layoutCatalog, out var layoutError))
                blocks.Add(layoutError);
            if (!spatialReader.TryRead(out var spatialCatalog, out var spatialError))
                blocks.Add(spatialError);
            if (blocks.Count > 0)
                return Blocked(blocks);

            var definition = layoutCatalog.Definition;
            var roleCode = natureEncounter ? "NatureHome" : "Farm";
            var area = definition.AreaSetInstances.FirstOrDefault(value =>
                string.Equals(value.AreaRoleCode, roleCode, StringComparison.Ordinal));
            var opposingRole = natureEncounter ? "Farm" : "NatureHome";
            var opposingArea = definition.AreaSetInstances.FirstOrDefault(value =>
                string.Equals(value.AreaRoleCode, opposingRole, StringComparison.Ordinal));
            if (area == null || opposingArea == null)
                return Blocked(new[] { "SimulationBattlefieldAreaSetUnavailable" });

            var approach = SelectApproachConnector(area, opposingArea);
            if (approach == null)
                return Blocked(new[] { "SimulationBattlefieldApproachConnectorUnavailable" });

            var areaPose = Pose(area.PlacementTransform.LocalXMeters,
                area.PlacementTransform.LocalZMeters, area.PlacementTransform.RotationDegrees,
                SimulationWorldLayoutCodes.ScenarioLocalMeters);
            var opposingPose = Pose(opposingArea.PlacementTransform.LocalXMeters,
                opposingArea.PlacementTransform.LocalZMeters,
                opposingArea.PlacementTransform.RotationDegrees,
                SimulationWorldLayoutCodes.ScenarioLocalMeters);
            var encounterPose = Compose(area.PlacementTransform, approach);
            var toOpposing = Normalize(opposingPose.XMeters - encounterPose.XMeters,
                opposingPose.ZMeters - encounterPose.ZMeters);
            if (toOpposing.Length <= 0d)
                toOpposing = Direction(approach.RotationDegrees);
            var toArea = Normalize(areaPose.XMeters - encounterPose.XMeters,
                areaPose.ZMeters - encounterPose.ZMeters);
            if (toArea.Length <= 0d)
                toArea = new Vector2(-toOpposing.X, -toOpposing.Z, 1d);
            var attackerPose = Pose(encounterPose.XMeters + toOpposing.X * 120d,
                encounterPose.ZMeters + toOpposing.Z * 120d,
                Degrees(toOpposing.X, toOpposing.Z),
                SimulationWorldLayoutCodes.ScenarioLocalMeters);
            var defenderPose = Pose(encounterPose.XMeters + toArea.X * 80d,
                encounterPose.ZMeters + toArea.Z * 80d,
                Degrees(-toOpposing.X, -toOpposing.Z),
                SimulationWorldLayoutCodes.ScenarioLocalMeters);

            var origin = new SimulationBattleSpatialOriginSnapshot
            {
                WorldLayoutStableId = definition.WorldLayoutStableId,
                WorldLayoutRevision = definition.WorldLayoutRevision,
                WorldLayoutHashSha256 = definition.WorldLayoutHashSha256,
                CapturedWorldRevision = capturedWorldRevision,
                AreaSetInstanceStableId = area.AreaSetInstanceStableId,
                H3Ref = FindNearestGraph(area, encounterPose)?.H3Ref ?? string.Empty,
                ApproachConnectorStableId = approach.ConnectorStableId,
                EncounterPose = encounterPose,
                AttackerPose = attackerPose,
                DefenderPose = defenderPose,
                GroundingEvidenceHashSha256 = layoutCatalog.GroundingBinding
                    .GroundingEvidenceHashSha256,
            };
            origin.QuantizedBattleOriginHashSha256 = Hash(
                CanonicalPose(origin.EncounterPose) + CanonicalPose(origin.AttackerPose)
                + CanonicalPose(origin.DefenderPose) + approach.ConnectorStableId);

            var runtimeProjection = runtimeProjectionProvider?.Create(sessionStableId.Trim(),
                encounterStableId.Trim(), roleCode)
                ?? new SimulationBattleRelevantRuntimeProjectionSnapshot
                {
                    EncounterScopeStableId = encounterStableId.Trim(),
                    BattleRelevantOverlayHashSha256 = Hash("empty-runtime-overlay"),
                };
            var context = BuildContext(sessionStableId.Trim(), encounterStableId.Trim(),
                origin, area, definition, spatialCatalog, capturedWorldRevision,
                runtimeProjection);
            var profile = natureEncounter
                ? SimulationBattlefieldDerivationCodes.NatureField500
                : SimulationBattlefieldDerivationCodes.FarmPerimeter500;
            var terrainHash = Hash(string.Join("|",
                SimulationBattlefieldDerivationCodes.ScenarioAuthored,
                profile,
                context.Items.Where(value => value.SourceKindCode == "Graph")
                    .Select(value => value.SourceHashSha256)
                    .OrderBy(value => value, StringComparer.Ordinal)));
            var derivationInputHash = Hash(string.Join("|",
                SimulationBattlefieldDerivationCodes.GeneratorRevision,
                profile + ".r1",
                context.ContextHashSha256,
                origin.QuantizedBattleOriginHashSha256,
                context.AnchorPolicyRevision,
                context.AnchorSetHashSha256,
                terrainHash));
            var plan = BuildPlan(context, origin, profile, derivationInputHash);
            var validationBlocks = plan.ValidationCodes.Where(value =>
                value.StartsWith("Simulation", StringComparison.Ordinal)).ToArray();
            return new SimulationBattlefieldDerivationSnapshot
            {
                SpatialOrigin = origin,
                WorldContext = context,
                BattlefieldPlan = plan,
                TacticalTerrainInputHashSha256 = terrainHash,
                BattlefieldDerivationInputHashSha256 = derivationInputHash,
                CanConfirm = validationBlocks.Length == 0,
                BlockingReasonCodes = validationBlocks,
            };
        }

        private static SimulationBattleWorldContextSnapshot BuildContext(
            string sessionStableId,
            string encounterStableId,
            SimulationBattleSpatialOriginSnapshot origin,
            SimulationWorldAreaSetInstanceResponse encounterArea,
            SimulationWorldLayoutDefinitionResponse definition,
            SimulationWorldActualE5SpatialCatalog spatialCatalog,
            long capturedWorldRevision,
            SimulationBattleRelevantRuntimeProjectionSnapshot runtimeProjection)
        {
            var items = new List<SimulationBattleWorldContextItemSnapshot>();
            var edgeRelations = new List<(string EdgeId, string From, string To,
                string Relation, string Connector)>();
            foreach (var area in definition.AreaSetInstances.OrderBy(value =>
                         value.AreaSetInstanceStableId, StringComparer.Ordinal))
            {
                var areaWorldPose = Pose(area.PlacementTransform.LocalXMeters,
                    area.PlacementTransform.LocalZMeters,
                    area.PlacementTransform.RotationDegrees,
                    SimulationWorldLayoutCodes.ScenarioLocalMeters);
                if (InsideContext(origin.EncounterPose, areaWorldPose, 0d))
                {
                    items.Add(Item(area.AreaSetInstanceStableId, "AreaSet",
                        area.AreaRoleCode, string.Empty, area.InstanceHashSha256,
                        areaWorldPose, 0d, 0d));
                }

                foreach (var graphInstance in area.GraphInstances.OrderBy(value =>
                             value.GraphInstanceStableId, StringComparer.Ordinal))
                {
                    var graphPose = Compose(area.PlacementTransform,
                        graphInstance.PlacementTransform);
                    if (!spatialCatalog.Graphs.TryGetValue(
                            graphInstance.LandscapeGraphStableId, out var graph))
                        continue;
                    var graphWidth = Math.Max(0d,
                        graph.Bounds.MaxEastingMeters - graph.Bounds.MinEastingMeters);
                    var graphDepth = Math.Max(0d,
                        graph.Bounds.MaxNorthingMeters - graph.Bounds.MinNorthingMeters);
                    if (InsideContext(origin.EncounterPose, graphPose,
                            Math.Max(graphWidth, graphDepth) / 2d))
                    {
                        items.Add(Item(graphInstance.GraphInstanceStableId, "Graph",
                            graphInstance.H3Ref, area.AreaSetInstanceStableId,
                            graph.GraphHashSha256, graphPose, graphWidth, graphDepth));
                    }

                    foreach (var node in graph.Nodes.OrderBy(value => value.NodeStableId,
                                 StringComparer.Ordinal))
                    {
                        var nodePose = Compose(graphPose, node.CenterEastingMeters,
                            node.CenterNorthingMeters, 0d);
                        if (!InsideContext(origin.EncounterPose, nodePose,
                                Math.Max(node.WidthMeters, node.DepthMeters) / 2d))
                            continue;
                        items.Add(Item(node.NodeStableId, "Node", node.SemanticCode,
                            node.ParentNodeStableId, graph.GraphHashSha256, nodePose,
                            node.WidthMeters, node.DepthMeters));
                    }

                    foreach (var placement in graph.Placements.OrderBy(value =>
                                 value.PlacementStableId, StringComparer.Ordinal))
                    {
                        var placementPose = Compose(graphPose, placement.EastingMeters,
                            placement.NorthingMeters, placement.RotationDegrees);
                        if (!InsideContext(origin.EncounterPose, placementPose,
                                Math.Max(placement.FootprintWidthMeters,
                                    placement.FootprintDepthMeters) / 2d))
                            continue;
                        items.Add(Item(placement.PlacementStableId, "Placement",
                            placement.CompositionKey, placement.NodeStableId,
                            Hash(string.Join("|", graph.GraphHashSha256,
                                placement.DeterministicSeed, placement.TopologyCode)),
                            placementPose, placement.FootprintWidthMeters,
                            placement.FootprintDepthMeters));
                    }

                    edgeRelations.AddRange(graph.Edges.Select(edge => (
                        edge.EdgeStableId, edge.FromNodeStableId, edge.ToNodeStableId,
                        edge.RelationCode, edge.ConnectorTypeCode)));

                    foreach (var connector in graphInstance.ExternalConnectors
                                 .OrderBy(value => value.ConnectorStableId,
                                     StringComparer.Ordinal))
                    {
                        var connectorPose = Compose(area.PlacementTransform,
                            graphInstance.PlacementTransform, connector);
                        if (!InsideContext(origin.EncounterPose, connectorPose, 0d))
                            continue;
                        items.Add(Item(connector.ConnectorStableId, "Connector",
                            connector.DirectionCode, graphInstance.GraphInstanceStableId,
                            connector.ConnectorPoseHashSha256, connectorPose,
                            connector.WidthMeters, connector.WidthMeters));
                    }
                }
            }

            var distinctItems = items.GroupBy(value => value.SourceStableId,
                    StringComparer.Ordinal).Select(group => group.First())
                .OrderBy(value => value.SourceStableId, StringComparer.Ordinal).ToArray();
            var portals = BuildBoundaryPortals(origin, encounterArea);
            var anchors = BuildAnchors(encounterStableId, origin, distinctItems, portals,
                edgeRelations);
            var anchorBySource = anchors.Where(value => value.SourceStableId.Length > 0)
                .GroupBy(value => value.SourceStableId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            var routes = edgeRelations.Where(edge => anchorBySource.ContainsKey(edge.From)
                                                     && anchorBySource.ContainsKey(edge.To))
                .OrderBy(edge => edge.EdgeId, StringComparer.Ordinal)
                .Select(edge => new SimulationBattlefieldRouteConstraintSnapshot
                {
                    RouteConstraintStableId = "battlefield-route:" + ShortHash(edge.EdgeId),
                    SourceRouteStableId = edge.EdgeId,
                    FromAnchorStableId = anchorBySource[edge.From].BattlefieldAnchorStableId,
                    ToAnchorStableId = anchorBySource[edge.To].BattlefieldAnchorStableId,
                    OrderedSemanticStableIds = new[] { edge.From, edge.To },
                    TravelTypeCodes = string.IsNullOrWhiteSpace(edge.Connector)
                        ? Array.Empty<string>() : new[] { edge.Connector },
                    MinimumWidthMeters = 4d,
                    ContinuityRequired = true,
                    RouteSignature = Hash(string.Join("|", edge.EdgeId, edge.From,
                        edge.To, edge.Relation, edge.Connector)),
                }).ToList();
            AddPortalRoutes(routes, anchors, portals, origin.ApproachConnectorStableId);

            var relations = routes.Select((route, index) =>
                    new SimulationBattlefieldRelationConstraintSnapshot
                    {
                        RelationConstraintStableId = "battlefield-relation:route:"
                                                     + index.ToString("D3", CultureInfo.InvariantCulture),
                        FromAnchorStableId = route.FromAnchorStableId,
                        ToAnchorStableId = route.ToAnchorStableId,
                        RelationCode = SimulationBattlefieldDerivationCodes.Connects,
                        Priority = 1,
                    })
                .Concat(anchors.Where(value => value.AnchorTypeCodes.Contains(
                        SimulationBattlefieldDerivationCodes.Objective, StringComparer.Ordinal))
                    .Select((anchor, index) =>
                        new SimulationBattlefieldRelationConstraintSnapshot
                        {
                            RelationConstraintStableId = "battlefield-relation:objective:"
                                                         + index.ToString("D3", CultureInfo.InvariantCulture),
                            FromAnchorStableId = anchors.First(value =>
                                value.SourceStableId == origin.ApproachConnectorStableId)
                                .BattlefieldAnchorStableId,
                            ToAnchorStableId = anchor.BattlefieldAnchorStableId,
                            RelationCode = SimulationBattlefieldDerivationCodes.LeadsTo,
                            Priority = 4,
                        }))
                .OrderBy(value => value.RelationConstraintStableId, StringComparer.Ordinal)
                .ToArray();

            var context = new SimulationBattleWorldContextSnapshot
            {
                ContextStableId = "battle-context:" + ShortHash(string.Join("|",
                    sessionStableId, encounterStableId)),
                CenterXMeters = Quantize(origin.EncounterPose.XMeters),
                CenterZMeters = Quantize(origin.EncounterPose.ZMeters),
                Items = distinctItems,
                BoundaryPortals = portals,
                Anchors = anchors,
                RouteConstraints = routes.OrderBy(value => value.RouteConstraintStableId,
                    StringComparer.Ordinal).ToArray(),
                RelationConstraints = relations,
                SourceWorldRevision = capturedWorldRevision,
                BattleRelevantRuntime = runtimeProjection,
                BattleRelevantOverlayHashSha256 = runtimeProjection
                    .BattleRelevantOverlayHashSha256,
            };
            context.StaticSpatialContextHashSha256 = Hash(CanonicalContext(context));
            context.EncounterScopeHashSha256 = Hash(string.Join("|", encounterStableId,
                CanonicalPose(origin.EncounterPose), origin.ApproachConnectorStableId));
            context.AttackerContextHashSha256 = Hash(CanonicalPose(origin.AttackerPose));
            context.DefenderContextHashSha256 = Hash(string.Join("|",
                CanonicalPose(origin.DefenderPose), string.Join(",", runtimeProjection.Formations
                    .OrderBy(value => value.FormationStableId, StringComparer.Ordinal)
                    .Select(value => value.FormationStableId + ":" + value.StateCode))));
            context.ContextHashSha256 = Hash(string.Join("|", context.SchemaVersion,
                context.ContextDerivationRuleVersion,
                context.StaticSpatialContextHashSha256,
                context.EncounterScopeHashSha256,
                context.AttackerContextHashSha256,
                context.DefenderContextHashSha256,
                context.BattleRelevantOverlayHashSha256));
            context.AnchorSetHashSha256 = Hash(CanonicalAnchors(context));
            return context;
        }

        private static SimulationBattlefieldPlanSnapshot BuildPlan(
            SimulationBattleWorldContextSnapshot context,
            SimulationBattleSpatialOriginSnapshot origin,
            string profile,
            string derivationInputHash)
        {
            var seedHash = Hash(derivationInputHash);
            var seed = ParseSeed(seedHash);
            var attackVector = Normalize(origin.DefenderPose.XMeters - origin.AttackerPose.XMeters,
                origin.DefenderPose.ZMeters - origin.AttackerPose.ZMeters);
            var attackDegrees = Degrees(attackVector.X, attackVector.Z);
            var placements = new List<SimulationBattlefieldAnchorPlacementSnapshot>();
            var used = new List<(double X, double Z, double Radius)>();
            var random = new Pcg32(seed, 1442695040888963407UL);
            foreach (var anchor in context.Anchors
                         .Where(value => value.PreservationPolicyCode !=
                                         SimulationBattlefieldDerivationCodes.ContextOnly)
                         .OrderBy(value => PolicyOrder(value.PreservationPolicyCode))
                         .ThenBy(value => value.BattlefieldAnchorStableId,
                             StringComparer.Ordinal))
            {
                var local = ToBattleLocal(origin.EncounterPose, attackDegrees,
                    anchor.SourcePose);
                var scale = profile == SimulationBattlefieldDerivationCodes.FarmPerimeter500
                    ? 0.42d : 0.46d;
                var x = Clamp(local.X * scale, -BattlefieldHalf + 18d,
                    BattlefieldHalf - 18d);
                var z = Clamp(local.Z * scale, -BattlefieldHalf + 18d,
                    BattlefieldHalf - 18d);
                if (anchor.PreservationPolicyCode ==
                    SimulationBattlefieldDerivationCodes.Preferred)
                {
                    x += (random.NextDouble() - 0.5d) * 12d;
                    z += (random.NextDouble() - 0.5d) * 12d;
                }
                var radius = Math.Max(4d, Math.Min(48d,
                    Math.Max(anchor.SourceWidthMeters, anchor.SourceDepthMeters) * scale / 2d));
                ResolveCollision(ref x, ref z, radius, used,
                    anchor.PreservationPolicyCode == SimulationBattlefieldDerivationCodes.Required);
                used.Add((x, z, radius));
                placements.Add(new SimulationBattlefieldAnchorPlacementSnapshot
                {
                    BattlefieldAnchorStableId = anchor.BattlefieldAnchorStableId,
                    BattlePose = Pose(x, z,
                        NormalizeDegrees(anchor.SourcePose.RotationDegrees - attackDegrees),
                        SimulationBattlefieldDerivationCodes.BattleLocalMeters),
                    WidthMeters = Math.Max(2d,
                        Math.Min(120d, anchor.SourceWidthMeters * scale)),
                    DepthMeters = Math.Max(2d,
                        Math.Min(120d, anchor.SourceDepthMeters * scale)),
                    SizeVariantCode = SimulationWorldLayoutCodes.Reference,
                });
            }

            var zones = new[]
            {
                Zone("zone:hostile-deployment", SimulationBattlefieldDerivationCodes.HostileDeployment,
                    0d, -190d, 330d, 80d, string.Empty),
                Zone("zone:allied-deployment", SimulationBattlefieldDerivationCodes.AlliedDeployment,
                    0d, 190d, 330d, 80d, FindObjectiveAnchor(context)),
                Zone("zone:reinforcement", SimulationBattlefieldDerivationCodes.ReinforcementGate,
                    -220d, -120d, 36d, 90d, FindPortalAnchor(context)),
                Zone("zone:retreat", SimulationBattlefieldDerivationCodes.RetreatGate,
                    220d, 180d, 36d, 90d, FindApproachAnchor(context, origin)),
            };
            var terrain = BuildTerrain(seed, profile);
            var validation = ValidatePlan(context, placements, zones);
            var plan = new SimulationBattlefieldPlanSnapshot
            {
                BattlefieldPlanStableId = "battlefield-plan:" + ShortHash(derivationInputHash),
                ProfileCode = profile,
                ProfileRevision = profile + ".r1",
                WidthMeters = BattlefieldSizeMeters,
                DepthMeters = BattlefieldSizeMeters,
                GridCellSizeMeters = GridCellSizeMeters,
                BattlefieldDerivationInputHashSha256 = derivationInputHash,
                BattlefieldSeedHashSha256 = seedHash,
                BattlefieldSeed = seed,
                AnchorPlacements = placements.OrderBy(value =>
                    value.BattlefieldAnchorStableId, StringComparer.Ordinal).ToArray(),
                Routes = context.RouteConstraints,
                Zones = zones,
                TerrainCells = terrain,
                ValidationCodes = validation,
            };
            plan.BattlefieldPlanHashSha256 = Hash(CanonicalPlan(plan));
            return plan;
        }

        private static SimulationBattlefieldAnchorSnapshot[] BuildAnchors(
            string encounterStableId,
            SimulationBattleSpatialOriginSnapshot origin,
            SimulationBattleWorldContextItemSnapshot[] items,
            SimulationBattleContextBoundaryPortalSnapshot[] portals,
            IReadOnlyCollection<(string EdgeId, string From, string To,
                string Relation, string Connector)> edges)
        {
            var connected = edges.SelectMany(value => new[] { value.From, value.To })
                .ToHashSet(StringComparer.Ordinal);
            var nearestObjective = items.Where(value => value.SourceKindCode is "Node" or "Placement")
                .OrderBy(value => DistanceSquared(origin.EncounterPose, value.Pose))
                .ThenBy(value => value.SourceStableId, StringComparer.Ordinal).FirstOrDefault();
            var anchors = new List<SimulationBattlefieldAnchorSnapshot>();
            foreach (var item in items.OrderBy(value => value.SourceStableId,
                         StringComparer.Ordinal))
            {
                var isApproach = item.SourceStableId == origin.ApproachConnectorStableId;
                var isObjective = nearestObjective != null
                                  && item.SourceStableId == nearestObjective.SourceStableId;
                var isConnector = item.SourceKindCode == "Connector";
                var isRoute = connected.Contains(item.SourceStableId);
                var policy = isApproach || isObjective
                    ? SimulationBattlefieldDerivationCodes.Required
                    : isConnector || isRoute || IsPreferredSemantic(item.SemanticCode)
                        ? SimulationBattlefieldDerivationCodes.Preferred
                        : SimulationBattlefieldDerivationCodes.ContextOnly;
                var types = new List<string>();
                if (item.SourceKindCode == "Connector")
                    types.Add(SimulationBattlefieldDerivationCodes.Gate);
                else if (item.SourceKindCode is "Placement" or "Node")
                    types.Add(SimulationBattlefieldDerivationCodes.Physical);
                else
                    types.Add(SimulationBattlefieldDerivationCodes.Area);
                if (isRoute) types.Add(SimulationBattlefieldDerivationCodes.Route);
                if (isObjective) types.Add(SimulationBattlefieldDerivationCodes.Objective);
                anchors.Add(new SimulationBattlefieldAnchorSnapshot
                {
                    BattlefieldAnchorStableId = "battlefield-anchor:"
                        + ShortHash(encounterStableId + "|" + item.SourceStableId),
                    SourceStableId = item.SourceStableId,
                    WorldEffectTargetStableId = policy ==
                        SimulationBattlefieldDerivationCodes.ContextOnly
                        ? string.Empty : item.SourceStableId,
                    SemanticCode = item.SemanticCode,
                    AnchorTypeCodes = types.Distinct(StringComparer.Ordinal).ToArray(),
                    PreservationPolicyCode = policy,
                    SourcePose = item.Pose,
                    SourceWidthMeters = item.WidthMeters,
                    SourceDepthMeters = item.DepthMeters,
                    ApprovedSizeVariantCodes = new[]
                    {
                        "Compact", SimulationWorldLayoutCodes.Reference, "Expanded",
                    },
                });
            }
            foreach (var portal in portals)
            {
                anchors.Add(new SimulationBattlefieldAnchorSnapshot
                {
                    BattlefieldAnchorStableId = "battlefield-anchor:"
                        + ShortHash(encounterStableId + "|" + portal.PortalStableId),
                    SourceStableId = portal.PortalStableId,
                    SemanticCode = SimulationBattlefieldDerivationCodes.ContextBoundaryPortal,
                    AnchorTypeCodes = new[]
                    {
                        SimulationBattlefieldDerivationCodes.ContextBoundaryPortal,
                        SimulationBattlefieldDerivationCodes.Gate,
                    },
                    PreservationPolicyCode = SimulationBattlefieldDerivationCodes.Required,
                    SourcePose = portal.Pose,
                    SourceWidthMeters = 8d,
                    SourceDepthMeters = 8d,
                    ApprovedSizeVariantCodes = new[] { SimulationWorldLayoutCodes.Reference },
                });
            }
            return anchors.OrderBy(value => value.BattlefieldAnchorStableId,
                StringComparer.Ordinal).ToArray();
        }

        private static SimulationBattleContextBoundaryPortalSnapshot[] BuildBoundaryPortals(
            SimulationBattleSpatialOriginSnapshot origin,
            SimulationWorldAreaSetInstanceResponse area)
        {
            var portals = new List<SimulationBattleContextBoundaryPortalSnapshot>();
            var ordinal = 0;
            foreach (var connector in area.ExternalConnectors.OrderBy(value =>
                         value.ConnectorStableId, StringComparer.Ordinal))
            {
                var source = Compose(area.PlacementTransform, connector);
                var direction = Normalize(source.XMeters - origin.EncounterPose.XMeters,
                    source.ZMeters - origin.EncounterPose.ZMeters);
                if (direction.Length <= 0d) direction = Direction(source.RotationDegrees);
                var distance = RaySquareDistance(direction.X, direction.Z, ContextHalf);
                var pose = Pose(origin.EncounterPose.XMeters + direction.X * distance,
                    origin.EncounterPose.ZMeters + direction.Z * distance,
                    Degrees(direction.X, direction.Z),
                    SimulationWorldLayoutCodes.ScenarioLocalMeters);
                portals.Add(new SimulationBattleContextBoundaryPortalSnapshot
                {
                    PortalStableId = "battle-context-portal:"
                        + ShortHash(connector.ConnectorStableId + "|" + ordinal),
                    SourceRouteStableId = connector.ConnectorStableId,
                    CrossingOrdinal = ordinal++,
                    Pose = pose,
                    SourceDirectionDegrees = pose.RotationDegrees,
                    TravelTypeCodes = connector.TravelTypeCodes.ToArray(),
                });
            }
            return portals.ToArray();
        }

        private static void AddPortalRoutes(
            ICollection<SimulationBattlefieldRouteConstraintSnapshot> routes,
            SimulationBattlefieldAnchorSnapshot[] anchors,
            SimulationBattleContextBoundaryPortalSnapshot[] portals,
            string approachConnectorStableId)
        {
            var approach = anchors.FirstOrDefault(value =>
                value.SourceStableId == approachConnectorStableId);
            if (approach == null) return;
            foreach (var portal in portals.OrderBy(value => value.PortalStableId,
                         StringComparer.Ordinal))
            {
                var portalAnchor = anchors.First(value =>
                    value.SourceStableId == portal.PortalStableId);
                routes.Add(new SimulationBattlefieldRouteConstraintSnapshot
                {
                    RouteConstraintStableId = "battlefield-route:portal:"
                        + ShortHash(portal.PortalStableId),
                    SourceRouteStableId = portal.SourceRouteStableId,
                    FromAnchorStableId = portalAnchor.BattlefieldAnchorStableId,
                    ToAnchorStableId = approach.BattlefieldAnchorStableId,
                    OrderedSemanticStableIds = new[]
                    {
                        portal.PortalStableId, approach.SourceStableId,
                    },
                    TravelTypeCodes = portal.TravelTypeCodes.ToArray(),
                    MinimumWidthMeters = 6d,
                    ContinuityRequired = true,
                    RouteSignature = Hash(portal.PortalStableId + "|"
                                          + approach.SourceStableId),
                });
            }
        }

        private static string[] ValidatePlan(
            SimulationBattleWorldContextSnapshot context,
            IReadOnlyCollection<SimulationBattlefieldAnchorPlacementSnapshot> placements,
            IReadOnlyCollection<SimulationBattlefieldZoneSnapshot> zones)
        {
            var codes = new List<string>();
            var placed = placements.Select(value => value.BattlefieldAnchorStableId)
                .ToHashSet(StringComparer.Ordinal);
            if (context.Anchors.Any(value => value.PreservationPolicyCode ==
                    SimulationBattlefieldDerivationCodes.Required
                    && !placed.Contains(value.BattlefieldAnchorStableId)))
                codes.Add("SimulationBattlefieldRequiredAnchorUnresolved");
            if (context.RouteConstraints.Any(value => value.ContinuityRequired
                    && (!context.Anchors.Any(anchor => anchor.BattlefieldAnchorStableId ==
                                                     value.FromAnchorStableId)
                        || !context.Anchors.Any(anchor => anchor.BattlefieldAnchorStableId ==
                                                  value.ToAnchorStableId))))
                codes.Add("SimulationBattlefieldRequiredRouteDisconnected");
            if (!context.Anchors.Any(value => value.AnchorTypeCodes.Contains(
                    SimulationBattlefieldDerivationCodes.Objective,
                    StringComparer.Ordinal)))
                codes.Add("SimulationBattlefieldObjectiveUnreachable");
            if (!zones.Any(value => value.ZoneKindCode ==
                                    SimulationBattlefieldDerivationCodes.AlliedDeployment)
                || !zones.Any(value => value.ZoneKindCode ==
                                       SimulationBattlefieldDerivationCodes.HostileDeployment))
                codes.Add("SimulationBattlefieldDeploymentZoneInvalid");
            if (codes.Count == 0) codes.Add("BattlefieldPlanValid");
            return codes.ToArray();
        }

        private static SimulationBattlefieldTerrainCellSnapshot[] BuildTerrain(
            ulong seed, string profile)
        {
            var count = (int)(BattlefieldSizeMeters / GridCellSizeMeters);
            var cells = new SimulationBattlefieldTerrainCellSnapshot[count * count];
            for (var z = 0; z < count; z++)
            for (var x = 0; x < count; x++)
            {
                var cellSeed = seed ^ ((ulong)(uint)x << 32) ^ (uint)z;
                var random = new Pcg32(cellSeed, 6364136223846793005UL);
                var farm = profile == SimulationBattlefieldDerivationCodes.FarmPerimeter500;
                var terrainRoll = random.NextUInt() % 100u;
                var terrain = farm
                    ? terrainRoll < 62 ? "FarmField" : terrainRoll < 88 ? "FarmTrack" : "TreeCover"
                    : terrainRoll < 58 ? "ForestFloor" : terrainRoll < 82 ? "RockyTrail" : "Meadow";
                cells[z * count + x] = new SimulationBattlefieldTerrainCellSnapshot
                {
                    CellX = x,
                    CellZ = z,
                    HeightCentimeters = (int)(random.NextUInt() % 180u),
                    MovementCostPermille = terrain is "TreeCover" or "ForestFloor" ? 1180
                        : terrain is "FarmTrack" or "RockyTrail" ? 940 : 1000,
                    TerrainCode = terrain,
                    Walkable = true,
                };
            }
            return cells;
        }

        private static SimulationWorldConnectorPoseResponse? SelectApproachConnector(
            SimulationWorldAreaSetInstanceResponse area,
            SimulationWorldAreaSetInstanceResponse opposingArea)
        {
            var opposing = Pose(opposingArea.PlacementTransform.LocalXMeters,
                opposingArea.PlacementTransform.LocalZMeters,
                opposingArea.PlacementTransform.RotationDegrees,
                SimulationWorldLayoutCodes.ScenarioLocalMeters);
            return area.ExternalConnectors
                .Where(value => value.TravelTypeCodes.Contains("PlayerTraversal",
                    StringComparer.Ordinal))
                .OrderBy(value => DistanceSquared(Compose(area.PlacementTransform, value),
                    opposing))
                .ThenBy(value => value.ConnectorStableId, StringComparer.Ordinal)
                .FirstOrDefault()
                ?? area.ExternalConnectors.OrderBy(value => value.ConnectorStableId,
                    StringComparer.Ordinal).FirstOrDefault();
        }

        private static SimulationWorldGraphInstanceResponse? FindNearestGraph(
            SimulationWorldAreaSetInstanceResponse area,
            SimulationBattleSpatialPoseSnapshot pose)
            => area.GraphInstances.OrderBy(value => DistanceSquared(
                    Compose(area.PlacementTransform, value.PlacementTransform), pose))
                .ThenBy(value => value.GraphInstanceStableId, StringComparer.Ordinal)
                .FirstOrDefault();

        private static bool IsPreferredSemantic(string value)
        {
            var normalized = value.ToLowerInvariant();
            return normalized.Contains("gate") || normalized.Contains("warehouse")
                   || normalized.Contains("storage") || normalized.Contains("road")
                   || normalized.Contains("route") || normalized.Contains("yard")
                   || normalized.Contains("production") || normalized.Contains("farm");
        }

        private static SimulationBattleWorldContextItemSnapshot Item(
            string id, string kind, string semantic, string parent, string hash,
            SimulationBattleSpatialPoseSnapshot pose, double width, double depth)
            => new()
            {
                SourceStableId = id,
                SourceKindCode = kind,
                SemanticCode = semantic,
                ParentStableId = parent,
                SourceHashSha256 = hash,
                Pose = pose,
                WidthMeters = width,
                DepthMeters = depth,
            };

        private static SimulationBattlefieldZoneSnapshot Zone(
            string id, string kind, double x, double z, double width, double depth,
            string sourceAnchor)
            => new()
            {
                ZoneStableId = id,
                ZoneKindCode = kind,
                CenterPose = Pose(x, z, 0d,
                    SimulationBattlefieldDerivationCodes.BattleLocalMeters),
                WidthMeters = width,
                DepthMeters = depth,
                SourceAnchorStableId = sourceAnchor,
            };

        private static string FindObjectiveAnchor(SimulationBattleWorldContextSnapshot context)
            => context.Anchors.FirstOrDefault(value => value.AnchorTypeCodes.Contains(
                    SimulationBattlefieldDerivationCodes.Objective,
                    StringComparer.Ordinal))?.BattlefieldAnchorStableId ?? string.Empty;
        private static string FindPortalAnchor(SimulationBattleWorldContextSnapshot context)
            => context.Anchors.FirstOrDefault(value => value.AnchorTypeCodes.Contains(
                    SimulationBattlefieldDerivationCodes.ContextBoundaryPortal,
                    StringComparer.Ordinal))?.BattlefieldAnchorStableId ?? string.Empty;
        private static string FindApproachAnchor(SimulationBattleWorldContextSnapshot context,
            SimulationBattleSpatialOriginSnapshot origin)
            => context.Anchors.FirstOrDefault(value => value.SourceStableId ==
                    origin.ApproachConnectorStableId)?.BattlefieldAnchorStableId ?? string.Empty;

        private static SimulationBattlefieldDerivationSnapshot Blocked(
            IEnumerable<string> reasonCodes) => new()
            {
                CanConfirm = false,
                BlockingReasonCodes = reasonCodes.Where(value =>
                        !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal).ToArray(),
            };

        private static SimulationBattleSpatialPoseSnapshot Compose(
            SimulationWorldPlacementTransformResponse parent,
            SimulationWorldPlacementTransformResponse child)
            => Compose(Pose(parent.LocalXMeters, parent.LocalZMeters,
                    parent.RotationDegrees, SimulationWorldLayoutCodes.ScenarioLocalMeters),
                child.LocalXMeters, child.LocalZMeters, child.RotationDegrees);

        private static SimulationBattleSpatialPoseSnapshot Compose(
            SimulationWorldPlacementTransformResponse parent,
            SimulationWorldConnectorPoseResponse child)
            => Compose(Pose(parent.LocalXMeters, parent.LocalZMeters,
                    parent.RotationDegrees, SimulationWorldLayoutCodes.ScenarioLocalMeters),
                child.LocalXMeters, child.LocalZMeters, child.RotationDegrees);

        private static SimulationBattleSpatialPoseSnapshot Compose(
            SimulationWorldPlacementTransformResponse area,
            SimulationWorldPlacementTransformResponse graph,
            SimulationWorldConnectorPoseResponse connector)
            => Compose(Compose(area, graph), connector.LocalXMeters,
                connector.LocalZMeters, connector.RotationDegrees);

        private static SimulationBattleSpatialPoseSnapshot Compose(
            SimulationBattleSpatialPoseSnapshot parent,
            double localX, double localZ, double rotation)
        {
            var radians = parent.RotationDegrees * Math.PI / 180d;
            return Pose(parent.XMeters + Math.Cos(radians) * localX
                        + Math.Sin(radians) * localZ,
                parent.ZMeters - Math.Sin(radians) * localX
                               + Math.Cos(radians) * localZ,
                NormalizeDegrees(parent.RotationDegrees + rotation),
                SimulationWorldLayoutCodes.ScenarioLocalMeters);
        }

        private static SimulationBattleSpatialPoseSnapshot Pose(
            double x, double z, double rotation, string coordinateSpace)
            => new()
            {
                CoordinateSpaceCode = coordinateSpace,
                XMeters = Quantize(x),
                ZMeters = Quantize(z),
                RotationDegrees = Quantize(NormalizeDegrees(rotation)),
            };

        private static bool InsideContext(SimulationBattleSpatialPoseSnapshot center,
            SimulationBattleSpatialPoseSnapshot value, double radius)
            => Math.Abs(value.XMeters - center.XMeters) <= ContextHalf + radius
               && Math.Abs(value.ZMeters - center.ZMeters) <= ContextHalf + radius;

        private static (double X, double Z) ToBattleLocal(
            SimulationBattleSpatialPoseSnapshot origin, double attackDegrees,
            SimulationBattleSpatialPoseSnapshot value)
        {
            var x = value.XMeters - origin.XMeters;
            var z = value.ZMeters - origin.ZMeters;
            var radians = -attackDegrees * Math.PI / 180d;
            return (Math.Cos(radians) * x + Math.Sin(radians) * z,
                -Math.Sin(radians) * x + Math.Cos(radians) * z);
        }

        private static void ResolveCollision(ref double x, ref double z, double radius,
            IReadOnlyCollection<(double X, double Z, double Radius)> used, bool required)
        {
            var currentX = x;
            var currentZ = z;
            if (!used.Any(value => DistanceSquared(currentX, currentZ, value.X, value.Z)
                                   < Math.Pow(radius + value.Radius, 2d))) return;
            for (var step = 1; step <= 16; step++)
            {
                var angle = step * 137.507764d * Math.PI / 180d;
                var distance = step * 4d;
                var candidateX = Clamp(x + Math.Cos(angle) * distance,
                    -BattlefieldHalf + radius, BattlefieldHalf - radius);
                var candidateZ = Clamp(z + Math.Sin(angle) * distance,
                    -BattlefieldHalf + radius, BattlefieldHalf - radius);
                if (used.Any(value => DistanceSquared(candidateX, candidateZ,
                        value.X, value.Z) < Math.Pow(radius + value.Radius, 2d)))
                    continue;
                x = candidateX;
                z = candidateZ;
                return;
            }
            if (!required)
            {
                x = Clamp(x + radius, -BattlefieldHalf + radius,
                    BattlefieldHalf - radius);
                z = Clamp(z - radius, -BattlefieldHalf + radius,
                    BattlefieldHalf - radius);
            }
        }

        private static double RaySquareDistance(double x, double z, double half)
        {
            var tx = Math.Abs(x) < 0.000001d ? double.MaxValue : half / Math.Abs(x);
            var tz = Math.Abs(z) < 0.000001d ? double.MaxValue : half / Math.Abs(z);
            return Math.Min(tx, tz);
        }

        private static Vector2 Normalize(double x, double z)
        {
            var length = Math.Sqrt(x * x + z * z);
            return length <= 0.000001d ? new Vector2(0d, 0d, 0d)
                : new Vector2(x / length, z / length, length);
        }

        private static Vector2 Direction(double degrees)
        {
            var radians = degrees * Math.PI / 180d;
            return new Vector2(Math.Cos(radians), Math.Sin(radians), 1d);
        }

        private static double Degrees(double x, double z)
            => NormalizeDegrees(Math.Atan2(z, x) * 180d / Math.PI);
        private static double NormalizeDegrees(double value)
        {
            var normalized = value % 360d;
            return normalized < 0d ? normalized + 360d : normalized;
        }
        private static double Quantize(double value) => Math.Round(value, 3,
            MidpointRounding.AwayFromZero);
        private static double Clamp(double value, double minimum, double maximum)
            => Math.Max(minimum, Math.Min(maximum, value));
        private static double DistanceSquared(
            SimulationBattleSpatialPoseSnapshot left,
            SimulationBattleSpatialPoseSnapshot right)
            => DistanceSquared(left.XMeters, left.ZMeters, right.XMeters, right.ZMeters);
        private static double DistanceSquared(double x1, double z1, double x2, double z2)
            => (x1 - x2) * (x1 - x2) + (z1 - z2) * (z1 - z2);
        private static int PolicyOrder(string value)
            => value == SimulationBattlefieldDerivationCodes.Required ? 0 : 1;

        private static string CanonicalPose(SimulationBattleSpatialPoseSnapshot value)
            => string.Join("|", Math.Round(value.XMeters, 0,
                    MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture),
                Math.Round(value.ZMeters, 0, MidpointRounding.AwayFromZero)
                    .ToString(CultureInfo.InvariantCulture),
                Math.Round(value.RotationDegrees, 0, MidpointRounding.AwayFromZero)
                    .ToString(CultureInfo.InvariantCulture));

        private static string CanonicalContext(SimulationBattleWorldContextSnapshot value)
        {
            var text = new StringBuilder();
            Add(text, value.SchemaVersion); Add(text, value.ContextRevision);
            Add(text, value.ContextDerivationRuleVersion);
            Add(text, value.CenterXMeters); Add(text, value.CenterZMeters);
            foreach (var item in value.Items.OrderBy(item => item.SourceStableId,
                         StringComparer.Ordinal))
            {
                Add(text, item.SourceStableId); Add(text, item.SourceKindCode);
                Add(text, item.SemanticCode); Add(text, item.ParentStableId);
                Add(text, item.SourceHashSha256); Add(text, CanonicalPose(item.Pose));
                Add(text, item.WidthMeters); Add(text, item.DepthMeters);
            }
            foreach (var portal in value.BoundaryPortals.OrderBy(item => item.PortalStableId,
                         StringComparer.Ordinal))
            {
                Add(text, portal.PortalStableId); Add(text, portal.SourceRouteStableId);
                Add(text, portal.CrossingOrdinal); Add(text, CanonicalPose(portal.Pose));
                Add(text, portal.SourceDirectionDegrees);
            }
            return text.ToString();
        }

        private static string CanonicalAnchors(SimulationBattleWorldContextSnapshot value)
        {
            var text = new StringBuilder();
            Add(text, value.AnchorPolicyRevision);
            foreach (var anchor in value.Anchors.OrderBy(item =>
                         item.BattlefieldAnchorStableId, StringComparer.Ordinal))
            {
                Add(text, anchor.BattlefieldAnchorStableId); Add(text, anchor.SourceStableId);
                Add(text, anchor.WorldEffectTargetStableId); Add(text, anchor.SemanticCode);
                foreach (var type in anchor.AnchorTypeCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(text, type);
                Add(text, anchor.PreservationPolicyCode); Add(text, anchor.AggregationPolicyCode);
                Add(text, CanonicalPose(anchor.SourcePose)); Add(text, anchor.SourceWidthMeters);
                Add(text, anchor.SourceDepthMeters);
            }
            foreach (var route in value.RouteConstraints.OrderBy(item =>
                         item.RouteConstraintStableId, StringComparer.Ordinal))
            {
                Add(text, route.RouteConstraintStableId); Add(text, route.SourceRouteStableId);
                Add(text, route.FromAnchorStableId); Add(text, route.ToAnchorStableId);
                foreach (var id in route.OrderedSemanticStableIds) Add(text, id);
                Add(text, route.MinimumWidthMeters); Add(text, route.ContinuityRequired);
                Add(text, route.RouteSignature);
            }
            return text.ToString();
        }

        private static string CanonicalPlan(SimulationBattlefieldPlanSnapshot value)
        {
            var text = new StringBuilder();
            Add(text, value.SchemaVersion); Add(text, value.ProfileRevision);
            Add(text, value.GeneratorRevision); Add(text, value.WidthMeters);
            Add(text, value.DepthMeters); Add(text, value.GridCellSizeMeters);
            Add(text, value.BattlefieldDerivationInputHashSha256);
            foreach (var placement in value.AnchorPlacements.OrderBy(item =>
                         item.BattlefieldAnchorStableId, StringComparer.Ordinal))
            {
                Add(text, placement.BattlefieldAnchorStableId);
                Add(text, CanonicalPose(placement.BattlePose));
                Add(text, placement.WidthMeters); Add(text, placement.DepthMeters);
                Add(text, placement.SizeVariantCode);
            }
            foreach (var zone in value.Zones.OrderBy(item => item.ZoneStableId,
                         StringComparer.Ordinal))
            {
                Add(text, zone.ZoneStableId); Add(text, zone.ZoneKindCode);
                Add(text, CanonicalPose(zone.CenterPose)); Add(text, zone.WidthMeters);
                Add(text, zone.DepthMeters); Add(text, zone.SourceAnchorStableId);
            }
            foreach (var cell in value.TerrainCells)
            {
                Add(text, cell.CellX); Add(text, cell.CellZ); Add(text, cell.HeightCentimeters);
                Add(text, cell.MovementCostPermille); Add(text, cell.TerrainCode);
                Add(text, cell.Walkable);
            }
            return text.ToString();
        }

        private static void Add(StringBuilder target, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture));
            target.Append(':').Append(text).Append('|');
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
        private static string ShortHash(string value) => Hash(value)[..24];
        private static ulong ParseSeed(string hash)
            => ulong.Parse(hash[..16], NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        private readonly struct Vector2
        {
            public Vector2(double x, double z, double length)
            {
                X = x;
                Z = z;
                Length = length;
            }

            public double X { get; }
            public double Z { get; }
            public double Length { get; }
        }

        private struct Pcg32
        {
            private ulong state;
            private readonly ulong increment;
            public Pcg32(ulong seed, ulong sequence)
            {
                state = 0UL;
                increment = (sequence << 1) | 1UL;
                NextUInt();
                state += seed;
                NextUInt();
            }
            public uint NextUInt()
            {
                var old = state;
                state = unchecked(old * 6364136223846793005UL + increment);
                var xorShifted = (uint)(((old >> 18) ^ old) >> 27);
                var rotation = (int)(old >> 59);
                return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
            }
            public double NextDouble() => NextUInt() / ((double)uint.MaxValue + 1d);
        }
    }
}
