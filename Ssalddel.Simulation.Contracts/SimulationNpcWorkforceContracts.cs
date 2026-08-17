using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationNpcCapabilityCodes
    {
        public const string WorkforceDelegate = "Simulation.WorkforceDelegate";
        public const string WarehouseInboundInspection = "Warehouse.InboundInspection";
        public const string WarehouseStorageMove = "Warehouse.StorageMove";
        public const string FreightTransport = "Freight.Transport";
        public const string RestaurantIngredientReceive = "Restaurant.IngredientReceive";
        public const string RestaurantCooking = "Restaurant.Cooking";
    }

    public static class SimulationNpcActionCodes
    {
        public const string WarehouseInboundInspection = "WarehouseInboundInspection";
        public const string WarehouseStorageMove = "WarehouseStorageMove";
        public const string FreightTransport = "FreightTransport";
        public const string RestaurantIngredientReceive = "RestaurantIngredientReceive";
        public const string RestaurantCooking = "RestaurantCooking";
    }

    public static class SimulationNpcActionPhaseCodes
    {
        public const string Scheduled = "Scheduled";
        public const string Navigating = "Navigating";
        public const string Working = "Working";
        public const string Completed = "Completed";
        public const string Blocked = "Blocked";
        public const string Cancelled = "Cancelled";
    }

    public static class SimulationNpcGrantKindCodes
    {
        public const string Initial = "Initial";
        public const string Delegated = "Delegated";
    }

    public static class SimulationNpcInventoryStateCodes
    {
        public const string PendingInspection = "PendingInspection";
        public const string StorageEligible = "StorageEligible";
        public const string PutAwayCompleted = "PutAwayCompleted";
        public const string OutboundRequested = "OutboundRequested";
        public const string Picked = "Picked";
        public const string OutboundReady = "OutboundReady";
        public const string MarketReceived = "MarketReceived";
        public const string MarketStorageEligible = "MarketStorageEligible";
        public const string MarketBackroomStored = "MarketBackroomStored";
        public const string Displayed = "Displayed";
    }

    public sealed class SimulationNpcWorkforceInitialStateRequest
    {
        public SimulationNpcOrganizationInitialRequest[] Organizations { get; set; }
            = Array.Empty<SimulationNpcOrganizationInitialRequest>();
        public SimulationNpcActorInitialRequest[] Actors { get; set; }
            = Array.Empty<SimulationNpcActorInitialRequest>();
        public SimulationNpcCapabilityGrantInitialRequest[] CapabilityGrants { get; set; }
            = Array.Empty<SimulationNpcCapabilityGrantInitialRequest>();
        public SimulationNpcWorkPolicyInitialRequest[] Policies { get; set; }
            = Array.Empty<SimulationNpcWorkPolicyInitialRequest>();
    }

    public sealed class SimulationNpcOrganizationInitialRequest
    {
        public string OrganizationStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string[] FacilityStableIds { get; set; } = Array.Empty<string>();
        public string[] AllowedCapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcActorInitialRequest
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string OrganizationStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HomeFacilityStableId { get; set; } = string.Empty;
        public string ReferenceRoleCode { get; set; } = string.Empty;
        public int MaximumConcurrentTasks { get; set; } = 1;
        public string[] AssignableCapabilityCodes { get; set; } = Array.Empty<string>();
        public SimulationNpcSkillInitialRequest[] Skills { get; set; }
            = Array.Empty<SimulationNpcSkillInitialRequest>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcSkillInitialRequest
    {
        public string CapabilityCode { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public sealed class SimulationNpcCapabilityGrantInitialRequest
    {
        public string GrantStableId { get; set; } = string.Empty;
        public string OrganizationStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public string GrantedByActorStableId { get; set; } = string.Empty;
        public bool CanDelegate { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcWorkPolicyInitialRequest
    {
        public string PolicyStableId { get; set; } = string.Empty;
        public string OrganizationStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string RequiredCapabilityCode { get; set; } = string.Empty;
        public bool AutomationEnabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public string PreferredActorStableId { get; set; } = string.Empty;
        public bool AutoDelegationEnabled { get; set; } = true;
        public int AutoDelegationBacklogThreshold { get; set; } = 2;
        public int TravelDurationTicks { get; set; } = 1;
        public int WorkDurationTicks { get; set; } = 2;
        public string InteractionPointKey { get; set; } = string.Empty;
        public string ActionVisualKey { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcPolicyChangeRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string PolicyStableId { get; set; } = string.Empty;
        public bool AutomationEnabled { get; set; }
        public int Priority { get; set; } = 100;
        public string PreferredActorStableId { get; set; } = string.Empty;
        public bool AutoDelegationEnabled { get; set; }
    }

    public sealed class SimulationNpcOrganizationSnapshot
    {
        public string OrganizationStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string[] FacilityStableIds { get; set; } = Array.Empty<string>();
        public string[] AllowedCapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcActorSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string OrganizationStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HomeFacilityStableId { get; set; } = string.Empty;
        public string ReferenceRoleCode { get; set; } = string.Empty;
        public int MaximumConcurrentTasks { get; set; }
        public string[] AssignableCapabilityCodes { get; set; } = Array.Empty<string>();
        public SimulationNpcSkillSnapshot[] Skills { get; set; }
            = Array.Empty<SimulationNpcSkillSnapshot>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcSkillSnapshot
    {
        public string CapabilityCode { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public sealed class SimulationNpcCapabilityGrantSnapshot
    {
        public string GrantStableId { get; set; } = string.Empty;
        public string OrganizationStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public string GrantedByActorStableId { get; set; } = string.Empty;
        public string GrantKindCode { get; set; } = string.Empty;
        public bool CanDelegate { get; set; }
        public bool Active { get; set; }
        public int GrantedTick { get; set; }
        public long Revision { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcWorkPolicySnapshot
    {
        public string PolicyStableId { get; set; } = string.Empty;
        public string OrganizationStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string RequiredCapabilityCode { get; set; } = string.Empty;
        public bool AutomationEnabled { get; set; }
        public int Priority { get; set; }
        public string PreferredActorStableId { get; set; } = string.Empty;
        public bool AutoDelegationEnabled { get; set; }
        public int AutoDelegationBacklogThreshold { get; set; }
        public int TravelDurationTicks { get; set; }
        public int WorkDurationTicks { get; set; }
        public string InteractionPointKey { get; set; } = string.Empty;
        public string ActionVisualKey { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcTaskAssignmentSnapshot
    {
        public string AssignmentStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string PolicyStableId { get; set; } = string.Empty;
        public string OrganizationStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string RequiredCapabilityCode { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public int AssignedTick { get; set; }
        public int PhaseStartedTick { get; set; }
        public int TravelDurationTicks { get; set; }
        public int WorkDurationTicks { get; set; }
        public int? CompletedTick { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public long Revision { get; set; }
    }

    public sealed class SimulationNpcWorkRecordSnapshot
    {
        public string WorkRecordStableId { get; set; } = string.Empty;
        public string AssignmentStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public int StartedTick { get; set; }
        public int CompletedTick { get; set; }
        public string[] ResultCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNpcActionProjection
    {
        public string ProjectionStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string InteractionPointKey { get; set; } = string.Empty;
        public string ActionVisualKey { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public decimal ProgressRate { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public long Revision { get; set; }
        public int WorldTick { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class SimulationNpcFacilityInventorySnapshot
    {
        public string InventoryStableId { get; set; } = string.Empty;
        public string LotStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string SourceTaskStableId { get; set; } = string.Empty;
        public int UpdatedTick { get; set; }
        public long Revision { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
