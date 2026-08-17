using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorld상호작용공간모판Codes
    {
        public const string SchemaVersion = "simulation-world-interaction-spatial-seedbed.v1";
        public const string CatalogSchemaVersion = "simulation-world-interaction-spatial-seedbed-catalog.v1";
        public const string ApprovedForSimulation = "ApprovedForSimulation";
        public const string Draft = "Draft";
        public const string Input = "Input";
        public const string Output = "Output";
        public const string Bidirectional = "Bidirectional";
        public const string Fixed = "Fixed";
        public const string Uniform = "Uniform";
    }

    public sealed class SimulationWorld상호작용공간모판Catalog
    {
        public string SchemaVersion { get; set; } =
            SimulationWorld상호작용공간모판Codes.CatalogSchemaVersion;
        public string Revision { get; set; } = string.Empty;
        public string WorldInteractionCatalogRevision { get; set; } = string.Empty;
        public string LandscapeGrammarRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public SimulationWorld상호작용공간모판Definition[] Definitions { get; set; } =
            Array.Empty<SimulationWorld상호작용공간모판Definition>();
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorld상호작용공간모판Definition
    {
        public string SchemaVersion { get; set; } =
            SimulationWorld상호작용공간모판Codes.SchemaVersion;
        public string StableId { get; set; } = string.Empty;
        public int Revision { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string AuthoredDocument { get; set; } = string.Empty;
        public string[] IncludedWiIds { get; set; } = Array.Empty<string>();
        public SimulationWorld상호작용공간모판InternalSpace[] InternalSpaces { get; set; } =
            Array.Empty<SimulationWorld상호작용공간모판InternalSpace>();
        public SimulationWorld상호작용공간모판WiBinding[] WiBindings { get; set; } =
            Array.Empty<SimulationWorld상호작용공간모판WiBinding>();
        public SimulationWorld상호작용공간모판InternalRelation[] InternalRelations { get; set; } =
            Array.Empty<SimulationWorld상호작용공간모판InternalRelation>();
        public SimulationWorld상호작용공간모판ExternalConnectorStub[] ExternalConnectorStubs { get; set; } =
            Array.Empty<SimulationWorld상호작용공간모판ExternalConnectorStub>();
        public SimulationWorld상호작용공간모판TransformConstraint TransformConstraint { get; set; } = new();
        public string ReviewStatusCode { get; set; } = string.Empty;
        public string[] EvidenceRefs { get; set; } = Array.Empty<string>();
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string SourceFileHashSha256 { get; set; } = string.Empty;
        public string AuthoredDocumentHashSha256 { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorld상호작용공간모판InternalSpace
    {
        public string SpaceCode { get; set; } = string.Empty;
        public string SpatialRoleCode { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public Simulation공간용량Snapshot[] BaseCapacities { get; set; } =
            Array.Empty<Simulation공간용량Snapshot>();
        public string[] AllowedLandscapeCompositionKeys { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorld상호작용공간모판WiBinding
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string InternalSpaceCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorld상호작용공간모판InternalRelation
    {
        public string RelationCode { get; set; } = string.Empty;
        public string FromSpaceCode { get; set; } = string.Empty;
        public string ToSpaceCode { get; set; } = string.Empty;
        public string ConnectorTypeCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorld상호작용공간모판ExternalConnectorStub
    {
        public string StubCode { get; set; } = string.Empty;
        public string InternalSpaceCode { get; set; } = string.Empty;
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string FlowDirectionCode { get; set; } = string.Empty;
        public string AdjacentWorldInteractionId { get; set; } = string.Empty;
    }

    public sealed class SimulationWorld상호작용공간모판TransformConstraint
    {
        public string[] AllowedRotationCodes { get; set; } = Array.Empty<string>();
        public string ScaleModeCode { get; set; } = SimulationWorld상호작용공간모판Codes.Fixed;
        public double MinimumWidthMeters { get; set; }
        public double MinimumDepthMeters { get; set; }
        public double PreferredWidthMeters { get; set; }
        public double PreferredDepthMeters { get; set; }
        public double MaximumWidthMeters { get; set; }
        public double MaximumDepthMeters { get; set; }
    }
}
