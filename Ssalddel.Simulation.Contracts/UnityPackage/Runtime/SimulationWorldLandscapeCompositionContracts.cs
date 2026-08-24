using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldLandscapeCompositionCodes
    {
        public const string SchemaVersion = "simulation-world-landscape-composition.v1";
        public const string AreaSetSchemaVersion = "simulation-world-area-set.v1";
        public const string LandscapeGraphSchemaVersion = "simulation-world-landscape-graph.v2";
        public const string GrammarRevision = "pyeongchang-landscape-grammar.v1";
        public const string Available = "Available";
        public const string WaitingForSpatialArtifact = "WaitingForSpatialArtifact";
        public const string WaitingForGrammarManifest = "WaitingForGrammarManifest";
        public const string PartialUnresolved = "PartialUnresolved";
        public const string CatalogMismatch = "CatalogMismatch";
        public const string Declared = "Declared";
        public const string GraphConnectorUnresolved = "GraphConnectorUnresolved";

        public const string ScenarioLocalMeters = "ScenarioLocalMeters";
        public const string LegacyUnspecifiedCoordinates = "LegacyUnspecified";
        public const string AreaSetOwner = "AreaSet";
        public const string AreaSetNetworkOwner = "AreaSetNetwork";

        public const string GraphDeliveryMode = "GraphV2";
        public const string LegacyTileDeliveryMode = "TileFacadeV1";
        public const string GraphBuildScope = "Graph";
        public const string LegacyTileBuildScope = "LegacyTile";

        public const string Area = "area";
        public const string Linear = "linear";
        public const string Junction = "junction";
        public const string Transition = "transition";
        public const string Landmark = "landmark";
        public const string Detail = "detail";

        public const string Contains = "contains";
        public const string Adjacent = "adjacent";
        public const string Connects = "connects";
        public const string TransitionsTo = "transitions-to";

        public const string GraphAdjacent = "Adjacent";
        public const string GraphConnected = "Connected";
        public const string GraphTransition = "Transition";
    }

    public sealed class SimulationWorldAreaSetDefinitionResponse
    {
        public string SchemaVersion { get; set; } =
            SimulationWorldLandscapeCompositionCodes.AreaSetSchemaVersion;
        public string AreaSetStableId { get; set; } = string.Empty;
        public int Revision { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string DocumentHashSha256 { get; set; } = string.Empty;
        public string CanonicalNetworkStableId { get; set; } = string.Empty;
        public string CoordinateSpaceCode { get; set; } = string.Empty;
        public string[] AreaRefs { get; set; } = Array.Empty<string>();
        public string[] ScenarioRouteRefs { get; set; } = Array.Empty<string>();
        public string[] CompletionAreaRefs { get; set; } = Array.Empty<string>();
        public SimulationWorldLandscapeGraphDescriptorResponse[] LandscapeGraphs { get; set; } =
            Array.Empty<SimulationWorldLandscapeGraphDescriptorResponse>();
        public SimulationWorldLandscapeGraphRelationResponse[] GraphRelations { get; set; } =
            Array.Empty<SimulationWorldLandscapeGraphRelationResponse>();
        public string DefinitionStatusCode { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldLandscapeGraphIndexResponse
    {
        public string SchemaVersion { get; set; } =
            SimulationWorldLandscapeCompositionCodes.LandscapeGraphSchemaVersion;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string CenterTileKey { get; set; } = string.Empty;
        public int RadiusTiles { get; set; }
        public SimulationWorldLandscapeGraphDescriptorResponse[] Graphs { get; set; } =
            Array.Empty<SimulationWorldLandscapeGraphDescriptorResponse>();
        public string[] CoveredTileKeys { get; set; } = Array.Empty<string>();
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class SimulationWorldLandscapeGraphDescriptorResponse
    {
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public string GraphRoleCode { get; set; } = string.Empty;
        public int GraphRevision { get; set; }
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string BuildStatusCode { get; set; } = string.Empty;
        public string GraphHashSha256 { get; set; } = string.Empty;
        public string SpatialOwnerKindCode { get; set; } = string.Empty;
        public string SpatialOwnerStableId { get; set; } = string.Empty;
        public string CoordinateSpaceCode { get; set; } = string.Empty;
        public SimulationWorldLandscapeBoundsResponse Bounds { get; set; } = new();
        public string[] AreaRefs { get; set; } = Array.Empty<string>();
        public string[] TileRefs { get; set; } = Array.Empty<string>();
        public string[] ScenarioRouteRefs { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorldLandscapeBoundsResponse
    {
        public double MinEastingMeters { get; set; }
        public double MinNorthingMeters { get; set; }
        public double MaxEastingMeters { get; set; }
        public double MaxNorthingMeters { get; set; }
    }

    public sealed class SimulationWorldLandscapeGraphRelationResponse
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string FromGraphStableId { get; set; } = string.Empty;
        public string ToGraphStableId { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public SimulationWorldLandscapeConnectorPairResponse ConnectorPair { get; set; } = new();
    }

    public sealed class SimulationWorldLandscapeConnectorPairResponse
    {
        public string FromConnectorStableId { get; set; } = string.Empty;
        public string ToConnectorStableId { get; set; } = string.Empty;
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string RouteSignature { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "여러 타일과 Area를 참조하는 하나의 경관 Graph를 Unity에 전달한다.",
        StepKey = "contract.landscape-graph",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 17,
        Boundary = "Graph는 표현용 공간 구조이며 Unity의 로드 상태나 운영 업무 상태를 확정하지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약 정의는 실행 효과나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class SimulationWorldLandscapeGraphResponse
    {
        public string SchemaVersion { get; set; } =
            SimulationWorldLandscapeCompositionCodes.LandscapeGraphSchemaVersion;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public string GraphBuildStableId { get; set; } = string.Empty;
        public string GraphRoleCode { get; set; } = string.Empty;
        public int GraphRevision { get; set; }
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string GraphHashSha256 { get; set; } = string.Empty;
        public string SpatialOwnerKindCode { get; set; } = string.Empty;
        public string SpatialOwnerStableId { get; set; } = string.Empty;
        public string CoordinateSpaceCode { get; set; } = string.Empty;
        public string GrammarRevision { get; set; } = string.Empty;
        public string GrammarHashSha256 { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public SimulationWorldLandscapeBoundsResponse Bounds { get; set; } = new();
        public string[] AreaRefs { get; set; } = Array.Empty<string>();
        public string[] TileRefs { get; set; } = Array.Empty<string>();
        public string[] ScenarioRouteRefs { get; set; } = Array.Empty<string>();
        public SimulationWorldLandscapeNodeResponse[] Nodes { get; set; } =
            Array.Empty<SimulationWorldLandscapeNodeResponse>();
        public SimulationWorldLandscapeEdgeResponse[] Edges { get; set; } =
            Array.Empty<SimulationWorldLandscapeEdgeResponse>();
        public SimulationWorldLandscapePlacementResponse[] Placements { get; set; } =
            Array.Empty<SimulationWorldLandscapePlacementResponse>();
        public SimulationWorldLandscapeExternalConnectorResponse[] ExternalConnectorStubs { get; set; } =
            Array.Empty<SimulationWorldLandscapeExternalConnectorResponse>();
        public SimulationWorldLandscapeUnresolvedResponse[] Unresolved { get; set; } =
            Array.Empty<SimulationWorldLandscapeUnresolvedResponse>();
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "공간 근거로 조립된 경관 Graph와 의미 기반 Composition 배치를 Unity에 전달한다.",
        StepKey = "contract.landscape-composition-tile",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 16,
        Boundary = "Prefab 경로·GUID·상품명은 노출하지 않으며, 응답은 표현 계획이지 운영 사실이나 실제 시설 존재의 확정이 아니다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약 정의는 실행 효과나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class SimulationWorldLandscapeCompositionTileResponse
    {
        public string SchemaVersion { get; set; } = SimulationWorldLandscapeCompositionCodes.SchemaVersion;
        public string TileKey { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string GraphBuildStableId { get; set; } = string.Empty;
        public string GraphHashSha256 { get; set; } = string.Empty;
        public string GrammarRevision { get; set; } = string.Empty;
        public string GrammarHashSha256 { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public SimulationWorldLandscapeNodeResponse[] Nodes { get; set; } =
            Array.Empty<SimulationWorldLandscapeNodeResponse>();
        public SimulationWorldLandscapeEdgeResponse[] Edges { get; set; } =
            Array.Empty<SimulationWorldLandscapeEdgeResponse>();
        public SimulationWorldLandscapePlacementResponse[] Placements { get; set; } =
            Array.Empty<SimulationWorldLandscapePlacementResponse>();
        public SimulationWorldLandscapeExternalConnectorResponse[] ExternalConnectorStubs { get; set; } =
            Array.Empty<SimulationWorldLandscapeExternalConnectorResponse>();
        public SimulationWorldLandscapeUnresolvedResponse[] Unresolved { get; set; } =
            Array.Empty<SimulationWorldLandscapeUnresolvedResponse>();
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldLandscapeNodeResponse
    {
        public string NodeStableId { get; set; } = string.Empty;
        public string ParentNodeStableId { get; set; } = string.Empty;
        public string NodeKindCode { get; set; } = string.Empty;
        public string SemanticCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public double CenterEastingMeters { get; set; }
        public double CenterNorthingMeters { get; set; }
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
    }

    public sealed class SimulationWorldLandscapeEdgeResponse
    {
        public string EdgeStableId { get; set; } = string.Empty;
        public string FromNodeStableId { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public string ToNodeStableId { get; set; } = string.Empty;
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldLandscapePlacementResponse
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string NodeStableId { get; set; } = string.Empty;
        public string OwnerTileKey { get; set; } = string.Empty;
        public string CompositionKey { get; set; } = string.Empty;
        public string TopologyCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public double EastingMeters { get; set; }
        public double NorthingMeters { get; set; }
        public double PhysicalElevationMeters { get; set; }
        public double RotationDegrees { get; set; }
        public bool Mirrored { get; set; }
        public int DeterministicSeed { get; set; }
        public double FootprintWidthMeters { get; set; }
        public double FootprintDepthMeters { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class SimulationWorldLandscapeExternalConnectorResponse
    {
        public string StubStableId { get; set; } = string.Empty;
        public string PlacementStableId { get; set; } = string.Empty;
        public string NeighborTileKey { get; set; } = string.Empty;
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string RouteSignature { get; set; } = string.Empty;
        public string DirectionCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public double WorldEastingMeters { get; set; }
        public double WorldNorthingMeters { get; set; }
        public double WidthMeters { get; set; }
    }

    public sealed class SimulationWorldLandscapeUnresolvedResponse
    {
        public string UnresolvedStableId { get; set; } = string.Empty;
        public string NodeStableId { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string RequiredSemanticCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
