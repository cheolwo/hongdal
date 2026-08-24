using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationConstructionPlacementKindCodes
    {
        public const string Building = "Building";
        public const string FenceSegment = "FenceSegment";
        public const string FenceCorner = "FenceCorner";
        public const string FenceGate = "FenceGate";
        public const string Tree = "Tree";
    }

    public static class SimulationConstructionPlacementZoneTypeCodes
    {
        public const string FarmProcessing = "FarmProcessing";
        public const string FarmSupport = "FarmSupport";
        public const string FarmFenceEdge = "FarmFenceEdge";
        public const string FarmEntrance = "FarmEntrance";
        public const string Protected = "Protected";
    }

    public sealed class SimulationConstructionPlacementZoneRequest
    {
        public string PlacementZoneStableId { get; set; } = string.Empty;
        public string TargetH2StableId { get; set; } = string.Empty;
        public string ZoneTypeCode { get; set; } = string.Empty;
        public string PlacementProfileRevision { get; set; } = string.Empty;
        public int MinXCentimeters { get; set; }
        public int MaxXCentimeters { get; set; }
        public int MinZCentimeters { get; set; }
        public int MaxZCentimeters { get; set; }
        public int TerrainSlopeMilliDegrees { get; set; }
        public string[] RoadAccessConnectorStableIds { get; set; } = Array.Empty<string>();
        public string FenceChainStableId { get; set; } = string.Empty;
        public int? FenceStartXCentimeters { get; set; }
        public int? FenceStartZCentimeters { get; set; }
    }

    public sealed class SimulationFarmConstructionPlacementPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string BlueprintStableId { get; set; } = string.Empty;
        public string PlacementZoneStableId { get; set; } = string.Empty;
        public string TargetH2StableId { get; set; } = string.Empty;
        public int LocalXCentimeters { get; set; }
        public int LocalZCentimeters { get; set; }
        public int RotationQuarterTurns { get; set; }
        public string AccessConnectorStableId { get; set; } = string.Empty;
        public string FenceChainStableId { get; set; } = string.Empty;
        public string DevelopmentOpportunityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmConstructionPlacementConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string PlacementProposalStableId { get; set; } = string.Empty;
        public string PreviewHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmConstructionPlacementPreviewSnapshot
    {
        public string PlacementProposalStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string BlueprintStableId { get; set; } = string.Empty;
        public string PlacementKindCode { get; set; } = string.Empty;
        public string PlacementZoneStableId { get; set; } = string.Empty;
        public string TargetH2StableId { get; set; } = string.Empty;
        public int LocalXCentimeters { get; set; }
        public int LocalZCentimeters { get; set; }
        public int RotationQuarterTurns { get; set; }
        public int FootprintWidthCentimeters { get; set; }
        public int FootprintDepthCentimeters { get; set; }
        public string AccessConnectorStableId { get; set; } = string.Empty;
        public string FenceChainStableId { get; set; } = string.Empty;
        public string PlacementProfileRevision { get; set; } = string.Empty;
        public string DevelopmentOpportunityStableId { get; set; } = string.Empty;
        public SimulationIntegratedItemRequirement[] MaterialRequirements { get; set; }
            = Array.Empty<SimulationIntegratedItemRequirement>();
        public string[] ReservedMaterialLotStableIds { get; set; } = Array.Empty<string>();
        public string[] SelectedActorStableIds { get; set; } = Array.Empty<string>();
        public int ConstructionTicks { get; set; }
        public string PreviewHashSha256 { get; set; } = string.Empty;
    }
}
