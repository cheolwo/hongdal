using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation타로상위규칙보정기
    {
        private static readonly HashSet<string> KnownRuleDomains = new HashSet<string>(
            new[]
            {
                Simulation업무규칙영역Codes.Production,
                Simulation업무규칙영역Codes.Consumption,
                Simulation업무규칙영역Codes.Transport,
                Simulation업무규칙영역Codes.Warehouse,
                Simulation업무규칙영역Codes.Market,
                Simulation업무규칙영역Codes.Facility,
                Simulation업무규칙영역Codes.Time,
            },
            StringComparer.Ordinal);

        private static readonly HashSet<string> KnownCalculationKinds = new HashSet<string>(
            new[]
            {
                Simulation타로보정계산방식Codes.Additive,
                Simulation타로보정계산방식Codes.Multiplier,
                Simulation타로보정계산방식Codes.MinimumClamp,
                Simulation타로보정계산방식Codes.MaximumClamp,
            },
            StringComparer.Ordinal);

        private static readonly HashSet<string> KnownMeanings = new HashSet<string>(
            new[]
            {
                Simulation타로보정의미Codes.Opportunity,
                Simulation타로보정의미Codes.Burden,
                Simulation타로보정의미Codes.Risk,
            },
            StringComparer.Ordinal);

        public Simulation타로규칙보정적용Result Apply(
            Simulation타로규칙보정적용Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.CurrentTurnNumber <= 0)
                throw Error("SimulationTarotModifierTurnInvalid");

            var allowedByKind = ValidateConnectionPoint(request.ConnectionPoint);
            ValidateBaseValue(request.BaseValue, request.ConnectionPoint);
            if (request.ModifierLines == null)
                throw Error("SimulationTarotModifierLinesInvalid");
            if (request.ModifierLines.Length == 0)
                return Unchanged(request);

            ValidateLines(request, allowedByKind);

            try
            {
                var additiveTotal = request.ModifierLines
                    .Where(value => value.CalculationKindCode
                        == Simulation타로보정계산방식Codes.Additive)
                    .Sum(value => value.ModifierValue);
                var multiplierProduct = request.ModifierLines
                    .Where(value => value.CalculationKindCode
                        == Simulation타로보정계산방식Codes.Multiplier)
                    .Aggregate(1m, (current, value) => current * value.ModifierValue);
                var minimumClamp = OptionalMaximum(request.ModifierLines
                    .Where(value => value.CalculationKindCode
                        == Simulation타로보정계산방식Codes.MinimumClamp)
                    .Select(value => value.ModifierValue));
                var maximumClamp = OptionalMinimum(request.ModifierLines
                    .Where(value => value.CalculationKindCode
                        == Simulation타로보정계산방식Codes.MaximumClamp)
                    .Select(value => value.ModifierValue));
                if (minimumClamp.HasValue && maximumClamp.HasValue
                    && minimumClamp.Value > maximumClamp.Value)
                {
                    throw Error("SimulationTarotModifierClampConflict");
                }

                var finalValue = (request.BaseValue + additiveTotal) * multiplierProduct;
                if (minimumClamp.HasValue && finalValue < minimumClamp.Value)
                    finalValue = minimumClamp.Value;
                if (maximumClamp.HasValue && finalValue > maximumClamp.Value)
                    finalValue = maximumClamp.Value;
                ValidateResultValue(finalValue, request.ConnectionPoint);

                return new Simulation타로규칙보정적용Result
                {
                    ConnectionPointStableId = request.ConnectionPoint.ConnectionPointStableId,
                    LowerRuleStableId = request.ConnectionPoint.LowerRuleStableId,
                    LowerRuleRevision = request.ConnectionPoint.LowerRuleRevision,
                    BaseValue = request.BaseValue,
                    AdditiveTotal = additiveTotal,
                    MultiplierProduct = multiplierProduct,
                    AppliedMinimumClamp = minimumClamp,
                    AppliedMaximumClamp = maximumClamp,
                    FinalValue = finalValue,
                    ValueUnitCode = request.ConnectionPoint.ValueUnitCode,
                    AppliedModifierLines = request.ModifierLines
                        .OrderBy(value => CalculationOrder(value.CalculationKindCode))
                        .ThenBy(value => value.ModifierLineStableId, StringComparer.Ordinal)
                        .Select(CloneLine)
                        .ToArray(),
                };
            }
            catch (OverflowException)
            {
                throw Error("SimulationTarotModifierOverflow");
            }
        }

        private static Dictionary<string, Simulation타로보정허용범위Definition>
            ValidateConnectionPoint(Simulation하위규칙보정연결지점Definition value)
        {
            if (value == null)
                throw Error("SimulationTarotModifierConnectionPointInvalid");
            RequireStableId(value.ConnectionPointStableId,
                "SimulationTarotModifierConnectionPointInvalid");
            RequireStableId(value.LowerRuleStableId,
                "SimulationTarotModifierLowerRuleInvalid");
            if (value.LowerRuleRevision <= 0)
                throw Error("SimulationTarotModifierLowerRuleInvalid");
            if (!KnownRuleDomains.Contains(value.RuleDomainCode))
                throw Error("SimulationTarotModifierRuleDomainInvalid");
            RequireStableId(value.ValueUnitCode, "SimulationTarotModifierValueUnitInvalid");
            if (value.MinimumResultValue.HasValue && value.MaximumResultValue.HasValue
                && value.MinimumResultValue.Value > value.MaximumResultValue.Value)
            {
                throw Error("SimulationTarotModifierResultRangeInvalid");
            }
            if (value.AllowedModifiers == null || value.AllowedModifiers.Length == 0)
                throw Error("SimulationTarotModifierAllowedRangesMissing");

            var result = new Dictionary<string, Simulation타로보정허용범위Definition>(
                StringComparer.Ordinal);
            foreach (var allowed in value.AllowedModifiers)
            {
                if (allowed == null || !KnownCalculationKinds.Contains(allowed.CalculationKindCode)
                    || allowed.MinimumModifierValue > allowed.MaximumModifierValue)
                {
                    throw Error("SimulationTarotModifierAllowedRangeInvalid");
                }
                RequireStableId(allowed.ModifierUnitCode,
                    "SimulationTarotModifierAllowedRangeInvalid");
                var expectedUnit = allowed.CalculationKindCode
                    == Simulation타로보정계산방식Codes.Multiplier
                    ? "ratio"
                    : value.ValueUnitCode;
                if (!string.Equals(allowed.ModifierUnitCode, expectedUnit, StringComparison.Ordinal)
                    || (allowed.CalculationKindCode
                            == Simulation타로보정계산방식Codes.Multiplier
                        && allowed.MinimumModifierValue <= 0m)
                    || !result.TryAdd(allowed.CalculationKindCode, allowed))
                {
                    throw Error("SimulationTarotModifierAllowedRangeInvalid");
                }
            }
            return result;
        }

        private static void ValidateLines(
            Simulation타로규칙보정적용Request request,
            Dictionary<string, Simulation타로보정허용범위Definition> allowedByKind)
        {
            var lineIds = new HashSet<string>(StringComparer.Ordinal);
            Simulation타로규칙보정선Snapshot? first = null;
            foreach (var line in request.ModifierLines)
            {
                if (line == null) throw Error("SimulationTarotModifierLineInvalid");
                RequireStableId(line.ModifierLineStableId, "SimulationTarotModifierLineInvalid");
                RequireStableId(line.UpperRuleStableId, "SimulationTarotModifierUpperRuleInvalid");
                if (line.UpperRuleRevision <= 0)
                    throw Error("SimulationTarotModifierUpperRuleInvalid");
                RequireStableId(line.SourceCardStableId, "SimulationTarotModifierCardInvalid");
                RequireStableId(line.SourceCardRevision, "SimulationTarotModifierCardInvalid");
                if (line.CardOrientationCode != Simulation타로카드방향Codes.Upright
                    && line.CardOrientationCode != Simulation타로카드방향Codes.Reversed)
                {
                    throw Error("SimulationTarotModifierCardOrientationInvalid");
                }
                RequireStableId(line.ResponseStableId, "SimulationTarotModifierResponseInvalid");
                RequireStableId(line.SourceTurnClosingStableId,
                    "SimulationTarotModifierTurnClosingInvalid");
                ValidateStableIds(line.SourceStableIds, "SimulationTarotModifierSourcesInvalid");
                if (!KnownMeanings.Contains(line.MeaningCode))
                    throw Error("SimulationTarotModifierMeaningInvalid");
                if (line.ActiveFromTurnNumber <= 0
                    || line.ActiveThroughTurnNumber < line.ActiveFromTurnNumber
                    || request.CurrentTurnNumber < line.ActiveFromTurnNumber
                    || request.CurrentTurnNumber > line.ActiveThroughTurnNumber)
                {
                    throw Error("SimulationTarotModifierInactive");
                }
                if (!lineIds.Add(line.ModifierLineStableId))
                    throw Error("SimulationTarotModifierLineDuplicate");
                ValidateTarget(line, request.ConnectionPoint);
                if (!allowedByKind.TryGetValue(line.CalculationKindCode, out var allowed)
                    || line.ModifierUnitCode != allowed.ModifierUnitCode
                    || line.ModifierValue < allowed.MinimumModifierValue
                    || line.ModifierValue > allowed.MaximumModifierValue)
                {
                    throw Error("SimulationTarotModifierNotAllowed");
                }

                if (first == null)
                {
                    first = line;
                }
                else if (first.UpperRuleStableId != line.UpperRuleStableId
                    || first.UpperRuleRevision != line.UpperRuleRevision
                    || first.SourceCardStableId != line.SourceCardStableId
                    || first.SourceCardRevision != line.SourceCardRevision
                    || first.CardOrientationCode != line.CardOrientationCode
                    || first.ResponseStableId != line.ResponseStableId
                    || first.SourceTurnClosingStableId != line.SourceTurnClosingStableId)
                {
                    throw Error("SimulationTarotModifierMultipleActiveCardsNotSupported");
                }
            }
        }

        private static void ValidateTarget(
            Simulation타로규칙보정선Snapshot line,
            Simulation하위규칙보정연결지점Definition target)
        {
            if (line.TargetConnectionPointStableId != target.ConnectionPointStableId
                || line.TargetRuleDomainCode != target.RuleDomainCode
                || line.CompatibleLowerRuleStableId != target.LowerRuleStableId
                || line.CompatibleLowerRuleRevision != target.LowerRuleRevision)
            {
                throw Error("SimulationTarotModifierTargetMismatch");
            }
        }

        private static void ValidateBaseValue(
            decimal value,
            Simulation하위규칙보정연결지점Definition target)
        {
            if ((target.MinimumResultValue.HasValue && value < target.MinimumResultValue.Value)
                || (target.MaximumResultValue.HasValue && value > target.MaximumResultValue.Value))
            {
                throw Error("SimulationTarotModifierBaseValueOutOfRange");
            }
        }

        private static void ValidateResultValue(
            decimal value,
            Simulation하위규칙보정연결지점Definition target)
        {
            if ((target.MinimumResultValue.HasValue && value < target.MinimumResultValue.Value)
                || (target.MaximumResultValue.HasValue && value > target.MaximumResultValue.Value))
            {
                throw Error("SimulationTarotModifierResultOutOfRange");
            }
        }

        private static Simulation타로규칙보정적용Result Unchanged(
            Simulation타로규칙보정적용Request request)
            => new Simulation타로규칙보정적용Result
            {
                ConnectionPointStableId = request.ConnectionPoint.ConnectionPointStableId,
                LowerRuleStableId = request.ConnectionPoint.LowerRuleStableId,
                LowerRuleRevision = request.ConnectionPoint.LowerRuleRevision,
                BaseValue = request.BaseValue,
                FinalValue = request.BaseValue,
                ValueUnitCode = request.ConnectionPoint.ValueUnitCode,
            };

        private static int CalculationOrder(string value)
            => value == Simulation타로보정계산방식Codes.Additive ? 0
                : value == Simulation타로보정계산방식Codes.Multiplier ? 1
                : value == Simulation타로보정계산방식Codes.MinimumClamp ? 2
                : 3;

        private static decimal? OptionalMaximum(IEnumerable<decimal> values)
        {
            var materialized = values.ToArray();
            return materialized.Length == 0 ? (decimal?)null : materialized.Max();
        }

        private static decimal? OptionalMinimum(IEnumerable<decimal> values)
        {
            var materialized = values.ToArray();
            return materialized.Length == 0 ? (decimal?)null : materialized.Min();
        }

        private static Simulation타로규칙보정선Snapshot CloneLine(
            Simulation타로규칙보정선Snapshot value)
            => new Simulation타로규칙보정선Snapshot
            {
                ModifierLineStableId = value.ModifierLineStableId,
                UpperRuleStableId = value.UpperRuleStableId,
                UpperRuleRevision = value.UpperRuleRevision,
                SourceCardStableId = value.SourceCardStableId,
                SourceCardRevision = value.SourceCardRevision,
                CardOrientationCode = value.CardOrientationCode,
                ResponseStableId = value.ResponseStableId,
                TargetConnectionPointStableId = value.TargetConnectionPointStableId,
                TargetRuleDomainCode = value.TargetRuleDomainCode,
                CompatibleLowerRuleStableId = value.CompatibleLowerRuleStableId,
                CompatibleLowerRuleRevision = value.CompatibleLowerRuleRevision,
                CalculationKindCode = value.CalculationKindCode,
                ModifierValue = value.ModifierValue,
                ModifierUnitCode = value.ModifierUnitCode,
                MeaningCode = value.MeaningCode,
                ActiveFromTurnNumber = value.ActiveFromTurnNumber,
                ActiveThroughTurnNumber = value.ActiveThroughTurnNumber,
                SourceTurnClosingStableId = value.SourceTurnClosingStableId,
                SourceStableIds = value.SourceStableIds.ToArray(),
            };

        private static void ValidateStableIds(string[] values, string errorCode)
        {
            if (values == null || values.Length == 0)
                throw Error(errorCode);
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                RequireStableId(value, errorCode);
                if (!unique.Add(value.Trim())) throw Error(errorCode);
            }
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
