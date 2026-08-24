using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationCoopConstructionCodes
    {
        public const string RuleRevision = "coop-construction.v1";
        public const string ProtectionRuleRevision = "world-protection-policy.v1";
        public const string Contribution = "CoopConstructionContribution";
        public const string Demolition = "CoopFacilityDemolition";
        public const string Restore = "CoopFacilityRestore";
        public const string Planned = "Planned";
        public const string Foundation = "Foundation";
        public const string Frame = "Frame";
        public const string Finishing = "Finishing";
        public const string Operational = "Operational";
        public const string Removed = "Removed";
        public const string Reserved = "Reserved";
        public const string Consumed = "Consumed";
        public const string HostedSessionProtection = "HostedSessionProtection";
        public const string DestructiveActionCheckpoint = "DestructiveActionCheckpoint";
        public const string CompensatingRestore = "CompensatingRestore";
        public const string FarmSmallStorageProject = "coop-project:farm-small-storage.v1";
        public const string FarmSmallStorageBlueprint = "blueprint:farm-coop-storage.v1";
        public const string FarmSmallStorageBuildSite = "h1:Farm:coop-storage-build-site";
    }

    public sealed class SimulationCoopSourceLotStateSnapshot
    {
        public string LotStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
    }

    public sealed class SimulationCoopContributionSnapshot
    {
        public string ContributionStableId { get; set; } = string.Empty;
        public string ProjectStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string SourceLotStableId { get; set; } = string.Empty;
        public long SourceLotRevisionBefore { get; set; }
        public decimal MaterialQuantity { get; set; }
        public decimal EffectiveWork { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
    }

    public sealed class SimulationCoopConstructionProjectSnapshot
    {
        public string ProjectStableId { get; set; } = string.Empty;
        public string BlueprintStableId { get; set; } = string.Empty;
        public string BuildSiteH1StableId { get; set; } = string.Empty;
        public string TargetFacilityStableId { get; set; } = string.Empty;
        public string StageCode { get; set; } = string.Empty;
        public decimal RequiredMaterialQuantity { get; set; }
        public decimal ContributedMaterialQuantity { get; set; }
        public decimal ProgressValue { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public int? CompletedWorldTick { get; set; }
        public string[] OpenedCapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] OpenedWorldInteractionIds { get; set; } = Array.Empty<string>();
        public string ProjectHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldProtectionCheckpointSnapshot
    {
        public string CheckpointStableId { get; set; } = string.Empty;
        public string CheckpointKindCode { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string[] TargetStableIds { get; set; } = Array.Empty<string>();
        public long BeforeWorldRevision { get; set; }
        public string SpatialStateHashSha256 { get; set; } = string.Empty;
        public string[] RelatedResourceRefs { get; set; } = Array.Empty<string>();
        public string[] RelatedConnectorRefs { get; set; } = Array.Empty<string>();
        public string CreatedByActionRequestId { get; set; } = string.Empty;
        public bool HistoricalEffectsDeleted { get; set; }
    }

    public sealed class SimulationWorldRestoreEffectSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string CheckpointStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string EffectTypeCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public bool DeletesHistoricalEffects { get; set; }
        public bool DuplicatesResources { get; set; }
    }

    public sealed class SimulationCoopConstructionStateSnapshot
    {
        public string RuleRevision { get; set; } = SimulationCoopConstructionCodes.RuleRevision;
        public string ProtectionRuleRevision { get; set; }
            = SimulationCoopConstructionCodes.ProtectionRuleRevision;
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public SimulationCoopConstructionProjectSnapshot[] Projects { get; set; }
            = Array.Empty<SimulationCoopConstructionProjectSnapshot>();
        public SimulationCoopContributionSnapshot[] Contributions { get; set; }
            = Array.Empty<SimulationCoopContributionSnapshot>();
        public SimulationCoopSourceLotStateSnapshot[] SourceLots { get; set; }
            = Array.Empty<SimulationCoopSourceLotStateSnapshot>();
        public SimulationWorldProtectionCheckpointSnapshot[] ProtectionCheckpoints { get; set; }
            = Array.Empty<SimulationWorldProtectionCheckpointSnapshot>();
        public SimulationWorldRestoreEffectSnapshot[] RestoreEffects { get; set; }
            = Array.Empty<SimulationWorldRestoreEffectSnapshot>();
        public bool UsesCompensatingEffects { get; set; } = true;
        public bool MutatesStaticHDefinitions { get; set; }
        public string StateHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public class SimulationCoopContributionPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string ProjectStableId { get; set; } = string.Empty;
        public string BlueprintStableId { get; set; } = string.Empty;
        public string BuildSiteH1StableId { get; set; } = string.Empty;
        public string SourceLotStableId { get; set; } = string.Empty;
        public long ExpectedSourceLotRevision { get; set; }
        public decimal RequestedQuantity { get; set; }
    }

    public sealed class SimulationCoopContributionConfirmRequest
        : SimulationCoopContributionPreviewRequest
    {
        public string CommandId { get; set; } = string.Empty;
    }

    public sealed class SimulationCoopConstructionPreviewSnapshot
    {
        public long BaseRevision { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string ProjectStableId { get; set; } = string.Empty;
        public string SourceLotStableId { get; set; } = string.Empty;
        public long SourceLotRevision { get; set; }
        public decimal OfferedQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RemainingRequiredQuantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string CurrentStageCode { get; set; } = string.Empty;
        public string ProjectedStageCode { get; set; } = string.Empty;
        public int DurationTicks { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string PreviewHashSha256 { get; set; } = string.Empty;
    }

    public class SimulationCoopProtectedActionPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string OwnerPlayerStableId { get; set; } = string.Empty;
        public string ProjectStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationCoopProtectedActionConfirmRequest
        : SimulationCoopProtectedActionPreviewRequest
    {
        public string CommandId { get; set; } = string.Empty;
    }
}
