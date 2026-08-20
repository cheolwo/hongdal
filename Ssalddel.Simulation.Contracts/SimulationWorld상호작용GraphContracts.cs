using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorld상호작용Graph상태Codes
    {
        public const string Ready = "Ready";
        public const string Partial = "Partial";
        public const string WaitingForGraph = "WaitingForLandscapeGraph";
        public const string WaitingForNode = "WaitingForLandscapeNode";
        public const string PathUnresolved = "SpatialPathUnresolved";
        public const string GraphRevisionMismatch = "LandscapeGraphRevisionMismatch";
        public const string ReviewRequired = "ReviewRequired";
        public const string ContextBound = "AreaSetContextBound";
        public const string NotSpatiallyApplicable = "NotSpatiallyApplicable";
        public const string NetworkRelationReady = "AreaSetNetworkRelationReady";
    }

    public static class SimulationWorld상호작용Graph검토Codes
    {
        public const string Approved = "ApprovedForSimulation";
        public const string Draft = "Draft";
    }

    public static class SimulationWorld상호작용공간참여Codes
    {
        public const string Required = "Required";
        public const string Contextual = "Contextual";
        public const string NonSpatial = "NonSpatial";
    }

    public static class SimulationWorld공간역할Codes
    {
        public const string FarmProductionPlot = "FarmProductionPlot";
        public const string FarmInternalRoad = "FarmInternalRoad";
        public const string FarmWorkYard = "FarmWorkYard";
        public const string FarmLoadingBay = "FarmLoadingBay";
        public const string FarmGate = "FarmGate";
    }

    public static class SimulationWorld공간용량근거Codes
    {
        public const string Scenario = "Scenario";
        public const string ReviewedDesign = "ReviewedDesign";
        public const string Derived = "Derived";
        public const string PublicData = "PublicData";
    }

    public sealed class SimulationWorld상호작용GraphBindingCatalog
    {
        public string SchemaVersion { get; set; } = "simulation-world-interaction-graph-binding.v1";
        public string AreaSetStableId { get; set; } = string.Empty;
        public string CatalogRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public SimulationWorld상호작용GraphBindingPlan[] Bindings { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphBindingPlan>();
        public SimulationWorld상호작용GraphTransitionPlan[] Transitions { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphTransitionPlan>();
    }

    public sealed class SimulationWorld상호작용GraphBindingPlan
    {
        public string BindingStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ParticipationCode { get; set; } =
            SimulationWorld상호작용공간참여Codes.Required;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string SpatialOwnerKindCode { get; set; } = string.Empty;
        public string SpatialOwnerStableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public int RequiredGraphRevision { get; set; }
        public string RequiredGraphHashSha256 { get; set; } = string.Empty;
        public string RequiredNodeSemanticCode { get; set; } = string.Empty;
        public string SpatialRoleCode { get; set; } = string.Empty;
        public string SpatialStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public SimulationWorld상호작용GraphCapacityPlan[] BaseCapacities { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphCapacityPlan>();
        public string ReviewStatusCode { get; set; } = string.Empty;
        public string H1Ref { get; set; } = string.Empty;
        public string H2Ref { get; set; } = string.Empty;
        public string H3Ref { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorld상호작용NetworkBindingCatalog
    {
        public string SchemaVersion { get; set; } =
            "simulation-world-interaction-graph-binding.v2";
        public string NetworkStableId { get; set; } = string.Empty;
        public string CatalogRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public SimulationWorld상호작용GraphBindingPlan[] Bindings { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphBindingPlan>();
        public SimulationWorld상호작용ContextBindingPlan[] ContextualBindings { get; set; } =
            Array.Empty<SimulationWorld상호작용ContextBindingPlan>();
        public string[] NonSpatialWiIds { get; set; } = Array.Empty<string>();
        public SimulationWorld상호작용GraphTransitionPlan[] Transitions { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphTransitionPlan>();
    }

    public sealed class SimulationWorld상호작용ContextBindingPlan
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ParticipationCode { get; set; } =
            SimulationWorld상호작용공간참여Codes.Contextual;
        public string ContextStableId { get; set; } = string.Empty;
        public string ContextBindingStateCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorld상호작용GraphCapacityPlan
    {
        public string CapacityCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string EvidenceReference { get; set; } = string.Empty;
        public string CapacityRuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationWorld상호작용GraphTransitionPlan
    {
        public string TransitionStableId { get; set; } = string.Empty;
        public string FromWorldInteractionId { get; set; } = string.Empty;
        public string ToWorldInteractionId { get; set; } = string.Empty;
        public bool ExternalConnectorRequired { get; set; }
        public string RequiredConnectorTypeCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorld상호작용Graph준비도Response
    {
        public string SchemaVersion { get; set; } = "simulation-world-interaction-graph-readiness.v1";
        public string AreaSetStableId { get; set; } = string.Empty;
        public int AreaSetRevision { get; set; }
        public string AreaSetDefinitionHashSha256 { get; set; } = string.Empty;
        public string BindingCatalogRevision { get; set; } = string.Empty;
        public string BindingCatalogHashSha256 { get; set; } = string.Empty;
        public string OverallStatusCode { get; set; } = string.Empty;
        public SimulationWorld상호작용GraphAuditResponse[] GraphAudits { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphAuditResponse>();
        public SimulationWorld상호작용GraphBindingResponse[] Bindings { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphBindingResponse>();
        public SimulationWorld상호작용GraphTransitionResponse[] Transitions { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphTransitionResponse>();
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorld상호작용GraphAuditResponse
    {
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public int GraphRevision { get; set; }
        public string GraphHashSha256 { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public int NodeCount { get; set; }
        public int EdgeCount { get; set; }
        public int ExternalConnectorCount { get; set; }
        public int UnresolvedCount { get; set; }
    }

    public sealed class SimulationWorld상호작용GraphBindingResponse
    {
        public string BindingStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ParticipationCode { get; set; } =
            SimulationWorld상호작용공간참여Codes.Required;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string SpatialOwnerKindCode { get; set; } = string.Empty;
        public string SpatialOwnerStableId { get; set; } = string.Empty;
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public int LandscapeGraphRevision { get; set; }
        public string LandscapeGraphHashSha256 { get; set; } = string.Empty;
        public string RequiredNodeSemanticCode { get; set; } = string.Empty;
        public string SpatialRoleCode { get; set; } = string.Empty;
        public string MatchedLandscapeNodeStableId { get; set; } = string.Empty;
        public string MatchedNodeEvidenceKindCode { get; set; } = string.Empty;
        public string SpatialStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public SimulationWorld상호작용GraphCapacityPlan[] BaseCapacities { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphCapacityPlan>();
        public string ReviewStatusCode { get; set; } = string.Empty;
        public string H1Ref { get; set; } = string.Empty;
        public string H2Ref { get; set; } = string.Empty;
        public string H3Ref { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public bool SpatialClosedLoop { get; set; }
        public string[] Limitations { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public Simulation공간정의InitialRequest? SpatialDefinition { get; set; }
    }

    public sealed class SimulationWorld상호작용GraphTransitionResponse
    {
        public string TransitionStableId { get; set; } = string.Empty;
        public string FromWorldInteractionId { get; set; } = string.Empty;
        public string ToWorldInteractionId { get; set; } = string.Empty;
        public string FromLandscapeNodeStableId { get; set; } = string.Empty;
        public string ToLandscapeNodeStableId { get; set; } = string.Empty;
        public string[] EdgeStableIds { get; set; } = Array.Empty<string>();
        public string ExternalConnectorStableId { get; set; } = string.Empty;
        public string NetworkRelationStableId { get; set; } = string.Empty;
        public string RouteGraphStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorld상호작용ContextBindingResponse
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ParticipationCode { get; set; } =
            SimulationWorld상호작용공간참여Codes.Contextual;
        public string ContextStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorld상호작용NonSpatialResponse
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ParticipationCode { get; set; } =
            SimulationWorld상호작용공간참여Codes.NonSpatial;
        public string StatusCode { get; set; } =
            SimulationWorld상호작용Graph상태Codes.NotSpatiallyApplicable;
    }

    public sealed class SimulationWorld상호작용Network준비도Response
    {
        public string SchemaVersion { get; set; } =
            "simulation-world-interaction-network-readiness.v1";
        public string NetworkStableId { get; set; } = string.Empty;
        public int NetworkRevision { get; set; }
        public string NetworkDefinitionHashSha256 { get; set; } = string.Empty;
        public string BindingCatalogRevision { get; set; } = string.Empty;
        public string BindingCatalogHashSha256 { get; set; } = string.Empty;
        public string OverallStatusCode { get; set; } = string.Empty;
        public SimulationWorld상호작용GraphAuditResponse[] GraphAudits { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphAuditResponse>();
        public SimulationWorld상호작용GraphBindingResponse[] DirectBindings { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphBindingResponse>();
        public SimulationWorld상호작용ContextBindingResponse[] ContextualBindings { get; set; } =
            Array.Empty<SimulationWorld상호작용ContextBindingResponse>();
        public SimulationWorld상호작용NonSpatialResponse[] NonSpatialBindings { get; set; } =
            Array.Empty<SimulationWorld상호작용NonSpatialResponse>();
        public SimulationWorld상호작용GraphTransitionResponse[] Transitions { get; set; } =
            Array.Empty<SimulationWorld상호작용GraphTransitionResponse>();
        public int TotalWorldInteractionCount { get; set; }
        public bool PresentationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
