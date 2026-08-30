using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 이미 봉인된 통합 배치 계획을 다시 계산하지 않고 실외·실내 실행 입력으로
    /// 나눈다. LH와 Unity는 이 경계 뒤에서 배치 규칙을 다시 해석하지 않는다.
    /// </summary>
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q338 통합 배치 계획을 실외·실내 실행 입력으로 분리하는 경계를 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "인터페이스 정의는 배치 계획 생성이나 실제 World 발현 증거가 아니다.")]
    public interface ISimulation세계자산배치Plan분리Service
    {
        Simulation분리세계자산배치Result Partition(
            Simulation세계자산배치Plan compatibilityPlan);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "봉인된 통합 세계자산 배치 계획을 실외·실내 실행 계획으로 결정적으로 분리한다.",
        StepKey = "application.world-asset-plan-partition",
        DependsOnStepKeys = new[] { "application.world-asset-placement" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 31,
        Boundary = "통합 계획과 WorldRevision을 다시 계산하지 않고 동일한 계획 identity의 실행 입력만 만든다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q338 봉인 계획을 동일 identity의 실외·실내 실행 입력으로 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행,
        Boundary = "결정적 분리는 실제 Prefab 배치·Scene 저장·Play Mode 증거를 만들지 않는다.")]
    public sealed class Simulation결정적세계자산배치Plan분리Service
        : ISimulation세계자산배치Plan분리Service
    {
        public Simulation분리세계자산배치Result Partition(
            Simulation세계자산배치Plan compatibilityPlan)
        {
            Validate(compatibilityPlan);
            return new Simulation분리세계자산배치Result
            {
                CompatibilityPlan = compatibilityPlan,
                ExteriorPlan = Exterior(compatibilityPlan),
                InteriorPlan = Interior(compatibilityPlan),
            };
        }

        private static Simulation실외자산배치Plan Exterior(
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

        private static Simulation실내자산배치Plan Interior(
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

        private static void Validate(Simulation세계자산배치Plan source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(source.CellStableId)
                || source.SourceWorldRevision < 0
                || !IsSha256(source.AssetPlacementPlanHashSha256)
                || source.Placements == null
                || source.InteriorPlanHandles == null
                || source.InteriorPlanBodies == null)
                throw new ArgumentException(
                    "SimulationWorldAssetPlacementPlanPartitionInputInvalid",
                    nameof(source));
        }

        private static bool IsSha256(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
               && value.All(Uri.IsHexDigit);

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

    public sealed class Simulation분리세계자산배치Result
    {
        public Simulation세계자산배치Plan CompatibilityPlan { get; set; }
            = new Simulation세계자산배치Plan();
        public Simulation실외자산배치Plan ExteriorPlan { get; set; }
            = new Simulation실외자산배치Plan();
        public Simulation실내자산배치Plan InteriorPlan { get; set; }
            = new Simulation실내자산배치Plan();
    }
}
