using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationFarmSurvivalCodes
    {
        public const string RuleRevision = "farm-survival.spring-preparation.r1";
        public const string InteractiveCombatRuleRevision =
            "farm-survival.spring-preparation.r2";
        public const string HeroTacticalCombatRuleRevision =
            "farm-survival.spring-preparation.r3";
        public const string ScenicSeasonRuleRevision =
            "farm-survival.scenic-season.r1";
        public const string DefaultRuleRevision = ScenicSeasonRuleRevision;

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
        public const string Sowing = "Sowing";
        public const string CropCare = "CropCare";
        public const string Harvesting = "Harvesting";
        public const string HarvestCollection = "HarvestCollection";
        public const string OutboundPacking = "OutboundPacking";
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
        public const string AwaitingCombat = "AwaitingCombat";
        public const string AwaitingResponse = "AwaitingResponse";
        public const string AwaitingDefenseChoice = "AwaitingDefenseChoice";
        public const string AwaitingAutoResolution = "AwaitingAutoResolution";
        public const string Resolved = "Resolved";

        public const string Trade = "choice:raider:trade";
        public const string Refuse = "choice:raider:refuse";
        public const string Deception = "choice:raider:deception";
        public const string AutomaticDefense = "choice:defense:auto";
        public const string DirectCombat = "choice:defense:direct";

        public const string DefenseSucceeded = "DefenseSucceeded";
        public const string InventoryTaken = "InventoryTaken";
        public const string FacilityDamaged = "FacilityDamaged";
        public const string ActorInjured = "ActorInjured";
        public const string TacticalWithdrawal = "TacticalWithdrawal";
        public const string TradeCompleted = "TradeCompleted";
        public const string ThreatRepelled = "ThreatRepelled";

        public const string DayFarm = "survival.day-farm";
        public const string TacticalLabor = "survival.tactical-labor";
        public const string DuskDefense = "survival.dusk-defense";
        public const string ZombieWarningPresentation = "survival.zombie-warning";
        public const string RaiderApproachPresentation = "survival.raider-approach";
        public const string DamageAssessmentPresentation = "survival.damage-assessment";
        public const string ScenicExplorationPresentation =
            "survival.scenic-exploration";
        public const string SeasonalDefenseWarningPresentation =
            "survival.seasonal-defense.warning";
        public const string SeasonalDefenseChoicePresentation =
            "survival.seasonal-defense.choice";
        public const string SeasonalDefenseAutomaticPresentation =
            "survival.seasonal-defense.automatic";
        public const string FarmHarvestPresentation = "farm.harvest";
        public const string FarmSowingPresentation = "farm.sowing";
        public const string FarmCropCarePresentation = "farm.crop-care";
        public const string FarmCollectionPresentation = "farm.harvest-collection";
        public const string FarmPackingPresentation = "farm.outbound-packing";
    }

    public static class SimulationFarmActorCapabilityCodes
    {
        public const string FarmTilling = "FarmTilling";
        public const string FarmSowing = "FarmSowing";
        public const string FarmCropCare = "FarmCropCare";
        public const string FarmHarvest = "FarmHarvest";
        public const string FarmCollection = "FarmCollection";
        public const string FarmPacking = "FarmPacking";
    }

    public static class Simulation수확Lot상태Codes
    {
        public const string HarvestedAtField = "HarvestedAtField";
        public const string CollectedAtYard = "CollectedAtYard";
        public const string PackedForShipment = "PackedForShipment";
    }

    public static class Simulation포장Lot상태Codes
    {
        public const string PreparedForShipment = "PreparedForShipment";
    }

    public sealed class SimulationFarmSurvivalInitialStateRequest
    {
        public string RuleRevision { get; set; }
            = SimulationFarmSurvivalCodes.DefaultRuleRevision;
        public string RegionStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public string FarmBuildingStableId { get; set; } = string.Empty;
        public decimal SupplyUnits { get; set; }
        public decimal RepairMaterialUnits { get; set; }
        public decimal SeedUnits { get; set; }
        public decimal WaterUnits { get; set; }
        public SimulationFarmActorInitialStateRequest[] Actors { get; set; }
            = Array.Empty<SimulationFarmActorInitialStateRequest>();
        public SimulationFarmSoilTileInitialStateRequest[] SoilTiles { get; set; }
            = Array.Empty<SimulationFarmSoilTileInitialStateRequest>();
        public SimulationFarmDefenseInitialStateRequest[] Defenses { get; set; }
            = Array.Empty<SimulationFarmDefenseInitialStateRequest>();
        public Simulation재배단위Snapshot[] CultivationUnits { get; set; }
            = Array.Empty<Simulation재배단위Snapshot>();
        public Simulation감자생산RuleSnapshot? PotatoProductionRule { get; set; }
    }

    public sealed class SimulationFarmActorInitialStateRequest
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string ActorKindCode { get; set; } = SimulationFarmSurvivalCodes.Npc;
        public string KoreanName { get; set; } = string.Empty;
        public decimal Health { get; set; } = 100m;
        public decimal Stamina { get; set; } = 100m;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarmSoilTileInitialStateRequest
    {
        public string SoilTileStableId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridY { get; set; }
        public string StateCode { get; set; } = SimulationFarmSurvivalCodes.Untilled;
        public decimal PhysicalAreaSquareMeters { get; set; } = 100m;
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
        public string PreferredSpatialStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmWorkConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignmentKindCode { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmWorkPlanItemRequest
    {
        public string PlanItemStableId { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignmentKindCode { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmWorkPlanPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public SimulationFarmWorkPlanItemRequest[] Items { get; set; }
            = Array.Empty<SimulationFarmWorkPlanItemRequest>();
    }

    public sealed class SimulationFarmWorkPlanConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationFarmWorkPlanItemRequest[] Items { get; set; }
            = Array.Empty<SimulationFarmWorkPlanItemRequest>();
    }

    public sealed class SimulationFarmWorkPlanItemPreviewSnapshot
    {
        public string PlanItemStableId { get; set; } = string.Empty;
        public int Priority { get; set; }
        public SimulationFarmWorkPreviewSnapshot Work { get; set; }
            = new SimulationFarmWorkPreviewSnapshot();
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarmWorkPlanPreviewSnapshot
    {
        public long ExpectedRevision { get; set; }
        public SimulationFarmWorkPlanItemPreviewSnapshot[] Items { get; set; }
            = Array.Empty<SimulationFarmWorkPlanItemPreviewSnapshot>();
        public decimal TotalRequiredLabor { get; set; }
        public decimal TotalStaminaCost { get; set; }
        public int EstimatedCompletionWorldTick { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
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
        public decimal SeedCost { get; set; }
        public decimal WaterCost { get; set; }
        public int DurationTicks { get; set; }
        public int EstimatedCompletionWorldTick { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
        public decimal ProjectedQuantity { get; set; }
        public string ProjectedQuantityUnitCode { get; set; } = string.Empty;
        public string ProjectedLotStableId { get; set; } = string.Empty;
        public Simulation공간상호작용PreviewSnapshot? SpatialInteraction { get; set; }
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
        public decimal SeedUnits { get; set; }
        public decimal WaterUnits { get; set; }
        public decimal ReservedSeedUnits { get; set; }
        public decimal ReservedWaterUnits { get; set; }
        public decimal RecoverableDamageUnits { get; set; }
        public SimulationFarmActorSnapshot[] Actors { get; set; }
            = Array.Empty<SimulationFarmActorSnapshot>();
        public SimulationFarmSoilTileSnapshot[] SoilTiles { get; set; }
            = Array.Empty<SimulationFarmSoilTileSnapshot>();
        public SimulationFarmDefenseSnapshot[] Defenses { get; set; }
            = Array.Empty<SimulationFarmDefenseSnapshot>();
        public Simulation재배단위Snapshot[] CultivationUnits { get; set; }
            = Array.Empty<Simulation재배단위Snapshot>();
        public Simulation수확LotSnapshot[] HarvestLots { get; set; }
            = Array.Empty<Simulation수확LotSnapshot>();
        public Simulation포장LotSnapshot[] PackageLots { get; set; }
            = Array.Empty<Simulation포장LotSnapshot>();
        public SimulationFarmWorkOrderSnapshot[] WorkOrders { get; set; }
            = Array.Empty<SimulationFarmWorkOrderSnapshot>();
        public SimulationThreatEncounterSnapshot[] Encounters { get; set; }
            = Array.Empty<SimulationThreatEncounterSnapshot>();
        public SimulationSurvivalDayReportSnapshot[] DayReports { get; set; }
            = Array.Empty<SimulationSurvivalDayReportSnapshot>();
        public SimulationFarmCombatStateSnapshot Combat { get; set; }
            = new SimulationFarmCombatStateSnapshot();
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
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFarmSoilTileSnapshot
    {
        public string SoilTileStableId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridY { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public decimal PhysicalAreaSquareMeters { get; set; }
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
        public decimal SeedCost { get; set; }
        public decimal WaterCost { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
        public string SelectedSpatialStableId { get; set; } = string.Empty;
        public string SpatialDefinitionRevision { get; set; } = string.Empty;
        public string SpatialDefinitionHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation수확LotSnapshot
    {
        public string HarvestLotStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CultivationUnitStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string CausedByTaskStableId { get; set; } = string.Empty;
        public int CreatedWorldTick { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation포장LotSnapshot
    {
        public string PackageLotStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string CausedByTaskStableId { get; set; } = string.Empty;
        public int CreatedWorldTick { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationThreatEncounterSnapshot
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string ThreatTypeCode { get; set; } = string.Empty;
        public int ThreatUnitCount { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public int OccurredWorldTick { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
        public string[] AvailableChoiceStableIds { get; set; }
            = Array.Empty<string>();
        public int? DecisionDeadlineWorldTick { get; set; }
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
