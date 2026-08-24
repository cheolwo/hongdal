using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationDecisionStateCodes
    {
        public const string Previewed = "Previewed";
        public const string Confirmed = "Confirmed";
        public const string Cancelled = "Cancelled";
    }

    public static class SimulationTaskStateCodes
    {
        public const string Scheduled = "Scheduled";
        public const string InProgress = "InProgress";
        public const string Blocked = "Blocked";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    public static class SimulationEffectStateCodes
    {
        public const string Pending = "Pending";
        public const string Applied = "Applied";
        public const string Cancelled = "Cancelled";
    }

    public sealed class SimulationDecisionPreviewRequest
    {
        public string DecisionStableId { get; set; } = string.Empty;
        public string DecisionTypeCode { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string[] TargetStableIds { get; set; } = Array.Empty<string>();
        public SimulationValueProjection[] ExpectedCosts { get; set; }
            = Array.Empty<SimulationValueProjection>();
        public SimulationValueProjection[] ExpectedEffects { get; set; }
            = Array.Empty<SimulationValueProjection>();
        public string[] Uncertainties { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public SimulationTaskPlanRequest Task { get; set; } = new SimulationTaskPlanRequest();
    }

    public sealed class SimulationTaskPlanRequest
    {
        public string TaskStableId { get; set; } = string.Empty;
        public string TaskTypeCode { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignedActorStableId { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
        public string PreferredOriginSpatialStableId { get; set; } = string.Empty;
        public string PreferredRouteSpatialStableId { get; set; } = string.Empty;
        public string PreferredDestinationSpatialStableId { get; set; } = string.Empty;
        public string RouteStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public decimal AssignedCapacity { get; set; }
        public string AssignedCapacityUnitCode { get; set; } = string.Empty;
        public int DurationTicks { get; set; } = 1;
        public string[] InputLotStableIds { get; set; } = Array.Empty<string>();
        public string[] OutputCandidateCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationValueProjection
    {
        public string ValueTypeCode { get; set; } = string.Empty;
        public string TargetLedgerStableId { get; set; } = string.Empty;
        public decimal BeforeValue { get; set; }
        public decimal Delta { get; set; }
        public decimal AfterValue { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationDecisionPreviewSnapshot
    {
        public SimulationDecisionSnapshot Decision { get; set; } = new SimulationDecisionSnapshot();
        public SimulationTaskPlanSnapshot TaskPlan { get; set; } = new SimulationTaskPlanSnapshot();
        public Simulation공간상호작용PreviewSnapshot? SpatialInteraction { get; set; }
    }

    public sealed class SimulationDecisionConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationDecisionPreviewRequest Preview { get; set; }
            = new SimulationDecisionPreviewRequest();
    }

    public sealed class SimulationDecisionSnapshot
    {
        public string DecisionStableId { get; set; } = string.Empty;
        public string DecisionTypeCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string SessionStableId { get; set; } = string.Empty;
        public string FactionStableId { get; set; } = string.Empty;
        public string TerritoryStableId { get; set; } = string.Empty;
        public string SettlementStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string[] TargetStableIds { get; set; } = Array.Empty<string>();
        public int CreatedTick { get; set; }
        public int? ConfirmedTick { get; set; }
        public SimulationValueProjection[] ExpectedCosts { get; set; }
            = Array.Empty<SimulationValueProjection>();
        public SimulationValueProjection[] ExpectedEffects { get; set; }
            = Array.Empty<SimulationValueProjection>();
        public string[] Uncertainties { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationTaskPlanSnapshot
    {
        public string TaskStableId { get; set; } = string.Empty;
        public string TaskTypeCode { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignedActorStableId { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
        public string PreferredOriginSpatialStableId { get; set; } = string.Empty;
        public string PreferredRouteSpatialStableId { get; set; } = string.Empty;
        public string PreferredDestinationSpatialStableId { get; set; } = string.Empty;
        public string RouteStableId { get; set; } = string.Empty;
        public string DestinationFacilityStableId { get; set; } = string.Empty;
        public string SelectedSpatialStableId { get; set; } = string.Empty;
        public string SpatialDefinitionRevision { get; set; } = string.Empty;
        public string SpatialDefinitionHashSha256 { get; set; } = string.Empty;
        public Simulation공간역할BindingSnapshot[] SpatialRoleBindings { get; set; }
            = Array.Empty<Simulation공간역할BindingSnapshot>();
        public decimal AssignedCapacity { get; set; }
        public string AssignedCapacityUnitCode { get; set; } = string.Empty;
        public int DurationTicks { get; set; }
        public string[] InputLotStableIds { get; set; } = Array.Empty<string>();
        public string[] OutputCandidateCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationTaskSnapshot
    {
        public string TaskStableId { get; set; } = string.Empty;
        public string TaskTypeCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CausedByDecisionStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string AssignedActorStableId { get; set; } = string.Empty;
        public string SelectedSpatialStableId { get; set; } = string.Empty;
        public string SpatialDefinitionRevision { get; set; } = string.Empty;
        public string SpatialDefinitionHashSha256 { get; set; } = string.Empty;
        public Simulation공간역할BindingSnapshot[] SpatialRoleBindings { get; set; }
            = Array.Empty<Simulation공간역할BindingSnapshot>();
        public decimal AssignedCapacity { get; set; }
        public string AssignedCapacityUnitCode { get; set; } = string.Empty;
        public int ScheduledStartTick { get; set; }
        public int ExpectedEndTick { get; set; }
        public int? ActualEndTick { get; set; }
        public string[] InputLotStableIds { get; set; } = Array.Empty<string>();
        public string[] OutputCandidateCodes { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationEffectRecord
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string EffectTypeCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public int? AppliedTick { get; set; }
        public string CausedByDecisionStableId { get; set; } = string.Empty;
        public string CausedByTaskStableId { get; set; } = string.Empty;
        public string TargetLedgerStableId { get; set; } = string.Empty;
        public decimal BeforeValue { get; set; }
        public decimal Delta { get; set; }
        public decimal AfterValue { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
