using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// LH가 이미 생성된 실내 계획을 언제 준비할지만 결정하도록 연결한다.
    /// 이 서비스는 실내를 생성하거나 재배치하지 않는다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Application,
        "LH 셀 상세도와 이미 고정된 실내 계획 handle을 연결한다.",
        StepKey = "application.lh-interior-plan-handle",
        DependsOnStepKeys = new[] { "application.lh-world-preview" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 33,
        Boundary = "LH는 실내를 생성하거나 재배치하지 않고 준비 상세도만 선택한다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "생성된 실내 계획과 LH 공간 실행 사이의 Application 연결 경계를 제공한다.",
        WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
        Boundary = "실행 경계 존재는 실제 Town 공간 결속이나 E5·E7 증거를 뜻하지 않는다.")]
    public sealed class SimulationLhInteriorPlanHandleService
    {
        public InteriorLhCellActivationPlan[] Bind(
            SimulationLhCellPreviewResponse preview,
            PresentationWorldDefinition worldDefinition)
        {
            if (preview is null) throw new ArgumentNullException(nameof(preview));
            if (worldDefinition is null) throw new ArgumentNullException(nameof(worldDefinition));

            return preview.Cells
                .OrderBy(value => value.Priority)
                .ThenBy(value => value.CellKey, StringComparer.Ordinal)
                .Select(cell => new InteriorLhCellActivationPlan
                {
                    CellKey = cell.CellKey,
                    FocusLevelCode = FocusLevel(cell.WindowRoleCode),
                    PlanHandles = worldDefinition.InteriorPlanHandles
                        .Where(handle => cell.Placements.Any(placement =>
                            string.Equals(
                                placement.GeneratedStableId,
                                handle.BuildingPlacementStableId,
                                StringComparison.Ordinal)
                            || (!string.IsNullOrWhiteSpace(handle.H1StableId)
                                && string.Equals(
                                    placement.H1StableId,
                                    handle.H1StableId,
                                    StringComparison.Ordinal))))
                        .OrderBy(handle => handle.BuildingPlacementStableId, StringComparer.Ordinal)
                        .ToArray(),
                    LhDeterminesPlacement = false,
                    PresentationOnly = true,
                })
                .ToArray();
        }

        private static string FocusLevel(string windowRoleCode)
            => windowRoleCode switch
            {
                SimulationLhWorldCodes.Detail => InteriorLayoutCodes.ObjectFocus,
                SimulationLhWorldCodes.Active => InteriorLayoutCodes.ZoneFocus,
                _ => InteriorLayoutCodes.OverviewFocus,
            };
    }
}
