using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// LH가 동결된 배치 계획에 적용할 수 있는 표현 수명주기다.
    /// 이 값은 Simulation 세계 상태나 WorldRevision이 아니다.
    /// </summary>
    public static class SimulationLhAssetPlanLifecycleCodes
    {
        public const string Prepared = "Prepared";
        public const string Active = "Active";
        public const string Cached = "Cached";
        public const string Released = "Released";

        public static bool IsKnown(string value)
            => value == Prepared || value == Active
                || value == Cached || value == Released;

        public static bool CanTransition(string current, string target)
            => current == target
                || (current == Prepared
                    && (target == Active || target == Cached
                        || target == Released))
                || (current == Active
                    && (target == Cached || target == Released))
                || (current == Cached
                    && (target == Active || target == Released))
                || (current == Released && target == Prepared);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Contract,
        "동결된 실외·실내 배치 계획의 LH 표현 수명주기 상태를 전달한다.",
        StepKey = "contract.lh-asset-plan-lifecycle",
        DependsOnStepKeys = new[] { "contract.lh-world-profile" },
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 12,
        Boundary = "수명주기 Revision은 표현 준비 상태이며 Simulation WorldRevision이나 배치 내용을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q338 동결 배치 계획의 LH 표현 수명주기 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "계약 존재는 실제 셀 활성화·Scene 배선·플레이 경험 증거가 아니다.")]
    public sealed class SimulationLhAssetPlanLifecycleSnapshot
    {
        public const string SchemaVersion =
            "simulation-lh-asset-plan-lifecycle.v1";

        public string CellStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string SourceCombinedPlanHashSha256 { get; set; } = string.Empty;
        public string ExteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
        public string InteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
        public string LifecycleStateCode { get; set; } =
            SimulationLhAssetPlanLifecycleCodes.Prepared;
        public long LifecycleRevision { get; set; }
        public bool PlacementContentFrozen { get; set; } = true;
        public bool PresentationOnly { get; set; } = true;
        public string LifecycleHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationLhAssetPlanLifecycleTransitionRequest
    {
        public long ExpectedLifecycleRevision { get; set; }
        public string TargetStateCode { get; set; } = string.Empty;
    }
}
