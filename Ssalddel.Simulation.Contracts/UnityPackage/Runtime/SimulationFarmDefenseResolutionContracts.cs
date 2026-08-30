using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarm방위결과Codes
    {
        public const string RuleRevision = "farm-defense-resolution.r1";
        public const string PlayableLoopStableId = "playable-loop:farm-barracks-defense.v1";
        public const string WorldInteractionId = "WI-FARM-DEFENSE-RESOLVE";
        public const string ExpectedRevisionMismatch = "FarmDefenseResolutionExpectedRevisionMismatch";
        public const string EncounterUnknown = "FarmDefenseResolutionEncounterUnknown";
        public const string EncounterAlreadyResolved = "FarmDefenseResolutionEncounterAlreadyResolved";
        public const string ResultNotSuccessful = "FarmDefenseResolutionResultNotSuccessful";
        public const string CommandPayloadConflict = "FarmDefenseResolutionCommandPayloadConflict";
    }

    public sealed class SimulationFarm방위전리품Definition
    {
        public string ItemStableId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public sealed class SimulationFarm방위확정결과Definition
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public bool DefenseSucceeded { get; set; }
        public int ThreatReductionUnits { get; set; }
        public long SafeUntilWorldTick { get; set; }
        public int ProductionModifierMilli { get; set; }
        public int RecoveryModifierMilli { get; set; }
        public SimulationFarm방위전리품Definition[] Loot { get; set; } = Array.Empty<SimulationFarm방위전리품Definition>();
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "전투 권위가 확정한 Farm 방어 성공 결과 묶음을 WI가 읽을 초기 계약으로 정의한다.",
        Boundary = "이 WI는 승패·피해·보정 수치·전리품을 계산하거나 선택하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위결과Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위결과InitialStateRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long InitialWorldRevision { get; set; }
        public long CurrentWorldTick { get; set; }
        public int ThreatUnits { get; set; }
        public int ProductionModifierMilli { get; set; } = 1000;
        public int RecoveryModifierMilli { get; set; } = 1000;
        public SimulationFarm방위확정결과Definition[] PendingResults { get; set; } = Array.Empty<SimulationFarm방위확정결과Definition>();
    }

    public sealed class SimulationFarm방위결과Snapshot
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public int ThreatReductionUnits { get; set; }
        public long SafeUntilWorldTick { get; set; }
        public int ProductionModifierMilli { get; set; }
        public int RecoveryModifierMilli { get; set; }
        public SimulationFarm방위전리품Definition[] Loot { get; set; } = Array.Empty<SimulationFarm방위전리품Definition>();
        public bool IsResolved { get; set; }
    }

    public sealed class SimulationFarm방위결과LedgerSnapshot
    {
        public string RuleRevision { get; set; } = SimulationFarm방위결과Codes.RuleRevision;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public long CurrentWorldTick { get; set; }
        public int ThreatUnits { get; set; }
        public long SafeUntilWorldTick { get; set; }
        public int ProductionModifierMilli { get; set; }
        public int RecoveryModifierMilli { get; set; }
        public SimulationFarm방위전리품Definition[] LootInventory { get; set; } = Array.Empty<SimulationFarm방위전리품Definition>();
        public SimulationFarm방위결과Snapshot[] Results { get; set; } = Array.Empty<SimulationFarm방위결과Snapshot>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; } = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위결과PreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위결과PreviewSnapshot
    {
        public string WorldInteractionId { get; set; } = SimulationFarm방위결과Codes.WorldInteractionId;
        public long ObservedWorldRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public int ThreatReductionUnits { get; set; }
        public long SafeUntilWorldTick { get; set; }
        public int LootLineCount { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarm방위결과ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm방위결과ConfirmResult
    {
        public SimulationFarm방위결과LedgerSnapshot Ledger { get; set; } = new SimulationFarm방위결과LedgerSnapshot();
        public Simulation행위발현Record ActionRecord { get; set; } = new Simulation행위발현Record();
        public bool Reused { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 방어 결과의 위협 감소·안전 기간·생산/회복 보정·전리품을 읽기 전용 카드로 투영한다.",
        Boundary = "카드는 전투 결과를 계산하거나 Confirm·WorldRevision을 변경하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위결과Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위결과CardSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public int ThreatReductionUnits { get; set; }
        public long SafeUntilWorldTick { get; set; }
        public int ProductionModifierMilli { get; set; }
        public int RecoveryModifierMilli { get; set; }
        public int LootLineCount { get; set; }
        public bool IsResolved { get; set; }
    }
}
