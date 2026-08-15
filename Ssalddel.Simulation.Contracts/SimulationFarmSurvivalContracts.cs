using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarmSurvivalCodes
    {
        public const string RuleRevision = "farm-survival.spring-preparation.r1";

        public const string Player = "Player";
        public const string Npc = "Npc";
        public const string PlayerDirect = "PlayerDirect";
        public const string NpcDelegated = "NpcDelegated";

        public const string Untilled = "Untilled";
        public const string Tilled = "Tilled";
        public const string Planned = "Planned";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";

        public const string Tilling = "Tilling";
        public const string FenceRepair = "FenceRepair";
        public const string StorageSecuring = "StorageSecuring";
        public const string LightingPreparation = "LightingPreparation";
        public const string GuardDuty = "GuardDuty";
        public const string MedicalRest = "MedicalRest";

        public const string Fence = "Fence";
        public const string StorageLock = "StorageLock";
        public const string Lighting = "Lighting";
        public const string GuardPost = "GuardPost";

        public const string ZombiePressure = "ZombiePressure";
        public const string RaiderFaction = "RaiderFaction";
        public const string Warning = "Warning";
        public const string AwaitingResponse = "AwaitingResponse";
        public const string Resolved = "Resolved";

        public const string Trade = "choice:raider:trade";
        public const string Refuse = "choice:raider:refuse";
        public const string Deception = "choice:raider:deception";

        public const string DefenseSucceeded = "DefenseSucceeded";
        public const string InventoryTaken = "InventoryTaken";
        public const string FacilityDamaged = "FacilityDamaged";
        public const string ActorInjured = "ActorInjured";
        public const string TradeCompleted = "TradeCompleted";
        public const string ThreatRepelled = "ThreatRepelled";

        public const string DayFarm = "survival.day-farm";
        public const string TacticalLabor = "survival.tactical-labor";
        public const string DuskDefense = "survival.dusk-defense";
        public const string ZombieWarningPresentation = "survival.zombie-warning";
        public const string RaiderApproachPresentation = "survival.raider-approach";
        public const string DamageAssessmentPresentation = "survival.damage-assessment";
    }

    public sealed class SimulationFarmSurvivalInitialStateRequest
    {
        public string RuleRevision { get; set; } = SimulationFarmSurvivalCodes.RuleRevision;
        public string RegionStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public string FarmBuildingStableId { get; set; } = string.Empty;
        public decimal SupplyUnits { get; set; }
        public decimal RepairMaterialUnits { get; set; }
        public SimulationFarmActorInitialStateRequest[] Actors { get; set; }
            = Array.Empty<SimulationFarmActorInitialStateRequest>();
        public SimulationFarmSoilTileInitialStateRequest[] SoilTiles { get; set; }
            = Array.Empty<SimulationFarmSoilTileInitialStateRequest>();
        public SimulationFarmDefenseInitialStateRequest[] Defenses { get; set; }
            = Array.Empty<SimulationFarmDefenseInitialStateRequest>();
    }

    public sealed class SimulationFarmActorInitialStateRequest
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string ActorKindCode { get; set; } = SimulationFarmSurvivalCodes.Npc;
        public string KoreanName { get; set; } = string.Empty;
        public decimal Health { get; set; } = 100m;
        public decimal Stamina { get; set; } = 100m;
    }

    public sealed class SimulationFarmSoilTileInitialStateRequest
    {
        public string SoilTileStableId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridY { get; set; }
        public string StateCode { get; set; } = SimulationFarmSurvivalCodes.Untilled;
    }

    public sealed class SimulationFarmDefenseInitialStateRequest
    {
        public string DefenseStableId { get; set; } = string.Empty;
        public string DefenseKindCode { get; set; } = string.Empty;
        public decimal Durability { get; set; }
        public bool Prepared { get; set; }
    }

    public sealed class SimulationFarmWorkPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignmentKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmWorkConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignmentKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmWorkPreviewSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignmentKindCode { get; set; } = string.Empty;
        public decimal RequiredLabor { get; set; }
        public decimal StaminaCost { get; set; }
        public decimal MaterialCost { get; set; }
        public int DurationTicks { get; set; }
        public int EstimatedCompletionWorldTick { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationThreatResponseConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ChoiceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmSurvivalStateSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public int DayNumber { get; set; } = 1;
        public int ChapterDayNumber { get; set; } = 1;
        public string RuleRevision { get; set; } = string.Empty;
        public string RegionStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public string FarmBuildingStableId { get; set; } = string.Empty;
        public string DayGoalCode { get; set; } = string.Empty;
        public decimal SupplyUnits { get; set; }
        public decimal RepairMaterialUnits { get; set; }
        public decimal RecoverableDamageUnits { get; set; }
        public SimulationFarmActorSnapshot[] Actors { get; set; }
            = Array.Empty<SimulationFarmActorSnapshot>();
        public SimulationFarmSoilTileSnapshot[] SoilTiles { get; set; }
            = Array.Empty<SimulationFarmSoilTileSnapshot>();
        public SimulationFarmDefenseSnapshot[] Defenses { get; set; }
            = Array.Empty<SimulationFarmDefenseSnapshot>();
        public SimulationFarmWorkOrderSnapshot[] WorkOrders { get; set; }
            = Array.Empty<SimulationFarmWorkOrderSnapshot>();
        public SimulationThreatEncounterSnapshot[] Encounters { get; set; }
            = Array.Empty<SimulationThreatEncounterSnapshot>();
        public SimulationSurvivalDayReportSnapshot[] DayReports { get; set; }
            = Array.Empty<SimulationSurvivalDayReportSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationFarmActorSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string ActorKindCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public decimal Health { get; set; }
        public decimal Stamina { get; set; }
        public bool Injured { get; set; }
        public string ActiveWorkOrderStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmSoilTileSnapshot
    {
        public string SoilTileStableId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridY { get; set; }
        public string StateCode { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmDefenseSnapshot
    {
        public string DefenseStableId { get; set; } = string.Empty;
        public string DefenseKindCode { get; set; } = string.Empty;
        public decimal Durability { get; set; }
        public bool Prepared { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmWorkOrderSnapshot
    {
        public string WorkOrderStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignmentKindCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public int StartedWorldTick { get; set; }
        public int CompletesWorldTick { get; set; }
        public decimal ReservedLabor { get; set; }
        public decimal StaminaCost { get; set; }
        public decimal MaterialCost { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationThreatEncounterSnapshot
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string ThreatTypeCode { get; set; } = string.Empty;
        public int ThreatUnitCount { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public int OccurredWorldTick { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
        public string SelectedChoiceStableId { get; set; } = string.Empty;
        public string OutcomeCode { get; set; } = string.Empty;
        public decimal SupplyLossUnits { get; set; }
        public decimal DamageUnits { get; set; }
        public string InjuredActorStableId { get; set; } = string.Empty;
        public bool Recoverable { get; set; } = true;
    }

    public sealed class SimulationSurvivalDayReportSnapshot
    {
        public int DayNumber { get; set; }
        public string ReportCode { get; set; } = string.Empty;
        public string KoreanSummary { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
