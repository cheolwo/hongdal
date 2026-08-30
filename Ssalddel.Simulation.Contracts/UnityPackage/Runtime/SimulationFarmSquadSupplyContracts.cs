using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarm분대보급Codes
    {
        public const string RuleRevision = "farm-defense-squad-supply.r1";
        public const string PlayableLoopStableId = "playable-loop:farm-barracks-defense.v1";
        public const string WorldInteractionId = "WI-SQUAD-SUPPLY";
        public const string ExpectedRevisionMismatch = "FarmSquadSupplyExpectedRevisionMismatch";
        public const string SquadUnknown = "FarmSquadSupplySquadUnknown";
        public const string SquadAlreadySupplied = "FarmSquadSupplySquadAlreadySupplied";
        public const string FoodInsufficient = "FarmSquadSupplyFoodInsufficient";
        public const string DurabilityRestoreInsufficient = "FarmSquadSupplyDurabilityRestoreInsufficient";
        public const string CommandPayloadConflict = "FarmSquadSupplyCommandPayloadConflict";
    }

    public sealed class SimulationFarm분대보급Requirement
    {
        public string SquadStableId { get; set; } = string.Empty;
        public int RequiredFoodUnits { get; set; }
        public int RequiredDurabilityRestoreUnits { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "Farm 방위 분대의 식량과 장비 내구도 복구 필요량을 권위 초기 상태로 정의한다.",
        Boundary = "밸런스 수치·개별 장비·훈련·출동·전투 결과를 결정하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm분대보급Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대보급InitialStateRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long InitialWorldRevision { get; set; }
        public int FoodStockUnits { get; set; }
        public int DurabilityRestoreCapacityUnits { get; set; }
        public SimulationFarm분대보급Requirement[] SquadRequirements { get; set; } = Array.Empty<SimulationFarm분대보급Requirement>();
    }

    public sealed class SimulationFarm분대보급StateSnapshot
    {
        public string SquadStableId { get; set; } = string.Empty;
        public int RequiredFoodUnits { get; set; }
        public int RequiredDurabilityRestoreUnits { get; set; }
        public bool IsSupplied { get; set; }
    }

    public sealed class SimulationFarm분대보급LedgerSnapshot
    {
        public string RuleRevision { get; set; } = SimulationFarm분대보급Codes.RuleRevision;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int FoodStockUnits { get; set; }
        public int DurabilityRestoreCapacityUnits { get; set; }
        public SimulationFarm분대보급StateSnapshot[] Squads { get; set; } = Array.Empty<SimulationFarm분대보급StateSnapshot>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; } = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm분대보급PreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm분대보급PreviewSnapshot
    {
        public string WorldInteractionId { get; set; } = SimulationFarm분대보급Codes.WorldInteractionId;
        public long ObservedWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
        public int RequiredFoodUnits { get; set; }
        public int RequiredDurabilityRestoreUnits { get; set; }
        public int AvailableFoodUnits { get; set; }
        public int AvailableDurabilityRestoreUnits { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarm분대보급ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarm분대보급ConfirmResult
    {
        public SimulationFarm분대보급LedgerSnapshot Ledger { get; set; } = new SimulationFarm분대보급LedgerSnapshot();
        public Simulation행위발현Record ActionRecord { get; set; } = new Simulation행위발현Record();
        public bool Reused { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 분대별 식량·장비 내구도 필요량과 준비 여부를 읽기 전용 카드로 투영한다.",
        Boundary = "카드는 보급량을 계산하거나 Confirm·WorldRevision을 변경하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm분대보급Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대보급CardSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string SquadStableId { get; set; } = string.Empty;
        public int RequiredFoodUnits { get; set; }
        public int RequiredDurabilityRestoreUnits { get; set; }
        public bool IsSupplied { get; set; }
    }
}
