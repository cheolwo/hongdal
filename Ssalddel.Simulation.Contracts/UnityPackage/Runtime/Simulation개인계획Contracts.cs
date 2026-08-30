using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation개인계획Codes
    {
        public const string WorldInteractionId = "WI-ACTOR-PLAN-SET";
        public const string RuleRevision = "personal-plan.r1";
        public const string 안내명 = "내면의 울림";
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "시간·장소 비종속 개인 계획 설정과 일회성 안정 근거 계약이다.",
        Boundary = "단일 슬롯 Fixture. 실제 회복량·진척·Save·UI 계약이 아니다.",
        WorldInteractionIds = new[] { Simulation개인계획Codes.WorldInteractionId })]
    public sealed class Simulation개인계획InitialState
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string PlanStableId { get; set; } = string.Empty;
        public string PolicyRevision { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int MaxDescriptionLength { get; set; }
        public string[] AllowedObjectiveStableIds { get; set; } = Array.Empty<string>();
    }

    public class Simulation개인계획PreviewRequest
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string PlanStableId { get; set; } = string.Empty;
        public string ObjectiveStableId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
    }

    public sealed class Simulation개인계획ConfirmRequest : Simulation개인계획PreviewRequest
    {
        public string CommandId { get; set; } = string.Empty;
    }

    public sealed class Simulation개인계획PreviewSnapshot
    {
        public long WorldRevision { get; set; }
        public string NormalizedDescription { get; set; } = string.Empty;
        public bool InitialStabilityEligibilityWouldBeRecorded { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public bool CanConfirm => BlockReasonCodes.Length == 0;
    }

    public sealed class Simulation개인계획Snapshot
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string ObjectiveStableId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string InitialStabilityEligibilityStableId { get; set; } = string.Empty;
        public bool RecoveryApplied => false;
        public string RecoveryNotAppliedReasonCode => "NumericRecoveryAndProgressPolicyUndefined";
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; } = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation개인계획ConfirmResult
    {
        public bool Reused { get; set; }
        public Simulation개인계획Snapshot State { get; set; } = new Simulation개인계획Snapshot();
    }
}
