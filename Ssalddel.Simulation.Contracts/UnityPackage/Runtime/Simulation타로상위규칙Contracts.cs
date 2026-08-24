using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation타로카드방향Codes
    {
        public const string Upright = "Upright";
        public const string Reversed = "Reversed";
    }

    public static class Simulation타로보정계산방식Codes
    {
        public const string Additive = "Additive";
        public const string Multiplier = "Multiplier";
        public const string MinimumClamp = "MinimumClamp";
        public const string MaximumClamp = "MaximumClamp";
    }

    public static class Simulation타로보정의미Codes
    {
        public const string Opportunity = "Opportunity";
        public const string Burden = "Burden";
        public const string Risk = "Risk";
    }

    public sealed class Simulation타로보정허용범위Definition
    {
        public string CalculationKindCode { get; set; } = string.Empty;
        public string ModifierUnitCode { get; set; } = string.Empty;
        public decimal MinimumModifierValue { get; set; }
        public decimal MaximumModifierValue { get; set; }
    }

    public sealed class Simulation하위규칙보정연결지점Definition
    {
        public string ConnectionPointStableId { get; set; } = string.Empty;
        public string LowerRuleStableId { get; set; } = string.Empty;
        public long LowerRuleRevision { get; set; }
        public string RuleDomainCode { get; set; } = string.Empty;
        public string ValueUnitCode { get; set; } = string.Empty;
        public decimal? MinimumResultValue { get; set; }
        public decimal? MaximumResultValue { get; set; }
        public Simulation타로보정허용범위Definition[] AllowedModifiers { get; set; }
            = Array.Empty<Simulation타로보정허용범위Definition>();
    }

    public sealed class Simulation타로규칙보정선Snapshot
    {
        public string ModifierLineStableId { get; set; } = string.Empty;
        public string UpperRuleStableId { get; set; } = string.Empty;
        public long UpperRuleRevision { get; set; }
        public string SourceCardStableId { get; set; } = string.Empty;
        public string SourceCardRevision { get; set; } = string.Empty;
        public string CardOrientationCode { get; set; } = string.Empty;
        public string ResponseStableId { get; set; } = string.Empty;
        public string TargetConnectionPointStableId { get; set; } = string.Empty;
        public string TargetRuleDomainCode { get; set; } = string.Empty;
        public string CompatibleLowerRuleStableId { get; set; } = string.Empty;
        public long CompatibleLowerRuleRevision { get; set; }
        public string CalculationKindCode { get; set; } = string.Empty;
        public decimal ModifierValue { get; set; }
        public string ModifierUnitCode { get; set; } = string.Empty;
        public string MeaningCode { get; set; } = string.Empty;
        public int ActiveFromTurnNumber { get; set; }
        public int ActiveThroughTurnNumber { get; set; }
        public string SourceTurnClosingStableId { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation타로규칙보정적용Request
    {
        public int CurrentTurnNumber { get; set; }
        public decimal BaseValue { get; set; }
        public Simulation하위규칙보정연결지점Definition ConnectionPoint { get; set; }
            = new Simulation하위규칙보정연결지점Definition();
        public Simulation타로규칙보정선Snapshot[] ModifierLines { get; set; }
            = Array.Empty<Simulation타로규칙보정선Snapshot>();
    }

    public sealed class Simulation타로규칙보정적용Result
    {
        public string ConnectionPointStableId { get; set; } = string.Empty;
        public string LowerRuleStableId { get; set; } = string.Empty;
        public long LowerRuleRevision { get; set; }
        public decimal BaseValue { get; set; }
        public decimal AdditiveTotal { get; set; }
        public decimal MultiplierProduct { get; set; } = 1m;
        public decimal? AppliedMinimumClamp { get; set; }
        public decimal? AppliedMaximumClamp { get; set; }
        public decimal FinalValue { get; set; }
        public string ValueUnitCode { get; set; } = string.Empty;
        public Simulation타로규칙보정선Snapshot[] AppliedModifierLines { get; set; }
            = Array.Empty<Simulation타로규칙보정선Snapshot>();
    }
}
