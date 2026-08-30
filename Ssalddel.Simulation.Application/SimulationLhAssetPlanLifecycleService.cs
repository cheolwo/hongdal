using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// LH가 이미 동결된 실외·실내 배치 계획의 준비·활성·캐시·해제만
    /// 전이하게 한다. 계획 내용과 Simulation WorldRevision은 변경하지 않는다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Application,
        "동결 배치 계획의 LH 준비·활성·캐시·해제 상태만 전이한다.",
        StepKey = "application.lh-asset-plan-lifecycle",
        DependsOnStepKeys = new[]
        {
            "contract.lh-asset-plan-lifecycle",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        Effects = SsalddelCodeEffect.UiStateMutation,
        FlowOrder = 34,
        Boundary = "표현 수명주기만 변경하며 배치 계획과 Simulation WorldRevision은 고정한다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q338 동결 배치 계획의 Prepare·Activate·Cache·Release 실행 경계를 제공한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행,
        Boundary = "수명주기 상태 전이는 실제 Unity GameObject 수명주기나 E5 이상 증거가 아니다.")]
    public sealed class SimulationLhAssetPlanLifecycleService
    {
        public SimulationLhAssetPlanLifecycleSnapshot Prepare(
            Simulation분리세계자산배치Result plans)
        {
            if (plans == null) throw new ArgumentNullException(nameof(plans));
            var combined = plans.CompatibilityPlan
                ?? throw new ArgumentException("SimulationLhCombinedPlanMissing");
            var exterior = plans.ExteriorPlan
                ?? throw new ArgumentException("SimulationLhExteriorPlanMissing");
            var interior = plans.InteriorPlan
                ?? throw new ArgumentException("SimulationLhInteriorPlanMissing");
            ValidatePlanSet(combined, exterior, interior);

            return Seal(new SimulationLhAssetPlanLifecycleSnapshot
            {
                CellStableId = combined.CellStableId,
                SourceWorldRevision = combined.SourceWorldRevision,
                SourceCombinedPlanHashSha256 =
                    combined.AssetPlacementPlanHashSha256,
                ExteriorPlacementPlanHashSha256 =
                    exterior.ExteriorPlacementPlanHashSha256,
                InteriorPlacementPlanHashSha256 =
                    interior.InteriorPlacementPlanHashSha256,
                LifecycleStateCode =
                    SimulationLhAssetPlanLifecycleCodes.Prepared,
                LifecycleRevision = 0,
                PlacementContentFrozen = true,
                PresentationOnly = true,
            });
        }

        public SimulationLhAssetPlanLifecycleSnapshot Transition(
            SimulationLhAssetPlanLifecycleSnapshot current,
            SimulationLhAssetPlanLifecycleTransitionRequest request)
        {
            ValidateSnapshot(current);
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedLifecycleRevision != current.LifecycleRevision)
                throw new InvalidOperationException(
                    "SimulationLhLifecycleRevisionMismatch");
            var target = (request.TargetStateCode ?? string.Empty).Trim();
            if (!SimulationLhAssetPlanLifecycleCodes.IsKnown(target)
                || !SimulationLhAssetPlanLifecycleCodes.CanTransition(
                    current.LifecycleStateCode, target))
                throw new InvalidOperationException(
                    "SimulationLhLifecycleTransitionInvalid");
            if (target == current.LifecycleStateCode)
                return Clone(current);

            return Seal(new SimulationLhAssetPlanLifecycleSnapshot
            {
                CellStableId = current.CellStableId,
                SourceWorldRevision = current.SourceWorldRevision,
                SourceCombinedPlanHashSha256 =
                    current.SourceCombinedPlanHashSha256,
                ExteriorPlacementPlanHashSha256 =
                    current.ExteriorPlacementPlanHashSha256,
                InteriorPlacementPlanHashSha256 =
                    current.InteriorPlacementPlanHashSha256,
                LifecycleStateCode = target,
                LifecycleRevision = current.LifecycleRevision + 1,
                PlacementContentFrozen = true,
                PresentationOnly = true,
            });
        }

        private static void ValidatePlanSet(
            Simulation세계자산배치Plan combined,
            Simulation실외자산배치Plan exterior,
            Simulation실내자산배치Plan interior)
        {
            if (string.IsNullOrWhiteSpace(combined.CellStableId)
                || combined.SourceWorldRevision < 0
                || !IsSha256(combined.AssetPlacementPlanHashSha256)
                || exterior.CellStableId != combined.CellStableId
                || interior.CellStableId != combined.CellStableId
                || exterior.SourceWorldRevision != combined.SourceWorldRevision
                || interior.SourceWorldRevision != combined.SourceWorldRevision
                || exterior.SourceCombinedPlanHashSha256 !=
                    combined.AssetPlacementPlanHashSha256
                || interior.SourceCombinedPlanHashSha256 !=
                    combined.AssetPlacementPlanHashSha256
                || !IsSha256(exterior.ExteriorPlacementPlanHashSha256)
                || !IsSha256(interior.InteriorPlacementPlanHashSha256))
                throw new ArgumentException(
                    "SimulationLhAssetPlanSetInvalid");
        }

        private static void ValidateSnapshot(
            SimulationLhAssetPlanLifecycleSnapshot value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var expected = Seal(Clone(value)).LifecycleHashSha256;
            if (string.IsNullOrWhiteSpace(value.CellStableId)
                || value.SourceWorldRevision < 0
                || value.LifecycleRevision < 0
                || !value.PlacementContentFrozen
                || !value.PresentationOnly
                || !SimulationLhAssetPlanLifecycleCodes.IsKnown(
                    value.LifecycleStateCode)
                || !IsSha256(value.SourceCombinedPlanHashSha256)
                || !IsSha256(value.ExteriorPlacementPlanHashSha256)
                || !IsSha256(value.InteriorPlacementPlanHashSha256)
                || !string.Equals(value.LifecycleHashSha256, expected,
                    StringComparison.Ordinal))
                throw new ArgumentException(
                    "SimulationLhAssetPlanLifecycleSnapshotInvalid",
                    nameof(value));
        }

        private static SimulationLhAssetPlanLifecycleSnapshot Seal(
            SimulationLhAssetPlanLifecycleSnapshot value)
        {
            value.LifecycleHashSha256 = Simulation세계자산CanonicalHash.Hash(
                string.Join("|", new[]
                {
                    SimulationLhAssetPlanLifecycleSnapshot.SchemaVersion,
                    value.CellStableId,
                    value.SourceWorldRevision.ToString(
                        CultureInfo.InvariantCulture),
                    value.SourceCombinedPlanHashSha256,
                    value.ExteriorPlacementPlanHashSha256,
                    value.InteriorPlacementPlanHashSha256,
                    value.LifecycleStateCode,
                    value.LifecycleRevision.ToString(
                        CultureInfo.InvariantCulture),
                    value.PlacementContentFrozen.ToString(),
                    value.PresentationOnly.ToString(),
                }));
            return value;
        }

        private static SimulationLhAssetPlanLifecycleSnapshot Clone(
            SimulationLhAssetPlanLifecycleSnapshot value)
            => new SimulationLhAssetPlanLifecycleSnapshot
            {
                CellStableId = value.CellStableId,
                SourceWorldRevision = value.SourceWorldRevision,
                SourceCombinedPlanHashSha256 =
                    value.SourceCombinedPlanHashSha256,
                ExteriorPlacementPlanHashSha256 =
                    value.ExteriorPlacementPlanHashSha256,
                InteriorPlacementPlanHashSha256 =
                    value.InteriorPlacementPlanHashSha256,
                LifecycleStateCode = value.LifecycleStateCode,
                LifecycleRevision = value.LifecycleRevision,
                PlacementContentFrozen = value.PlacementContentFrozen,
                PresentationOnly = value.PresentationOnly,
                LifecycleHashSha256 = value.LifecycleHashSha256,
            };

        private static bool IsSha256(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
               && value.All(Uri.IsHexDigit);
    }
}
