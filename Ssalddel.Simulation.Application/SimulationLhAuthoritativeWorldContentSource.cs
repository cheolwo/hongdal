using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// E6 적용 여부와 무관하게 H5 상대 공간을 권위 원본으로 읽어 L3 셀 계획을 만든다.
    /// Floating Origin과 표현 계절은 H5 배치 해시를 변경하지 않는다.
    /// </summary>
    public sealed class H5AuthoritativeSimulationLhCellContentSource :
        ISimulationLhCellContentSource
    {
        private readonly SimulationWorldLayoutCatalog catalog;

        public H5AuthoritativeSimulationLhCellContentSource(
            ISimulationWorldLayoutCatalogReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (!reader.TryRead(out catalog, out var errorCode))
                throw new InvalidOperationException(errorCode);
        }

        public string ContentSourceCode => SimulationLhWorldCodes.AuthoritativeWorld;

        public SimulationLhCellPlanResponse CreateCellPlan(
            SimulationLhWindowCell windowCell,
            SimulationLhCellContentContext context)
        {
            var definition = catalog.Definition;
            var cellX = (windowCell.CellX - SimulationLhWorldService.CenterL3X)
                        * SimulationLhWorldService.L3CellSizeMeters;
            var cellZ = (windowCell.CellY - SimulationLhWorldService.CenterL3Y)
                        * SimulationLhWorldService.L3CellSizeMeters;
            var area = definition.AreaSetInstances
                .OrderBy(item => DistanceSquared(item.PlacementTransform.LocalXMeters,
                    item.PlacementTransform.LocalZMeters, cellX, cellZ))
                .ThenBy(item => item.AreaSetInstanceStableId, StringComparer.Ordinal)
                .First();
            var graphAnchors = area.GraphInstances.Select(graph => new
                {
                    Graph = graph,
                    Pose = Compose(area.PlacementTransform, graph.PlacementTransform),
                })
                .Concat(definition.CorridorInstances.Select(corridor => new
                {
                    Graph = new SimulationWorldGraphInstanceResponse
                    {
                        GraphInstanceStableId = corridor.CorridorInstanceStableId,
                        LandscapeGraphStableId = corridor.LandscapeGraphStableId,
                        H3Ref = corridor.LandscapeGraphStableId,
                    },
                    Pose = corridor.PlacementTransform,
                }))
                .OrderBy(item => DistanceSquared(item.Pose.LocalXMeters,
                    item.Pose.LocalZMeters, cellX, cellZ))
                .ThenBy(item => item.Graph.LandscapeGraphStableId, StringComparer.Ordinal)
                .ToArray();
            var nearestGraph = graphAnchors.First();
            var placements = graphAnchors
                .Where(item => CellX(item.Pose.LocalXMeters) == windowCell.CellX
                               && CellY(item.Pose.LocalZMeters) == windowCell.CellY)
                .Select(item => new SimulationLhPlacementResponse
                {
                    GeneratedStableId = "lh-h5-anchor:" + Hash(item.Graph.GraphInstanceStableId).Substring(0, 24),
                    OwnerCellKey = windowCell.CellKey,
                    LayerCode = "H3Intent",
                    CompositionKey = item.Graph.LandscapeGraphStableId,
                    H1StableId = string.Empty,
                    EvidenceKindCode = SimulationLhWorldCodes.AuthoritativeWorld,
                    LocalXMeters = item.Pose.LocalXMeters - cellX,
                    LocalZMeters = item.Pose.LocalZMeters - cellZ,
                    RotationDegrees = item.Pose.RotationDegrees,
                    UniformScale = 1d,
                    FixedAnchor = true,
                    CollisionEligible = false,
                    PresentationOnly = true,
                }).OrderBy(item => item.GeneratedStableId, StringComparer.Ordinal).ToArray();
            var bindings = new[]
            {
                Binding("H4", area.AreaSetInstanceStableId),
                Binding("H3", nearestGraph.Graph.LandscapeGraphStableId),
            };
            var connectors = CreateConnectors(windowCell.CellX, windowCell.CellY);
            var baseHash = Hash(string.Join("|", new[]
            {
                definition.WorldLayoutStableId,
                definition.WorldLayoutRevision.ToString(CultureInfo.InvariantCulture),
                definition.WorldLayoutHashSha256,
                catalog.GroundingBinding.PlacementAuthorityCode,
                catalog.GroundingBinding.WorldGroundingStateCode,
                windowCell.CellKey,
                area.AreaSetInstanceStableId,
                nearestGraph.Graph.LandscapeGraphStableId,
                string.Join(",", placements.Select(item => item.GeneratedStableId)),
            }));
            return new SimulationLhCellPlanResponse
            {
                CellKey = windowCell.CellKey,
                CellX = windowCell.CellX,
                CellY = windowCell.CellY,
                L2ParentCellKey = $"kr5186:l2:{SimulationLhWorldGrid.FloorDiv(windowCell.CellX, 4)}:{SimulationLhWorldGrid.FloorDiv(windowCell.CellY, 4)}",
                WindowRoleCode = windowCell.WindowRoleCode,
                Priority = windowCell.Priority,
                ContentSourceCode = ContentSourceCode,
                BasePlanHashSha256 = baseHash,
                PresentationHashSha256 = Hash(baseHash + "|" + context.Season.SeasonCode
                                                    + "|" + context.Season.SeasonRuleVersion),
                HBindings = bindings,
                Placements = placements,
                Connectors = connectors,
                RequiredCapabilityCodes = context.RequiredCapabilityCodes.Length == 0
                    ? DefaultCapabilities(windowCell.WindowRoleCode)
                    : context.RequiredCapabilityCodes.OrderBy(item => item, StringComparer.Ordinal).ToArray(),
                PlayerTraversalRequired = windowCell.WindowRoleCode != SimulationLhWorldCodes.Prefetch,
                PresentationOnly = true,
            };
        }

        private static SimulationWorldPlacementTransformResponse Compose(
            SimulationWorldPlacementTransformResponse parent,
            SimulationWorldPlacementTransformResponse child)
        {
            var radians = parent.RotationDegrees * Math.PI / 180d;
            return new SimulationWorldPlacementTransformResponse
            {
                CoordinateSpaceCode = SimulationWorldLayoutCodes.ScenarioLocalMeters,
                LocalXMeters = parent.LocalXMeters + Math.Cos(radians) * child.LocalXMeters
                               + Math.Sin(radians) * child.LocalZMeters,
                LocalZMeters = parent.LocalZMeters - Math.Sin(radians) * child.LocalXMeters
                               + Math.Cos(radians) * child.LocalZMeters,
                RotationDegrees = (parent.RotationDegrees + child.RotationDegrees) % 360d,
            };
        }

        private static int CellX(double x) => SimulationLhWorldService.CenterL3X
            + (int)Math.Floor((x + SimulationLhWorldService.L3CellSizeMeters / 2d)
                              / SimulationLhWorldService.L3CellSizeMeters);
        private static int CellY(double z) => SimulationLhWorldService.CenterL3Y
            + (int)Math.Floor((z + SimulationLhWorldService.L3CellSizeMeters / 2d)
                              / SimulationLhWorldService.L3CellSizeMeters);
        private static double DistanceSquared(double x, double z, double targetX, double targetZ)
            => (x - targetX) * (x - targetX) + (z - targetZ) * (z - targetZ);
        private static SimulationLhHBindingResponse Binding(string level, string stableId) => new()
        {
            HLevelCode = level,
            SpatialStableId = stableId,
            StateCode = SimulationLhWorldCodes.ApprovedReference,
            WorldInteractionIds = Array.Empty<string>(),
        };
        private static string[] DefaultCapabilities(string role) => role == SimulationLhWorldCodes.Prefetch
            ? new[] { SimulationLhWorldCodes.TerrainVisual }
            : new[]
            {
                SimulationLhWorldCodes.TerrainVisual,
                SimulationLhWorldCodes.Collision,
                SimulationLhWorldCodes.Connector,
                SimulationLhWorldCodes.NpcNavigation,
            };
        private static SimulationLhConnectorResponse[] CreateConnectors(int x, int y) => new[]
        {
            Connector(x, y, x, y + 1, "N"), Connector(x, y, x + 1, y, "E"),
            Connector(x, y, x, y - 1, "S"), Connector(x, y, x - 1, y, "W"),
        };
        private static SimulationLhConnectorResponse Connector(int x, int y, int neighborX, int neighborY, string side)
        {
            var first = SimulationLhWorldService.L3CellKey(x, y);
            var second = SimulationLhWorldService.L3CellKey(neighborX, neighborY);
            var pair = new[] { first, second }.OrderBy(item => item, StringComparer.Ordinal).ToArray();
            var hash = Hash(pair[0] + "|" + pair[1] + "|h5-connector.v1");
            return new SimulationLhConnectorResponse
            {
                ConnectorStableId = "lh-h5-connector:" + hash.Substring(0, 24),
                SideCode = side,
                NeighborCellKey = second,
                BoundaryHashSha256 = hash,
                Passable = SimulationLhWorldGrid.IsInsideApprovedCoverage(neighborX, neighborY),
            };
        }
        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(value))
                .Select(item => item.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
