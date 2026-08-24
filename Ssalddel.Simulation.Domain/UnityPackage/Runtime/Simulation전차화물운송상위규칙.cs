using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation전차화물운송상위규칙
    {
        public const string UpperRuleStableId = "tarot-rule:major.chariot.transport.v1";
        public const long UpperRuleRevision = 1;
        public const string SourceCardStableId = "tarot:major.chariot";
        public const string SourceCardRevision = "tarot-card:r1";
        public const string ResponseStableId =
            Simulation전차운송대응StableIds.FastTransport;

        private readonly Simulation타로상위규칙보정기 modifier =
            new Simulation타로상위규칙보정기();

        public Simulation타로운송보정PreviewSnapshot CreateUprightFastTransportPreview(
            Simulation타로운송기준후보Snapshot baseline,
            string sourceTurnClosingStableId)
        {
            ValidateBaseline(baseline);
            RequireStableId(sourceTurnClosingStableId,
                "SimulationTarotTransportTurnClosingInvalid");

            return CreateUprightResponsePreview(
                baseline,
                sourceTurnClosingStableId,
                Simulation전차운송대응StableIds.FastTransport);
        }

        public Simulation타로운송보정PreviewSnapshot CreateUprightResponsePreview(
            Simulation타로운송기준후보Snapshot baseline,
            string sourceTurnClosingStableId,
            string responseStableId)
        {
            ValidateBaseline(baseline);
            RequireStableId(sourceTurnClosingStableId,
                "SimulationTarotTransportTurnClosingInvalid");
            if (responseStableId != Simulation전차운송대응StableIds.FastTransport
                && responseStableId != Simulation전차운송대응StableIds.SafeTransport
                && responseStableId
                    != Simulation전차운송대응StableIds.ConsolidatedTransport)
            {
                throw Error("SimulationTarotTransportResponseInvalid");
            }

            var fast = responseStableId
                == Simulation전차운송대응StableIds.FastTransport;
            var safe = responseStableId
                == Simulation전차운송대응StableIds.SafeTransport;
            var durationModifier = fast ? -1m : 1m;
            var throughputMultiplier = fast ? 1.20m : 1m;
            var fuelMultiplier = fast ? 1.10m : safe ? 1.05m : .85m;
            var laborMultiplier = fast ? 1.10m : safe ? 1.20m : .90m;
            var riskModifier = fast ? 5m : safe ? -4m : -1m;
            var durationMeaning = fast
                ? Simulation타로보정의미Codes.Opportunity
                : Simulation타로보정의미Codes.Burden;
            var fuelMeaning = responseStableId
                == Simulation전차운송대응StableIds.ConsolidatedTransport
                ? Simulation타로보정의미Codes.Opportunity
                : Simulation타로보정의미Codes.Burden;
            var laborMeaning = responseStableId
                == Simulation전차운송대응StableIds.ConsolidatedTransport
                ? Simulation타로보정의미Codes.Opportunity
                : Simulation타로보정의미Codes.Burden;

            var metrics = new[]
            {
                Apply(
                    baseline,
                    Simulation타로운송지표Codes.DurationTicks,
                    baseline.DurationTicks,
                    "turn",
                    1m,
                    30m,
                    Simulation타로보정계산방식Codes.Additive,
                    durationModifier,
                    -2m,
                    2m,
                    durationMeaning,
                    responseStableId,
                    sourceTurnClosingStableId),
                Apply(
                    baseline,
                    Simulation타로운송지표Codes.ThroughputCapacity,
                    baseline.ThroughputCapacity,
                    baseline.QuantityUnitCode,
                    0.01m,
                    baseline.VehicleCapacity,
                    Simulation타로보정계산방식Codes.Multiplier,
                    throughputMultiplier,
                    0.50m,
                    1.50m,
                    Simulation타로보정의미Codes.Opportunity,
                    responseStableId,
                    sourceTurnClosingStableId),
                Apply(
                    baseline,
                    Simulation타로운송지표Codes.FuelConsumption,
                    baseline.FuelConsumption,
                    baseline.FuelUnitCode,
                    0.01m,
                    null,
                    Simulation타로보정계산방식Codes.Multiplier,
                    fuelMultiplier,
                    0.50m,
                    2m,
                    fuelMeaning,
                    responseStableId,
                    sourceTurnClosingStableId),
                Apply(
                    baseline,
                    Simulation타로운송지표Codes.LaborConsumption,
                    baseline.LaborConsumption,
                    baseline.LaborUnitCode,
                    0.01m,
                    null,
                    Simulation타로보정계산방식Codes.Multiplier,
                    laborMultiplier,
                    0.50m,
                    2m,
                    laborMeaning,
                    responseStableId,
                    sourceTurnClosingStableId),
                Apply(
                    baseline,
                    Simulation타로운송지표Codes.RiskPercentPoint,
                    baseline.RiskPercentPoint,
                    "percentage-point",
                    0m,
                    100m,
                    Simulation타로보정계산방식Codes.Additive,
                    riskModifier,
                    -20m,
                    20m,
                    fast
                        ? Simulation타로보정의미Codes.Risk
                        : Simulation타로보정의미Codes.Opportunity,
                    responseStableId,
                    sourceTurnClosingStableId),
            };

            var duration = metrics.Single(value =>
                value.MetricCode == Simulation타로운송지표Codes.DurationTicks);
            if (duration.FinalValue != decimal.Truncate(duration.FinalValue))
                throw Error("SimulationTarotTransportDurationNotWholeTick");

            return new Simulation타로운송보정PreviewSnapshot
            {
                PreviewStableId = "tarot-transport-preview:"
                    + baseline.TransportRequestStableId + ":"
                    + ResponseSlug(responseStableId) + ":"
                    + baseline.CurrentTurnNumber,
                TransportRequestStableId = baseline.TransportRequestStableId,
                UpperRuleStableId = UpperRuleStableId,
                UpperRuleRevision = UpperRuleRevision,
                SourceCardStableId = SourceCardStableId,
                SourceCardRevision = SourceCardRevision,
                CardOrientationCode = Simulation타로카드방향Codes.Upright,
                ResponseStableId = responseStableId,
                ActiveTurnNumber = baseline.CurrentTurnNumber,
                IsCandidateOnly = true,
                DoesNotApplyResourceLedgers = true,
                Metrics = metrics,
                SourceStableIds = baseline.SourceStableIds
                    .Concat(new[]
                    {
                        SourceCardStableId,
                        SourceCardRevision,
                        UpperRuleStableId,
                        sourceTurnClosingStableId,
                    })
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
            };
        }

        private Simulation타로운송지표보정Snapshot Apply(
            Simulation타로운송기준후보Snapshot baseline,
            string metricCode,
            decimal baseValue,
            string valueUnitCode,
            decimal? minimumResult,
            decimal? maximumResult,
            string calculationKind,
            decimal modifierValue,
            decimal minimumModifier,
            decimal maximumModifier,
            string meaningCode,
            string responseStableId,
            string sourceTurnClosingStableId)
        {
            var connectionPoint = new Simulation하위규칙보정연결지점Definition
            {
                ConnectionPointStableId = "modifier-point:transport."
                    + ResponseSlug(responseStableId) + "." + MetricSlug(metricCode),
                LowerRuleStableId = baseline.LowerRuleStableId,
                LowerRuleRevision = baseline.LowerRuleRevision,
                RuleDomainCode = Simulation업무규칙영역Codes.Transport,
                ValueUnitCode = valueUnitCode,
                MinimumResultValue = minimumResult,
                MaximumResultValue = maximumResult,
                AllowedModifiers = new[]
                {
                    new Simulation타로보정허용범위Definition
                    {
                        CalculationKindCode = calculationKind,
                        ModifierUnitCode = calculationKind
                            == Simulation타로보정계산방식Codes.Multiplier
                            ? "ratio"
                            : valueUnitCode,
                        MinimumModifierValue = minimumModifier,
                        MaximumModifierValue = maximumModifier,
                    },
                },
            };
            var line = new Simulation타로규칙보정선Snapshot
            {
                ModifierLineStableId = "tarot-modifier:chariot."
                    + ResponseSlug(responseStableId) + "."
                    + MetricSlug(metricCode),
                UpperRuleStableId = UpperRuleStableId,
                UpperRuleRevision = UpperRuleRevision,
                SourceCardStableId = SourceCardStableId,
                SourceCardRevision = SourceCardRevision,
                CardOrientationCode = Simulation타로카드방향Codes.Upright,
                ResponseStableId = responseStableId,
                TargetConnectionPointStableId = connectionPoint.ConnectionPointStableId,
                TargetRuleDomainCode = Simulation업무규칙영역Codes.Transport,
                CompatibleLowerRuleStableId = baseline.LowerRuleStableId,
                CompatibleLowerRuleRevision = baseline.LowerRuleRevision,
                CalculationKindCode = calculationKind,
                ModifierValue = modifierValue,
                ModifierUnitCode = calculationKind
                    == Simulation타로보정계산방식Codes.Multiplier
                    ? "ratio"
                    : valueUnitCode,
                MeaningCode = meaningCode,
                ActiveFromTurnNumber = baseline.CurrentTurnNumber,
                ActiveThroughTurnNumber = baseline.CurrentTurnNumber,
                SourceTurnClosingStableId = sourceTurnClosingStableId,
                SourceStableIds = new[]
                {
                    SourceCardStableId,
                    UpperRuleStableId,
                    sourceTurnClosingStableId,
                },
            };
            var result = modifier.Apply(new Simulation타로규칙보정적용Request
            {
                CurrentTurnNumber = baseline.CurrentTurnNumber,
                BaseValue = baseValue,
                ConnectionPoint = connectionPoint,
                ModifierLines = new[] { line },
            });
            return new Simulation타로운송지표보정Snapshot
            {
                MetricCode = metricCode,
                BaseValue = result.BaseValue,
                FinalValue = result.FinalValue,
                UnitCode = result.ValueUnitCode,
                ModifierLines = result.AppliedModifierLines,
            };
        }

        private static void ValidateBaseline(Simulation타로운송기준후보Snapshot value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            RequireStableId(value.TransportRequestStableId,
                "SimulationTarotTransportStableIdInvalid");
            RequireStableId(value.LowerRuleStableId,
                "SimulationTarotTransportLowerRuleInvalid");
            if (value.LowerRuleRevision <= 0 || value.CurrentTurnNumber <= 0)
                throw Error("SimulationTarotTransportLowerRuleInvalid");
            if (value.DurationTicks <= 0 || value.DurationTicks > 30
                || value.CargoQuantity <= 0m
                || value.ThroughputCapacity <= 0m
                || value.VehicleCapacity < value.CargoQuantity
                || value.VehicleCapacity < value.ThroughputCapacity
                || value.FuelConsumption <= 0m
                || value.LaborConsumption <= 0m
                || value.RiskPercentPoint < 0m
                || value.RiskPercentPoint > 100m)
            {
                throw Error("SimulationTarotTransportBaselineInvalid");
            }
            RequireStableId(value.QuantityUnitCode, "SimulationTarotTransportUnitInvalid");
            RequireStableId(value.FuelUnitCode, "SimulationTarotTransportUnitInvalid");
            RequireStableId(value.LaborUnitCode, "SimulationTarotTransportUnitInvalid");
            ValidateStableIds(value.SourceStableIds,
                "SimulationTarotTransportSourcesInvalid");
        }

        private static string MetricSlug(string value)
            => value == Simulation타로운송지표Codes.DurationTicks ? "duration-ticks"
                : value == Simulation타로운송지표Codes.ThroughputCapacity ? "throughput-capacity"
                : value == Simulation타로운송지표Codes.FuelConsumption ? "fuel-consumption"
                : value == Simulation타로운송지표Codes.LaborConsumption ? "labor-consumption"
                : "risk-percent-point";

        private static string ResponseSlug(string value)
            => value == Simulation전차운송대응StableIds.FastTransport
                ? "fast-transport"
                : value == Simulation전차운송대응StableIds.SafeTransport
                    ? "safe-transport"
                    : "consolidated-transport";

        private static void ValidateStableIds(string[] values, string errorCode)
        {
            if (values == null || values.Length == 0
                || values.Any(string.IsNullOrWhiteSpace)
                || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
            {
                throw Error(errorCode);
            }
            foreach (var value in values) RequireStableId(value, errorCode);
        }

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                throw Error(errorCode);
            }
        }

        private static SimulationContractException Error(string errorCode)
            => new SimulationContractException(errorCode);
    }
}
