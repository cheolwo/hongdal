using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation수출Cargo준비상태Codes
    {
        public const string Scheduled = "Scheduled";
        public const string ReadyForHandoff = "ReadyForHandoff";
        public const string HandoffScheduled = "HandoffScheduled";
        public const string HandedOffInSimulation = "HandedOffInSimulation";
    }

    public sealed class Simulation수출Cargo준비PreviewRequest
    {
        public string CargoPreparationStableId { get; set; } = string.Empty;
        public string SourceExportPreparationStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public string RouteStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public int RequiredPreparationTicks { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수출Cargo준비ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation수출Cargo준비PreviewRequest CargoPreparation { get; set; }
            = new Simulation수출Cargo준비PreviewRequest();
    }

    public sealed class Simulation수출Cargo준비PreviewSnapshot
    {
        public string CargoPreparationStableId { get; set; } = string.Empty;
        public string SourceExportPreparationStableId { get; set; } = string.Empty;
        public string RootExportPreparationStableId { get; set; } = string.Empty;
        public int ExportPreparationAttemptNumber { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public string RouteStableId { get; set; } = string.Empty;
        public string OriginFacilityStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotCreateOperationalHandoff { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation수출Cargo준비Snapshot
    {
        public string CargoPreparationStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = Simulation수출Cargo준비상태Codes.Scheduled;
        public long Revision { get; set; }
        public string SourceExportPreparationStableId { get; set; } = string.Empty;
        public string RootExportPreparationStableId { get; set; } = string.Empty;
        public int ExportPreparationAttemptNumber { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public long CargoRevision { get; set; }
        public string RouteStableId { get; set; } = string.Empty;
        public string OriginFacilityStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int RequiredPreparationTicks { get; set; }
        public int ScheduledTick { get; set; }
        public int? ReadyForHandoffTick { get; set; }
        public string? HandoffStableId { get; set; }
        public int? HandoffCompletedTick { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
