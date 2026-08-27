using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation타로방향결정정책Codes
    {
        public const string SeededHash = "SeededHash";
        public const string RecoveryShare51 = "RecoveryShare51";
    }

    public static class Simulation메이저아르카나활성상태Codes
    {
        public const string Active = "Active";
        public const string Ended = "Ended";
    }

    public static class Simulation메이저아르카나종료이유Codes
    {
        public const string Replaced = "Replaced";
        public const string Deactivated = "Deactivated";
    }

    public static class Simulation상위아르카나영향방식Codes
    {
        public const string Numeric = "Numeric";
        public const string Interpretive = "Interpretive";
        public const string Ordering = "Ordering";
        public const string DirectionOnly = "DirectionOnly";
    }

    public static class Simulation상위아르카나해석Codes
    {
        public const string OpportunityEmphasis = "OpportunityEmphasis";
        public const string RiskEmphasis = "RiskEmphasis";
    }

    public static class SimulationTown생활복구Codes
    {
        public const string ApprovedFixtureProfile = "town-npc-life:approved-fixture.v1";
        public const string RuleRevision = "town-npc-life-recovery.r1";
        public const string ContentionRuleRevision = "town-item-contention.r1";
        public const string InfluencePolicyRevision = "arcana-lower-card-influence.r1";
        public const string EffectBindingCode = "effect-binding:town:resident-life-recovery";
        public const string ResidentLifeCardStableId = "team-role-card:town:resident-life-recovery";
        public const string ClerkCardStableId = "team-role-card:town:clerk-order-fulfillment";
        public const string ClerkNpcStableId = "npc:town:clerk-01";
        public const string ResidentAStableId = "npc:town:resident-a";
        public const string ResidentBStableId = "npc:town:resident-b";
        public const string EmergencyFoodItemStableId = "item:town:emergency-food-pack";
        public const string PortableBatteryItemStableId = "item:town:portable-battery-pack";
        public const string WeatherproofTarpItemStableId = "item:town:weatherproof-tarp";
    }

    public static class SimulationTown욕구Codes
    {
        public const string Sustenance = "Sustenance";
        public const string Utility = "Utility";
        public const string Shelter = "Shelter";
    }

    public static class SimulationTown목표상태Codes
    {
        public const string Selected = "Selected";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string NoEligibleGoal = "NoEligibleGoal";
    }

    public static class SimulationTown주문단계Codes
    {
        public const string Reserved = "Reserved";
        public const string Picked = "Picked";
        public const string Packed = "Packed";
        public const string ReadyForPickup = "ReadyForPickup";
        public const string Received = "Received";
        public const string Consumed = "Consumed";
    }

    public sealed class Simulation메이저아르카나선택Snapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string OfferStableId { get; set; } = string.Empty;
        public string SelectionSourceCode { get; set; } = string.Empty;
        public long SelectedAtWorldRevision { get; set; }
        public int SelectedAtWorldTick { get; set; }
    }

    public sealed class Simulation메이저아르카나방향판정Snapshot
    {
        public string DirectionCode { get; set; } = string.Empty;
        public long RecoveryShareMicro { get; set; }
        public decimal RecoveryOutput { get; set; }
        public decimal ThreatOutput { get; set; }
        public string ContextPlayerStableId { get; set; } = string.Empty;
        public long EvidenceRevision { get; set; }
        public string EvidenceHashSha256 { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public long DecidedAtWorldRevision { get; set; }
        public int DecidedAtWorldTick { get; set; }
    }

    public sealed class Simulation메이저아르카나활성Snapshot
    {
        public string MajorArcanaActivationStableId { get; set; } = string.Empty;
        public int ActivationSequence { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public Simulation메이저아르카나선택Snapshot Selection { get; set; } = new();
        public Simulation메이저아르카나방향판정Snapshot OrientationDecision { get; set; }
            = new();
        public long ActivatedAtWorldRevision { get; set; }
        public int ActivatedAtWorldTick { get; set; }
        public long? EndedAtWorldRevision { get; set; }
        public int? EndedAtWorldTick { get; set; }
        public string EndReasonCode { get; set; } = string.Empty;
        public string SupersededByActivationStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation상위아르카나영향PolicyDefinition
    {
        public string TargetCardFamilyCode { get; set; } = string.Empty;
        public string InfluenceModeCode { get; set; } = string.Empty;
        public string[] AllowedEffectBindingCodes { get; set; } = Array.Empty<string>();
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation상위아르카나방향상속Snapshot
    {
        public string InheritanceStableId { get; set; } = string.Empty;
        public string MajorArcanaActivationStableId { get; set; } = string.Empty;
        public string SourceCardStableId { get; set; } = string.Empty;
        public string DirectionCode { get; set; } = string.Empty;
        public string TargetCardFamilyCode { get; set; } = string.Empty;
        public string TargetCardStableId { get; set; } = string.Empty;
        public string TargetCardCopyStableId { get; set; } = string.Empty;
        public string InfluenceModeCode { get; set; } = string.Empty;
        public decimal? NumericMultiplier { get; set; }
        public string InterpretationCode { get; set; } = string.Empty;
        public string[] AllowedEffectBindingCodes { get; set; } = Array.Empty<string>();
        public string InfluencePolicyRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationEffect배율계보Snapshot
    {
        public string BreakdownStableId { get; set; } = string.Empty;
        public string EffectBindingCode { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public decimal LowerCardMultiplier { get; set; } = 1m;
        public decimal ArcanaOrientationMultiplier { get; set; } = 1m;
        public decimal PsychologicalPeriodMultiplier { get; set; } = 1m;
        public decimal RawMultiplier { get; set; } = 1m;
        public decimal ClampedMultiplier { get; set; } = 1m;
        public decimal FinalValue { get; set; }
        public string ValueUnitCode { get; set; } = string.Empty;
        public string MajorArcanaActivationStableId { get; set; } = string.Empty;
        public string InheritanceStableId { get; set; } = string.Empty;
        public string LowerCardStableId { get; set; } = string.Empty;
        public string PeriodStateCode { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public long PeriodRevision { get; set; }
        public string PeriodStateHashSha256 { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationTown욕구Snapshot
    {
        public string NeedCode { get; set; } = string.Empty;
        public decimal Severity { get; set; }
        public long Revision { get; set; }
    }

    public sealed class SimulationTown물품Snapshot
    {
        public string ItemStableId { get; set; } = string.Empty;
        public string KoreanName { get; set; } = string.Empty;
        public string ItemRoleCode { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public decimal BaseLifeRecovery { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ReservedByNpcStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
    }

    public sealed class SimulationTown목표Snapshot
    {
        public string GoalStableId { get; set; } = string.Empty;
        public string NpcStableId { get; set; } = string.Empty;
        public string NeedCode { get; set; } = string.Empty;
        public decimal SourceSeverity { get; set; }
        public string TargetItemStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public int SelectedAtWorldTick { get; set; }
        public int? CompletedAtWorldTick { get; set; }
        public long Revision { get; set; }
    }

    public sealed class SimulationTown주문Snapshot
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string NpcStableId { get; set; } = string.Empty;
        public string GoalStableId { get; set; } = string.Empty;
        public string ItemStableId { get; set; } = string.Empty;
        public string StageCode { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public string[] WorldInteractionHistoryIds { get; set; }
            = Array.Empty<string>();
        public string AssignedClerkNpcStableId { get; set; } = string.Empty;
        public long RequestedAtWorldRevision { get; set; }
        public int RequestedAtWorldTick { get; set; }
        public int StageChangedAtWorldTick { get; set; }
        public long Revision { get; set; }
        public SimulationEffect배율계보Snapshot? ConsumptionBreakdown { get; set; }
    }

    public sealed class SimulationTownNpcLifeSnapshot
    {
        public string NpcStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public SimulationTown욕구Snapshot[] Needs { get; set; }
            = Array.Empty<SimulationTown욕구Snapshot>();
        public string CurrentGoalStableId { get; set; } = string.Empty;
        public string CurrentGoalStateCode { get; set; } = string.Empty;
        public string CurrentOrderStableId { get; set; } = string.Empty;
        public string CurrentOrderStageCode { get; set; } = string.Empty;
        public string ReservedItemStableId { get; set; } = string.Empty;
        public string LastConsumedItemStableId { get; set; } = string.Empty;
        public string NextGoalReasonCode { get; set; } = string.Empty;
        public long Revision { get; set; }
    }

    public sealed class SimulationTownNpcLifeStateSnapshot
    {
        public string ProfileStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string ContentionRuleRevision { get; set; } = string.Empty;
        public string ContextPlayerStableId { get; set; } = string.Empty;
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
        public SimulationTown물품Snapshot[] Items { get; set; }
            = Array.Empty<SimulationTown물품Snapshot>();
        public SimulationTownNpcLifeSnapshot[] Npcs { get; set; }
            = Array.Empty<SimulationTownNpcLifeSnapshot>();
        public SimulationTown목표Snapshot[] Goals { get; set; }
            = Array.Empty<SimulationTown목표Snapshot>();
        public SimulationTown주문Snapshot[] Orders { get; set; }
            = Array.Empty<SimulationTown주문Snapshot>();
        public string StateHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
