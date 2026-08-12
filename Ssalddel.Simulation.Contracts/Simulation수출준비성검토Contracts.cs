using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation수출준비성검토상태Codes
    {
        public const string Scheduled = "Scheduled";
        public const string ReadyCandidate = "ReadyCandidate";
        public const string ActionRequired = "ActionRequired";
    }

    public static class Simulation수출준비성검토결과Codes
    {
        public const string ReadyCandidate = "ReadyCandidate";
        public const string ActionRequired = "ActionRequired";
    }

    public static class Simulation수출준비성보완Codes
    {
        public const string DocumentsNotPrepared = "DocumentsNotPrepared";
        public const string InspectionPreparationNotReady = "InspectionPreparationNotReady";
    }

    public sealed class Simulation수출준비성검토PreviewRequest
    {
        public string ReviewStableId { get; set; } = string.Empty;
        public string SourcePortReceiptStableId { get; set; } = string.Empty;
        public string ReviewingFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public bool DocumentsPrepared { get; set; }
        public bool InspectionPreparationReady { get; set; }
        public int RequiredReviewTicks { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수출준비성검토ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation수출준비성검토PreviewRequest Review { get; set; }
            = new Simulation수출준비성검토PreviewRequest();
    }

    public sealed class Simulation수출준비성검토PreviewSnapshot
    {
        public string ReviewStableId { get; set; } = string.Empty;
        public string SourcePortReceiptStableId { get; set; } = string.Empty;
        public string? ParentReviewStableId { get; set; }
        public int AttemptNumber { get; set; }
        public string CargoStableId { get; set; } = string.Empty;
        public string SourceExportCargoHandoffStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ReviewingFacilityStableId { get; set; } = string.Empty;
        public bool DocumentsPrepared { get; set; }
        public bool InspectionPreparationReady { get; set; }
        public string OutcomeCode { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } = Array.Empty<string>();
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotCreateOperationalExport { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation수출준비성검토Snapshot
    {
        public string ReviewStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = Simulation수출준비성검토상태Codes.Scheduled;
        public long Revision { get; set; }
        public string SourcePortReceiptStableId { get; set; } = string.Empty;
        public string? ParentReviewStableId { get; set; }
        public int AttemptNumber { get; set; }
        public string CargoStableId { get; set; } = string.Empty;
        public string SourceExportCargoHandoffStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ReviewingFacilityStableId { get; set; } = string.Empty;
        public bool DocumentsPrepared { get; set; }
        public bool InspectionPreparationReady { get; set; }
        public string OutcomeCode { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } = Array.Empty<string>();
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int RequiredReviewTicks { get; set; }
        public int ScheduledTick { get; set; }
        public int? CompletedTick { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
