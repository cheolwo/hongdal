using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation감자생산규칙
    {
        private readonly Simulation자원효과묶음Validator effectValidator;

        public Simulation감자생산규칙()
            : this(new Simulation자원효과묶음Validator())
        {
        }

        public Simulation감자생산규칙(Simulation자원효과묶음Validator validator)
        {
            effectValidator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public Simulation감자생산PreviewResult CreatePreview(Simulation감자생산Request request)
        {
            Validate(request);

            var unit = request.CultivationUnit;
            var rule = request.Rule;
            var effectiveArea = Round(unit.PhysicalAreaSquareMeters
                * unit.EffectiveCultivationAreaRatio);
            var baseQuantity = Round(effectiveArea
                * rule.BaseYieldKilogramsPerSquareMeter);
            var expectedQuantity = Round(baseQuantity
                * request.EnvironmentFactor
                * request.InputFactor
                * request.FacilityFactor
                * request.LossFactor);
            if (expectedQuantity <= 0m)
                throw new SimulationContractException("SimulationPotatoProductionQuantityInvalid");

            var sources = MergeSources(
                request.SourceStableIds,
                unit.SourceStableIds,
                rule.SourceStableIds,
                new[]
                {
                    unit.CultivationUnitStableId,
                    unit.TileStableId,
                    unit.CultivationStableId,
                    request.EnvironmentSnapshotStableId,
                    request.DecisionStableId,
                    request.CompletedTaskStableId,
                });
            var bundle = new Simulation자원효과묶음Snapshot
            {
                EffectBundleStableId = request.EffectBundleStableId.Trim(),
                RuleStableId = rule.RuleStableId.Trim(),
                RuleRevision = rule.RuleRevision,
                RuleDomainCode = Simulation업무규칙영역Codes.Production,
                ModeCode = "Simulation",
                StateCode = SimulationEffectStateCodes.Pending,
                CausedByDecisionStableId = request.DecisionStableId.Trim(),
                CausedByTaskStableId = request.CompletedTaskStableId.Trim(),
                AppliedTick = null,
                SourceStableIds = sources,
                Lines = new[]
                {
                    new Simulation자원효과선Snapshot
                    {
                        EffectLineStableId = request.EffectLineStableId.Trim(),
                        MutationKindCode = Simulation자원변동유형Codes.Production,
                        RoleCode = Simulation자원효과역할Codes.Output,
                        ResourceTypeCode = "HarvestStock",
                        TargetLedgerStableId = request.HarvestLedgerStableId.Trim(),
                        ProductStableId = unit.ProductStableId.Trim(),
                        LotStableId = request.HarvestLotStableId.Trim(),
                        BeforeValue = 0m,
                        Delta = expectedQuantity,
                        AfterValue = expectedQuantity,
                        UnitCode = "kg",
                        SourceStableIds = sources,
                    },
                },
            };
            effectValidator.Validate(bundle);

            return new Simulation감자생산PreviewResult
            {
                CultivationUnitStableId = unit.CultivationUnitStableId.Trim(),
                TileStableId = unit.TileStableId.Trim(),
                HarvestLotStableId = request.HarvestLotStableId.Trim(),
                CompletedTick = request.CompletedTick,
                EffectiveCultivationAreaSquareMeters = effectiveArea,
                BaseHarvestQuantityKilograms = baseQuantity,
                ExpectedHarvestQuantityKilograms = expectedQuantity,
                PendingEffectBundle = bundle,
            };
        }

        private static void Validate(Simulation감자생산Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.EffectBundleStableId, "SimulationPotatoProductionEffectBundleInvalid");
            RequireStableId(request.EffectLineStableId, "SimulationPotatoProductionEffectLineInvalid");
            RequireStableId(request.DecisionStableId, "SimulationPotatoProductionDecisionInvalid");
            RequireStableId(request.CompletedTaskStableId, "SimulationPotatoProductionTaskInvalid");
            RequireStableId(request.HarvestLotStableId, "SimulationPotatoProductionHarvestLotInvalid");
            RequireStableId(request.HarvestLedgerStableId, "SimulationPotatoProductionLedgerInvalid");
            RequireStableId(request.EnvironmentSnapshotStableId,
                "SimulationPotatoProductionEnvironmentSnapshotInvalid");
            if (request.DecisionStateCode != SimulationDecisionStateCodes.Confirmed)
                throw new SimulationContractException("SimulationPotatoProductionDecisionNotConfirmed");
            if (request.TaskStateCode != SimulationTaskStateCodes.Completed)
                throw new SimulationContractException("SimulationPotatoProductionTaskNotCompleted");
            if (request.CompletedTick < 0)
                throw new SimulationContractException("SimulationPotatoProductionCompletedTickInvalid");

            ValidateUnit(request.CultivationUnit);
            ValidateRule(request.Rule, request.CultivationUnit);
            ValidateFactor(request.EnvironmentFactor, request.Rule.MinimumEnvironmentFactor,
                request.Rule.MaximumEnvironmentFactor, "Environment");
            ValidateFactor(request.InputFactor, request.Rule.MinimumInputFactor,
                request.Rule.MaximumInputFactor, "Input");
            ValidateFactor(request.FacilityFactor, request.Rule.MinimumFacilityFactor,
                request.Rule.MaximumFacilityFactor, "Facility");
            ValidateFactor(request.LossFactor, request.Rule.MinimumLossFactor,
                request.Rule.MaximumLossFactor, "Loss");
            ValidateSources(request.SourceStableIds, false,
                "SimulationPotatoProductionSourcesInvalid");
        }

        private static void ValidateUnit(Simulation재배단위Snapshot unit)
        {
            if (unit == null)
                throw new SimulationContractException("SimulationCultivationUnitMissing");
            RequireStableId(unit.CultivationUnitStableId, "SimulationCultivationUnitStableIdInvalid");
            RequireStableId(unit.TileStableId, "SimulationCultivationTileStableIdInvalid");
            RequireStableId(unit.CultivationStableId, "SimulationCultivationStableIdInvalid");
            RequireStableId(unit.ProductStableId, "SimulationCultivationProductStableIdInvalid");
            RequireStableId(unit.CropVariantStableId, "SimulationCultivationVariantStableIdInvalid");
            if (unit.Revision <= 0
                || unit.StateCode != Simulation재배단위상태Codes.HarvestReady
                || unit.PhysicalAreaSquareMeters <= 0m
                || unit.EffectiveCultivationAreaRatio <= 0m
                || unit.EffectiveCultivationAreaRatio > 1m)
            {
                throw new SimulationContractException("SimulationCultivationUnitNotHarvestReady");
            }
            ValidateSources(unit.SourceStableIds, true, "SimulationCultivationUnitSourcesInvalid");
        }

        private static void ValidateRule(
            Simulation감자생산RuleSnapshot rule,
            Simulation재배단위Snapshot unit)
        {
            if (rule == null)
                throw new SimulationContractException("SimulationPotatoProductionRuleMissing");
            RequireStableId(rule.RuleStableId, "SimulationPotatoProductionRuleStableIdInvalid");
            RequireStableId(rule.ProductStableId, "SimulationPotatoProductionRuleProductInvalid");
            RequireStableId(rule.CropVariantStableId, "SimulationPotatoProductionRuleVariantInvalid");
            if (rule.RuleRevision <= 0
                || rule.SourceTypeCode != Simulation생산규칙SourceTypeCodes.Fixture
                || rule.BaseYieldKilogramsPerSquareMeter <= 0m
                || rule.ProductStableId != unit.ProductStableId
                || rule.CropVariantStableId != unit.CropVariantStableId)
            {
                throw new SimulationContractException("SimulationPotatoProductionRuleInvalid");
            }
            ValidateRange(rule.MinimumEnvironmentFactor, rule.MaximumEnvironmentFactor, "Environment");
            ValidateRange(rule.MinimumInputFactor, rule.MaximumInputFactor, "Input");
            ValidateRange(rule.MinimumFacilityFactor, rule.MaximumFacilityFactor, "Facility");
            ValidateRange(rule.MinimumLossFactor, rule.MaximumLossFactor, "Loss");
            if (rule.MaximumLossFactor > 1m)
                throw new SimulationContractException("SimulationPotatoProductionLossRangeInvalid");
            ValidateSources(rule.SourceStableIds, true, "SimulationPotatoProductionRuleSourcesInvalid");
            if (rule.Limitations == null || rule.Limitations.Length == 0
                || rule.Limitations.Any(string.IsNullOrWhiteSpace))
            {
                throw new SimulationContractException("SimulationPotatoProductionRuleLimitationsMissing");
            }
        }

        private static void ValidateRange(decimal minimum, decimal maximum, string name)
        {
            if (minimum <= 0m || maximum < minimum)
                throw new SimulationContractException("SimulationPotatoProduction" + name + "RangeInvalid");
        }

        private static void ValidateFactor(decimal value, decimal minimum, decimal maximum, string name)
        {
            if (value < minimum || value > maximum)
                throw new SimulationContractException("SimulationPotatoProduction" + name + "FactorInvalid");
        }

        private static void ValidateSources(string[] values, bool requireAny, string errorCode)
        {
            if (values == null || (requireAny && values.Length == 0))
                throw new SimulationContractException(errorCode);
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                RequireStableId(value, errorCode);
                if (!unique.Add(value.Trim()))
                    throw new SimulationContractException(errorCode);
            }
        }

        private static string[] MergeSources(params string[][] groups)
            => groups.SelectMany(value => value ?? Array.Empty<string>())
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                throw new SimulationContractException(errorCode);
            }
        }

        private static decimal Round(decimal value)
            => Math.Round(value, 3, MidpointRounding.AwayFromZero);
    }
}
