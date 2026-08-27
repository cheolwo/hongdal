using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation영역건물발전Codes
    {
        public const string CatalogRevision = "area-building-tech-tree.r1";

        public const string Nature = "Nature";
        public const string Farm = "Farm";
        public const string Town = "Town";
        public const string Hub = "Hub";
        public const string City = "City";

        public const string Foundation = "Foundation";
        public const string Operations = "Operations";
        public const string Specialization = "Specialization";
        public const string Resilience = "Resilience";
        public const string Landmark = "Landmark";

        public const string Locked = "Locked";
        public const string Available = "Available";
        public const string Planned = "Planned";
        public const string Building = "Building";
        public const string Operational = "Operational";
        public const string Damaged = "Damaged";
        public const string Suspended = "Suspended";

        public const string BeginBuildingConstruction = "BeginBuildingConstruction";
        public const string ConstructionWorldInteractionId = "WI-CON-01";
        public const string ExpansionBuildWorkKind = "AreaBuildingConstruction";

        public const string NatureCabinBlueprint = "blueprint:nature-cabin.v1";
        public const string NatureWorkbenchBlueprint = "blueprint:nature-workbench.v1";
        public const string NatureStorageRackBlueprint = "blueprint:nature-storage-rack.v1";
        public const string NaturePalisadeBlueprint = "blueprint:nature-palisade.v1";
        public const string NatureLearningLodgeBlueprint =
            "blueprint:nature-learning-lodge.v1";

        public const string NatureLearningLodgeFacility =
            "facility:nature-learning-lodge";
        public const string NatureLearningLodgeH1 = "h1:nature:learning-lodge";

        public const string BlueprintInvalid = "SimulationAreaBuildingBlueprintInvalid";
        public const string BlueprintLocked = "SimulationAreaBuildingBlueprintLocked";
        public const string AlreadyOperational =
            "SimulationAreaBuildingAlreadyOperational";
        public const string ConstructionActive =
            "SimulationAreaBuildingConstructionActive";
        public const string Day2Required = "SimulationAreaBuildingDay2Required";
        public const string CabinAccessRequired =
            "SimulationAreaBuildingCabinAccessRequired";
        public const string TimberInsufficient =
            "SimulationAreaBuildingTimberInsufficient";
        public const string RebuildPartInsufficient =
            "SimulationAreaBuildingRebuildPartInsufficient";
        public const string PlacementOutsideHome =
            "SimulationAreaBuildingPlacementOutsideHome";
        public const string PlacementOverlap =
            "SimulationAreaBuildingPlacementOverlap";
        public const string CatalogInvalid = "SimulationAreaBuildingCatalogInvalid";
        public const string CatalogHashMismatch =
            "SimulationAreaBuildingCatalogHashMismatch";
    }

    public sealed class Simulation영역건물발전CatalogSnapshot
    {
        public string Revision { get; set; } = string.Empty;
        public string HashSha256 { get; set; } = string.Empty;
        public Simulation건물청사진Definition[] Blueprints { get; set; }
            = Array.Empty<Simulation건물청사진Definition>();
        public Simulation승인가르침자료Snapshot[] ApprovedTeachingMaterials { get; set; }
            = Array.Empty<Simulation승인가르침자료Snapshot>();
    }

    public sealed class Simulation건물청사진Definition
    {
        public string BlueprintStableId { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public string StageCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] RequiredOperationalBlueprintStableIds { get; set; }
            = Array.Empty<string>();
        public int RequiredTimberQuantity { get; set; }
        public int RequiredRebuildPartQuantity { get; set; }
        public int ConstructionSeconds { get; set; }
        public int FootprintWidthCentimeters { get; set; }
        public int FootprintDepthCentimeters { get; set; }
        public int ClearanceCentimeters { get; set; }
        public bool Optional { get; set; }
    }

    public sealed class Simulation승인가르침자료Snapshot
    {
        public string TeachingMaterialStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string HashSha256 { get; set; } = string.Empty;
        public string TopicCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string ShortSummary { get; set; } = string.Empty;
        public string SourceKindCode { get; set; } = string.Empty;
        public string SourceReference { get; set; } = string.Empty;
        public string ViewpointAndLimitations { get; set; } = string.Empty;
        public bool AdminApproved { get; set; }
    }

    public sealed class Simulation영역건물발전Snapshot
    {
        public string CatalogRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public Simulation건물발전NodeSnapshot[] Nodes { get; set; }
            = Array.Empty<Simulation건물발전NodeSnapshot>();
        public Simulation승인가르침자료Snapshot[] ApprovedTeachingMaterials { get; set; }
            = Array.Empty<Simulation승인가르침자료Snapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class Simulation건물발전NodeSnapshot
    {
        public string BlueprintStableId { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public string StageCode { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = Simulation영역건물발전Codes.Locked;
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool IsDay2Priority { get; set; }
        public int CompletedWorkSeconds { get; set; }
        public int RequiredWorkSeconds { get; set; }
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
        public int CompletedLearningVisitCount { get; set; }
    }

    public sealed class Simulation학습방문Snapshot
    {
        public string VisitStableId { get; set; } = string.Empty;
        public string NpcStableId { get; set; } = string.Empty;
        public string BuildingFacilityStableId { get; set; } = string.Empty;
        public string TeachingMaterialStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int StartedCycleIndex { get; set; }
        public int StartedAtSecond { get; set; }
        public int CompletedAtSecond { get; set; }
        public bool SimulationOnly { get; set; } = true;
    }
}
