using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldLayoutCodes
    {
        public const string DefinitionSchemaVersion = "simulation-world-layout-definition.v1";
        public const string GroundingBindingSchemaVersion = "simulation-world-grounding-binding.v1";
        public const string GroundingReadinessSchemaVersion = "simulation-world-grounding-readiness.v1";
        public const string ParentLocalMeters = "ParentLocalMeters";
        public const string ScenarioLocalMeters = "ScenarioLocalMeters";
        public const string Reference = "Reference";
        public const string None = "None";
        public const string ScenarioRelative = "ScenarioRelative";
        public const string E6Grounded = "E6Grounded";
        public const string NotRequired = "NotRequired";
        public const string Optional = "Optional";
        public const string Required = "Required";
        public const string NotApplicable = "NotApplicable";
        public const string NotApplied = "NotApplied";
        public const string Grounded = "Grounded";
        public const string NotStarted = "NotStarted";
        public const string Partial = "Partial";
        public const string Ready = "Ready";
        public const string Blocked = "Blocked";
        public const string Disallow = "Disallow";
        public const string BoundaryTouch = "BoundaryTouch";
        public const string TransitionOverlap = "TransitionOverlap";
        public const string ContainmentAllowed = "ContainmentAllowed";
        public const string AbstractTravel = "AbstractTravel";
        public const string PhysicalCorridor = "PhysicalCorridor";
        public const string ReservedCorridor = "ReservedCorridor";
        public const string Composed = "Composed";
        public const string Reserved = "Reserved";
        public const string Hub = "Hub";
        public const string LegacyCityHub = "CityHub";
        public const string City = "City";

        public static string NormalizeAreaRoleCode(string value) =>
            string.Equals(value, LegacyCityHub, StringComparison.Ordinal)
                ? Hub
                : value ?? string.Empty;
    }

    public sealed class SimulationWorldPlacementTransformResponse
    {
        public string CoordinateSpaceCode { get; set; } = string.Empty;
        public double LocalXMeters { get; set; }
        public double LocalZMeters { get; set; }
        public double RotationDegrees { get; set; }
        public string SizeVariantCode { get; set; } = SimulationWorldLayoutCodes.Reference;
        public string MirrorCode { get; set; } = SimulationWorldLayoutCodes.None;
    }

    public sealed class SimulationWorldConnectorPoseResponse
    {
        public string ConnectorStableId { get; set; } = string.Empty;
        public string CoordinateSpaceCode { get; set; } = SimulationWorldLayoutCodes.ParentLocalMeters;
        public double LocalXMeters { get; set; }
        public double LocalZMeters { get; set; }
        public double RotationDegrees { get; set; }
        public double WidthMeters { get; set; }
        public string DirectionCode { get; set; } = string.Empty;
        public string[] TravelTypeCodes { get; set; } = Array.Empty<string>();
        public string ConnectorPoseHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldGraphInstanceResponse
    {
        public string GraphInstanceStableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public string H3Ref { get; set; } = string.Empty;
        public SimulationWorldPlacementTransformResponse PlacementTransform { get; set; } = new();
        public SimulationWorldConnectorPoseResponse[] ExternalConnectors { get; set; } =
            Array.Empty<SimulationWorldConnectorPoseResponse>();
        public string SourcePatternHashSha256 { get; set; } = string.Empty;
        public string PlacementHashSha256 { get; set; } = string.Empty;
        public string InstanceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldAreaSetInstanceResponse
    {
        public string AreaSetInstanceStableId { get; set; } = string.Empty;
        public string BlueprintStableId { get; set; } = string.Empty;
        public string AreaRoleCode { get; set; } = string.Empty;
        public string[] LegacyAreaRoleCodes { get; set; } = Array.Empty<string>();
        public string LoadPolicyCode { get; set; } = string.Empty;
        public SimulationWorldPlacementTransformResponse PlacementTransform { get; set; } = new();
        public SimulationWorldGraphInstanceResponse[] GraphInstances { get; set; } =
            Array.Empty<SimulationWorldGraphInstanceResponse>();
        public SimulationWorldConnectorPoseResponse[] ExternalConnectors { get; set; } =
            Array.Empty<SimulationWorldConnectorPoseResponse>();
        public string PlacementHashSha256 { get; set; } = string.Empty;
        public string InstanceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldAreaAnchorResponse
    {
        public string AreaSetStableId { get; set; } = string.Empty;
        public string CanonicalAreaRoleCode { get; set; } = string.Empty;
        public string[] LegacyAreaRoleCodes { get; set; } = Array.Empty<string>();
        public string PlacementStateCode { get; set; } =
            SimulationWorldLayoutCodes.Composed;
        public string AreaCharacterProfileCode { get; set; } = string.Empty;
        public string[] PlacementRuleCodes { get; set; } = Array.Empty<string>();
        public SimulationWorldPlacementTransformResponse FixedPlacementTransform { get; set; } = new();
        public bool CanPrefetchMetadata { get; set; } = true;
        public bool CanTraverse { get; set; }
        public bool CanActivate { get; set; }
        public string AnchorHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldReservedConnectionResponse
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string FromAreaSetInstanceStableId { get; set; } = string.Empty;
        public string ToAreaSetInstanceStableId { get; set; } = string.Empty;
        public string SpatialRealizationCode { get; set; } =
            SimulationWorldLayoutCodes.ReservedCorridor;
        public string RelationKindCode { get; set; } = string.Empty;
        public string ConnectionHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldCorridorInstanceResponse
    {
        public string CorridorInstanceStableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public SimulationWorldPlacementTransformResponse PlacementTransform { get; set; } = new();
        public string FromAreaSetInstanceStableId { get; set; } = string.Empty;
        public string FromConnectorStableId { get; set; } = string.Empty;
        public string ToAreaSetInstanceStableId { get; set; } = string.Empty;
        public string ToConnectorStableId { get; set; } = string.Empty;
        public string RelationStableId { get; set; } = string.Empty;
        public SimulationWorldConnectorPoseResponse[] ExternalConnectors { get; set; } =
            Array.Empty<SimulationWorldConnectorPoseResponse>();
        public string PlacementHashSha256 { get; set; } = string.Empty;
        public string InstanceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldOverlapRuleResponse
    {
        public string FromInstanceStableId { get; set; } = string.Empty;
        public string ToInstanceStableId { get; set; } = string.Empty;
        public string OverlapPolicyCode { get; set; } = SimulationWorldLayoutCodes.Disallow;
        public string CorridorInstanceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldLayoutRelationResponse
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string FromAreaSetInstanceStableId { get; set; } = string.Empty;
        public string ToAreaSetInstanceStableId { get; set; } = string.Empty;
        public string RelationKindCode { get; set; } = string.Empty;
        public string SpatialRealizationCode { get; set; } = string.Empty;
        public string CorridorInstanceStableId { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Contract,
        "H4 AreaSet과 H3 회랑의 H5 상대 공간 배치를 전달한다.",
        StepKey = "contract.world-layout-definition",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 28,
        Boundary = "ScenarioRelative H5는 E6 없이도 권위 세계이며 AreaSetNetwork나 Simulation 상태를 변경하지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약 정의는 실행 효과나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class SimulationWorldLayoutDefinitionResponse
    {
        public string SchemaVersion { get; set; } = SimulationWorldLayoutCodes.DefinitionSchemaVersion;
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldIntentStableId { get; set; } = string.Empty;
        public string AreaSetNetworkStableId { get; set; } = string.Empty;
        public string CoordinateSpaceCode { get; set; } = SimulationWorldLayoutCodes.ScenarioLocalMeters;
        public string WorldGroundingPolicyCode { get; set; } = SimulationWorldLayoutCodes.Optional;
        public SimulationWorldAreaSetInstanceResponse[] AreaSetInstances { get; set; } =
            Array.Empty<SimulationWorldAreaSetInstanceResponse>();
        public SimulationWorldAreaAnchorResponse[] AreaAnchors { get; set; } =
            Array.Empty<SimulationWorldAreaAnchorResponse>();
        public SimulationWorldCorridorInstanceResponse[] CorridorInstances { get; set; } =
            Array.Empty<SimulationWorldCorridorInstanceResponse>();
        public SimulationWorldReservedConnectionResponse[] ReservedConnections { get; set; } =
            Array.Empty<SimulationWorldReservedConnectionResponse>();
        public SimulationWorldLayoutRelationResponse[] Relations { get; set; } =
            Array.Empty<SimulationWorldLayoutRelationResponse>();
        public SimulationWorldOverlapRuleResponse[] OverlapRules { get; set; } =
            Array.Empty<SimulationWorldOverlapRuleResponse>();
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldGroundingBindingResponse
    {
        public string SchemaVersion { get; set; } = SimulationWorldLayoutCodes.GroundingBindingSchemaVersion;
        public string GroundingBindingStableId { get; set; } = string.Empty;
        public int GroundingBindingRevision { get; set; }
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
        public string PlacementAuthorityCode { get; set; } = SimulationWorldLayoutCodes.ScenarioRelative;
        public string WorldGroundingStateCode { get; set; } = SimulationWorldLayoutCodes.NotApplied;
        public string E6AnchorStableId { get; set; } = string.Empty;
        public string GroundingEvidenceHashSha256 { get; set; } = string.Empty;
        public string BindingHashSha256 { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldGroundingReadinessResponse
    {
        public string SchemaVersion { get; set; } = SimulationWorldLayoutCodes.GroundingReadinessSchemaVersion;
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public string GroundingReadinessStateCode { get; set; } = SimulationWorldLayoutCodes.NotStarted;
        public string[] AvailableEvidenceKindCodes { get; set; } = Array.Empty<string>();
        public string[] MissingEvidenceKindCodes { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string ReadinessHashSha256 { get; set; } = string.Empty;
        public bool AppliesAuthority { get; set; }
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
