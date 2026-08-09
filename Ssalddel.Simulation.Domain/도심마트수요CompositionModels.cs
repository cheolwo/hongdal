using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class 도심마트수요CompositionPolicyCodes
    {
        public const string BasePlusGroupConfirmed = "BasePlusGroupConfirmed";
    }

    public sealed class 도심마트수요CompositionRule
    {
        public string RuleStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string HardDemandPolicyCode { get; set; } =
            도심마트수요CompositionPolicyCodes.BasePlusGroupConfirmed;
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class 도심마트수요CompositionComponentWorldState
    {
        public string StableId { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public int StartsAtTick { get; set; }
        public int EndsAtTick { get; set; }
        public int ParticipantCount { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public bool IsHardDemand { get; set; }
    }

    public sealed class 도심마트수요CompositionWorldState
    {
        public string StableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string ScenarioStableId { get; set; } = string.Empty;
        public string InterpretationRevision { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string QuantityUnitCode { get; set; } = string.Empty;
        public decimal BaseScenarioDemand { get; set; }
        public decimal GroupIntentDemand { get; set; }
        public decimal GroupConfirmedDemand { get; set; }
        public decimal HardDemand { get; set; }
        public string HardDemandPolicyCode { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
        public SimulationDataLineage[] SourceLineage { get; set; } =
            Array.Empty<SimulationDataLineage>();
        public 도심마트수요CompositionComponentWorldState[] Components { get; set; } =
            Array.Empty<도심마트수요CompositionComponentWorldState>();
    }

    public sealed class 도심마트수요CompositionInterpreter
    {
        public 도심마트수요CompositionWorldState Interpret(
            도심마트수요시나리오DataSnapshot baseDemand,
            도심마트주문자집단수요SimulationDataSnapshot groupDemand,
            도심마트수요CompositionRule rule)
        {
            if (baseDemand == null) throw new ArgumentNullException(nameof(baseDemand));
            if (groupDemand == null) throw new ArgumentNullException(nameof(groupDemand));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            도심마트공급경영SimulationDataValidator.Validate(baseDemand);
            도심마트공급경영SimulationDataValidator.Validate(groupDemand);
            ValidateRule(rule);
            if (!string.Equals(baseDemand.SessionStableId, groupDemand.SessionStableId, StringComparison.Ordinal))
                throw new SimulationContractException("DemandCompositionSessionMismatch");
            if (!string.Equals(baseDemand.ScenarioStableId, groupDemand.ScenarioStableId, StringComparison.Ordinal))
                throw new SimulationContractException("DemandCompositionScenarioMismatch");

            var products = baseDemand.DemandSegments.Select(value => value.ProductStableId)
                .Concat(groupDemand.Groups.Select(value => value.ProductStableId))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (products.Length != 1)
                throw new SimulationContractException("DemandCompositionSingleProductRequired");
            var units = baseDemand.DemandSegments.Select(value => value.QuantityUnitCode)
                .Concat(groupDemand.Groups.Select(value => value.QuantityUnitCode))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (units.Length != 1)
                throw new SimulationContractException("DemandCompositionQuantityUnitMismatch");

            var productStableId = products[0];
            var quantityUnitCode = units[0];
            var components = new List<도심마트수요CompositionComponentWorldState>();
            components.AddRange(baseDemand.DemandSegments
                .OrderBy(value => value.StartsAtTick)
                .ThenBy(value => value.DemandSegmentStableId, StringComparer.Ordinal)
                .Select(value => Component(
                    "demand-component:base:" + value.DemandSegmentStableId,
                    SimulationDemandSourceTypeCodes.BaseScenarioDemand,
                    value.DemandSegmentStableId,
                    value.ProductStableId,
                    value.StartsAtTick,
                    value.EndsAtTick,
                    0,
                    value.ExpectedQuantity,
                    value.QuantityUnitCode,
                    true)));
            foreach (var group in groupDemand.Groups
                .OrderBy(value => value.OrdererGroupStableId, StringComparer.Ordinal))
            {
                components.Add(Component(
                    "demand-component:intent:" + group.OrdererGroupStableId,
                    SimulationDemandSourceTypeCodes.GroupIntentDemand,
                    group.OrdererGroupStableId,
                    group.ProductStableId,
                    group.RequestedFulfillmentStartsAtTick,
                    group.RequestedFulfillmentEndsAtTick,
                    group.IntentParticipantCount,
                    group.IntentQuantity,
                    group.QuantityUnitCode,
                    false));
                components.Add(Component(
                    "demand-component:confirmed:" + group.OrdererGroupStableId,
                    SimulationDemandSourceTypeCodes.GroupConfirmedDemand,
                    group.OrdererGroupStableId,
                    group.ProductStableId,
                    group.RequestedFulfillmentStartsAtTick,
                    group.RequestedFulfillmentEndsAtTick,
                    group.ConfirmedParticipantCount,
                    group.ConfirmedQuantity,
                    group.QuantityUnitCode,
                    true));
            }

            var componentArray = components
                .OrderBy(value => value.StartsAtTick)
                .ThenBy(value => value.SourceTypeCode, StringComparer.Ordinal)
                .ThenBy(value => value.StableId, StringComparer.Ordinal)
                .ToArray();
            EnsureUnique(componentArray.Select(value => value.StableId));
            var baseTotal = Sum(componentArray, SimulationDemandSourceTypeCodes.BaseScenarioDemand);
            var intentTotal = Sum(componentArray, SimulationDemandSourceTypeCodes.GroupIntentDemand);
            var confirmedTotal = Sum(componentArray, SimulationDemandSourceTypeCodes.GroupConfirmedDemand);
            var hardDemand = componentArray.Where(value => value.IsHardDemand).Sum(value => value.Quantity);
            if (hardDemand != baseTotal + confirmedTotal)
                throw new SimulationContractException("DemandCompositionHardDemandInvariantInvalid");

            return new 도심마트수요CompositionWorldState
            {
                StableId = "demand-composition:" + baseDemand.ScenarioStableId + ":" + productStableId,
                SessionStableId = baseDemand.SessionStableId,
                ScenarioStableId = baseDemand.ScenarioStableId,
                InterpretationRevision = "demand-composition:" + baseDemand.DataRevision
                    + ":" + groupDemand.DataRevision + ":" + rule.RuleRevision,
                ProductStableId = productStableId,
                QuantityUnitCode = quantityUnitCode,
                BaseScenarioDemand = baseTotal,
                GroupIntentDemand = intentTotal,
                GroupConfirmedDemand = confirmedTotal,
                HardDemand = hardDemand,
                HardDemandPolicyCode = rule.HardDemandPolicyCode,
                LimitationText = rule.LimitationText,
                SourceLineage = new[]
                {
                    new SimulationDataLineage
                    {
                        SourceStableId = baseDemand.SnapshotStableId,
                        SourceDataRevision = baseDemand.DataRevision,
                        RuleRevision = baseDemand.DemandRuleRevision,
                    },
                    new SimulationDataLineage
                    {
                        SourceStableId = groupDemand.SnapshotStableId,
                        SourceDataRevision = groupDemand.DataRevision,
                        RuleRevision = rule.RuleRevision,
                    },
                },
                Components = componentArray,
            };
        }

        private static 도심마트수요CompositionComponentWorldState Component(
            string stableId,
            string sourceTypeCode,
            string sourceStableId,
            string productStableId,
            int startsAtTick,
            int endsAtTick,
            int participantCount,
            decimal quantity,
            string quantityUnitCode,
            bool isHardDemand)
            => new 도심마트수요CompositionComponentWorldState
            {
                StableId = stableId,
                SourceTypeCode = sourceTypeCode,
                SourceStableId = sourceStableId,
                ProductStableId = productStableId,
                StartsAtTick = startsAtTick,
                EndsAtTick = endsAtTick,
                ParticipantCount = participantCount,
                Quantity = quantity,
                QuantityUnitCode = quantityUnitCode,
                IsHardDemand = isHardDemand,
            };

        private static decimal Sum(
            IEnumerable<도심마트수요CompositionComponentWorldState> components,
            string sourceTypeCode)
            => components.Where(value => string.Equals(
                    value.SourceTypeCode,
                    sourceTypeCode,
                    StringComparison.Ordinal))
                .Sum(value => value.Quantity);

        private static void ValidateRule(도심마트수요CompositionRule rule)
        {
            RequireStableId(rule.RuleStableId, "DemandCompositionRuleStableIdInvalid");
            RequireText(rule.RuleRevision, "DemandCompositionRuleRevisionMissing");
            if (!string.Equals(rule.HardDemandPolicyCode,
                    도심마트수요CompositionPolicyCodes.BasePlusGroupConfirmed,
                    StringComparison.Ordinal))
                throw new SimulationContractException("DemandCompositionPolicyInvalid");
            RequireText(rule.LimitationText, "DemandCompositionLimitationMissing");
        }

        private static void EnsureUnique(IEnumerable<string> values)
        {
            if (values.Distinct(StringComparer.Ordinal).Count() != values.Count())
                throw new SimulationContractException("DemandCompositionComponentStableIdDuplicate");
        }

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(errorCode);
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
        }
    }

    public static class 도심마트감자수요CompositionSimulationFixture
    {
        public static 도심마트수요CompositionRule Rule()
            => new 도심마트수요CompositionRule
            {
                RuleStableId = "demand-composition-rule:potato:1",
                RuleRevision = "potato-demand-composition:1",
                HardDemandPolicyCode = 도심마트수요CompositionPolicyCodes.BasePlusGroupConfirmed,
                LimitationText = "집단 의향은 참고 신호이며 확정 주문 집계로 자동 전환하지 않습니다.",
            };

        public static 도심마트수요CompositionWorldState Create()
            => new 도심마트수요CompositionInterpreter().Interpret(
                도심마트감자4주DemandSimulationFixture.CreateScenario(),
                도심마트공동주택주문자집단SimulationFixture.Create(),
                Rule());
    }
}
