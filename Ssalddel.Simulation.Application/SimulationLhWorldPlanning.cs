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
    /// 플레이어 주변에서 먼저 준비할 실행 셀과 준비 순서를 계산한다.
    /// 현재 계약의 L3 셀 키는 호환 Adapter이며 계산 자체는 H 의미를 알지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public interface ISimulationLhWindowPlanner
    {
        SimulationLhWindowPlan Plan(
            SimulationLhCellPreviewRequest request,
            SimulationLhWorldProfileResponse profile);
    }

    public sealed class SimulationLhWindowPlan
    {
        public SimulationLhWindowCell[] Cells { get; init; }
            = Array.Empty<SimulationLhWindowCell>();
        public string[] OutsideCoverageCellKeys { get; init; }
            = Array.Empty<string>();
    }

    public sealed class SimulationLhWindowCell
    {
        public string CellKey { get; init; } = string.Empty;
        public int CellX { get; init; }
        public int CellY { get; init; }
        public string WindowRoleCode { get; init; } = string.Empty;
        public int Priority { get; init; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class SimulationLhWindowPlanner : ISimulationLhWindowPlanner
    {
        public SimulationLhWindowPlan Plan(
            SimulationLhCellPreviewRequest request,
            SimulationLhWorldProfileResponse profile)
        {
            ValidateWindowPolicy(profile);
            if (!SimulationLhWorldGrid.TryParseL3CellKey(
                    request.FocusL3CellKey, out var focusX, out var focusY))
                throw new ArgumentException("SimulationLhFocusCellInvalid", nameof(request));
            if (!SimulationLhWorldGrid.IsInsideApprovedCoverage(focusX, focusY))
                throw new ArgumentOutOfRangeException(
                    nameof(request), "SimulationLhFocusOutsideApprovedH4");

            var cells = new List<SimulationLhWindowCell>();
            var outside = new List<string>();
            var movement = DirectionVector(request.MovementDirectionCode);
            for (var y = focusY - profile.PrefetchRadius;
                 y <= focusY + profile.PrefetchRadius; y++)
            for (var x = focusX - profile.PrefetchRadius;
                 x <= focusX + profile.PrefetchRadius; x++)
            {
                var cellKey = SimulationLhWorldGrid.L3CellKey(x, y);
                if (!SimulationLhWorldGrid.IsInsideApprovedCoverage(x, y))
                {
                    outside.Add(cellKey);
                    continue;
                }

                var dx = x - focusX;
                var dy = y - focusY;
                var distance = Math.Max(Math.Abs(dx), Math.Abs(dy));
                var role = distance <= profile.DetailRadius
                    ? SimulationLhWorldCodes.Detail
                    : distance <= profile.ActiveRadius
                        ? SimulationLhWorldCodes.Active
                        : SimulationLhWorldCodes.Prefetch;
                var directionalBias = dx * movement.X + dy * movement.Y > 0 ? -10 : 0;
                cells.Add(new SimulationLhWindowCell
                {
                    CellKey = cellKey,
                    CellX = x,
                    CellY = y,
                    WindowRoleCode = role,
                    Priority = distance * 100 + directionalBias + Math.Abs(dx) + Math.Abs(dy),
                });
            }

            return new SimulationLhWindowPlan
            {
                Cells = cells.OrderBy(value => value.Priority)
                    .ThenBy(value => value.CellKey, StringComparer.Ordinal).ToArray(),
                OutsideCoverageCellKeys = outside
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };
        }

        private static void ValidateWindowPolicy(SimulationLhWorldProfileResponse profile)
        {
            if (profile == null
                || profile.DetailRadius < 0
                || profile.ActiveRadius < profile.DetailRadius
                || profile.PrefetchRadius < profile.ActiveRadius
                || profile.MaxConcurrentPreparations < 1
                || profile.BoundaryPrefetchFraction <= 0d
                || profile.BoundaryPrefetchFraction > .5d)
                throw new ArgumentException("SimulationDynamicMapWindowPolicyInvalid",
                    nameof(profile));
        }

        private static (int X, int Y) DirectionVector(string directionCode)
            => directionCode switch
            {
                SimulationLhWorldCodes.North => (0, 1),
                SimulationLhWorldCodes.NorthEast => (1, 1),
                SimulationLhWorldCodes.East => (1, 0),
                SimulationLhWorldCodes.SouthEast => (1, -1),
                SimulationLhWorldCodes.South => (0, -1),
                SimulationLhWorldCodes.SouthWest => (-1, -1),
                SimulationLhWorldCodes.West => (-1, 0),
                SimulationLhWorldCodes.NorthWest => (-1, 1),
                _ => (0, 0),
            };
    }

    /// <summary>
    /// 스트리밍 범위 계산기가 고른 실행 셀 안에 들어갈 H 의미와 배치를 공급한다.
    /// 실제 E5·E6 공급자는 이 경계를 구현하고 스트리밍 실행기는 그대로 재사용한다.
    /// </summary>
    public interface ISimulationLhCellContentSource
    {
        string ContentSourceCode { get; }

        SimulationLhCellPlanResponse CreateCellPlan(
            SimulationLhWindowCell windowCell,
            SimulationLhCellContentContext context);
    }

    public sealed class SimulationLhCellContentContext
    {
        public SimulationLhSeasonSnapshot Season { get; init; } = new();
        public string[] RequiredCapabilityCodes { get; init; } = Array.Empty<string>();
        public int WorldTick { get; init; }
        public long WorldRevision { get; init; }
    }

    /// <summary>
    /// 기존 결정론적 로컬·서버 시험 결과를 보존하는 시나리오 절차생성 셀 내용 공급자다.
    /// 실제 E5·E6 자료가 없을 때 권위 있는 자료인 것처럼 자동 승격하지 않는다.
    /// </summary>
    public sealed class ScenarioProceduralSimulationLhCellContentSource
        : ISimulationLhCellContentSource
    {
        private static readonly string[] ProceduralCompositionKeys =
        {
            "farm:감자밭 두렁:A",
            "farm:감자밭 두렁:B",
            "farm:혼합 작물밭:A",
            "nature:초지·야생화:A",
            "nature:숲 가장자리:A",
            "nature:침엽수림 군집:A",
        };

        private readonly SimulationLhSpatialKnowledgeProvider spatialKnowledge;

        public ScenarioProceduralSimulationLhCellContentSource()
            : this(SimulationLhSpatialKnowledgeProvider.LoadEmbedded())
        {
        }

        internal ScenarioProceduralSimulationLhCellContentSource(
            SimulationLhSpatialKnowledgeProvider knowledge)
        {
            spatialKnowledge = knowledge ?? throw new ArgumentNullException(nameof(knowledge));
        }

        public string ContentSourceCode => SimulationLhWorldCodes.ScenarioProcedural;

        public SimulationLhCellPlanResponse CreateCellPlan(
            SimulationLhWindowCell windowCell,
            SimulationLhCellContentContext context)
        {
            var hBindings = CreateHBindings(windowCell.CellX, windowCell.CellY);
            var placements = CreatePlacements(
                windowCell.CellKey, windowCell.CellX, windowCell.CellY);
            var connectors = CreateConnectors(windowCell.CellX, windowCell.CellY);
            var capabilities = context.RequiredCapabilityCodes.Length == 0
                ? DefaultCapabilities(windowCell.WindowRoleCode)
                : context.RequiredCapabilityCodes
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var baseHash = Hash(string.Join("|", new[]
            {
                SimulationLhWorldCodes.WorldSeed,
                SimulationLhWorldCodes.GeneratorVersion,
                windowCell.CellKey,
                string.Join(",", hBindings.Select(value =>
                    value.HLevelCode + ":" + value.SpatialStableId + ":" + value.StateCode)),
                string.Join(",", placements.Select(PlacementCanonical)),
                string.Join(",", connectors.Select(value =>
                    value.ConnectorStableId + ":" + value.BoundaryHashSha256)),
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
                PresentationHashSha256 = Hash(baseHash + "|"
                    + context.Season.SeasonCode + "|" + context.Season.SeasonRuleVersion),
                HBindings = hBindings,
                Placements = placements,
                Connectors = connectors,
                RequiredCapabilityCodes = capabilities,
                PlayerTraversalRequired =
                    windowCell.WindowRoleCode != SimulationLhWorldCodes.Prefetch,
                PresentationOnly = true,
            };
        }

        private SimulationLhHBindingResponse[] CreateHBindings(int x, int y)
        {
            var values = new List<SimulationLhHBindingResponse>
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
                {
                    values.Add(Binding("H1", binding.InteractionH1Ref,
                        SimulationLhWorldCodes.ApprovedReference,
                        binding.WorldInteractionIds));
                }
            }
            return values.ToArray();
        }

        private SimulationLhPlacementResponse[] CreatePlacements(
            string cellKey, int x, int y)
        {
            var values = new List<SimulationLhPlacementResponse>();
            var packing = spatialKnowledge.GetSpace("packing-area");
            var production = spatialKnowledge.GetSpace("production-plot");
            var collection = spatialKnowledge.GetSpace("collection-area");
            var loading = spatialKnowledge.GetSpace("loading-area");
            AddFixedAnchor(values, cellKey, x, y,
                SimulationLhWorldService.CenterL3X,
                SimulationLhWorldService.CenterL3Y,
                "farmhouse", packing.PreferredCompositionKey,
                packing.InteractionH1Ref, -18d, 12d, 0d);
            AddFixedAnchor(values, cellKey, x, y,
                SimulationLhWorldService.CenterL3X + 1,
                SimulationLhWorldService.CenterL3Y,
                "potato-field", production.PreferredCompositionKey,
                production.InteractionH1Ref, 4d, -6d, 0d);
            AddFixedAnchor(values, cellKey, x, y,
                SimulationLhWorldService.CenterL3X,
                SimulationLhWorldService.CenterL3Y + 1,
                "work-yard", collection.PreferredCompositionKey,
                collection.InteractionH1Ref, 10d, 8d, 90d);
            AddFixedAnchor(values, cellKey, x, y,
                SimulationLhWorldService.CenterL3X + 1,
                SimulationLhWorldService.CenterL3Y + 1,
                "farm-gate", loading.PreferredCompositionKey,
                loading.InteractionH1Ref, -8d, -10d, 0d);

            var randomBytes = HashBytes(string.Join("|", new[]
            {
                SimulationLhWorldCodes.WorldSeed,
                SimulationLhWorldCodes.GeneratorVersion,
                "L3Surface",
                cellKey,
            }));
            var count = 3 + randomBytes[0] % 3;
            for (var index = 0; index < count; index++)
            {
                var offset = 1 + index * 5;
                var key = ProceduralCompositionKeys[
                    randomBytes[offset] % ProceduralCompositionKeys.Length];
                var identity = Hash(cellKey + "|L3Surface|" + index + "|" + key);
                values.Add(new SimulationLhPlacementResponse
                {
                    GeneratedStableId = "lh-object:" + identity.Substring(0, 24),
                    OwnerCellKey = cellKey,
                    LayerCode = "L3Surface",
                    CompositionKey = key,
                    H1StableId = key.StartsWith("farm:", StringComparison.Ordinal)
                        ? "h1-stock:farm-production" : string.Empty,
                    EvidenceKindCode = SimulationLhWorldCodes.ScenarioProcedural,
                    LocalXMeters = -50d + randomBytes[offset + 1] / 255d * 100d,
                    LocalZMeters = -50d + randomBytes[offset + 2] / 255d * 100d,
                    RotationDegrees = randomBytes[offset + 3] / 255d * 360d,
                    UniformScale = .85d + randomBytes[offset + 4] / 255d * .3d,
                    CollisionEligible = false,
                    PresentationOnly = true,
                });
            }
            return values.OrderBy(value => value.GeneratedStableId, StringComparer.Ordinal)
                .ToArray();
        }

        private static void AddFixedAnchor(
            ICollection<SimulationLhPlacementResponse> values,
            string cellKey, int x, int y, int expectedX, int expectedY,
            string slot, string compositionKey, string h1StableId,
            double localX, double localZ, double rotation)
        {
            if (x != expectedX || y != expectedY) return;
            var identity = Hash(cellKey + "|H3Intent|" + slot);
            values.Add(new SimulationLhPlacementResponse
            {
                GeneratedStableId = "lh-anchor:" + identity.Substring(0, 24),
                OwnerCellKey = cellKey,
                LayerCode = "H3Intent",
                CompositionKey = compositionKey,
                H1StableId = h1StableId,
                EvidenceKindCode = SimulationLhWorldCodes.ScenarioProcedural,
                LocalXMeters = localX,
                LocalZMeters = localZ,
                RotationDegrees = rotation,
                UniformScale = 1d,
                FixedAnchor = true,
                CollisionEligible = false,
                PresentationOnly = true,
            });
        }

        private static SimulationLhConnectorResponse[] CreateConnectors(int x, int y)
            => new[]
            {
                Connector(x, y, x, y + 1, "N"),
                Connector(x, y, x + 1, y, "E"),
                Connector(x, y, x, y - 1, "S"),
                Connector(x, y, x - 1, y, "W"),
            };

        private static SimulationLhConnectorResponse Connector(
            int x, int y, int neighborX, int neighborY, string side)
        {
            var cell = SimulationLhWorldGrid.L3CellKey(x, y);
            var neighbor = SimulationLhWorldGrid.L3CellKey(neighborX, neighborY);
            var pair = new[] { cell, neighbor }
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var boundaryHash = Hash(pair[0] + "|" + pair[1] + "|connector.v1");
            return new SimulationLhConnectorResponse
            {
                ConnectorStableId = "lh-connector:" + boundaryHash.Substring(0, 24),
                SideCode = side,
                NeighborCellKey = neighbor,
                BoundaryHashSha256 = boundaryHash,
                Passable = SimulationLhWorldGrid.IsInsideApprovedCoverage(
                    neighborX, neighborY),
            };
        }

        private static string[] DefaultCapabilities(string role)
        {
            if (role == SimulationLhWorldCodes.Prefetch)
                return new[] { SimulationLhWorldCodes.TerrainVisual };
            if (role == SimulationLhWorldCodes.Active)
                return new[]
                {
                    SimulationLhWorldCodes.TerrainVisual,
                    SimulationLhWorldCodes.Collision,
                    SimulationLhWorldCodes.Connector,
                };
            return new[]
            {
                SimulationLhWorldCodes.TerrainVisual,
                SimulationLhWorldCodes.Collision,
                SimulationLhWorldCodes.Connector,
                SimulationLhWorldCodes.H1Interaction,
                SimulationLhWorldCodes.NpcNavigation,
                SimulationLhWorldCodes.SeasonPresentation,
            };
        }

        private static SimulationLhHBindingResponse Binding(
            string level, string stableId, string stateCode,
            params string[] worldInteractionIds)
            => new()
            {
                HLevelCode = level,
                SpatialStableId = stableId,
                StateCode = stateCode,
                WorldInteractionIds = worldInteractionIds,
            };

        private static string PlacementCanonical(SimulationLhPlacementResponse value)
            => string.Join(":", new[]
            {
                value.GeneratedStableId,
                value.CompositionKey,
                value.H1StableId,
                value.LocalXMeters.ToString("R", CultureInfo.InvariantCulture),
                value.LocalZMeters.ToString("R", CultureInfo.InvariantCulture),
                value.RotationDegrees.ToString("R", CultureInfo.InvariantCulture),
                value.UniformScale.ToString("R", CultureInfo.InvariantCulture),
                value.FixedAnchor.ToString(),
            });

        private static byte[] HashBytes(string canonical)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
        }

        private static string Hash(string canonical)
        {
            var bytes = HashBytes(canonical);
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var value in bytes) builder.Append(value.ToString("x2"));
            return builder.ToString();
        }
    }

    internal static class SimulationLhWorldGrid
    {
        public static string L3CellKey(int x, int y)
            => $"kr5186:l3:{x}:{y}";

        public static bool TryParseL3CellKey(string cellKey, out int x, out int y)
        {
            x = 0;
            y = 0;
            var parts = (cellKey ?? string.Empty).Split(':');
            if (parts.Length != 4 || parts[0] != "kr5186" || parts[1] != "l3")
                return false;
            return int.TryParse(parts[2], NumberStyles.Integer,
                       CultureInfo.InvariantCulture, out x)
                   && int.TryParse(parts[3], NumberStyles.Integer,
                       CultureInfo.InvariantCulture, out y);
        }

        public static bool IsInsideApprovedCoverage(int x, int y)
            => x >= SimulationLhWorldService.MinimumL3X
               && x <= SimulationLhWorldService.MaximumL3X
               && y >= SimulationLhWorldService.MinimumL3Y
               && y <= SimulationLhWorldService.MaximumL3Y;

        public static int FloorDiv(int value, int divisor)
        {
            var quotient = value / divisor;
            return value % divisor < 0 ? quotient - 1 : quotient;
        }
    }
}
