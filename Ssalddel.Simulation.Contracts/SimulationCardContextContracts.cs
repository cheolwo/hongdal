using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationCardHierarchyTierCodes
    {
        public const string Meta = "Meta";
        public const string Context = "Context";
        public const string Action = "Action";
        public const string Knowledge = "Knowledge";
        public const string Research = "Research";
    }

    public static class SimulationCardFamilyCodes
    {
        public const string Tarot = "Tarot";
        public const string TurnClosing = "TurnClosing";
        public const string Culture = "Culture";
        public const string TeamRole = "TeamRole";
        public const string BattleSnapshot = "BattleSnapshot";
        public const string ConceptInformation = "ConceptInformation";
        public const string ResearchSeedbed = "ResearchSeedbed";
    }

    public static class SimulationCardAuthorityCodes
    {
        public const string ServerMutable = "ServerMutable";
        public const string ServerFrozenSnapshot = "ServerFrozenSnapshot";
        public const string ProjectionReadOnly = "ProjectionReadOnly";
        public const string ResearchOnly = "ResearchOnly";
    }

    public static class SimulationCardContextRelationCodes
    {
        public const string Relevant = "Relevant";
        public const string Recommended = "Recommended";
        public const string Warned = "Warned";
        public const string Contrasted = "Contrasted";
        public const string AvailabilityExplained = "AvailabilityExplained";
        public const string BlockExplained = "BlockExplained";
    }

    public static class SimulationTarotFrameScopeCodes
    {
        public const string Turn = "Turn";
        public const string Day = "Day";
        public const string Season = "Season";
        public const string Region = "Region";
        public const string Incident = "Incident";
    }

    public static class SimulationTarotThemeCodes
    {
        public const string Growth = "Growth";
        public const string Abundance = "Abundance";
        public const string Nurture = "Nurture";
        public const string Movement = "Movement";
        public const string Balance = "Balance";
        public const string Flow = "Flow";
        public const string Collapse = "Collapse";
        public const string Disruption = "Disruption";
    }

    public static class SimulationTarotContextProposalCodes
    {
        public const string Growth = "Growth";
        public const string Movement = "Movement";
        public const string Balance = "Balance";
        public const string Disruption = "Disruption";
    }

    public static class SimulationTarotIncidentEvaluationResultCodes
    {
        public const string NoIncident = "NoIncident";
        public const string IncidentSelected = "IncidentSelected";
        public const string Blocked = "Blocked";
    }

    public sealed class SimulationTarotFrameSnapshot
    {
        public string FrameStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public string FrameScopeCode { get; set; } = string.Empty;
        public string ScopeTargetStableId { get; set; } = string.Empty;
        public int StartsAtTurnNumber { get; set; }
        public int EndsAtTurnNumber { get; set; }
        public string[] ThemeCodes { get; set; } = Array.Empty<string>();
        public string[] ContextProposalStableIds { get; set; } = Array.Empty<string>();
        public string SourceDrawStableId { get; set; } = string.Empty;
        public string SourceOfferStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationTarotFrameSetSnapshot
    {
        public long Revision { get; set; }
        public long SourceWorldRevision { get; set; }
        public int SourceTurnNumber { get; set; }
        public SimulationTarotFrameSnapshot[] ActiveFrames { get; set; }
            = Array.Empty<SimulationTarotFrameSnapshot>();
        public string FrameSetHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationTarotContextProposalSnapshot
    {
        public string ProposalStableId { get; set; } = string.Empty;
        public string ContextProposalCode { get; set; } = string.Empty;
        public string SourceFrameStableId { get; set; } = string.Empty;
        public string FrameScopeCode { get; set; } = string.Empty;
        public string ScopeTargetStableId { get; set; } = string.Empty;
        public string SourceThemeCode { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public int SourceTurnNumber { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationCardContextRelationSnapshot
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string SourceFrameStableId { get; set; } = string.Empty;
        public string TargetCardFamilyCode { get; set; } = string.Empty;
        public string TargetCardStableId { get; set; } = string.Empty;
        public string TargetCardCopyStableId { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public string RuleRevision { get; set; } = string.Empty;
        public long AvailabilityRevision { get; set; }
        public long SourceWorldRevision { get; set; }
        public int SourceTurnNumber { get; set; }
        public bool ChangesAvailability { get; set; }
    }

    public sealed class SimulationTarotIncidentEvaluationSnapshot
    {
        public string EvaluationStableId { get; set; } = string.Empty;
        public string[] ProposalStableIds { get; set; } = Array.Empty<string>();
        public string EvaluationResultCode { get; set; } = string.Empty;
        public string IncidentStableId { get; set; } = string.Empty;
        public string[] EffectStableIds { get; set; } = Array.Empty<string>();
        public long EvaluatedWorldRevision { get; set; }
        public int EvaluatedTurnNumber { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationTarotContextStateSnapshot
    {
        public SimulationTarotFrameSetSnapshot FrameSet { get; set; } = new();
        public SimulationTarotContextProposalSnapshot[] Proposals { get; set; }
            = Array.Empty<SimulationTarotContextProposalSnapshot>();
        public SimulationCardContextRelationSnapshot[] Relations { get; set; }
            = Array.Empty<SimulationCardContextRelationSnapshot>();
        public SimulationTarotIncidentEvaluationSnapshot[] IncidentEvaluations { get; set; }
            = Array.Empty<SimulationTarotIncidentEvaluationSnapshot>();
        public string ContextStateHashSha256 { get; set; } = string.Empty;
    }
}
