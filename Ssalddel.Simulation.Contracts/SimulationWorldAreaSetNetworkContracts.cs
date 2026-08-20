using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldAreaSetNetworkCodes
    {
        public const string SchemaVersion = "simulation-world-area-set-network.v1";
        public const string PlayerTraversal = "PlayerTraversal";
        public const string CargoLogistics = "CargoLogistics";
        public const string OneWay = "OneWay";
        public const string Persistent = "Persistent";
        public const string OnDemand = "OnDemand";
        public const string ActualE5 = "ActualE5";
    }

    public sealed class SimulationWorldAreaSetNetworkResponse
    {
        public string SchemaVersion { get; set; } = SimulationWorldAreaSetNetworkCodes.SchemaVersion;
        public string NetworkStableId { get; set; } = string.Empty;
        public int Revision { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string CoordinateSpaceCode { get; set; } =
            SimulationWorldLandscapeCompositionCodes.ScenarioLocalMeters;
        public string EvidenceStageCode { get; set; } =
            SimulationWorldAreaSetNetworkCodes.ActualE5;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string DocumentHashSha256 { get; set; } = string.Empty;
        public string DefinitionStatusCode { get; set; } = string.Empty;
        public SimulationWorldAreaSetNetworkAreaResponse[] AreaSets { get; set; } =
            Array.Empty<SimulationWorldAreaSetNetworkAreaResponse>();
        public SimulationWorldLandscapeGraphDescriptorResponse[] RouteGraphs { get; set; } =
            Array.Empty<SimulationWorldLandscapeGraphDescriptorResponse>();
        public SimulationWorldAreaSetNetworkRelationResponse[] Relations { get; set; } =
            Array.Empty<SimulationWorldAreaSetNetworkRelationResponse>();
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldAreaSetNetworkAreaResponse
    {
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AreaRoleCode { get; set; } = string.Empty;
        public string LoadPolicyCode { get; set; } = string.Empty;
        public string DefaultEntryConnectorStableId { get; set; } = string.Empty;
        public int AreaSetRevision { get; set; }
        public string DefinitionHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldAreaSetNetworkRelationResponse
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string FromAreaSetStableId { get; set; } = string.Empty;
        public string FromConnectorStableId { get; set; } = string.Empty;
        public string ToAreaSetStableId { get; set; } = string.Empty;
        public string ToConnectorStableId { get; set; } = string.Empty;
        public string RelationKindCode { get; set; } = string.Empty;
        public string DirectionCode { get; set; } = SimulationWorldAreaSetNetworkCodes.OneWay;
        public string RouteGraphStableId { get; set; } = string.Empty;
        public string RouteSignature { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
