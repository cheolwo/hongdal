using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation공간능력Codes
    {
        public const string Storage = "Spatial.Storage";
        public const string CargoAccessible = "Spatial.CargoAccessible";
        public const string WorkerAccessible = "Spatial.WorkerAccessible";
        public const string DroppedTimberPickupAnchor =
            "Spatial.DroppedTimberPickupAnchor";
        public const string InspectionWorkArea = "Spatial.InspectionWorkArea";
        public const string LoadingWorkArea = "Spatial.LoadingWorkArea";
        public const string CropProduction = "Spatial.CropProduction";
        public const string TillingWorkArea = "Spatial.TillingWorkArea";
        public const string SowingWorkArea = "Spatial.SowingWorkArea";
        public const string CropCareWorkArea = "Spatial.CropCareWorkArea";
        public const string WaterAccessible = "Spatial.WaterAccessible";
        public const string HarvestWorkArea = "Spatial.HarvestWorkArea";
        public const string CollectionWorkArea = "Spatial.CollectionWorkArea";
        public const string PackingWorkArea = "Spatial.PackingWorkArea";
        public const string VehicleAccessible = "Spatial.VehicleAccessible";
        public const string CargoRoute = "Spatial.CargoRoute";
        public const string UnloadingWorkArea = "Spatial.UnloadingWorkArea";
        public const string PickingWorkArea = "Spatial.PickingWorkArea";
        public const string OutboundStagingArea =
            "Spatial.OutboundStagingArea";
        public const string DisplayArea = "Spatial.DisplayArea";
        public const string CustomerAccessible = "Spatial.CustomerAccessible";
        public const string PickupArea = "Spatial.PickupArea";
        public const string RepairWorkArea = "Spatial.RepairWorkArea";
        public const string Traversable = "Spatial.Traversable";
        public const string ObservationArea = "Spatial.ObservationArea";
        public const string ThreatMonitoringArea = "Spatial.ThreatMonitoringArea";
        public const string EmergencyAccess = "Spatial.EmergencyAccess";
        public const string PlayerEscapeRoute = "Spatial.PlayerEscapeRoute";
        public const string SafeCore = "Spatial.SafeCore";
        public const string RestorationWorkArea = "Spatial.RestorationWorkArea";
        public const string RestArea = "Spatial.RestArea";
    }

    public static class Simulation공간근거종류Codes
    {
        public const string Scenario = "Scenario";
        public const string LandscapeGraph = "LandscapeGraph";
    }

    public static class Simulation공간접근상태Codes
    {
        public const string Available = "Available";
        public const string Unavailable = "Unavailable";
    }

    public static class Simulation공간용량Codes
    {
        public const string StorageCapacity = "StorageCapacity";
        public const string WorkArea = "WorkArea";
        public const string EscapeRouteCapacity = "EscapeRouteCapacity";
        public const string RestorationMaterial = "RestorationMaterial";
        public const string RestAreaParty = "RestAreaParty";
        public const string RecoverySupply = "RecoverySupply";
    }

    public static class Simulation공간예약상태Codes
    {
        public const string Reserved = "Reserved";
        public const string Consumed = "Consumed";
        public const string Released = "Released";
        public const string Cancelled = "Cancelled";
    }

    public static class Simulation공간차단Codes
    {
        public const string DefinitionUnavailable = "SimulationSpatialDefinitionUnavailable";
        public const string CapabilityMissing = "SimulationSpatialCapabilityMissing";
        public const string CapacityInsufficient = "SimulationSpatialCapacityInsufficient";
        public const string AccessUnavailable = "SimulationSpatialAccessUnavailable";
        public const string ReservationConflict = "SimulationSpatialReservationConflict";
    }

    public static class Simulation공간역할Codes
    {
        public const string Primary = "Primary";
        public const string OriginLoading = "OriginLoading";
        public const string TransportRoute = "TransportRoute";
        public const string DestinationUnloading = "DestinationUnloading";
    }

    public sealed class Simulation공간세계InitialStateRequest
    {
        public Simulation공간정의InitialRequest[] Definitions { get; set; }
            = Array.Empty<Simulation공간정의InitialRequest>();
    }

    public sealed class Simulation공간정의InitialRequest
    {
        public string SpatialStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public string LandscapeNodeStableId { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string AccessStateCode { get; set; } = Simulation공간접근상태Codes.Available;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public Simulation공간용량Snapshot[] BaseCapacities { get; set; }
            = Array.Empty<Simulation공간용량Snapshot>();
        public string DefinitionRevision { get; set; } = string.Empty;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation공간용량Snapshot
    {
        public string CapacityCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
    }

    public sealed class Simulation공간정의Snapshot
    {
        public string SpatialStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public string LandscapeNodeStableId { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string AccessStateCode { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public Simulation공간용량Snapshot[] BaseCapacities { get; set; }
            = Array.Empty<Simulation공간용량Snapshot>();
        public string DefinitionRevision { get; set; } = string.Empty;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation공간실행상태Snapshot
    {
        public string SpatialStableId { get; set; } = string.Empty;
        public string AccessStateCode { get; set; } = string.Empty;
        public Simulation공간용량Snapshot[] OccupiedCapacities { get; set; }
            = Array.Empty<Simulation공간용량Snapshot>();
        public Simulation공간용량Snapshot[] ReservedCapacities { get; set; }
            = Array.Empty<Simulation공간용량Snapshot>();
        public string[] ActiveTaskStableIds { get; set; } = Array.Empty<string>();
        public long Revision { get; set; }
    }

    public sealed class Simulation공간예약Snapshot
    {
        public string ReservationStableId { get; set; } = string.Empty;
        public string SpatialStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string ReservationKindCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public int ReservedAtTick { get; set; }
        public int? ConsumedAtTick { get; set; }
        public int? ReleasedAtTick { get; set; }
        public long CreatedRevision { get; set; }
        public long? FinalizedRevision { get; set; }
    }

    public sealed class Simulation공간상호작용PreviewSnapshot
    {
        public string PreferredSpatialStableId { get; set; } = string.Empty;
        public string SelectedSpatialStableId { get; set; } = string.Empty;
        public string[] RequiredCapabilityCodes { get; set; } = Array.Empty<string>();
        public Simulation공간용량Snapshot[] RequiredCapacities { get; set; }
            = Array.Empty<Simulation공간용량Snapshot>();
        public string DefinitionRevision { get; set; } = string.Empty;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public Simulation공간역할BindingSnapshot[] RoleBindings { get; set; }
            = Array.Empty<Simulation공간역할BindingSnapshot>();
    }

    public sealed class Simulation공간역할BindingSnapshot
    {
        public string RoleCode { get; set; } = string.Empty;
        public string PreferredSpatialStableId { get; set; } = string.Empty;
        public string SelectedSpatialStableId { get; set; } = string.Empty;
        public string DefinitionRevision { get; set; } = string.Empty;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string[] RequiredCapabilityCodes { get; set; } = Array.Empty<string>();
        public Simulation공간용량Snapshot[] RequiredCapacities { get; set; }
            = Array.Empty<Simulation공간용량Snapshot>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationTaskCancelRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
    }
}
