using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationActorEquipmentCodes
    {
        public const string RuleRevision = "actor-equipment.r1";

        public const string AcquireWorldInteractionId = "WI-ACTOR-01";
        public const string ChangeEquipmentWorldInteractionId = "WI-ACTOR-02";

        public const string Equip = "Equip";
        public const string Unequip = "Unequip";
        public const string Swap = "Swap";

        public const string WorldPickup = "WorldPickup";
        public const string Inventory = "Inventory";
        public const string Equipped = "Equipped";

        public const string MainHand = "MainHand";
        public const string OffHand = "OffHand";
        public const string Head = "Head";
        public const string Body = "Body";
        public const string Legs = "Legs";
        public const string Feet = "Feet";
        public const string Back = "Back";
        public const string Accessory = "Accessory";

        public const string Woodcutting = "capability:woodcutting";
        public const string TerrainGrading = "capability:terrain-grading";
        public const string Mining = "capability:mining";

        public const string AxeDefinitionStableId = "item-definition:tool:axe.basic";
        public const string ShovelDefinitionStableId = "item-definition:tool:shovel.basic";
        public const string PickaxeDefinitionStableId = "item-definition:tool:pickaxe.basic";

        public const string ExpectedRevisionMismatch = "ActorEquipmentExpectedRevisionMismatch";
        public const string Disabled = "ActorEquipmentDisabled";
        public const string ActorMismatch = "ActorEquipmentActorMismatch";
        public const string ItemInstanceNotFound = "ActorEquipmentItemInstanceNotFound";
        public const string ItemNotInWorld = "ActorEquipmentItemNotInWorld";
        public const string ItemNotInInventory = "ActorEquipmentItemNotInInventory";
        public const string SlotNotAllowed = "ActorEquipmentSlotNotAllowed";
        public const string SlotOccupied = "ActorEquipmentSlotOccupied";
        public const string ItemNotEquipped = "ActorEquipmentItemNotEquipped";
        public const string OperationNotSupported = "ActorEquipmentOperationNotSupported";
        public const string CommandPayloadConflict = "ActorEquipmentCommandPayloadConflict";
    }

    public sealed class SimulationItemDefinitionSnapshot
    {
        public string ItemDefinitionStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public bool Stackable { get; set; }
        public int InventoryCapacityUnits { get; set; } = 1;
        public string[] AllowedSlotCodes { get; set; } = Array.Empty<string>();
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public string VisualKey { get; set; } = string.Empty;
    }

    public sealed class SimulationOwnedItemInstanceInitialState
    {
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public string ItemDefinitionStableId { get; set; } = string.Empty;
        public string LocationCode { get; set; } = SimulationActorEquipmentCodes.WorldPickup;
        public string SlotCode { get; set; } = string.Empty;
        public string SourceSpatialStableId { get; set; } = string.Empty;
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Actor 물품 인스턴스와 장착 슬롯의 초기 권위 계약을 정의한다.",
        Boundary = "수량 재료 원장과 물리 장착 인스턴스를 분리한다.")]
    public sealed class SimulationActorEquipmentInitialStateRequest
    {
        public string RuleRevision { get; set; } = SimulationActorEquipmentCodes.RuleRevision;
        public string ActorStableId { get; set; } = string.Empty;
        public bool LegacyAutoEquipCompatibility { get; set; }
        public SimulationItemDefinitionSnapshot[] ItemDefinitions { get; set; }
            = Array.Empty<SimulationItemDefinitionSnapshot>();
        public SimulationOwnedItemInstanceInitialState[] ItemInstances { get; set; }
            = Array.Empty<SimulationOwnedItemInstanceInitialState>();
    }

    public sealed class SimulationOwnedItemInstanceSnapshot
    {
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public string ItemDefinitionStableId { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
        public string SourceSpatialStableId { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
    }

    public sealed class SimulationEquipmentSlotSnapshot
    {
        public string SlotCode { get; set; } = string.Empty;
        public string EquippedItemInstanceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationActorEquipmentStateSnapshot
    {
        public bool IsEnabled { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public long EquipmentRevision { get; set; }
        public SimulationItemDefinitionSnapshot[] ItemDefinitions { get; set; }
            = Array.Empty<SimulationItemDefinitionSnapshot>();
        public SimulationOwnedItemInstanceSnapshot[] ItemInstances { get; set; }
            = Array.Empty<SimulationOwnedItemInstanceSnapshot>();
        public SimulationEquipmentSlotSnapshot[] Slots { get; set; }
            = Array.Empty<SimulationEquipmentSlotSnapshot>();
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationActorItemAcquirePreviewRequest
    {
        public long ObservedEquipmentRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public string SpecializationWorldInteractionId { get; set; } = string.Empty;
    }

    public sealed class SimulationActorItemAcquirePreviewSnapshot
    {
        public string ArchetypeWorldInteractionId { get; set; }
            = SimulationActorEquipmentCodes.AcquireWorldInteractionId;
        public string SpecializationWorldInteractionId { get; set; } = string.Empty;
        public long ObservedEquipmentRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationActorItemAcquireConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedEquipmentRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public string SpecializationWorldInteractionId { get; set; } = string.Empty;
    }

    public sealed class SimulationActorEquipmentChangePreviewRequest
    {
        public long ObservedEquipmentRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string OperationCode { get; set; } = SimulationActorEquipmentCodes.Equip;
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
        public string SwapItemInstanceStableId { get; set; } = string.Empty;
        public string SpecializationWorldInteractionId { get; set; } = string.Empty;
    }

    public sealed class SimulationActorEquipmentChangePreviewSnapshot
    {
        public string ArchetypeWorldInteractionId { get; set; }
            = SimulationActorEquipmentCodes.ChangeEquipmentWorldInteractionId;
        public string SpecializationWorldInteractionId { get; set; } = string.Empty;
        public long ObservedEquipmentRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string OperationCode { get; set; } = string.Empty;
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationActorEquipmentChangeConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedEquipmentRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string OperationCode { get; set; } = SimulationActorEquipmentCodes.Equip;
        public string ItemInstanceStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
        public string SwapItemInstanceStableId { get; set; } = string.Empty;
        public string SpecializationWorldInteractionId { get; set; } = string.Empty;
    }
}
