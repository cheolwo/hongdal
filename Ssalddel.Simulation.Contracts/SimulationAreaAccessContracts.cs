using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationAreaAccessCodes
    {
        public const string RuleRevision = "player-area-access.r1";
        public const string PlayerOwner = SimulationNatureMindCodes.DefaultPlayerStableId;
        public const string FarmAreaSet = "area-set:sim:pyeongchang:farm-production.v1";
        public const string HubAreaSet = "area-set:sim:pyeongchang:logistics-hub.v1";
        public const string FarmToHubConnector = "area-set-relation:actual-e5:farmtocityhub";
        public const string FarmToHubSourceHHashSha256 = "6a1d3c42408f1f7909ade1f0ddc020d0c635c5a5475b299cb977b3c11189abbe";
        public const string Permanent = "Permanent";
        public const string Locked = "Locked";
        public const string Granted = "Granted";
        public const string Entered = "Entered";
        public const string FarmHubShipmentEvidence = "evidence:farm-hub-shipment-completed";
        public const string PlayerAreaTraversal = "PlayerAreaTraversal";
        public const string PlayerAreaTraversalTask = "PlayerAreaTraversalTask";
        public const string HubManufacturingWorldInteraction = "WI-MFG-01";
    }

    public sealed class SimulationPlayerAreaAccessSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AccessLevelCode { get; set; } = string.Empty;
        public string AccessStateCode { get; set; } = string.Empty;
        public string[] GrantedByEvidenceIds { get; set; } = Array.Empty<string>();
        public long GrantedAtWorldRevision { get; set; }
        public string RevocationPolicyCode { get; set; } = string.Empty;
        public string SourceHDefinitionHashSha256 { get; set; } = string.Empty;
        public string[] AvailableWorldInteractionIds { get; set; } = Array.Empty<string>();
        public long Revision { get; set; }
        public string AccessHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationPlayerAreaAccessStateSnapshot
    {
        public string RuleRevision { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public string CurrentAreaSetStableId { get; set; } = string.Empty;
        public SimulationPlayerAreaAccessSnapshot[] AccessEntries { get; set; } = Array.Empty<SimulationPlayerAreaAccessSnapshot>();
        public bool MutatesStaticHDefinitions { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationAreaTraversalPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string TargetAreaSetStableId { get; set; } = string.Empty;
        public string ConnectorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationAreaTraversalConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string TargetAreaSetStableId { get; set; } = string.Empty;
        public string ConnectorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationAreaTraversalPreviewSnapshot
    {
        public long BaseRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string FromAreaSetStableId { get; set; } = string.Empty;
        public string TargetAreaSetStableId { get; set; } = string.Empty;
        public string ConnectorStableId { get; set; } = string.Empty;
        public string AccessStateCode { get; set; } = string.Empty;
        public string[] EvidenceIds { get; set; } = Array.Empty<string>();
        public string[] NewWorldInteractionIds { get; set; } = Array.Empty<string>();
        public int DurationTicks { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string PreviewHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
