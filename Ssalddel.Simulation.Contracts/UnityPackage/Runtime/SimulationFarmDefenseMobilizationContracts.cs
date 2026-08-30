using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarm방위소집Codes
    {
        public const string RuleRevision = "farm-defense-mobilization.r1";
        public const string PlayableLoopStableId =
            "playable-loop:farm-barracks-defense.v1";
        public const string WorldInteractionId = "WI-FARM-DEFENSE-MOBILIZE";
        public const string 대기 = "Stationed";
        public const string 출동 = "Mobilized";
        public const string ExpectedRevisionMismatch =
            "FarmDefenseMobilizationExpectedRevisionMismatch";
        public const string SquadUnknown = "FarmDefenseSquadUnknown";
        public const string SquadNotReady = "FarmDefenseSquadNotReady";
        public const string ThreatUnknown = "FarmDefenseThreatUnknown";
        public const string AlreadyMobilized = "FarmDefenseSquadAlreadyMobilized";
        public const string CommandPayloadConflict =
            "FarmDefenseMobilizationCommandPayloadConflict";
    }

    public sealed class SimulationFarm방위분대Definition
    {
        public string SquadStableId { get; set; } = string.Empty;
        public string[] AssignedWorkerStableIds { get; set; } = Array.Empty<string>();
        public bool IsReady { get; set; }
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Farm 방위 분대 소집 원장의 초기 권위 계약을 정의한다.",
        Boundary = "전투 결과·부상·보급 소비·공간 배치를 포함하지 않는 첫 소집 WI 계약이다.",
        WorldInteractionIds = new[] { SimulationFarm방위소집Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위소집InitialStateRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long InitialWorldRevision { get; set; }
        public SimulationFarm방위분대Definition[] Squads { get; set; }
            = Array.Empty<SimulationFarm방위분대Definition>();
        public string[] ApproachingThreatStableIds { get; set; }
            = Array.Empty<string>();
    }

    public sealed class SimulationFarm방위분대Snapshot
    {
        public string SquadStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = SimulationFarm방위소집Codes.대기;
        public bool IsReady { get; set; }
        public string MobilizedThreatStableId { get; set; } = string.Empty;
        public string[] AssignedWorkerStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarm방위소집LedgerSnapshot
    {
        public string RuleRevision { get; set; } = SimulationFarm방위소집Codes.RuleRevision;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public SimulationFarm방위분대Snapshot[] Squads { get; set; }
            = Array.Empty<SimulationFarm방위분대Snapshot>();
        public string[] ApproachingThreatStableIds { get; set; }
            = Array.Empty<string>();
        public string[] SuspendedProductionWorkerStableIds { get; set; }
            = Array.Empty<string>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; }
            = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위소집PreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
        public string ThreatStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위소집PreviewSnapshot
    {
        public string WorldInteractionId { get; set; }
            = SimulationFarm방위소집Codes.WorldInteractionId;
        public long ObservedWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
        public string ThreatStableId { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] SuspendedWorkerStableIds { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarm방위소집ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
        public string ThreatStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위소집ConfirmResult
    {
        public SimulationFarm방위소집LedgerSnapshot Ledger { get; set; }
            = new SimulationFarm방위소집LedgerSnapshot();
        public Simulation행위발현Record ActionRecord { get; set; }
            = new Simulation행위발현Record();
        public bool Reused { get; set; }
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Farm 방위 분대의 대기·출동·생산 중단 상태를 읽기 전용 카드로 투영한다.",
        Boundary = "표현 상태 사본이며 WorldRevision을 변경하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위소집Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위소집CardSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string ThreatStableId { get; set; } = string.Empty;
        public int AssignedWorkerCount { get; set; }
        public bool ProductionContributionSuspended { get; set; }
    }
}
