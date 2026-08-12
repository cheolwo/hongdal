using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation업무규칙영역Codes
    {
        public const string Production = "Production";
        public const string Consumption = "Consumption";
        public const string Transport = "Transport";
        public const string Warehouse = "Warehouse";
        public const string Market = "Market";
        public const string Facility = "Facility";
        public const string Time = "Time";
    }

    public static class Simulation자원변동유형Codes
    {
        public const string Production = "Production";
        public const string Consumption = "Consumption";
        public const string Reservation = "Reservation";
        public const string ReservationRelease = "ReservationRelease";
        public const string Transfer = "Transfer";
        public const string Transformation = "Transformation";
        public const string Loss = "Loss";
        public const string Recovery = "Recovery";
        public const string ExternalInflow = "ExternalInflow";
        public const string ExternalOutflow = "ExternalOutflow";
        public const string CapacityChange = "CapacityChange";
        public const string Reconciliation = "Reconciliation";
    }

    public static class Simulation자원효과역할Codes
    {
        public const string Output = "Output";
        public const string Input = "Input";
        public const string Source = "Source";
        public const string Target = "Target";
        public const string Available = "Available";
        public const string Reserved = "Reserved";
        public const string Byproduct = "Byproduct";
        public const string Loss = "Loss";
        public const string Record = "Record";
        public const string Capacity = "Capacity";
    }

    public sealed class Simulation자원효과묶음Snapshot
    {
        public string EffectBundleStableId { get; set; } = string.Empty;
        public string RuleStableId { get; set; } = string.Empty;
        public long RuleRevision { get; set; }
        public string RuleDomainCode { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationEffectStateCodes.Pending;
        public string CausedByDecisionStableId { get; set; } = string.Empty;
        public string CausedByTaskStableId { get; set; } = string.Empty;
        public int? AppliedTick { get; set; }
        public Simulation자원효과선Snapshot[] Lines { get; set; }
            = Array.Empty<Simulation자원효과선Snapshot>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation자원효과선Snapshot
    {
        public string EffectLineStableId { get; set; } = string.Empty;
        public string MutationKindCode { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string ResourceTypeCode { get; set; } = string.Empty;
        public string TargetLedgerStableId { get; set; } = string.Empty;
        public string? ProductStableId { get; set; }
        public string? LotStableId { get; set; }
        public decimal BeforeValue { get; set; }
        public decimal Delta { get; set; }
        public decimal AfterValue { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string? ConservationGroupStableId { get; set; }
        public decimal ConservationQuantity { get; set; }
        public string? ConservationUnitCode { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation자원원장상태Snapshot
    {
        public long Revision { get; set; }
        public int WorldTick { get; set; }
        public Simulation자원원장항목Snapshot[] Ledgers { get; set; }
            = Array.Empty<Simulation자원원장항목Snapshot>();
        public string[] AppliedEffectBundleStableIds { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation자원원장항목Snapshot
    {
        public string LedgerStableId { get; set; } = string.Empty;
        public string ResourceTypeCode { get; set; } = string.Empty;
        public string? ProductStableId { get; set; }
        public string? LotStableId { get; set; }
        public decimal Value { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation자원효과적용Result
    {
        public Simulation자원원장상태Snapshot State { get; set; }
            = new Simulation자원원장상태Snapshot();
        public Simulation자원효과묶음Snapshot AppliedEffectBundle { get; set; }
            = new Simulation자원효과묶음Snapshot();
    }
}
