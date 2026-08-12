using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation수출Cargo인계상태Codes
    {
        public const string Scheduled = "Scheduled";
        public const string HandedOffInSimulation = "HandedOffInSimulation";
    }

    public sealed class Simulation수출Cargo인계PreviewRequest
    {
        public string HandoffStableId { get; set; } = string.Empty;
        public string SourceCargoPreparationStableId { get; set; } = string.Empty;
        public string ReceivingFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public int RequiredHandoffTicks { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수출Cargo인계ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation수출Cargo인계PreviewRequest Handoff { get; set; }
            = new Simulation수출Cargo인계PreviewRequest();
    }

    public sealed class Simulation수출Cargo인계PreviewSnapshot
    {
        public string HandoffStableId { get; set; } = string.Empty;
        public string SourceCargoPreparationStableId { get; set; } = string.Empty;
        public string SourceExportPreparationStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ReceivingFacilityStableId { get; set; } = string.Empty;
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotCreateLogisticsMovement { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation수출Cargo인계Snapshot
    {
        public string HandoffStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = Simulation수출Cargo인계상태Codes.Scheduled;
        public long Revision { get; set; }
        public string SourceCargoPreparationStableId { get; set; } = string.Empty;
        public string SourceExportPreparationStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ReceivingFacilityStableId { get; set; } = string.Empty;
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int RequiredHandoffTicks { get; set; }
        public int ScheduledTick { get; set; }
        public int? CompletedTick { get; set; }
        public string? LogisticsMovementCargoStableId { get; set; }
        public string? LogisticsMovementTaskStableId { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
