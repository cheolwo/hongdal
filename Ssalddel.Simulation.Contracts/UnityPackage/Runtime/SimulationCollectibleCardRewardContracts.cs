using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationCollectibleCardRewardCodes
    {
        public const string RuleRevision = "collectible-card-reward.r1";
        public const string CatalogRevision = "collectible-card-catalog.pyeongchang.r1";
        public const string Farm = "Farm";
        public const string Exploration = "Exploration";
        public const string FarmTillingCompleted = "FarmTillingCompleted";
        public const string NewL2Tile = "NewL2Tile";
        public const string NewL1Area = "NewL1Area";
        public const string Pending = "Pending";
        public const string Drawn = "Drawn";
        public const string Success = "Success";
        public const string Failure = "Failure";
        public const string CatalogCapacitySuppressed = "CatalogCapacitySuppressed";
        public const string ScenarioEvidence = "Scenario";
    }

    public sealed class SimulationTileTraversalConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string FromL2TileKey { get; set; } = string.Empty;
        public string ToL2TileKey { get; set; } = string.Empty;
    }

    public sealed class SimulationCollectibleCardDrawRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationCollectibleCardTransferRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string OwnerActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationTileTraversalConfirmResponse
    {
        public SimulationWorldExplorationStateSnapshot Exploration { get; set; }
            = new SimulationWorldExplorationStateSnapshot();
        public SimulationCollectibleCardRewardStateSnapshot Rewards { get; set; }
            = new SimulationCollectibleCardRewardStateSnapshot();
        public bool WasNewL2Tile { get; set; }
        public bool WasNewL1Area { get; set; }
        public string[] CreatedOpportunityStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationCollectibleCardDrawResponse
    {
        public SimulationCollectibleCardCopySnapshot DrawnCard { get; set; }
            = new SimulationCollectibleCardCopySnapshot();
        public SimulationCollectibleCardRewardStateSnapshot Rewards { get; set; }
            = new SimulationCollectibleCardRewardStateSnapshot();
    }

    public sealed class SimulationCollectibleCardTransferResponse
    {
        public SimulationCollectibleCardCopySnapshot TransferredCard { get; set; }
            = new SimulationCollectibleCardCopySnapshot();
        public SimulationCollectibleCardRewardStateSnapshot Rewards { get; set; }
            = new SimulationCollectibleCardRewardStateSnapshot();
    }

    public sealed class SimulationWorldExplorationStateSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public SimulationActorTilePositionSnapshot[] ActorTilePositions { get; set; }
            = Array.Empty<SimulationActorTilePositionSnapshot>();
        public string[] RevealedL2TileKeys { get; set; } = Array.Empty<string>();
        public string[] RevealedL1AreaKeys { get; set; } = Array.Empty<string>();
        public SimulationWorldDiscoveryEventSnapshot[] DiscoveryEvents { get; set; }
            = Array.Empty<SimulationWorldDiscoveryEventSnapshot>();
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationActorTilePositionSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string CurrentL2TileKey { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldDiscoveryEventSnapshot
    {
        public string EventStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string TriggerCode { get; set; } = string.Empty;
        public string SpatialUnitKey { get; set; } = string.Empty;
        public int WorldTick { get; set; }
    }

    public sealed class SimulationCollectibleCardRewardStateSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string CatalogRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public SimulationCollectibleCardProbabilityProfileSnapshot ProbabilityProfile { get; set; }
            = new SimulationCollectibleCardProbabilityProfileSnapshot();
        public SimulationCollectibleCardDefinitionSnapshot[] Definitions { get; set; }
            = Array.Empty<SimulationCollectibleCardDefinitionSnapshot>();
        public SimulationCardDrawOpportunitySnapshot[] DrawOpportunities { get; set; }
            = Array.Empty<SimulationCardDrawOpportunitySnapshot>();
        public SimulationCollectibleCardCopySnapshot[] Cards { get; set; }
            = Array.Empty<SimulationCollectibleCardCopySnapshot>();
        public SimulationCollectibleCardPitySnapshot[] PityStates { get; set; }
            = Array.Empty<SimulationCollectibleCardPitySnapshot>();
        public SimulationCollectibleRewardEvaluationSnapshot[] Evaluations { get; set; }
            = Array.Empty<SimulationCollectibleRewardEvaluationSnapshot>();
        public SimulationCollectibleCardTransferSnapshot[] Transfers { get; set; }
            = Array.Empty<SimulationCollectibleCardTransferSnapshot>();
        public bool SupportsRemoteTransfer { get; set; }
        public bool HasExpiry { get; set; }
        public bool PresentationOnly { get; set; }
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationCollectibleCardProbabilityProfileSnapshot
    {
        public decimal FarmBasePercent { get; set; }
        public decimal NewL2BasePercent { get; set; }
        public decimal NewL1BasePercent { get; set; }
        public decimal MatchingActiveRoleBonusPercentagePoints { get; set; }
        public int GuaranteedAfterConsecutiveFailures { get; set; }
    }

    public sealed class SimulationCollectibleCardDefinitionSnapshot
    {
        public string CardDefinitionStableId { get; set; } = string.Empty;
        public string FamilyCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationCardDrawOpportunitySnapshot
    {
        public string OpportunityStableId { get; set; } = string.Empty;
        public string OwnerActorStableId { get; set; } = string.Empty;
        public string FamilyCode { get; set; } = string.Empty;
        public string TriggerCode { get; set; } = string.Empty;
        public string SourceEventStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int AwardedWorldTick { get; set; }
        public decimal AppliedProbabilityPercent { get; set; }
        public decimal ActiveRoleBonusPercentagePoints { get; set; }
        public int PityFailureCountBefore { get; set; }
        public bool WasGuaranteed { get; set; }
        public string DrawnCardCopyStableId { get; set; } = string.Empty;
        public bool HasExpiry { get; set; }
    }

    public sealed class SimulationCollectibleCardCopySnapshot
    {
        public string CardCopyStableId { get; set; } = string.Empty;
        public string CardDefinitionStableId { get; set; } = string.Empty;
        public string OwnerActorStableId { get; set; } = string.Empty;
        public string FamilyCode { get; set; } = string.Empty;
        public string KoreanTitle { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
        public string AcquiredFromOpportunityStableId { get; set; } = string.Empty;
        public int AcquiredWorldTick { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationCollectibleCardPitySnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string FamilyCode { get; set; } = string.Empty;
        public int EligibleAttemptCount { get; set; }
        public int ConsecutiveFailureCount { get; set; }
    }

    public sealed class SimulationCollectibleRewardEvaluationSnapshot
    {
        public string EvaluationStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string FamilyCode { get; set; } = string.Empty;
        public string TriggerCode { get; set; } = string.Empty;
        public string SourceEventStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public int AttemptOrdinal { get; set; }
        public decimal AppliedProbabilityPercent { get; set; }
        public decimal ActiveRoleBonusPercentagePoints { get; set; }
        public int PityFailureCountBefore { get; set; }
        public bool WasGuaranteed { get; set; }
        public decimal DeterministicSamplePercent { get; set; }
        public string ResultCode { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationCollectibleCardTransferSnapshot
    {
        public string TransferStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string FromActorStableId { get; set; } = string.Empty;
        public string ToActorStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
    }
}
