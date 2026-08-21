using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationIntegratedWorldActionCodes
    {
        public const string ManufacturingOrder = "WI-MFG-01";
        public const string ConstructionOrder = "WI-CON-01";
        public const string Recruitment = "WI-MIL-01";
        public const string Training = "WI-MIL-02";
        public const string FormationDeployment = "WI-MIL-03";
        public const string FacilityRepair = "WI-WORLD-04";
        public const string PotatoPackaging = "WI-FARM-06";
        public const string CargoTransfer = "WI-LOG-03";
    }

    public static class SimulationFacilityLifecycleCodes
    {
        public const string Planned = "Planned";
        public const string UnderConstruction = "UnderConstruction";
        public const string Operational = "Operational";
        public const string Removed = "Removed";
    }

    public static class SimulationFacilityIntegrityCodes
    {
        public const string Intact = "Intact";
        public const string Damaged = "Damaged";
        public const string Disabled = "Disabled";
    }

    public static class SimulationFacilityMaintenanceCodes
    {
        public const string None = "None";
        public const string Repairing = "Repairing";
    }

    public static class SimulationFacilityCapabilityStateCodes
    {
        public const string Active = "Active";
        public const string Restricted = "Restricted";
        public const string Suspended = "Suspended";
    }

    public static class SimulationIntegratedCapabilityCodes
    {
        public const string Storage = "Spatial.Storage";
        public const string CargoAccessible = "Spatial.CargoAccessible";
        public const string LoadingWorkArea = "Spatial.LoadingWorkArea";
        public const string WorkerAccessible = "Spatial.WorkerAccessible";
        public const string ManufacturingWorkArea = "Spatial.ManufacturingWorkArea";
        public const string FinishedGoodsInspectionArea = "Spatial.FinishedGoodsInspectionArea";
        public const string Recruitment = "Military.Recruitment";
        public const string Training = "Military.Training";
        public const string Garrison = "Military.Garrison";
    }

    public static class SimulationIntegratedItemCodes
    {
        public const string ManufacturingRawMaterial = "ManufacturingRawMaterial";
        public const string TransportBox = "TransportBox";
        public const string FacilityComponent = "FacilityComponent";
        public const string CoopFacilityComponent = "CoopFacilityComponent";
        public const string HarvestPotato = "HarvestPotato";
        public const string PackagedPotato = "PackagedPotato";
    }

    public static class SimulationManufacturingJobStateCodes
    {
        public const string Reserved = "Reserved";
        public const string Processing = "Processing";
        public const string AwaitingInspection = "AwaitingInspection";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    public static class SimulationConstructionProjectStateCodes
    {
        public const string Planned = "Planned";
        public const string Building = "Building";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    public static class SimulationFormationStateCodes
    {
        public const string Recruiting = "Recruiting";
        public const string Recruited = "Recruited";
        public const string Training = "Training";
        public const string Trained = "Trained";
        public const string Ready = "Ready";
        public const string BattleLocked = "BattleLocked";
    }

    public static class SimulationActorCommitmentCodes
    {
        public const string SimulationTask = "SimulationTask";
        public const string Training = "Training";
        public const string FormationDuty = "FormationDuty";
        public const string BattleLock = "BattleLock";
    }

    public static class SimulationIntegratedWorldEffectCodes
    {
        public const string FacilityBattleDamage = "FacilityBattleDamage";
        public const string FacilityRepair = "FacilityRepair";
        public const string ActorInjury = "ActorInjury";
    }

    public sealed class SimulationIntegratedWorldInitialStateRequest
    {
        public string ScenarioRevision { get; set; } = string.Empty;
        public string ScenarioHashSha256 { get; set; } = string.Empty;
        public SimulationFacilityDefinitionRequest[] FacilityDefinitions { get; set; }
            = Array.Empty<SimulationFacilityDefinitionRequest>();
        public SimulationScenarioFacilitySeedRequest[] FacilitySeeds { get; set; }
            = Array.Empty<SimulationScenarioFacilitySeedRequest>();
        public SimulationIntegratedActorSeedRequest[] Actors { get; set; }
            = Array.Empty<SimulationIntegratedActorSeedRequest>();
        public SimulationIntegratedLotSeedRequest[] Lots { get; set; }
            = Array.Empty<SimulationIntegratedLotSeedRequest>();
        public SimulationManufacturingRecipeRequest[] ManufacturingRecipes { get; set; }
            = Array.Empty<SimulationManufacturingRecipeRequest>();
        public SimulationFacilityBlueprintRequest[] FacilityBlueprints { get; set; }
            = Array.Empty<SimulationFacilityBlueprintRequest>();
    }

    public sealed class SimulationFacilityDefinitionRequest
    {
        public string FacilityDefinitionStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string HashSha256 { get; set; } = string.Empty;
        public string FacilityTypeCode { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public SimulationFacilityCapacityDefinitionRequest[] Capacities { get; set; }
            = Array.Empty<SimulationFacilityCapacityDefinitionRequest>();
    }

    public sealed class SimulationFacilityCapacityDefinitionRequest
    {
        public string CapacityCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
    }

    public sealed class SimulationScenarioFacilitySeedRequest
    {
        public string FacilityStableId { get; set; } = string.Empty;
        public string FacilityDefinitionStableId { get; set; } = string.Empty;
        public string PlacementH1StableId { get; set; } = string.Empty;
        public string[] AccessConnectorStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationIntegratedActorSeedRequest
    {
        public string ActorStableId { get; set; } = string.Empty;
        public int EligibilityRank { get; set; }
        public bool FarmLaborEligible { get; set; }
    }

    public sealed class SimulationIntegratedLotSeedRequest
    {
        public string LotStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationManufacturingRecipeRequest
    {
        public string RecipeStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string HashSha256 { get; set; } = string.Empty;
        public int ProcessingTicks { get; set; }
        public SimulationIntegratedItemRequirement[] Inputs { get; set; }
            = Array.Empty<SimulationIntegratedItemRequirement>();
        public SimulationIntegratedItemRequirement[] Outputs { get; set; }
            = Array.Empty<SimulationIntegratedItemRequirement>();
    }

    public sealed class SimulationFacilityBlueprintRequest
    {
        public string BlueprintStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string HashSha256 { get; set; } = string.Empty;
        public string FacilityDefinitionStableId { get; set; } = string.Empty;
        public string SettlementFacilityTypeCode { get; set; } = string.Empty;
        public string SettlementDistrictStableId { get; set; } = string.Empty;
        public int ConstructionTicks { get; set; }
        public SimulationIntegratedItemRequirement[] Materials { get; set; }
            = Array.Empty<SimulationIntegratedItemRequirement>();
    }

    public sealed class SimulationIntegratedItemRequirement
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
    }

    public sealed class SimulationIntegratedWorldCommandRequest
    {
        public string ActionCode { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationManufacturingOrderPayload? Manufacturing { get; set; }
        public SimulationConstructionOrderPayload? Construction { get; set; }
        public SimulationRecruitmentPayload? Recruitment { get; set; }
        public SimulationTrainingPayload? Training { get; set; }
        public SimulationFormationDeploymentPayload? FormationDeployment { get; set; }
        public SimulationFacilityRepairPayload? FacilityRepair { get; set; }
        public SimulationPotatoPackagingPayload? PotatoPackaging { get; set; }
        public SimulationCargoTransferPayload? CargoTransfer { get; set; }
    }

    public sealed class SimulationManufacturingOrderPayload
    {
        public string RecipeStableId { get; set; } = string.Empty;
        public string PreferredManufacturingFacilityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationConstructionOrderPayload
    {
        public string BlueprintStableId { get; set; } = string.Empty;
        public string BuildSiteH1StableId { get; set; } = string.Empty;
    }

    public sealed class SimulationRecruitmentPayload
    {
        public string RecruitmentPolicyStableId { get; set; } = string.Empty;
        public int ActorCount { get; set; } = 8;
    }

    public sealed class SimulationTrainingPayload
    {
        public string FormationStableId { get; set; } = string.Empty;
        public int TrainingTicks { get; set; } = 1;
    }

    public sealed class SimulationFormationDeploymentPayload
    {
        public string FormationStableId { get; set; } = string.Empty;
        public string GarrisonFacilityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFacilityRepairPayload
    {
        public string FacilityStableId { get; set; } = string.Empty;
        public int RepairTicks { get; set; } = 1;
    }

    public sealed class SimulationPotatoPackagingPayload
    {
        public string PotatoLotStableId { get; set; } = string.Empty;
        public string TransportBoxLotStableId { get; set; } = string.Empty;
        public decimal PotatoQuantity { get; set; }
        public decimal BoxQuantity { get; set; }
        public string PackagingPolicyCode { get; set; }
            = "IntegratedWorldPotatoPackagingPolicy.v2";
    }

    public sealed class SimulationCargoTransferPayload
    {
        public string SourceLotStableId { get; set; } = string.Empty;
        public string TargetFacilityStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public int TransportTicks { get; set; } = 1;
    }

    public sealed class SimulationIntegratedWorldPreviewSnapshot
    {
        public string ActionCode { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SelectedActorStableIds { get; set; } = Array.Empty<string>();
        public string SelectedFacilityStableId { get; set; } = string.Empty;
        public string[] ReservedLotStableIds { get; set; } = Array.Empty<string>();
        public string PreviewHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationIntegratedWorldSnapshot
    {
        public string ScenarioRevision { get; set; } = string.Empty;
        public string ScenarioHashSha256 { get; set; } = string.Empty;
        public SimulationRuntimeFacilitySnapshot[] Facilities { get; set; }
            = Array.Empty<SimulationRuntimeFacilitySnapshot>();
        public SimulationFacilityRestrictionSnapshot[] FacilityRestrictions { get; set; }
            = Array.Empty<SimulationFacilityRestrictionSnapshot>();
        public SimulationManufacturingJobSnapshot[] ManufacturingJobs { get; set; }
            = Array.Empty<SimulationManufacturingJobSnapshot>();
        public SimulationConstructionProjectSnapshot[] ConstructionProjects { get; set; }
            = Array.Empty<SimulationConstructionProjectSnapshot>();
        public SimulationIntegratedActorSnapshot[] Actors { get; set; }
            = Array.Empty<SimulationIntegratedActorSnapshot>();
        public SimulationFormationSnapshot[] Formations { get; set; }
            = Array.Empty<SimulationFormationSnapshot>();
        public SimulationActorCommitmentSnapshot[] ActorCommitments { get; set; }
            = Array.Empty<SimulationActorCommitmentSnapshot>();
        public SimulationActorInjurySnapshot[] ActorInjuries { get; set; }
            = Array.Empty<SimulationActorInjurySnapshot>();
        public SimulationIntegratedLotSnapshot[] Lots { get; set; }
            = Array.Empty<SimulationIntegratedLotSnapshot>();
        public SimulationIntegratedReservationSnapshot[] Reservations { get; set; }
            = Array.Empty<SimulationIntegratedReservationSnapshot>();
        public SimulationWorldEffectSnapshot[] WorldEffects { get; set; }
            = Array.Empty<SimulationWorldEffectSnapshot>();
        public SimulationPendingWorldEffectEntrySnapshot[] PendingWorldEffects { get; set; }
            = Array.Empty<SimulationPendingWorldEffectEntrySnapshot>();
        public SimulationAppliedWorldEffectReceiptSnapshot[] AppliedWorldEffectReceipts { get; set; }
            = Array.Empty<SimulationAppliedWorldEffectReceiptSnapshot>();
        public SimulationFacilityRepairJobSnapshot[] RepairJobs { get; set; }
            = Array.Empty<SimulationFacilityRepairJobSnapshot>();
        public SimulationIntegratedCargoMovementSnapshot[] CargoMovements { get; set; }
            = Array.Empty<SimulationIntegratedCargoMovementSnapshot>();
    }

    public sealed class SimulationRuntimeFacilitySnapshot
    {
        public string FacilityStableId { get; set; } = string.Empty;
        public string FacilityDefinitionStableId { get; set; } = string.Empty;
        public string FacilityDefinitionRevision { get; set; } = string.Empty;
        public string FacilityDefinitionHashSha256 { get; set; } = string.Empty;
        public string PlacementH1StableId { get; set; } = string.Empty;
        public string SettlementFacilityTypeCode { get; set; } = string.Empty;
        public string SettlementDistrictStableId { get; set; } = string.Empty;
        public string[] AccessConnectorStableIds { get; set; } = Array.Empty<string>();
        public string LifecycleCode { get; set; } = SimulationFacilityLifecycleCodes.Operational;
        public string IntegrityCode { get; set; } = SimulationFacilityIntegrityCodes.Intact;
        public string MaintenanceCode { get; set; } = SimulationFacilityMaintenanceCodes.None;
        public string[] DefinedCapabilityCodes { get; set; } = Array.Empty<string>();
        public SimulationRuntimeFacilityCapacitySnapshot[] DefinedCapacities { get; set; }
            = Array.Empty<SimulationRuntimeFacilityCapacitySnapshot>();
        public SimulationEffectiveFacilityCapabilitySnapshot[] EffectiveCapabilities { get; set; }
            = Array.Empty<SimulationEffectiveFacilityCapabilitySnapshot>();
    }

    public sealed class SimulationRuntimeFacilityCapacitySnapshot
    {
        public string CapacityCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
    }

    public sealed class SimulationEffectiveFacilityCapabilitySnapshot
    {
        public string CapabilityCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationFacilityCapabilityStateCodes.Active;
        public string[] SourceRestrictionStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationFacilityRestrictionSnapshot
    {
        public string RestrictionStableId { get; set; } = string.Empty;
        public string SourceEffectStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public string RestrictionLevelCode { get; set; } = string.Empty;
        public string ResolvedByEffectStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationManufacturingJobSnapshot
    {
        public string ManufacturingJobStableId { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string RecipeRevision { get; set; } = string.Empty;
        public string RecipeHashSha256 { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int ProcessingStartsAtTick { get; set; }
        public int ProcessingCompletesAtTick { get; set; }
        public SimulationIntegratedItemRequirement[] ResolvedInputRequirements { get; set; }
            = Array.Empty<SimulationIntegratedItemRequirement>();
        public SimulationIntegratedItemRequirement[] ResolvedOutputSpecification { get; set; }
            = Array.Empty<SimulationIntegratedItemRequirement>();
        public string[] ReservedInputLotStableIds { get; set; } = Array.Empty<string>();
        public string[] ConsumedInputLotStableIds { get; set; } = Array.Empty<string>();
        public string[] OutputLotStableIds { get; set; } = Array.Empty<string>();
        public string ActorStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationConstructionProjectSnapshot
    {
        public string ConstructionProjectStableId { get; set; } = string.Empty;
        public string BlueprintStableId { get; set; } = string.Empty;
        public string BlueprintRevision { get; set; } = string.Empty;
        public string BlueprintHashSha256 { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string TargetFacilityStableId { get; set; } = string.Empty;
        public string BuildSiteH1StableId { get; set; } = string.Empty;
        public int ConstructionStartsAtTick { get; set; }
        public int ConstructionCompletesAtTick { get; set; }
        public SimulationIntegratedItemRequirement[] ResolvedMaterialRequirements { get; set; }
            = Array.Empty<SimulationIntegratedItemRequirement>();
        public string[] ReservedMaterialLotStableIds { get; set; } = Array.Empty<string>();
        public string[] ConsumedMaterialLotStableIds { get; set; } = Array.Empty<string>();
        public string ActorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationIntegratedActorSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public int EligibilityRank { get; set; }
        public bool FarmLaborEligible { get; set; }
    }

    public sealed class SimulationFormationSnapshot
    {
        public string FormationStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public string GarrisonFacilityStableId { get; set; } = string.Empty;
        public int? StateCompletesAtTick { get; set; }
    }

    public sealed class SimulationActorCommitmentSnapshot
    {
        public string CommitmentStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string CommitmentCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    public sealed class SimulationActorInjurySnapshot
    {
        public string InjuryStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string SourceEffectStableId { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    public sealed class SimulationIntegratedLotSnapshot
    {
        public string LotStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationIntegratedReservationSnapshot
    {
        public string ReservationStableId { get; set; } = string.Empty;
        public string OwnerStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ReservationKindCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string StateCode { get; set; } = "Reserved";
    }

    public sealed class SimulationWorldEffectSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string PayloadCanonical { get; set; } = string.Empty;
    }

    public sealed class SimulationPendingWorldEffectEntrySnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public int EarliestWorldTick { get; set; }
    }

    public sealed class SimulationAppliedWorldEffectReceiptSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public long AppliedWorldRevision { get; set; }
    }

    public sealed class SimulationFacilityRepairJobSnapshot
    {
        public string RepairJobStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string[] TargetRestrictionStableIds { get; set; } = Array.Empty<string>();
        public string[] ReservedMaterialLotStableIds { get; set; } = Array.Empty<string>();
        public int CompletesAtTick { get; set; }
        public string StateCode { get; set; } = "Repairing";
    }

    public sealed class SimulationIntegratedCargoMovementSnapshot
    {
        public string MovementStableId { get; set; } = string.Empty;
        public string SourceLotStableId { get; set; } = string.Empty;
        public string TargetFacilityStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public int CompletesAtTick { get; set; }
        public string StateCode { get; set; } = "InTransit";
        public string OutputLotStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleRelevantRuntimeProjectionSnapshot
    {
        public string EncounterScopeStableId { get; set; } = string.Empty;
        public SimulationRuntimeFacilitySnapshot[] Facilities { get; set; }
            = Array.Empty<SimulationRuntimeFacilitySnapshot>();
        public SimulationFormationSnapshot[] Formations { get; set; }
            = Array.Empty<SimulationFormationSnapshot>();
        public string[] BattleAvailableActorStableIds { get; set; } = Array.Empty<string>();
        public string BattleRelevantOverlayHashSha256 { get; set; } = string.Empty;
    }
}
