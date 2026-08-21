using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationNatureMindCodes
    {
        public const string RuleRevision = "nature-mind-balance.r1";
        public const string DefaultPlayerStableId = "player:session:owner";
        public const string RecoveryAxis = "Recovery";
        public const string ThreatAxis = "Threat";
        public const string MixedBand = "Mixed";
        public const string RecoveryDominantBand = "RecoveryDominant";
        public const string ThreatDominantBand = "ThreatDominant";
        public const string FarmHarvestDispositionCompleted =
            "FarmHarvestDispositionCompleted";
        public const string FarmStorageFact = "fact:farm-storage-utilization";
    }

    public static class SimulationNaturePeriodCodes
    {
        public const string RuleRevision = "nature-period-state.r1";
        public const string ExitThresholdPolicyRevision =
            "nature-period-exit-threshold.r1";
        public const string OrdinaryPeriod = "OrdinaryPeriod";
        public const string GwangbokPeriod = "GwangbokPeriod";
        public const string DarkAgePeriod = "DarkAgePeriod";
        public const string EnteredEffect = "NaturePeriodEnteredEffect";
        public const string ExitedEffect = "NaturePeriodExitedEffect";
        public const string GwangbokEntryReason = "RecoveryShareAtLeast80Percent";
        public const string DarkAgeEntryReason = "ThreatShareAtLeast80Percent";
        public const string OrdinaryFallbackReason = "SpecialPeriodThresholdNotMet";
        public const string GwangbokExitReason = "RecoveryShareBelow75Percent";
        public const string DarkAgeExitReason = "ThreatShareBelow75Percent";
        public const string GwangbokRevelationCandidate =
            "revelation:nature.farm-storage-opportunity";
        public const string DarkAgeRecoveryWorldInteraction = "WI-NATURE-04";
    }

    public sealed class SimulationNatureMindInitialStateRequest
    {
        public string RuleRevision { get; set; } = SimulationNatureMindCodes.RuleRevision;
        public SimulationNatureMindPlayerInitialStateRequest[] Players { get; set; }
            = Array.Empty<SimulationNatureMindPlayerInitialStateRequest>();
    }

    public sealed class SimulationNatureMindPlayerInitialStateRequest
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public decimal RecoveryBaseOutput { get; set; }
        public decimal ThreatBaseOutput { get; set; }
    }

    public sealed class SimulationMindImpactEffectSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string AxisCode { get; set; } = string.Empty;
        public decimal Magnitude { get; set; }
        public int AppliedWorldTick { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureMindContributorSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public decimal Magnitude { get; set; }
    }

    public sealed class SimulationNatureMindBalanceSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public decimal RecoveryOutput { get; set; }
        public decimal ThreatOutput { get; set; }
        public decimal RecoveryShare { get; set; }
        public decimal ThreatShare { get; set; }
        public decimal InterpretationStrength { get; set; }
        public string InterpretationBandCode { get; set; } = string.Empty;
        public SimulationNatureMindContributorSnapshot[] TopRecoveryContributors
            { get; set; } = Array.Empty<SimulationNatureMindContributorSnapshot>();
        public SimulationNatureMindContributorSnapshot[] TopThreatContributors
            { get; set; } = Array.Empty<SimulationNatureMindContributorSnapshot>();
        public long Revision { get; set; }
        public string BalanceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureMindStateSnapshot
    {
        public string RuleRevision { get; set; } = string.Empty;
        public SimulationNatureMindBalanceSnapshot[] Balances { get; set; }
            = Array.Empty<SimulationNatureMindBalanceSnapshot>();
        public SimulationMindImpactEffectSnapshot[] Effects { get; set; }
            = Array.Empty<SimulationMindImpactEffectSnapshot>();
        public SimulationNaturePeriodStateSnapshot[] Periods { get; set; }
            = Array.Empty<SimulationNaturePeriodStateSnapshot>();
        public SimulationNaturePeriodHistorySnapshot[] PeriodHistory { get; set; }
            = Array.Empty<SimulationNaturePeriodHistorySnapshot>();
        public SimulationNaturePeriodTransitionEffectSnapshot[] PeriodTransitionEffects
            { get; set; } = Array.Empty<SimulationNaturePeriodTransitionEffectSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNaturePeriodStateSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string PeriodStateCode { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public long SourceBalanceRevision { get; set; }
        public string SourceBalanceHashSha256 { get; set; } = string.Empty;
        public int EnteredAtWorldTick { get; set; }
        public string EnterReasonCode { get; set; } = string.Empty;
        public string ExitThresholdPolicyRevision { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string PeriodStateHashSha256 { get; set; } = string.Empty;
        public int BaseRecoveryWorkDurationTicks { get; set; }
        public int EffectiveRecoveryWorkDurationTicks { get; set; }
        public int WorkDurationModifierTicks { get; set; }
        public string[] CandidateStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNaturePeriodHistorySnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int EnterTick { get; set; }
        public int? ExitTick { get; set; }
        public string[] MajorOutcomeRefs { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationNaturePeriodTransitionEffectSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string EffectTypeCode { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string PeriodInstanceStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public string SourceBalanceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureFarmInterpretationSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string FactStableId { get; set; } = string.Empty;
        public decimal FactValue { get; set; }
        public string FactUnitCode { get; set; } = string.Empty;
        public string FactStateHashSha256 { get; set; } = string.Empty;
        public string InferenceCode { get; set; } = string.Empty;
        public string InferenceText { get; set; } = string.Empty;
        public string MoodProjectionCode { get; set; } = string.Empty;
        public string[] PrioritizedCardStableIds { get; set; } = Array.Empty<string>();
        public SimulationNatureMindBalanceSnapshot Balance { get; set; }
            = new SimulationNatureMindBalanceSnapshot();
        public SimulationNaturePeriodStateSnapshot Period { get; set; }
            = new SimulationNaturePeriodStateSnapshot();
        public bool ChangesSharedFact { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
