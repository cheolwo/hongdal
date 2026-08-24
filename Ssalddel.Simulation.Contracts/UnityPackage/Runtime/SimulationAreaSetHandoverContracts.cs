using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationAreaSetHandoverCodes
    {
        public const string SchemaVersion = "area-set-handover-plan.v1";
        public const string PlannerRevision = "area-set-handover-planner.r1";

        public const string Known = "Known";
        public const string H3PrepareRequested = "H3PrepareRequested";
        public const string H2PrefetchRequested = "H2PrefetchRequested";
        public const string H1TraversalPreparationRequested =
            "H1TraversalPreparationRequested";

        public const string H5Known = "H5Known";
        public const string NotAvailable = "NotAvailable";
        public const string NotResident = "NotResident";
        public const string AccessUnknown = "AccessUnknown";
        public const string PreviewConfirmWorldTick = "PreviewConfirmWorldTick";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Contract,
        "현재 AreaSet에서 물리 회랑으로 이어지는 다음 AreaSet의 단계별 준비 후보를 전달한다.",
        StepKey = "contract.area-set-handover-plan",
        DependsOnStepKeys = new[] { "contract.world-layout-definition", "contract.lh-world-profile" },
        ExecutionStage = SsalddelCodeExecutionStage.Preview,
        FlowOrder = 29,
        Boundary = "인계 계획은 자료·메모리 준비 후보이며 현재 AreaSet, 이동 권한, WorldTick 또는 운영 상태를 변경하지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약 정의는 실행 효과나 E 단계 달성 증거를 소유하지 않는다.",
        SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E1공간계약)]
    public sealed class SimulationAreaSetHandoverPlanResponse
    {
        public string SchemaVersion { get; set; } =
            SimulationAreaSetHandoverCodes.SchemaVersion;
        public string PlannerRevision { get; set; } =
            SimulationAreaSetHandoverCodes.PlannerRevision;
        public string RequestEpoch { get; set; } = string.Empty;
        public string CurrentAreaSetStableId { get; set; } = string.Empty;
        public string FocusL3CellKey { get; set; } = string.Empty;
        public string MovementDirectionCode { get; set; } = SimulationLhWorldCodes.None;
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
        public string WorldGroundingStateCode { get; set; } = string.Empty;
        public string AvailabilityCode { get; set; } =
            SimulationAreaSetHandoverCodes.NotAvailable;
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public SimulationAreaSetHandoverCandidateResponse[] Candidates { get; set; }
            = Array.Empty<SimulationAreaSetHandoverCandidateResponse>();
        public string PlanHashSha256 { get; set; } = string.Empty;
        public bool ChangesCurrentAreaSet { get; set; }
        public bool RequiresExplicitTraversalConfirm { get; set; } = true;
        public bool IsCandidateOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationAreaSetHandoverCandidateResponse
    {
        public string TargetAreaSetStableId { get; set; } = string.Empty;
        public string RelationStableId { get; set; } = string.Empty;
        public string CorridorInstanceStableId { get; set; } = string.Empty;
        public string CorridorLandscapeGraphStableId { get; set; } = string.Empty;
        public string OverlapPolicyCode { get; set; } = string.Empty;
        public string SpatialRealizationCode { get; set; } = string.Empty;
        public double DistanceToTransitionMeters { get; set; }
        public double HeadingAlignment01 { get; set; }
        public int Priority { get; set; }
        public string PreparationTargetCode { get; set; } =
            SimulationAreaSetHandoverCodes.Known;
        public string SemanticDepthCode { get; set; } = "H4";
        public string ArtifactAvailabilityCode { get; set; } =
            SimulationAreaSetHandoverCodes.H5Known;
        public string ResidencyStateCode { get; set; } =
            SimulationAreaSetHandoverCodes.NotResident;
        public string SimulationAccessStateCode { get; set; } =
            SimulationAreaSetHandoverCodes.AccessUnknown;
        public string ActivationAuthorityCode { get; set; } =
            SimulationAreaSetHandoverCodes.PreviewConfirmWorldTick;
        public string[] RequiredCapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool RequiresActualE5Package { get; set; } = true;
        public bool RequiresE6Grounding { get; set; }
        public bool CanRequestTraversal { get; set; }
        public bool CanActivate { get; set; }
        public string CandidateHashSha256 { get; set; } = string.Empty;
    }
}
