using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Application,
        "플레이어 L3 위치를 기준으로 H 계보와 결정론적 주변 공간 후보를 미리 계산한다.",
        StepKey = "application.lh-world-preview",
        DependsOnStepKeys = new[] { "contract.lh-world-profile" },
        ExecutionStage = SsalddelCodeExecutionStage.Preview,
        ReadsFrom = SsalddelCodeDataScope.SimulationState | SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 31,
        Boundary = "Preview는 H 권위·운영 원장·자원 원장을 변경하지 않고 ScenarioProcedural 배치 후보만 반환한다.")]
    public sealed class SimulationLhWorldService
    {
        public const string ProfileRevision = "lh-world.pyeongchang-farm.r1";
        public const string AreaSetRevision = "area-set.pyeongchang-farm-hub-town.r1";
        public const string SeasonRuleVersion = "simulation-season.28-day.r1";
        public const int L3CellSizeMeters = 125;
        public const int CenterL3X = 2801;
        public const int CenterL3Y = 4581;
        public const int MinimumL3X = 2780;
        public const int MaximumL3X = 2823;
        public const int MinimumL3Y = 4560;
        public const int MaximumL3Y = 4603;

        private static readonly string[] SeasonCodes =
        {
            SimulationLhWorldCodes.Spring,
            SimulationLhWorldCodes.Summer,
            SimulationLhWorldCodes.Autumn,
            SimulationLhWorldCodes.Winter,
        };

        private static readonly HashSet<string> CapabilityCodes = new(
            new[]
            {
                SimulationLhWorldCodes.TerrainVisual,
                SimulationLhWorldCodes.Collision,
                SimulationLhWorldCodes.Connector,
                SimulationLhWorldCodes.H1Interaction,
                SimulationLhWorldCodes.NpcNavigation,
                SimulationLhWorldCodes.SeasonPresentation,
            }, StringComparer.Ordinal);

        private static readonly HashSet<string> DirectionCodes = new(
            new[]
            {
                SimulationLhWorldCodes.None,
                SimulationLhWorldCodes.North,
                SimulationLhWorldCodes.NorthEast,
                SimulationLhWorldCodes.East,
                SimulationLhWorldCodes.SouthEast,
                SimulationLhWorldCodes.South,
                SimulationLhWorldCodes.SouthWest,
                SimulationLhWorldCodes.West,
                SimulationLhWorldCodes.NorthWest,
            }, StringComparer.Ordinal);

        private static readonly string[] ProceduralCompositionKeys =
        {
            "farm:감자밭 두렁:A",
            "farm:감자밭 두렁:B",
            "farm:혼합 작물밭:A",
            "nature:초지·야생화:A",
            "nature:숲 가장자리:A",
            "nature:침엽수림 군집:A",
        };

        private static readonly SimulationLhSpatialKnowledgeProvider DefaultSpatialKnowledge =
            SimulationLhSpatialKnowledgeProvider.LoadEmbedded();
        private readonly SimulationLhSpatialKnowledgeProvider spatialKnowledge;

        public SimulationLhWorldService()
        {
            spatialKnowledge = DefaultSpatialKnowledge;
        }

        public static SimulationLhWorldProfileResponse CreateDefaultProfile()
        {
            var profile = new SimulationLhWorldProfileResponse
            {
                SchemaVersion = SimulationLhWorldCodes.SchemaVersion,
                ProfileRevision = ProfileRevision,
                WorldSeed = SimulationLhWorldCodes.WorldSeed,
                GeneratorVersion = SimulationLhWorldCodes.GeneratorVersion,
                AreaSetStableId = PyeongchangAreaSetStableIds.AreaSet,
                AreaSetRevision = AreaSetRevision,
                AreaSetBoundaryHashSha256 = Hash(string.Join("|", new[]
                {
                    PyeongchangAreaSetStableIds.AreaSet,
                    MinimumL3X.ToString(CultureInfo.InvariantCulture),
                    MaximumL3X.ToString(CultureInfo.InvariantCulture),
                    MinimumL3Y.ToString(CultureInfo.InvariantCulture),
                    MaximumL3Y.ToString(CultureInfo.InvariantCulture),
                })),
                SpatialKnowledgeRevision = DefaultSpatialKnowledge.StatusRevision,
                SpatialCompositionPlanStableId =
                    DefaultSpatialKnowledge.CompositionPlanStableId,
                Levels = new[]
                {
                    Level("L0", 8000, "H4"),
                    Level("L1", 2000, "H3"),
                    Level("L2", 500, "H2"),
                    Level("L3", L3CellSizeMeters, "H1"),
                },
                DetailRadius = 1,
                ActiveRadius = 2,
                PrefetchRadius = 4,
                MaxConcurrentPreparations = 4,
                BoundaryPrefetchFraction = .25d,
                MainThreadAssemblyBudgetMilliseconds = 2d,
                CachedCellCapacity = 32,
                OriginShiftThresholdWorldUnits = 2048,
                GenerationLayers = new[]
                {
                    Layer("H4Boundary", Array.Empty<string>(), 0, "AuthoritativeEnvelope"),
                    Layer("H3Intent", new[] { "H4Boundary" }, 500, "ApprovedAnchor"),
                    Layer("H2BlockPlan", new[] { "H3Intent" }, 250, "OwnedWithinBounds"),
                    Layer("H1Workspace", new[] { "H2BlockPlan" }, 125, "OwnedWithinBounds"),
                    Layer("L3Surface", new[] { "H1Workspace" }, 60, "OverlappingBounds"),
                    Layer("SeasonOverlay", new[] { "L3Surface" }, 0, "PresentationOnly"),
                },
                PresentationOnly = true,
                IsOperationalState = false,
            };
            profile.ProfileHashSha256 = ProfileHash(profile);
            return profile;
        }

        public SimulationLhCellPreviewResponse Preview(
            SimulationLhCellPreviewRequest request,
            int dayNumber,
            int worldTick,
            long worldRevision)
        {
            ValidateRequest(request, worldRevision);
            if (!TryParseL3CellKey(request.FocusL3CellKey, out var focusX, out var focusY))
                throw new ArgumentException("SimulationLhFocusCellInvalid", nameof(request));
            if (!IsInsideCoverage(focusX, focusY))
                throw new ArgumentOutOfRangeException(
                    nameof(request), "SimulationLhFocusOutsideApprovedH4");

            var profile = CreateDefaultProfile();
            var season = CreateSeason(dayNumber);
            var cells = new List<SimulationLhCellPlanResponse>();
            var outside = new List<string>();
            var movement = DirectionVector(request.MovementDirectionCode);

            for (var y = focusY - profile.PrefetchRadius;
                 y <= focusY + profile.PrefetchRadius; y++)
            for (var x = focusX - profile.PrefetchRadius;
                 x <= focusX + profile.PrefetchRadius; x++)
            {
                var cellKey = L3CellKey(x, y);
                if (!IsInsideCoverage(x, y))
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
                var priority = distance * 100 + directionalBias
                               + Math.Abs(dx) + Math.Abs(dy);
                cells.Add(CreateCellPlan(
                    cellKey, x, y, role, priority, season,
                    request.RequiredCapabilityCodes));
            }

            return new SimulationLhCellPreviewResponse
            {
                RequestEpoch = request.RequestEpoch.Trim(),
                RecipeStableId = request.RecipeStableId,
                AreaSetStableId = profile.AreaSetStableId,
                WorldTick = worldTick,
                WorldRevision = worldRevision,
                Season = season,
                Profile = profile,
                Cells = cells.OrderBy(value => value.Priority)
                    .ThenBy(value => value.CellKey, StringComparer.Ordinal).ToArray(),
                OutsideCoverageCellKeys = outside.OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                IsCandidateOnly = true,
                DoesNotApplyResourceLedgers = true,
                IsOperationalState = false,
            };
        }

        public static SimulationLhSeasonSnapshot CreateSeason(int dayNumber)
        {
            if (dayNumber < 1) throw new ArgumentOutOfRangeException(nameof(dayNumber));
            var zeroBasedDay = dayNumber - 1;
            var seasonIndex = (zeroBasedDay / 28) % SeasonCodes.Length;
            var seasonDay = zeroBasedDay % 28 + 1;
            return new SimulationLhSeasonSnapshot
            {
                SeasonCode = SeasonCodes[seasonIndex],
                SeasonIndex = seasonIndex,
                SeasonDay = seasonDay,
                SeasonProgress01 = (seasonDay - 1) / 27d,
                NextSeasonCode = SeasonCodes[(seasonIndex + 1) % SeasonCodes.Length],
                SeasonRuleVersion = SeasonRuleVersion,
                DayNumber = dayNumber,
            };
        }

        public static string L3CellKey(int x, int y) => $"kr5186:l3:{x}:{y}";

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

        private SimulationLhCellPlanResponse CreateCellPlan(
            string cellKey,
            int x,
            int y,
            string role,
            int priority,
            SimulationLhSeasonSnapshot season,
            IReadOnlyCollection<string> requestedCapabilities)
        {
            var hBindings = CreateHBindings(x, y);
            var placements = CreatePlacements(cellKey, x, y);
            var connectors = CreateConnectors(x, y);
            var capabilities = requestedCapabilities.Count == 0
                ? DefaultCapabilities(role)
                : requestedCapabilities.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var baseHash = Hash(string.Join("|", new[]
            {
                SimulationLhWorldCodes.WorldSeed,
                SimulationLhWorldCodes.GeneratorVersion,
                cellKey,
                string.Join(",", hBindings.Select(value =>
                    value.HLevelCode + ":" + value.SpatialStableId + ":" + value.StateCode)),
                string.Join(",", placements.Select(PlacementCanonical)),
                string.Join(",", connectors.Select(value =>
                    value.ConnectorStableId + ":" + value.BoundaryHashSha256)),
            }));
            return new SimulationLhCellPlanResponse
            {
                CellKey = cellKey,
                CellX = x,
                CellY = y,
                L2ParentCellKey = $"kr5186:l2:{FloorDiv(x, 4)}:{FloorDiv(y, 4)}",
                WindowRoleCode = role,
                Priority = priority,
                BasePlanHashSha256 = baseHash,
                PresentationHashSha256 = Hash(baseHash + "|"
                    + season.SeasonCode + "|" + season.SeasonRuleVersion),
                HBindings = hBindings,
                Placements = placements,
                Connectors = connectors,
                RequiredCapabilityCodes = capabilities,
                PlayerTraversalRequired = role != SimulationLhWorldCodes.Prefetch,
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
            if (Math.Abs(x - CenterL3X) <= 1 && Math.Abs(y - CenterL3Y) <= 1)
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
            AddFixedAnchor(values, cellKey, x, y, CenterL3X, CenterL3Y,
                "farmhouse", packing.PreferredCompositionKey, packing.InteractionH1Ref,
                -18d, 12d, 0d);
            AddFixedAnchor(values, cellKey, x, y, CenterL3X + 1, CenterL3Y,
                "potato-field", production.PreferredCompositionKey,
                production.InteractionH1Ref,
                4d, -6d, 0d);
            AddFixedAnchor(values, cellKey, x, y, CenterL3X, CenterL3Y + 1,
                "work-yard", collection.PreferredCompositionKey,
                collection.InteractionH1Ref,
                10d, 8d, 90d);
            AddFixedAnchor(values, cellKey, x, y, CenterL3X + 1, CenterL3Y + 1,
                "farm-gate", loading.PreferredCompositionKey,
                loading.InteractionH1Ref,
                -8d, -10d, 0d);

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
                var localX = -50d + randomBytes[offset + 1] / 255d * 100d;
                var localZ = -50d + randomBytes[offset + 2] / 255d * 100d;
                var rotation = randomBytes[offset + 3] / 255d * 360d;
                var scale = .85d + randomBytes[offset + 4] / 255d * .3d;
                var identity = Hash(cellKey + "|L3Surface|" + index + "|" + key);
                values.Add(new SimulationLhPlacementResponse
                {
                    GeneratedStableId = "lh-object:" + identity.Substring(0, 24),
                    OwnerCellKey = cellKey,
                    LayerCode = "L3Surface",
                    CompositionKey = key,
                    H1StableId = key.StartsWith("farm:", StringComparison.Ordinal)
                        ? "h1-stock:farm-production"
                        : string.Empty,
                    EvidenceKindCode = SimulationLhWorldCodes.ScenarioProcedural,
                    LocalXMeters = localX,
                    LocalZMeters = localZ,
                    RotationDegrees = rotation,
                    UniformScale = scale,
                    FixedAnchor = false,
                    CollisionEligible = false,
                    PresentationOnly = true,
                });
            }
            return values.OrderBy(value => value.GeneratedStableId, StringComparer.Ordinal)
                .ToArray();
        }

        private static void AddFixedAnchor(
            ICollection<SimulationLhPlacementResponse> values,
            string cellKey,
            int x,
            int y,
            int expectedX,
            int expectedY,
            string slot,
            string compositionKey,
            string h1StableId,
            double localX,
            double localZ,
            double rotation)
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
            var cell = L3CellKey(x, y);
            var neighbor = L3CellKey(neighborX, neighborY);
            var pair = new[] { cell, neighbor }.OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var boundaryHash = Hash(pair[0] + "|" + pair[1] + "|connector.v1");
            return new SimulationLhConnectorResponse
            {
                ConnectorStableId = "lh-connector:" + boundaryHash.Substring(0, 24),
                SideCode = side,
                NeighborCellKey = neighbor,
                BoundaryHashSha256 = boundaryHash,
                Passable = IsInsideCoverage(neighborX, neighborY),
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

        private static void ValidateRequest(
            SimulationLhCellPreviewRequest request, long worldRevision)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.RequestEpoch)
                || string.IsNullOrWhiteSpace(request.SessionStableId)
                || request.RecipeStableId != SimulationWorldStreamCodes.PyeongchangFarmRecipe
                || PyeongchangAreaSetStableIds.NormalizeAreaSet(request.AreaSetStableId)
                != PyeongchangAreaSetStableIds.AreaSet
                || !DirectionCodes.Contains(request.MovementDirectionCode)
                || request.RequiredCapabilityCodes.Any(value => !CapabilityCodes.Contains(value))
                || request.KnownCellPlanHashesSha256.Any(value =>
                    value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character))))
                throw new ArgumentException("SimulationLhCellPreviewRequestInvalid", nameof(request));
            if (request.ExpectedWorldRevision != worldRevision)
                throw new InvalidOperationException("SimulationExpectedRevisionMismatch");
        }

        private static bool IsInsideCoverage(int x, int y)
            => x >= MinimumL3X && x <= MaximumL3X
               && y >= MinimumL3Y && y <= MaximumL3Y;

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

        private static int FloorDiv(int value, int divisor)
        {
            var quotient = value / divisor;
            var remainder = value % divisor;
            return remainder < 0 ? quotient - 1 : quotient;
        }

        private static SimulationLhLevelResponse Level(
            string levelCode, int sizeMeters, string defaultHLevelCode)
            => new()
            {
                LevelCode = levelCode,
                CellSizeMeters = sizeMeters,
                DefaultHLevelCode = defaultHLevelCode,
            };

        private static SimulationLhGenerationLayerResponse Layer(
            string layerCode,
            string[] dependencies,
            int paddingMeters,
            string ownershipRuleCode)
            => new()
            {
                LayerCode = layerCode,
                DependsOnLayerCodes = dependencies,
                MaximumPaddingMeters = paddingMeters,
                OwnershipRuleCode = ownershipRuleCode,
            };

        private static SimulationLhHBindingResponse Binding(
            string level,
            string stableId,
            string stateCode,
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

        private static string ProfileHash(SimulationLhWorldProfileResponse value)
            => Hash(string.Join("|", new[]
            {
                value.SchemaVersion,
                value.ProfileRevision,
                value.WorldSeed,
                value.GeneratorVersion,
                value.AreaSetStableId,
                value.AreaSetRevision,
                value.AreaSetBoundaryHashSha256,
                value.SpatialKnowledgeRevision,
                value.SpatialCompositionPlanStableId,
                string.Join(",", value.Levels.Select(level =>
                    level.LevelCode + ":" + level.CellSizeMeters + ":"
                    + level.DefaultHLevelCode)),
                value.DetailRadius.ToString(CultureInfo.InvariantCulture),
                value.ActiveRadius.ToString(CultureInfo.InvariantCulture),
                value.PrefetchRadius.ToString(CultureInfo.InvariantCulture),
                value.MaxConcurrentPreparations.ToString(CultureInfo.InvariantCulture),
                value.BoundaryPrefetchFraction.ToString("R", CultureInfo.InvariantCulture),
                value.MainThreadAssemblyBudgetMilliseconds.ToString("R", CultureInfo.InvariantCulture),
                string.Join(",", value.GenerationLayers.Select(layer =>
                    layer.LayerCode + ":" + string.Join("+", layer.DependsOnLayerCodes)
                    + ":" + layer.MaximumPaddingMeters + ":" + layer.OwnershipRuleCode)),
            }));

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
}
