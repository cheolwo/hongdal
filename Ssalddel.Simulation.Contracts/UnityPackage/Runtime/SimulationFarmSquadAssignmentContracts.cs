using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarm분대배정Codes
    {
        public const string RuleRevision = "farm-defense-squad-assignment.r1";
        public const string PlayableLoopStableId = "playable-loop:farm-barracks-defense.v1";
        public const string WorldInteractionId = "WI-SQUAD-ASSIGN";
        public const string ExpectedRevisionMismatch = "FarmSquadAssignmentExpectedRevisionMismatch";
        public const string OutpostUnknown = "FarmSquadAssignmentOutpostUnknown";
        public const string SlotUnknown = "FarmSquadAssignmentSlotUnknown";
        public const string SquadUnknown = "FarmSquadAssignmentSquadUnknown";
        public const string SquadAlreadyAssigned = "FarmSquadAssignmentSquadAlreadyAssigned";
        public const string SlotOccupied = "FarmSquadAssignmentSlotOccupied";
        public const string CommandPayloadConflict = "FarmSquadAssignmentCommandPayloadConflict";
    }

    public sealed class SimulationFarm경비초소Definition
    {
        public string OutpostStableId { get; set; } = string.Empty;
        public string[] SlotStableIds { get; set; } = Array.Empty<string>();
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "Farm 경비 초소 슬롯에 분대를 하나 배정하는 권위 초기 계약을 정의한다.",
        Boundary = "출동·전투·보급·영웅 조작·시설 배치를 포함하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm분대배정Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대배정InitialStateRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long InitialWorldRevision { get; set; }
        public string[] SquadStableIds { get; set; } = Array.Empty<string>();
        public SimulationFarm경비초소Definition[] Outposts { get; set; } = Array.Empty<SimulationFarm경비초소Definition>();
    }

    public sealed class SimulationFarm분대배정Snapshot
    {
        public string OutpostStableId { get; set; } = string.Empty;
        public string SlotStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm초소SlotSnapshot
    {
        public string OutpostStableId { get; set; } = string.Empty;
        public string SlotStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm분대배정LedgerSnapshot
    {
        public string RuleRevision { get; set; } = SimulationFarm분대배정Codes.RuleRevision;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string[] SquadStableIds { get; set; } = Array.Empty<string>();
        public SimulationFarm초소SlotSnapshot[] Slots { get; set; } = Array.Empty<SimulationFarm초소SlotSnapshot>();
        public SimulationFarm분대배정Snapshot[] Assignments { get; set; } = Array.Empty<SimulationFarm분대배정Snapshot>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; } = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm분대배정PreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string OutpostStableId { get; set; } = string.Empty;
        public string SlotStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm분대배정PreviewSnapshot
    {
        public string WorldInteractionId { get; set; } = SimulationFarm분대배정Codes.WorldInteractionId;
        public long ObservedWorldRevision { get; set; }
        public string OutpostStableId { get; set; } = string.Empty;
        public string SlotStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarm분대배정ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string OutpostStableId { get; set; } = string.Empty;
        public string SlotStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm분대배정ConfirmResult
    {
        public SimulationFarm분대배정LedgerSnapshot Ledger { get; set; } = new SimulationFarm분대배정LedgerSnapshot();
        public Simulation행위발현Record ActionRecord { get; set; } = new Simulation행위발현Record();
        public bool Reused { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 경비 초소 슬롯과 배정 분대를 읽기 전용 관리 카드로 투영한다.",
        Boundary = "카드는 Confirm 권위나 WorldRevision 변경 권한을 갖지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm분대배정Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대배정CardSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string OutpostStableId { get; set; } = string.Empty;
        public string SlotStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
    }
}
