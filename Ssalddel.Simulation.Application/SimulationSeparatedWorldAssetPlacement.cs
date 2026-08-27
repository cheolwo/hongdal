using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// v26 통합 계획을 다시 계산하지 않고 실외와 실내의 실행 입력으로 분리한다.
    /// 통합 계획은 Save/Replay 호환 Facade로 계속 보존한다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "통합 세계자산 계획을 실외 배치와 실내 배치 계획으로 분리한다.",
        StepKey = "application.separated-world-asset-placement",
        DependsOnStepKeys = new[] { "application.world-asset-placement" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 30,
        Boundary = "기존 v26 계획과 hash를 변경하지 않고 파생 실행 계획만 만든다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "통합 세계자산 계획을 실외·실내 실행 계획과 실외 환경 표현 입력으로 분리한다.",
        Boundary = "계획 분리는 실제 Unity 공간 활성화나 E7 플레이 증거를 만들지 않는다.",
        WorkOrderIds = new[] { "E9-WO-WORLD-MAP-ASSET-PLACEMENT" })]
    public sealed class Simulation분리세계자산배치Coordinator
    {
        private readonly ISimulation세계자산배치Engine compatibilityEngine;

        public Simulation분리세계자산배치Coordinator()
            : this(new Simulation결정적세계자산배치Engine())
        {
        }

        public Simulation분리세계자산배치Coordinator(
            ISimulation세계자산배치Engine worldAssetPlacementEngine)
            => compatibilityEngine = worldAssetPlacementEngine
                ?? throw new ArgumentNullException(nameof(worldAssetPlacementEngine));

        public Simulation분리세계자산배치Result Compose(
            Simulation세계자산배치Request request)
        {
            var compatibilityPlan = compatibilityEngine.Compose(request);
            return new Simulation분리세계자산배치Result
            {
                CompatibilityPlan = compatibilityPlan,
                ExteriorPlan = Simulation세계자산배치PlanPartitioner
                    .Exterior(compatibilityPlan),
                InteriorPlan = Simulation세계자산배치PlanPartitioner
                    .Interior(compatibilityPlan),
            };
        }
    }

    public sealed class Simulation분리세계자산배치Result
    {
        public Simulation세계자산배치Plan CompatibilityPlan { get; set; }
            = new Simulation세계자산배치Plan();
        public Simulation실외자산배치Plan ExteriorPlan { get; set; }
            = new Simulation실외자산배치Plan();
        public Simulation실내자산배치Plan InteriorPlan { get; set; }
            = new Simulation실내자산배치Plan();
    }

    public sealed class Simulation결정적실외자산배치Engine
        : ISimulation실외자산배치Engine
    {
        private readonly Simulation분리세계자산배치Coordinator coordinator;

        public Simulation결정적실외자산배치Engine()
            : this(new Simulation결정적세계자산배치Engine())
        {
        }

        public Simulation결정적실외자산배치Engine(
            ISimulation세계자산배치Engine compatibilityEngine)
            => coordinator = new Simulation분리세계자산배치Coordinator(
                compatibilityEngine);

        public Simulation실외자산배치Plan ComposeExterior(
            Simulation세계자산배치Request request)
            => coordinator.Compose(request).ExteriorPlan;
    }

    public sealed class Simulation결정적실내자산배치Engine
        : ISimulation실내자산배치Engine
    {
        private readonly Simulation분리세계자산배치Coordinator coordinator;

        public Simulation결정적실내자산배치Engine()
            : this(new Simulation결정적세계자산배치Engine())
        {
        }

        public Simulation결정적실내자산배치Engine(
            ISimulation세계자산배치Engine compatibilityEngine)
            => coordinator = new Simulation분리세계자산배치Coordinator(
                compatibilityEngine);

        public Simulation실내자산배치Plan ComposeInterior(
            Simulation세계자산배치Request request)
            => coordinator.Compose(request).InteriorPlan;
    }

    public sealed class Simulation결정적실외환경표현Engine
        : ISimulation실외환경표현Engine
    {
        public Simulation실외환경표현Plan ComposePresentation(
            Simulation실외환경표현Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var exterior = request.ExteriorPlacementPlan
                ?? throw new ArgumentException("SimulationExteriorPlacementPlanMissing");
            var atmosphere = request.Atmosphere
                ?? throw new ArgumentException("SimulationAtmosphereSnapshotMissing");
            ValidateHash(exterior.ExteriorPlacementPlanHashSha256,
                "SimulationExteriorPlacementPlanHashInvalid");
            if (!string.IsNullOrWhiteSpace(request.SurfaceStateHashSha256))
                ValidateHash(request.SurfaceStateHashSha256,
                    "SimulationSurfaceStateHashInvalid");

            var weather = atmosphere.IsEnabled
                ? Normalize(atmosphere.WeatherCode, "Clear") : "Clear";
            var surfaceAppearance = SurfaceAppearance(weather,
                atmosphere.PrecipitationPermille);
            var windResponse = atmosphere.WindIntensityPermille >= 700
                ? "StrongWind" : atmosphere.WindIntensityPermille >= 300
                    ? "Wind" : "Still";
            var placements = exterior.Placements
                .OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
                .Select(value => new Simulation실외PlacementPresentationSnapshot
                {
                    PlacementStableId = value.PlacementStableId,
                    VisualVariantCode = "Weather." + weather,
                    SurfaceAppearanceCode = surfaceAppearance,
                    WindResponseCode = windResponse,
                    Visible = true,
                }).ToArray();
            var result = new Simulation실외환경표현Plan
            {
                CellStableId = exterior.CellStableId,
                SourceExteriorPlacementPlanHashSha256 =
                    exterior.ExteriorPlacementPlanHashSha256,
                AtmosphereRuleRevision = atmosphere.RuleRevision ?? string.Empty,
                WeatherCode = weather,
                SurfaceModeCode = request.SurfaceModeCode ?? string.Empty,
                SurfaceStateHashSha256 = request.SurfaceStateHashSha256
                    ?? string.Empty,
                Placements = placements,
            };
            result.PresentationPlanHashSha256 = Simulation세계자산CanonicalHash.Hash(
                string.Join("|", new[]
                {
                    result.CellStableId,
                    result.SourceExteriorPlacementPlanHashSha256,
                    result.AtmosphereRuleRevision,
                    result.WeatherCode,
                    atmosphere.TransitionProgressPermille.ToString(),
                    atmosphere.PrecipitationPermille.ToString(),
                    atmosphere.WindIntensityPermille.ToString(),
                    result.SurfaceModeCode,
                    result.SurfaceStateHashSha256,
                    string.Join(",", placements.Select(value =>
                        value.PlacementStableId + ":" + value.VisualVariantCode
                        + ":" + value.SurfaceAppearanceCode + ":"
                        + value.WindResponseCode)),
                }));
            return result;
        }

        private static string SurfaceAppearance(string weather,
            int precipitationPermille)
        {
            if (weather.IndexOf("Snow", StringComparison.OrdinalIgnoreCase) >= 0)
                return "SnowCovered";
            return precipitationPermille > 0 ? "Wet" : "Dry";
        }

        private static string Normalize(string value, string fallback)
            => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

        private static void ValidateHash(string value, string code)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 64)
                throw new ArgumentException(code);
        }
    }

    internal static class Simulation세계자산배치PlanPartitioner
    {
        public static Simulation실외자산배치Plan Exterior(
            Simulation세계자산배치Plan source)
        {
            var placements = source.Placements
                .Where(value => value.PlacementKindCode !=
                                Simulation세계자산배치Codes.InteriorOverlay)
                .OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
                .ToArray();
            var result = new Simulation실외자산배치Plan
            {
                CellStableId = source.CellStableId,
                SourceWorldRevision = source.SourceWorldRevision,
                SourceCombinedPlanHashSha256 =
                    source.AssetPlacementPlanHashSha256,
                Placements = placements,
            };
            result.ExteriorPlacementPlanHashSha256 =
                Simulation세계자산CanonicalHash.Hash(string.Join("|", new[]
                {
                    result.CellStableId,
                    result.SourceWorldRevision.ToString(CultureInfo.InvariantCulture),
                    result.SourceCombinedPlanHashSha256,
                    string.Join(",", placements.Select(PlacementCanonical)),
                }));
            return result;
        }

        public static Simulation실내자산배치Plan Interior(
            Simulation세계자산배치Plan source)
        {
            var overlays = source.Placements
                .Where(value => value.PlacementKindCode ==
                                Simulation세계자산배치Codes.InteriorOverlay)
                .OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
                .ToArray();
            var result = new Simulation실내자산배치Plan
            {
                CellStableId = source.CellStableId,
                SourceWorldRevision = source.SourceWorldRevision,
                SourceCombinedPlanHashSha256 =
                    source.AssetPlacementPlanHashSha256,
                InteriorPlanHandles = source.InteriorPlanHandles
                    .OrderBy(value => value.BuildingPlacementStableId,
                        StringComparer.Ordinal).ToArray(),
                InteriorPlanBodies = source.InteriorPlanBodies
                    .OrderBy(value => value.BuildingPlacementStableId,
                        StringComparer.Ordinal).ToArray(),
                OverlayPlacements = overlays,
            };
            result.InteriorPlacementPlanHashSha256 =
                Simulation세계자산CanonicalHash.Hash(string.Join("|", new[]
                {
                    result.CellStableId,
                    result.SourceWorldRevision.ToString(CultureInfo.InvariantCulture),
                    result.SourceCombinedPlanHashSha256,
                    string.Join(",", result.InteriorPlanHandles.Select(value =>
                        value.BuildingPlacementStableId + ":"
                        + value.InteriorPlacementPlanHashSha256)),
                    string.Join(",", result.InteriorPlanBodies.Select(value =>
                        value.BuildingPlacementStableId + ":" + value.BodyHashSha256)),
                    string.Join(",", overlays.Select(PlacementCanonical)),
                }));
            return result;
        }

        private static string PlacementCanonical(
            Simulation세계자산PlacementSnapshot value)
            => string.Join(":", new[]
            {
                value.PlacementStableId,
                value.ParentPlacementStableId,
                value.OwnerCellStableId,
                value.PlacementKindCode,
                value.CompositionKey,
                value.StateCode,
                value.LocalXMeters.ToString("R", CultureInfo.InvariantCulture),
                value.LocalYMeters.ToString("R", CultureInfo.InvariantCulture),
                value.LocalZMeters.ToString("R", CultureInfo.InvariantCulture),
                value.RotationDegrees.ToString("R", CultureInfo.InvariantCulture),
                value.UniformScale.ToString("R", CultureInfo.InvariantCulture),
            });
    }
}
