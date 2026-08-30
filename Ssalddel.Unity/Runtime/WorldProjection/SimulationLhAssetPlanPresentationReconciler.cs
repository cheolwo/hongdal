using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Unity.WorldProjection
{
    public static class SimulationLhAssetPlanPresentationCommandCodes
    {
        public const string Prepare = "Prepare";
        public const string Activate = "Activate";
        public const string Cache = "Cache";
        public const string Release = "Release";
    }

    public sealed class SimulationLhAssetPlanPresentationState
    {
        public string CellStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public long LifecycleRevision { get; set; }
        public string LifecycleStateCode { get; set; } = string.Empty;
        public string SourceCombinedPlanHashSha256 { get; set; } = string.Empty;
        public string ExteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
        public string InteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
        public string LifecycleHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationLhAssetPlanPresentationCommand
    {
        public string CellStableId { get; set; } = string.Empty;
        public string CommandCode { get; set; } = string.Empty;
        public long LifecycleRevision { get; set; }
        public string ExteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
        public string InteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationLhAssetPlanPresentationChangeSet
    {
        public SimulationLhAssetPlanPresentationState[] States { get; set; }
            = Array.Empty<SimulationLhAssetPlanPresentationState>();
        public SimulationLhAssetPlanPresentationCommand[] Commands { get; set; }
            = Array.Empty<SimulationLhAssetPlanPresentationCommand>();
        public string[] UnchangedCellStableIds { get; set; }
            = Array.Empty<string>();
    }

    /// <summary>
    /// Simulation이 동결한 계획과 LH 수명주기를 Unity 표현 명령으로 변환한다.
    /// Prefab 선택과 Simulation 상태 변경은 수행하지 않는다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.ClientAdapter,
        "LH 배치 계획 수명주기 상태를 Unity Prepare·Activate·Cache·Release 표현 명령으로 변환한다.",
        StepKey = "unity.lh-asset-plan-presentation-reconcile",
        DependsOnStepKeys = new[]
        {
            "application.lh-asset-plan-window-reconcile",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Presentation,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        Effects = SsalddelCodeEffect.UiStateMutation,
        FlowOrder = 41,
        Boundary = "Prefab 선택이나 GameObject 생성 없이 표현 명령만 만들며 Simulation 권위 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q338 LH 수명주기 상태를 Unity 표현 명령으로 변환하는 Client 경계를 제공한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행,
        Boundary = "표현 명령 생성은 실제 GameObject 생성·Scene 배선·Play Mode 증거가 아니다.")]
    public sealed class SimulationLhAssetPlanPresentationReconciler
    {
        private static readonly StableIdReconciler<
            SimulationLhAssetPlanPresentationState> Reconciler =
            new StableIdReconciler<SimulationLhAssetPlanPresentationState>(
                new StableIdReconciliationPolicy<
                    SimulationLhAssetPlanPresentationState>(
                    value => value.CellStableId,
                    presentationEquivalent: (current, incoming) =>
                        string.Equals(current.LifecycleHashSha256,
                            incoming.LifecycleHashSha256,
                            StringComparison.Ordinal),
                    dataRevisionComparison: (incoming, current) =>
                        incoming.LifecycleRevision.CompareTo(
                            current.LifecycleRevision)));

        public SimulationLhAssetPlanPresentationChangeSet Reconcile(
            IEnumerable<SimulationLhAssetPlanPresentationState> current,
            IEnumerable<SimulationLhAssetPlanLifecycleSnapshot> incoming)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));
            var before = current.ToArray();
            var after = incoming.Select(ToPresentationState)
                .OrderBy(value => value.CellStableId, StringComparer.Ordinal)
                .ToArray();

            StableIdChangeSet<SimulationLhAssetPlanPresentationState> changes;
            try
            {
                changes = Reconciler.Reconcile(before, after);
            }
            catch (StableIdReconciliationException error)
            {
                throw new InvalidOperationException(
                    "SimulationLhPresentationReconcile" + error.ErrorCode
                    + ":" + error.StableId, error);
            }
            EnsurePlanIdentityStable(before, after);

            var commands = changes.Added.Concat(changes.Updated)
                .Select(ToCommand)
                .Concat(changes.Removed.Select(value =>
                    new SimulationLhAssetPlanPresentationCommand
                    {
                        CellStableId = value.CellStableId,
                        CommandCode =
                            SimulationLhAssetPlanPresentationCommandCodes.Release,
                        LifecycleRevision = value.LifecycleRevision,
                        ExteriorPlacementPlanHashSha256 =
                            value.ExteriorPlacementPlanHashSha256,
                        InteriorPlacementPlanHashSha256 =
                            value.InteriorPlacementPlanHashSha256,
                    }))
                .OrderBy(value => value.CellStableId, StringComparer.Ordinal)
                .ToArray();
            return new SimulationLhAssetPlanPresentationChangeSet
            {
                States = after,
                Commands = commands,
                UnchangedCellStableIds = changes.Unchanged
                    .Select(value => value.CellStableId)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };
        }

        private static SimulationLhAssetPlanPresentationState
            ToPresentationState(SimulationLhAssetPlanLifecycleSnapshot value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.CellStableId)
                || value.SourceWorldRevision < 0 || value.LifecycleRevision < 0
                || !value.PlacementContentFrozen || !value.PresentationOnly
                || !SimulationLhAssetPlanLifecycleCodes.IsKnown(
                    value.LifecycleStateCode)
                || !IsSha256(value.SourceCombinedPlanHashSha256)
                || !IsSha256(value.ExteriorPlacementPlanHashSha256)
                || !IsSha256(value.InteriorPlacementPlanHashSha256)
                || !IsSha256(value.LifecycleHashSha256))
                throw new ArgumentException(
                    "SimulationLhPresentationLifecycleSnapshotInvalid");
            return new SimulationLhAssetPlanPresentationState
            {
                CellStableId = value.CellStableId,
                SourceWorldRevision = value.SourceWorldRevision,
                LifecycleRevision = value.LifecycleRevision,
                LifecycleStateCode = value.LifecycleStateCode,
                SourceCombinedPlanHashSha256 =
                    value.SourceCombinedPlanHashSha256,
                ExteriorPlacementPlanHashSha256 =
                    value.ExteriorPlacementPlanHashSha256,
                InteriorPlacementPlanHashSha256 =
                    value.InteriorPlacementPlanHashSha256,
                LifecycleHashSha256 = value.LifecycleHashSha256,
            };
        }

        private static SimulationLhAssetPlanPresentationCommand ToCommand(
            SimulationLhAssetPlanPresentationState value)
            => new SimulationLhAssetPlanPresentationCommand
            {
                CellStableId = value.CellStableId,
                CommandCode = value.LifecycleStateCode switch
                {
                    SimulationLhAssetPlanLifecycleCodes.Prepared =>
                        SimulationLhAssetPlanPresentationCommandCodes.Prepare,
                    SimulationLhAssetPlanLifecycleCodes.Active =>
                        SimulationLhAssetPlanPresentationCommandCodes.Activate,
                    SimulationLhAssetPlanLifecycleCodes.Cached =>
                        SimulationLhAssetPlanPresentationCommandCodes.Cache,
                    SimulationLhAssetPlanLifecycleCodes.Released =>
                        SimulationLhAssetPlanPresentationCommandCodes.Release,
                    _ => throw new InvalidOperationException(
                        "SimulationLhPresentationLifecycleStateInvalid"),
                },
                LifecycleRevision = value.LifecycleRevision,
                ExteriorPlacementPlanHashSha256 =
                    value.ExteriorPlacementPlanHashSha256,
                InteriorPlacementPlanHashSha256 =
                    value.InteriorPlacementPlanHashSha256,
            };

        private static void EnsurePlanIdentityStable(
            IEnumerable<SimulationLhAssetPlanPresentationState> current,
            IEnumerable<SimulationLhAssetPlanPresentationState> incoming)
        {
            var before = current.ToDictionary(value => value.CellStableId,
                StringComparer.Ordinal);
            foreach (var value in incoming)
            {
                if (!before.TryGetValue(value.CellStableId, out var existing))
                    continue;
                if (existing.SourceWorldRevision != value.SourceWorldRevision
                    || existing.SourceCombinedPlanHashSha256 !=
                        value.SourceCombinedPlanHashSha256
                    || existing.ExteriorPlacementPlanHashSha256 !=
                        value.ExteriorPlacementPlanHashSha256
                    || existing.InteriorPlacementPlanHashSha256 !=
                        value.InteriorPlacementPlanHashSha256)
                    throw new InvalidOperationException(
                        "SimulationLhPresentationPlanIdentityChanged:"
                        + value.CellStableId);
            }
        }

        private static bool IsSha256(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
               && value.All(Uri.IsHexDigit);
    }
}
