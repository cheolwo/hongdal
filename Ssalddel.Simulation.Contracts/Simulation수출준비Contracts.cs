using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation수출준비검사결과Codes
    {
        public const string Passed = "Passed";
        public const string Failed = "Failed";
    }

    public static class Simulation수출준비상태Codes
    {
        public const string Scheduled = "Scheduled";
        public const string Packaging = "Packaging";
        public const string ReworkScheduled = "ReworkScheduled";
        public const string Reworking = "Reworking";
        public const string Inspection = "Inspection";
        public const string HandoffCandidateReady = "HandoffCandidateReady";
        public const string ReworkRequired = "ReworkRequired";
    }

    public sealed class Simulation수출준비PreviewRequest
    {
        public string PreparationStableId { get; set; } = string.Empty;
        public string? PreviousPreparationStableId { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string PackingFacilityStableId { get; set; } = string.Empty;
        public string HandoffFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public int PackagingTicks { get; set; }
        public int InspectionTicks { get; set; }
        public string InspectionOutcomeCode { get; set; } = string.Empty;
        public string? FailureReasonCode { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수출준비ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation수출준비PreviewRequest Preparation { get; set; }
            = new Simulation수출준비PreviewRequest();
    }

    public sealed class Simulation수출재작업PreviewRequest
    {
        public string FailedPreparationStableId { get; set; } = string.Empty;
        public string RetryPreparationStableId { get; set; } = string.Empty;
        public string ReworkFacilityStableId { get; set; } = string.Empty;
        public string HandoffFacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public int ReworkTicks { get; set; }
        public int InspectionTicks { get; set; }
        public string InspectionOutcomeCode { get; set; } = string.Empty;
        public string? FailureReasonCode { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation수출재작업ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public Simulation수출재작업PreviewRequest Rework { get; set; }
            = new Simulation수출재작업PreviewRequest();
    }

    public sealed class Simulation수출준비PreviewSnapshot
    {
        public string PreparationStableId { get; set; } = string.Empty;
        public string RootPreparationStableId { get; set; } = string.Empty;
        public string? PreviousPreparationStableId { get; set; }
        public int AttemptNumber { get; set; }
        public bool IsReworkAttempt { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string InspectionOutcomeCode { get; set; } = string.Empty;
        public string PackageLotCandidateStableId { get; set; } = string.Empty;
        public string HandoffCandidateStableId { get; set; } = string.Empty;
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotCreateOperationalExport { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public SimulationDecisionPreviewSnapshot CommonDecisionPreview { get; set; }
            = new SimulationDecisionPreviewSnapshot();
    }

    public sealed class Simulation수출준비Snapshot
    {
        public string PreparationStableId { get; set; } = string.Empty;
        public string RootPreparationStableId { get; set; } = string.Empty;
        public string? PreviousPreparationStableId { get; set; }
        public int AttemptNumber { get; set; }
        public bool IsReworkAttempt { get; set; }
        public string StateCode { get; set; } = Simulation수출준비상태Codes.Scheduled;
        public long Revision { get; set; }
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string PackingFacilityStableId { get; set; } = string.Empty;
        public string HandoffFacilityStableId { get; set; } = string.Empty;
        public string PackageLotCandidateStableId { get; set; } = string.Empty;
        public string HandoffCandidateStableId { get; set; } = string.Empty;
        public string InspectionOutcomeCode { get; set; } = string.Empty;
        public string? FailureReasonCode { get; set; }
        public string DecisionStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public int PackagingTicks { get; set; }
        public int InspectionTicks { get; set; }
        public int ReservedTick { get; set; }
        public int? PackagedTick { get; set; }
        public int? InspectedTick { get; set; }
        public int? HandoffCandidateReadyTick { get; set; }
        public bool CanRetry { get; set; }
        public string? CargoPreparationStableId { get; set; }
        public string? CargoStableId { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
