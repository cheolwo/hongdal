using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarm방위귀환Codes
    {
        public const string RuleRevision = "farm-defense-return.r1";
        public const string PlayableLoopStableId = "playable-loop:farm-barracks-defense.v1";
        public const string WorldInteractionId = "WI-FARM-DEFENSE-RETURN";
        public const string ExpectedRevisionMismatch = "FarmDefenseReturnExpectedRevisionMismatch";
        public const string ReturnUnknown = "FarmDefenseReturnUnknown";
        public const string ResultNotResolved = "FarmDefenseReturnResultNotResolved";
        public const string AlreadyReturned = "FarmDefenseReturnAlreadyReturned";
        public const string CommandPayloadConflict = "FarmDefenseReturnCommandPayloadConflict";
    }

    public sealed class SimulationFarm방위귀환Definition
    {
        public string ReturnStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public string OutpostStableId { get; set; } = string.Empty;
        public bool ResultResolved { get; set; }
        public string[] TreatmentRequiredActorStableIds { get; set; } = Array.Empty<string>();
        public string[] ProductionRejoinCandidateActorStableIds { get; set; } = Array.Empty<string>();
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "방어 결과가 확정된 분대를 초소로 귀환시키고 치료·생산 재합류 후속 대기열에 인계하는 초기 계약을 정의한다.",
        Boundary = "전리품을 다시 지급하거나 치료·휴식·생산 재합류를 실행하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위귀환Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위귀환InitialStateRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long InitialWorldRevision { get; set; }
        public SimulationFarm방위귀환Definition[] Returns { get; set; } = Array.Empty<SimulationFarm방위귀환Definition>();
    }

    public sealed class SimulationFarm방위귀환StateSnapshot
    {
        public string ReturnStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public string OutpostStableId { get; set; } = string.Empty;
        public bool ResultResolved { get; set; }
        public bool IsReturned { get; set; }
        public string[] TreatmentRequiredActorStableIds { get; set; } = Array.Empty<string>();
        public string[] ProductionRejoinCandidateActorStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarm방위귀환LedgerSnapshot
    {
        public string RuleRevision { get; set; } = SimulationFarm방위귀환Codes.RuleRevision;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public SimulationFarm방위귀환StateSnapshot[] Returns { get; set; } = Array.Empty<SimulationFarm방위귀환StateSnapshot>();
        public string[] PendingTreatmentActorStableIds { get; set; } = Array.Empty<string>();
        public string[] PendingProductionRejoinActorStableIds { get; set; } = Array.Empty<string>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; } = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위귀환PreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string ReturnStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위귀환PreviewSnapshot
    {
        public string WorldInteractionId { get; set; } = SimulationFarm방위귀환Codes.WorldInteractionId;
        public long ObservedWorldRevision { get; set; }
        public string ReturnStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public string OutpostStableId { get; set; } = string.Empty;
        public int TreatmentRequiredCount { get; set; }
        public int ProductionRejoinCandidateCount { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarm방위귀환ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string ReturnStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위귀환ConfirmResult
    {
        public SimulationFarm방위귀환LedgerSnapshot Ledger { get; set; } = new SimulationFarm방위귀환LedgerSnapshot();
        public Simulation행위발현Record ActionRecord { get; set; } = new Simulation행위발현Record();
        public bool Reused { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "분대의 초소 귀환과 후속 치료·생산 재합류 대기 건수를 읽기 전용 카드로 투영한다.",
        Boundary = "카드는 귀환 Confirm이나 치료·생산 상태를 변경하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위귀환Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위귀환CardSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string ReturnStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public string OutpostStableId { get; set; } = string.Empty;
        public int TreatmentRequiredCount { get; set; }
        public int ProductionRejoinCandidateCount { get; set; }
        public bool IsReturned { get; set; }
    }
}
