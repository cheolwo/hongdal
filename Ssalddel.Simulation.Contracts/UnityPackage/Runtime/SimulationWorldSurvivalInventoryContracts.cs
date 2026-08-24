using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldSurvivalInventoryCodes
    {
        public const string RuleRevision = "world-survival-inventory.pyeongchang-farm.r1";
        public const string PublicAcquisition = "PublicAcquisition";
        public const string ManagerOnly = "ManagerOnly";
        public const string Locked = "Locked";
        public const string Allowed = "Allowed";
        public const string Blocked = "Blocked";
        public const string SimulationScenario = "SimulationScenario";

        public const string ExpectedRevisionMismatch = "SimulationExpectedRevisionMismatch";
        public const string PlayerNotFound = "SimulationWorldInventoryPlayerNotFound";
        public const string BuildingNotFound = "SimulationWorldInventoryBuildingNotFound";
        public const string PlayerOutsideBuilding = "SimulationWorldInventoryPlayerOutsideBuilding";
        public const string ContainerNotFound = "SimulationWorldInventoryContainerNotFound";
        public const string ContainerBuildingMismatch = "SimulationWorldInventoryContainerBuildingMismatch";
        public const string ContainerAccessDenied = "SimulationWorldInventoryContainerAccessDenied";
        public const string ItemStackNotFound = "SimulationWorldInventoryItemStackNotFound";
        public const string ItemStackContainerMismatch = "SimulationWorldInventoryItemStackContainerMismatch";
        public const string QuantityUnavailable = "SimulationWorldInventoryQuantityUnavailable";
        public const string PlayerCapacityExceeded = "SimulationWorldInventoryPlayerCapacityExceeded";
        public const string AcquisitionBlocked = "SimulationWorldInventoryAcquisitionBlocked";
        public const string CommandPayloadConflict = "SimulationCommandPayloadConflict";
        public const string OperationalInventoryForbidden = "SimulationWorldOperationalInventoryForbidden";
        public const string SaveReplayPending = "SimulationWorldInventorySaveReplayPending";
    }

    /// <summary>
    /// 공공데이터 건물 사실을 복제하지 않고, 그 건물을 Simulation 내부 공간의 닻으로
    /// 참조하는 초기 상태다. 내부 공간과 재고는 Scenario 근거이며 운영 재고가 아니다.
    /// </summary>
    public sealed class SimulationWorldInventoryInitialStateRequest
    {
        public string RuleRevision { get; set; } = SimulationWorldSurvivalInventoryCodes.RuleRevision;
        public SimulationWorldBuildingInteriorInitialStateRequest[] Buildings { get; set; }
            = Array.Empty<SimulationWorldBuildingInteriorInitialStateRequest>();
        public SimulationWorldPlayerInitialStateRequest[] Players { get; set; }
            = Array.Empty<SimulationWorldPlayerInitialStateRequest>();
        public SimulationWorldContainerInitialStateRequest[] Containers { get; set; }
            = Array.Empty<SimulationWorldContainerInitialStateRequest>();
        public SimulationWorldItemStackInitialStateRequest[] ItemStacks { get; set; }
            = Array.Empty<SimulationWorldItemStackInitialStateRequest>();
        public bool IsOperationalInventory { get; set; }
    }

    public sealed class SimulationWorldBuildingInteriorInitialStateRequest
    {
        public string BuildingStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public string RegionStableId { get; set; } = string.Empty;
        public string BuildingEvidenceKindCode { get; set; } = string.Empty;
        public string SourceRecordStableId { get; set; } = string.Empty;
        public string InteriorSpaceStableId { get; set; } = string.Empty;
        public string InteriorEvidenceKindCode { get; set; }
            = SimulationWorldSurvivalInventoryCodes.SimulationScenario;
    }

    public sealed class SimulationWorldPlayerInitialStateRequest
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string CurrentBuildingStableId { get; set; } = string.Empty;
        public decimal InventoryCapacityUnits { get; set; }
        public string[] ManagedContainerStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorldContainerInitialStateRequest
    {
        public string ContainerStableId { get; set; } = string.Empty;
        public string BuildingStableId { get; set; } = string.Empty;
        public string InteriorSpaceStableId { get; set; } = string.Empty;
        public string AccessPolicyCode { get; set; }
            = SimulationWorldSurvivalInventoryCodes.PublicAcquisition;
        public decimal CapacityUnits { get; set; }
        public string[] ManagerPlayerStableIds { get; set; } = Array.Empty<string>();
        public string EvidenceKindCode { get; set; }
            = SimulationWorldSurvivalInventoryCodes.SimulationScenario;
    }

    public sealed class SimulationWorldItemStackInitialStateRequest
    {
        public string ItemStackStableId { get; set; } = string.Empty;
        public string ContainerStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string BuildingItemRelationStableId { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; }
            = SimulationWorldSurvivalInventoryCodes.SimulationScenario;
    }

    public sealed class SimulationWorldItemAcquisitionPreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string BuildingStableId { get; set; } = string.Empty;
        public string ContainerStableId { get; set; } = string.Empty;
        public string ItemStackStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public sealed class SimulationWorldItemAcquisitionConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string BuildingStableId { get; set; } = string.Empty;
        public string ContainerStableId { get; set; } = string.Empty;
        public string ItemStackStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public sealed class SimulationWorldItemAcquisitionPreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string BuildingStableId { get; set; } = string.Empty;
        public string ContainerStableId { get; set; } = string.Empty;
        public string ItemStackStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal ContainerQuantityBefore { get; set; }
        public decimal ContainerQuantityAfter { get; set; }
        public decimal PlayerQuantityBefore { get; set; }
        public decimal PlayerQuantityAfter { get; set; }
        public string EligibilityStateCode { get; set; } = string.Empty;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public bool CanConfirm { get; set; }
        public bool StateChanged { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldItemAcquisitionResultSnapshot
    {
        public string CommandId { get; set; } = string.Empty;
        public long AppliedWorldRevision { get; set; }
        public int AppliedWorldTick { get; set; }
        public SimulationWorldItemTransferSnapshot Transfer { get; set; }
            = new SimulationWorldItemTransferSnapshot();
        public SimulationWorldInventorySnapshot Inventory { get; set; }
            = new SimulationWorldInventorySnapshot();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldInventorySnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public SimulationWorldBuildingInteriorSnapshot[] Buildings { get; set; }
            = Array.Empty<SimulationWorldBuildingInteriorSnapshot>();
        public SimulationWorldContainerSnapshot[] Containers { get; set; }
            = Array.Empty<SimulationWorldContainerSnapshot>();
        public SimulationWorldItemStackSnapshot[] ContainerItemStacks { get; set; }
            = Array.Empty<SimulationWorldItemStackSnapshot>();
        public SimulationWorldPlayerInventorySnapshot[] Players { get; set; }
            = Array.Empty<SimulationWorldPlayerInventorySnapshot>();
        public SimulationWorldItemTransferSnapshot[] Transfers { get; set; }
            = Array.Empty<SimulationWorldItemTransferSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldBuildingInteriorSnapshot
    {
        public string BuildingStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public string RegionStableId { get; set; } = string.Empty;
        public string BuildingEvidenceKindCode { get; set; } = string.Empty;
        public string SourceRecordStableId { get; set; } = string.Empty;
        public string InteriorSpaceStableId { get; set; } = string.Empty;
        public string InteriorEvidenceKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldContainerSnapshot
    {
        public string ContainerStableId { get; set; } = string.Empty;
        public string BuildingStableId { get; set; } = string.Empty;
        public string InteriorSpaceStableId { get; set; } = string.Empty;
        public string AccessPolicyCode { get; set; } = string.Empty;
        public decimal CapacityUnits { get; set; }
        public string[] ManagerPlayerStableIds { get; set; } = Array.Empty<string>();
        public string EvidenceKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldItemStackSnapshot
    {
        public string ItemStackStableId { get; set; } = string.Empty;
        public string ContainerStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string BuildingItemRelationStableId { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldPlayerInventorySnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string CurrentBuildingStableId { get; set; } = string.Empty;
        public decimal InventoryCapacityUnits { get; set; }
        public string[] ManagedContainerStableIds { get; set; } = Array.Empty<string>();
        public SimulationWorldPlayerItemSnapshot[] Items { get; set; }
            = Array.Empty<SimulationWorldPlayerItemSnapshot>();
    }

    public sealed class SimulationWorldPlayerItemSnapshot
    {
        public string ItemCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldItemTransferSnapshot
    {
        public string TransferStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string BuildingStableId { get; set; } = string.Empty;
        public string SourceContainerStableId { get; set; } = string.Empty;
        public string SourceItemStackStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public long AppliedWorldRevision { get; set; }
        public string EvidenceKindCode { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
    }
}
