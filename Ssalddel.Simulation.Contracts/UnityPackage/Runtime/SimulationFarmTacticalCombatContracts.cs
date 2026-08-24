using System;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// 1인칭 영웅 전투의 서버 판정 결과를 다음 WorldTick의 전술 명령으로
    /// 연결하는 Simulation 전용 안정 코드다.
    /// </summary>
    public static class SimulationFarmTacticalCombatCodes
    {
        public const string RuleRevision = "farm-combat.hero-tactical-opportunity.r1";

        public const string Rally = "Rally";
        public const string Breakthrough = "Breakthrough";

        public const string AdvanceAndAttack = "AdvanceAndAttack";
        public const string HoldFormation = "HoldFormation";
        public const string TacticalRetreat = "TacticalRetreat";

        public const string Open = "Open";
        public const string Confirmed = "Confirmed";
        public const string Resolved = "Resolved";
        public const string Available = "Available";
        public const string Reserved = "Reserved";
        public const string Consumed = "Consumed";
        public const string Expired = "Expired";

        public const string Allied = "Allied";
        public const string Hostile = "Hostile";
        public const string Perimeter = "Perimeter";
        public const string Forward = "Forward";
        public const string InnerFarm = "InnerFarm";

        public const string TacticalWithdrawal = "TacticalWithdrawal";
    }

    public sealed class SimulationTacticalOrderPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationTacticalOrderConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationTacticalOrderPreviewSnapshot
    {
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
        public int BaseResponseScore { get; set; }
        public int OpportunityBonusScore { get; set; }
        public int PreparednessScore { get; set; }
        public int ProjectedResponseScore { get; set; }
        public string ProjectedFrontPositionCode { get; set; } = string.Empty;
        public int ProjectedCombatStrengthDelta { get; set; }
        public int ProjectedRecoverableInjuryCount { get; set; }
        public decimal ProjectedFacilityDamageUnits { get; set; }
        public decimal ProjectedSupplyLossUnits { get; set; }
        public bool ProjectedDefenseSucceeded { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string PresentationKey { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationFarmTacticalCombatStateSnapshot
    {
        public string RuleRevision { get; set; }
            = SimulationFarmTacticalCombatCodes.RuleRevision;
        public SimulationTacticalFrontSnapshot[] Fronts { get; set; }
            = Array.Empty<SimulationTacticalFrontSnapshot>();
        public SimulationTacticalSquadSnapshot[] Squads { get; set; }
            = Array.Empty<SimulationTacticalSquadSnapshot>();
        public SimulationTacticalOpportunitySnapshot[] Opportunities { get; set; }
            = Array.Empty<SimulationTacticalOpportunitySnapshot>();
        public SimulationTacticalOrderWindowSnapshot[] OrderWindows { get; set; }
            = Array.Empty<SimulationTacticalOrderWindowSnapshot>();
        public SimulationTacticalOrderSnapshot[] Orders { get; set; }
            = Array.Empty<SimulationTacticalOrderSnapshot>();
        public SimulationTacticalResolutionSnapshot[] Resolutions { get; set; }
            = Array.Empty<SimulationTacticalResolutionSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationTacticalFrontSnapshot
    {
        public string FrontStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationTacticalSquadSnapshot
    {
        public string SquadStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int CombatStrength { get; set; }
        public int RecoverableInjuryCount { get; set; }
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationTacticalOpportunitySnapshot
    {
        public string OpportunityStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string SourceReactionStableId { get; set; } = string.Empty;
        public string EarningActorStableId { get; set; } = string.Empty;
        public string OpportunityKindCode { get; set; } = string.Empty;
        public int Quality { get; set; }
        public int CreatedWorldTick { get; set; }
        public int ExpiresWorldTick { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string ReservedOrderStableId { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationTacticalOrderWindowSnapshot
    {
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string AuthorizedActorStableId { get; set; } = string.Empty;
        public int OpenedWorldTick { get; set; }
        public int ClosesWorldTick { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string ConfirmedOrderStableId { get; set; } = string.Empty;
        public string[] AllowedOrderCodes { get; set; } = Array.Empty<string>();
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationTacticalOrderSnapshot
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
        public int ConfirmedWorldTick { get; set; }
        public int ResolvesWorldTick { get; set; }
        public bool AutomaticallySelected { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationTacticalResolutionSnapshot
    {
        public string ResolutionStableId { get; set; } = string.Empty;
        public string OrderStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string ConsumedOpportunityStableId { get; set; } = string.Empty;
        public int ResolvedWorldTick { get; set; }
        public int PreparednessScore { get; set; }
        public int TacticalResponseScore { get; set; }
        public bool DefenseSucceeded { get; set; }
        public string OutcomeCode { get; set; } = string.Empty;
        public string FrontPositionCode { get; set; } = string.Empty;
        public int CombatStrengthDelta { get; set; }
        public int RecoverableInjuryCount { get; set; }
        public decimal FacilityDamageUnits { get; set; }
        public decimal SupplyLossUnits { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
    }
}
