using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// LH Window 역할을 동결 배치 계획의 표현 수명주기로 변환한다.
    /// 계획 생성·재배치·Simulation WorldRevision 변경은 수행하지 않는다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Application,
        "LH Window 역할과 캐시 용량을 동결 배치 계획의 표현 수명주기로 조정한다.",
        StepKey = "application.lh-asset-plan-window-reconcile",
        DependsOnStepKeys = new[]
        {
            "application.lh-world-preview",
            "application.lh-asset-plan-lifecycle",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        Effects = SsalddelCodeEffect.UiStateMutation,
        FlowOrder = 35,
        Boundary = "셀 활성 우선순위와 수명주기만 결정하며 계획 생성·재배치·Simulation 상태 변경은 수행하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q338 LH Window와 캐시 용량을 배치 계획 수명주기에 결속한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2공간실행,
        Boundary = "Headless 조정 결과는 실제 Scene 활성화·카메라 이동·Game View 증거가 아니다.")]
    public sealed class SimulationLhAssetPlanWindowReconciler
    {
        private readonly SimulationLhAssetPlanLifecycleService lifecycle;

        public SimulationLhAssetPlanWindowReconciler()
            : this(new SimulationLhAssetPlanLifecycleService())
        {
        }

        public SimulationLhAssetPlanWindowReconciler(
            SimulationLhAssetPlanLifecycleService lifecycleService)
            => lifecycle = lifecycleService
                ?? throw new ArgumentNullException(nameof(lifecycleService));

        public SimulationLhAssetPlanWindowReconcileResult Reconcile(
            IEnumerable<SimulationLhAssetPlanLifecycleSnapshot> current,
            IEnumerable<SimulationLhAssetPlanLifecycleSnapshot> available,
            SimulationLhCellPreviewResponse preview,
            int cachedCellCapacity)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (available == null) throw new ArgumentNullException(nameof(available));
            if (preview == null) throw new ArgumentNullException(nameof(preview));
            if (cachedCellCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(cachedCellCapacity));

            var currentByCell = Index(current, "Current");
            var availableByCell = Index(available, "Available");
            var desiredRoles = IndexRoles(preview.Cells);
            var results = new Dictionary<string,
                SimulationLhAssetPlanLifecycleSnapshot>(StringComparer.Ordinal);

            foreach (var role in desiredRoles.OrderBy(value => value.Key,
                         StringComparer.Ordinal))
            {
                var source = Resolve(role.Key, currentByCell, availableByCell);
                if (currentByCell.TryGetValue(role.Key, out var existing)
                    && availableByCell.TryGetValue(role.Key, out var incoming))
                    EnsureSamePlan(existing, incoming);
                results.Add(role.Key, ApplyWindowRole(source, role.Value));
            }

            foreach (var pair in currentByCell
                         .Where(value => !desiredRoles.ContainsKey(value.Key))
                         .OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                var value = pair.Value;
                if (value.LifecycleStateCode ==
                    SimulationLhAssetPlanLifecycleCodes.Active
                    || value.LifecycleStateCode ==
                    SimulationLhAssetPlanLifecycleCodes.Prepared)
                    value = Move(value,
                        SimulationLhAssetPlanLifecycleCodes.Cached);
                results.Add(pair.Key, value);
            }

            var cached = results.Values
                .Where(value => value.LifecycleStateCode ==
                                SimulationLhAssetPlanLifecycleCodes.Cached)
                .OrderByDescending(value => desiredRoles.ContainsKey(
                    value.CellStableId))
                .ThenBy(value => value.CellStableId, StringComparer.Ordinal)
                .ToArray();
            foreach (var value in cached.Skip(cachedCellCapacity))
                results[value.CellStableId] = Move(value,
                    SimulationLhAssetPlanLifecycleCodes.Released);

            var ordered = results.Values.OrderBy(value => value.CellStableId,
                StringComparer.Ordinal).ToArray();
            var response = new SimulationLhAssetPlanWindowReconcileResult
            {
                RequestEpoch = preview.RequestEpoch ?? string.Empty,
                SourceWorldRevision = preview.WorldRevision,
                Cells = ordered,
            };
            response.ReconcileHashSha256 =
                Simulation세계자산CanonicalHash.Hash(string.Join("|", new[]
                {
                    response.RequestEpoch,
                    response.SourceWorldRevision.ToString(
                        CultureInfo.InvariantCulture),
                    cachedCellCapacity.ToString(CultureInfo.InvariantCulture),
                    string.Join(",", ordered.Select(value =>
                        value.CellStableId + ":" + value.LifecycleHashSha256)),
                }));
            return response;
        }

        private SimulationLhAssetPlanLifecycleSnapshot ApplyWindowRole(
            SimulationLhAssetPlanLifecycleSnapshot source,
            string windowRoleCode)
        {
            if (windowRoleCode == SimulationLhWorldCodes.Detail
                || windowRoleCode == SimulationLhWorldCodes.Active)
            {
                if (source.LifecycleStateCode ==
                    SimulationLhAssetPlanLifecycleCodes.Released)
                    source = Move(source,
                        SimulationLhAssetPlanLifecycleCodes.Prepared);
                return Move(source,
                    SimulationLhAssetPlanLifecycleCodes.Active);
            }

            if (windowRoleCode != SimulationLhWorldCodes.Prefetch)
                throw new ArgumentException(
                    "SimulationLhWindowRoleInvalid");
            if (source.LifecycleStateCode ==
                SimulationLhAssetPlanLifecycleCodes.Active)
                return Move(source,
                    SimulationLhAssetPlanLifecycleCodes.Cached);
            if (source.LifecycleStateCode ==
                SimulationLhAssetPlanLifecycleCodes.Released)
                return Move(source,
                    SimulationLhAssetPlanLifecycleCodes.Prepared);
            return Move(source, source.LifecycleStateCode);
        }

        private SimulationLhAssetPlanLifecycleSnapshot Move(
            SimulationLhAssetPlanLifecycleSnapshot current, string target)
            => lifecycle.Transition(current,
                new SimulationLhAssetPlanLifecycleTransitionRequest
                {
                    ExpectedLifecycleRevision = current.LifecycleRevision,
                    TargetStateCode = target,
                });

        private static SimulationLhAssetPlanLifecycleSnapshot Resolve(
            string cellStableId,
            IReadOnlyDictionary<string,
                SimulationLhAssetPlanLifecycleSnapshot> current,
            IReadOnlyDictionary<string,
                SimulationLhAssetPlanLifecycleSnapshot> available)
        {
            if (current.TryGetValue(cellStableId, out var existing))
                return existing;
            if (available.TryGetValue(cellStableId, out var incoming))
                return incoming;
            throw new InvalidOperationException(
                "SimulationLhAssetPlanUnavailable:" + cellStableId);
        }

        private static Dictionary<string,
            SimulationLhAssetPlanLifecycleSnapshot> Index(
            IEnumerable<SimulationLhAssetPlanLifecycleSnapshot> values,
            string role)
        {
            var result = new Dictionary<string,
                SimulationLhAssetPlanLifecycleSnapshot>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.CellStableId)
                    || !result.TryAdd(value.CellStableId, value))
                    throw new ArgumentException(
                        "SimulationLhAssetPlan" + role + "Invalid");
            }
            return result;
        }

        private static Dictionary<string, string> IndexRoles(
            IEnumerable<SimulationLhCellPlanResponse> cells)
        {
            if (cells == null)
                throw new ArgumentException("SimulationLhPreviewCellsMissing");
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var cell in cells)
            {
                if (cell == null || string.IsNullOrWhiteSpace(cell.CellKey)
                    || !result.TryAdd(cell.CellKey, cell.WindowRoleCode))
                    throw new ArgumentException(
                        "SimulationLhPreviewCellInvalid");
            }
            return result;
        }

        private static void EnsureSamePlan(
            SimulationLhAssetPlanLifecycleSnapshot current,
            SimulationLhAssetPlanLifecycleSnapshot incoming)
        {
            if (current.SourceWorldRevision != incoming.SourceWorldRevision
                || current.SourceCombinedPlanHashSha256 !=
                    incoming.SourceCombinedPlanHashSha256
                || current.ExteriorPlacementPlanHashSha256 !=
                    incoming.ExteriorPlacementPlanHashSha256
                || current.InteriorPlacementPlanHashSha256 !=
                    incoming.InteriorPlacementPlanHashSha256)
                throw new InvalidOperationException(
                    "SimulationLhAssetPlanIdentityChanged");
        }
    }

    public sealed class SimulationLhAssetPlanWindowReconcileResult
    {
        public string RequestEpoch { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public SimulationLhAssetPlanLifecycleSnapshot[] Cells { get; set; }
            = Array.Empty<SimulationLhAssetPlanLifecycleSnapshot>();
        public string ReconcileHashSha256 { get; set; } = string.Empty;
    }
}
