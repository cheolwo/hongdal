using System;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Evidence;

namespace Ssalddel.Unity.Sensors
{
    public static class SensorSourceTypeCodes
    {
        public const string PhysicalSensor = "PhysicalSensor";
        public const string PublicObservation = "PublicObservation";
        public const string SimulatedFixture = "SimulatedFixture";
    }

    public static class SensorConditionCodes
    {
        public const string Normal = "Normal";
        public const string Dry = "Dry";
        public const string Critical = "Critical";
        public const string Waterlogged = "Waterlogged";
        public const string Stale = "Stale";
        public const string Offline = "Offline";
        public const string Unknown = "Unknown";
    }

    public sealed class 농장SensorState
    {
        public string SensorId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string SourceTypeCode { get; set; } = string.Empty;

        public string MeasurementTypeCode { get; set; } = string.Empty;

        public decimal? Value { get; set; }

        public string Unit { get; set; } = string.Empty;

        public DateTimeOffset? ObservedAt { get; set; }

        public string DataStatusCode { get; set; } = string.Empty;

        public string ConditionCode { get; set; } = SensorConditionCodes.Unknown;

        public ProjectionRule근거Reference Interpretation { get; set; } = new ProjectionRule근거Reference();
    }

    public sealed class 외부SensorVisualState
    {
        public string EquipmentStateCode { get; set; } = string.Empty;

        public string IndicatorStateCode { get; set; } = string.Empty;

        public string MaterialStateCode { get; set; } = string.Empty;
    }

    public sealed class SensorProjection
    {
        public string SensorId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string ConditionCode { get; set; } = string.Empty;

        public string[] EvidenceCardIds { get; set; } = Array.Empty<string>();

        public 외부SensorVisualState VisualState { get; set; } = new 외부SensorVisualState();
    }

    public sealed class SensorProjectionResolver
    {
        public SensorProjection Resolve(농장SensorState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            StableDataId.EnsureValid(state.SensorId, nameof(state.SensorId));
            if (state.Revision < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(state.Revision));
            }

            return new SensorProjection
            {
                SensorId = state.SensorId,
                Revision = state.Revision,
                ConditionCode = state.ConditionCode,
                EvidenceCardIds = state.Interpretation.EvidenceCardIds,
                VisualState = ResolveVisual(state.ConditionCode),
            };
        }

        private static 외부SensorVisualState ResolveVisual(string conditionCode)
        {
            switch (conditionCode)
            {
                case SensorConditionCodes.Normal:
                    return External("Online", "Stable", "Normal");
                case SensorConditionCodes.Dry:
                    return External("Online", "Warning", "Dry");
                case SensorConditionCodes.Critical:
                    return External("Online", "Critical", "Critical");
                case SensorConditionCodes.Waterlogged:
                    return External("Online", "Warning", "Waterlogged");
                case SensorConditionCodes.Stale:
                    return External("Stale", "Stale", "Muted");
                case SensorConditionCodes.Offline:
                    return External("Offline", "Off", "Muted");
                default:
                    return External("Unknown", "Unknown", "Neutral");
            }
        }

        private static 외부SensorVisualState External(
            string equipment,
            string indicator,
            string material)
        {
            return new 외부SensorVisualState
            {
                EquipmentStateCode = equipment,
                IndicatorStateCode = indicator,
                MaterialStateCode = material,
            };
        }

    }
}
