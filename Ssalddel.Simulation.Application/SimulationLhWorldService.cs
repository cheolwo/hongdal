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
        "플레이어 L3 위치를 기준으로 스트리밍 범위와 셀 내용을 분리해 주변 공간 후보를 계산한다.",
        StepKey = "application.lh-world-preview",
        DependsOnStepKeys = new[] { "contract.lh-world-profile" },
        ExecutionStage = SsalddelCodeExecutionStage.Preview,
        ReadsFrom = SsalddelCodeDataScope.SimulationState | SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 31,
        Boundary = "Preview는 공간 권위·운영 원장·자원 원장을 변경하지 않는다. 시나리오 셀 내용 공급자는 실제 E5·E6 근거로 자동 승격되지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
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

        private static readonly SimulationLhSpatialKnowledgeProvider DefaultSpatialKnowledge =
            SimulationLhSpatialKnowledgeProvider.LoadEmbedded();

        private readonly ISimulationLhWindowPlanner windowPlanner;
        private readonly ISimulationLhCellContentSource cellContentSource;
        private readonly ISimulationAreaSetHandoverPlanner? handoverPlanner;

        public SimulationLhWorldService()
            : this(new SimulationLhWindowPlanner(),
                new ScenarioProceduralSimulationLhCellContentSource(), null)
        {
        }

        public SimulationLhWorldService(
            ISimulationLhWindowPlanner streamingWindowPlanner,
            ISimulationLhCellContentSource contentSource)
            : this(streamingWindowPlanner, contentSource, null)
        {
        }

        public SimulationLhWorldService(
            ISimulationLhWindowPlanner streamingWindowPlanner,
            ISimulationLhCellContentSource contentSource,
            ISimulationAreaSetHandoverPlanner? areaSetHandoverPlanner)
        {
            windowPlanner = streamingWindowPlanner
                ?? throw new ArgumentNullException(nameof(streamingWindowPlanner));
            cellContentSource = contentSource
                ?? throw new ArgumentNullException(nameof(contentSource));
            handoverPlanner = areaSetHandoverPlanner;
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
            long worldRevision,
            SimulationPlayerAreaAccessStateSnapshot? areaAccess = null)
        {
            ValidateRequest(request, worldRevision);
            var profile = CreateDefaultProfile();
            var season = CreateSeason(dayNumber);
            var window = windowPlanner.Plan(request, profile);
            var context = new SimulationLhCellContentContext
            {
                Season = season,
                RequiredCapabilityCodes = request.RequiredCapabilityCodes,
                WorldTick = worldTick,
                WorldRevision = worldRevision,
            };
            var cells = window.Cells
                .Select(value => cellContentSource.CreateCellPlan(value, context))
                .OrderBy(value => value.Priority)
                .ThenBy(value => value.CellKey, StringComparer.Ordinal)
                .ToArray();
            var handover = handoverPlanner?.Plan(
                new SimulationAreaSetHandoverPlanRequest
                {
                    RequestEpoch = request.RequestEpoch,
                    FocusL3CellKey = request.FocusL3CellKey,
                    MovementDirectionCode = request.MovementDirectionCode,
                    CurrentAreaSetStableId = areaAccess?.CurrentAreaSetStableId
                                             ?? string.Empty,
                    AreaAccess = areaAccess,
                }) ?? new SimulationAreaSetHandoverPlanResponse
                {
                    RequestEpoch = request.RequestEpoch.Trim(),
                    FocusL3CellKey = request.FocusL3CellKey,
                    MovementDirectionCode = request.MovementDirectionCode,
                    AvailabilityCode = SimulationAreaSetHandoverCodes.NotAvailable,
                    BlockingReasonCodes = new[] { "AreaSetHandoverPlannerNotConfigured" },
                    Candidates = Array.Empty<SimulationAreaSetHandoverCandidateResponse>(),
                    PlanHashSha256 = Hash("area-set-handover:not-configured|"
                                            + request.RequestEpoch.Trim()),
                };

            return new SimulationLhCellPreviewResponse
            {
                RequestEpoch = request.RequestEpoch.Trim(),
                RecipeStableId = request.RecipeStableId,
                AreaSetStableId = profile.AreaSetStableId,
                ContentSourceCode = cellContentSource.ContentSourceCode,
                WorldTick = worldTick,
                WorldRevision = worldRevision,
                Season = season,
                Profile = profile,
                AreaSetHandover = handover,
                Cells = cells,
                OutsideCoverageCellKeys = window.OutsideCoverageCellKeys,
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

        private static SimulationLhLevelResponse Level(
            string levelCode, int sizeMeters, string primaryHQueryLevelCode)
            => new()
            {
                LevelCode = levelCode,
                CellSizeMeters = sizeMeters,
                // 기존 JSON 소비자 호환용 필드다. L과 H의 동일성을 뜻하지 않는다.
                DefaultHLevelCode = primaryHQueryLevelCode,
                PrimaryHQueryLevelCode = primaryHQueryLevelCode,
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
                    + level.PrimaryHQueryLevelCode)),
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

        private static string Hash(string canonical)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var value in bytes) builder.Append(value.ToString("x2"));
            return builder.ToString();
        }
    }
}
