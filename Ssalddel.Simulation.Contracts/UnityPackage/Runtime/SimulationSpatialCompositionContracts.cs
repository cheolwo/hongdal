using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationSpatialCompositionCodes
    {
        public const string SchemaVersion = "placement-composition-graph.v1";
        public const string PlacementControlRevision =
            "placement-control-hierarchy.v3";
        public const string RuleRevision =
            "spatial-composition-rule-catalog.r1";

        public const string H1 = "H1";
        public const string H2 = "H2";
        public const string H3 = "H3";
        public const string H4 = "H4";

        public const string Blocked = "Blocked";
        public const string Qualified = "Qualified";
        public const string Formed = "Formed";
        public const string Degraded = "Degraded";
        public const string NotReady = "NotReady";
        public const string PartiallyReady = "PartiallyReady";
        public const string Ready = "Ready";

        public const string FormationAuthority = "SimulationFormationAuthority";
        public const string ReadinessOnly = "ReadinessOnly";

    }

    public sealed class SpatialCompositionRelationRule
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string FromChildDefinitionStableId { get; set; } = string.Empty;
        public string FromConnectorRoleCode { get; set; } = string.Empty;
        public string ToChildDefinitionStableId { get; set; } = string.Empty;
        public string ToConnectorRoleCode { get; set; } = string.Empty;
        public string MovementKindCode { get; set; } = string.Empty;
    }

    public sealed class SpatialCompositionRule
    {
        public string RuleStableId { get; set; } = string.Empty;
        public string TargetLevelCode { get; set; } = string.Empty;
        public string TargetDefinitionStableId { get; set; } = string.Empty;
        public string AuthorityCode { get; set; } =
            SimulationSpatialCompositionCodes.FormationAuthority;
        public string[] RequiredChildDefinitionStableIds { get; set; }
            = Array.Empty<string>();
        public string[] OptionalChildDefinitionStableIds { get; set; }
            = Array.Empty<string>();
        public string[] RequiredCapabilityCodes { get; set; }
            = Array.Empty<string>();
        public string[] RequiredPlayableLoopStableIds { get; set; }
            = Array.Empty<string>();
        public SpatialCompositionRelationRule[] Relations { get; set; }
            = Array.Empty<SpatialCompositionRelationRule>();
        public decimal MinimumStorageCapacityKgm { get; set; }
        public int MinimumWorkAreaSlots { get; set; }
        public bool RequiresPlacementValidation { get; set; } = true;
    }

    public sealed class SpatialCompositionRuleCatalog
    {
        public string SchemaVersion { get; set; } =
            SimulationSpatialCompositionCodes.SchemaVersion;
        public string Revision { get; set; } =
            SimulationSpatialCompositionCodes.RuleRevision;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public SpatialCompositionRule[] Rules { get; set; }
            = Array.Empty<SpatialCompositionRule>();
    }

    public sealed class SpatialCompositionChildEvidence
    {
        public string SpatialInstanceStableId { get; set; } = string.Empty;
        public string DefinitionStableId { get; set; } = string.Empty;
        public string LevelCode { get; set; } = string.Empty;
        public bool Operational { get; set; }
        public bool PlacementValidated { get; set; }
        public string PlacementPlanSchemaVersion { get; set; } = string.Empty;
        public string PlacementPlanHashSha256 { get; set; } = string.Empty;
        public string[] CapabilityCodes { get; set; } = Array.Empty<string>();
        public string[] ConnectorRoleCodes { get; set; } = Array.Empty<string>();
        public decimal StorageCapacityKgm { get; set; }
        public int WorkAreaSlots { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SpatialCompositionInstanceSnapshot
    {
        public string SpatialInstanceStableId { get; set; } = string.Empty;
        public string DefinitionStableId { get; set; } = string.Empty;
        public string LevelCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string[] ChildSpatialInstanceStableIds { get; set; }
            = Array.Empty<string>();
        public int FormedWorldTick { get; set; }
        public int LastEvaluatedWorldTick { get; set; }
    }

    public sealed class SpatialCompositionAssessment
    {
        public string RuleStableId { get; set; } = string.Empty;
        public string TargetLevelCode { get; set; } = string.Empty;
        public string TargetDefinitionStableId { get; set; } = string.Empty;
        public string AuthorityCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string SpatialInstanceStableId { get; set; } = string.Empty;
        public string[] SatisfiedChildDefinitionStableIds { get; set; }
            = Array.Empty<string>();
        public string[] MissingChildDefinitionStableIds { get; set; }
            = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourcePlacementPlanHashes { get; set; }
            = Array.Empty<string>();
    }

    public sealed class SimulationSpatialCompositionStateSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationSpatialCompositionCodes.SchemaVersion;
        public string AreaCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string PlacementControlRevision { get; set; } =
            SimulationSpatialCompositionCodes.PlacementControlRevision;
        public string RuleCatalogRevision { get; set; } = string.Empty;
        public string RuleCatalogHashSha256 { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public SpatialCompositionInstanceSnapshot[] Instances { get; set; }
            = Array.Empty<SpatialCompositionInstanceSnapshot>();
        public SpatialCompositionAssessment[] Assessments { get; set; }
            = Array.Empty<SpatialCompositionAssessment>();
        public string GraphHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SpatialCompositionGraphHandle
    {
        public string SchemaVersion { get; set; } =
            SimulationSpatialCompositionCodes.SchemaVersion;
        public string AreaCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string RuleCatalogRevision { get; set; } = string.Empty;
        public string RuleCatalogHashSha256 { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string GraphHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SpatialCompositionEvaluationRequest
    {
        public string AreaCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public bool CommitQualifiedFormations { get; set; }
        public SpatialCompositionRuleCatalog RuleCatalog { get; set; } = new();
        public SpatialCompositionChildEvidence[] ChildEvidence { get; set; }
            = Array.Empty<SpatialCompositionChildEvidence>();
        public string[] ClosedPlayableLoopStableIds { get; set; }
            = Array.Empty<string>();
        public SimulationSpatialCompositionStateSnapshot? PreviousState
            { get; set; }
    }
}
