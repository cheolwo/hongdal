using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation전차운송대응StableIds
    {
        public const string FastTransport = "tarot-response:chariot.fast-transport";
        public const string SafeTransport = "tarot-response:chariot.safe-transport";
        public const string ConsolidatedTransport =
            "tarot-response:chariot.consolidated-transport";
    }

    public static class Simulation타로운송지표Codes
    {
        public const string DurationTicks = "DurationTicks";
        public const string ThroughputCapacity = "ThroughputCapacity";
        public const string FuelConsumption = "FuelConsumption";
        public const string LaborConsumption = "LaborConsumption";
        public const string RiskPercentPoint = "RiskPercentPoint";
    }

    public sealed class Simulation타로운송기준후보Snapshot
    {
        public string TransportRequestStableId { get; set; } = string.Empty;
        public string LowerRuleStableId { get; set; } = string.Empty;
        public long LowerRuleRevision { get; set; }
        public int CurrentTurnNumber { get; set; }
        public int DurationTicks { get; set; }
        public decimal CargoQuantity { get; set; }
        public decimal ThroughputCapacity { get; set; }
        public decimal VehicleCapacity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public decimal FuelConsumption { get; set; }
        public string FuelUnitCode { get; set; } = string.Empty;
        public decimal LaborConsumption { get; set; }
        public string LaborUnitCode { get; set; } = string.Empty;
        public decimal RiskPercentPoint { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation타로운송지표보정Snapshot
    {
        public string MetricCode { get; set; } = string.Empty;
        public decimal BaseValue { get; set; }
        public decimal FinalValue { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public Simulation타로규칙보정선Snapshot[] ModifierLines { get; set; }
            = Array.Empty<Simulation타로규칙보정선Snapshot>();
    }

    public sealed class Simulation타로운송보정PreviewSnapshot
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public string TransportRequestStableId { get; set; } = string.Empty;
        public string UpperRuleStableId { get; set; } = string.Empty;
        public long UpperRuleRevision { get; set; }
        public string SourceCardStableId { get; set; } = string.Empty;
        public string SourceCardRevision { get; set; } = string.Empty;
        public string CardOrientationCode { get; set; } = string.Empty;
        public string ResponseStableId { get; set; } = string.Empty;
        public int ActiveTurnNumber { get; set; }
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotApplyResourceLedgers { get; set; }
        public Simulation타로운송지표보정Snapshot[] Metrics { get; set; }
            = Array.Empty<Simulation타로운송지표보정Snapshot>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation타로화물운송PreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string ResponseStableId { get; set; }
            = Simulation전차운송대응StableIds.FastTransport;
        public SimulationFreightTransportPreviewRequest Freight { get; set; }
            = new SimulationFreightTransportPreviewRequest();
    }

    public sealed class Simulation타로화물운송통합PreviewSnapshot
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public long BaseRevision { get; set; }
        public int ActiveTurnNumber { get; set; }
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotApplyResourceLedgers { get; set; }
        public string BaselinePolicyStableId { get; set; } = string.Empty;
        public SimulationFreightTransportPreviewSnapshot LowerRulePreview { get; set; }
            = new SimulationFreightTransportPreviewSnapshot();
        public SimulationActiveTurnCardEffectSnapshot? ActiveTarotCard { get; set; }
        public Simulation타로운송보정PreviewSnapshot? TarotRulePreview { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
