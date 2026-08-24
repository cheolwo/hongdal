using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class 도심마트기본방문주문SimulationBuilder
    {
        public 도심마트주문SimulationDataSnapshot Build(
            도심마트수요시나리오DataSnapshot demandScenario,
            도심마트기본방문주문생성SimulationRule rule)
        {
            if (demandScenario == null) throw new ArgumentNullException(nameof(demandScenario));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            도심마트공급경영SimulationDataValidator.Validate(demandScenario);
            ValidateRule(rule);

            var orders = demandScenario.DemandSegments
                .OrderBy(segment => segment.StartsAtTick)
                .ThenBy(segment => segment.DemandSegmentStableId, StringComparer.Ordinal)
                .SelectMany(segment => CreateOrders(demandScenario, segment, rule))
                .OrderBy(order => order.CreatedTick)
                .ThenBy(order => order.OrderStableId, StringComparer.Ordinal)
                .ToArray();

            var snapshot = new 도심마트주문SimulationDataSnapshot
            {
                SnapshotStableId = "order-snapshot:" + demandScenario.ScenarioStableId
                    + ":" + rule.RuleRevision,
                SessionStableId = demandScenario.SessionStableId,
                ScenarioStableId = demandScenario.ScenarioStableId,
                DataRevision = "order-stream:" + demandScenario.DataRevision
                    + ":" + rule.RuleRevision
                    + ":seed-" + demandScenario.ScenarioSeed.ToString(CultureInfo.InvariantCulture),
                AsOfTick = demandScenario.AsOfTick,
                ModeCode = SimulationModeCodes.Simulation,
                IsOperationalState = false,
                DemandScenarioDataRevision = demandScenario.DataRevision,
                GenerationRuleRevision = rule.RuleRevision,
                ScenarioSeed = demandScenario.ScenarioSeed,
                QuantityBasisCode = rule.QuantityBasisCode,
                SplitStrategyCode = rule.SplitStrategyCode,
                GenerationLimitationText = rule.LimitationText,
                SourceLineage = new[]
                {
                    new SimulationDataLineage
                    {
                        SourceStableId = demandScenario.SnapshotStableId,
                        SourceDataRevision = demandScenario.DataRevision,
                        RuleRevision = rule.RuleRevision,
                    },
                },
                Orders = orders,
                Allocations = Array.Empty<도심마트주문재고할당SimulationData>(),
            };

            도심마트공급경영SimulationDataValidator.Validate(snapshot);
            return snapshot;
        }

        private static IReadOnlyList<도심마트주문SimulationData> CreateOrders(
            도심마트수요시나리오DataSnapshot demandScenario,
            도심마트기간별수요SimulationData segment,
            도심마트기본방문주문생성SimulationRule rule)
        {
            var tickCount = segment.EndsAtTick - segment.StartsAtTick + 1;
            if (rule.OrdersPerTickPattern.Length != tickCount)
                throw new SimulationContractException("OrderGenerationTickPatternLengthMismatch");

            var orderCount = rule.OrdersPerTickPattern.Sum();
            if (orderCount <= 0)
                throw new SimulationContractException("OrderGenerationOrderCountInvalid");

            var factor = DecimalFactor(rule.QuantityDecimalPlaces);
            var scaledQuantity = segment.ExpectedQuantity * factor;
            if (scaledQuantity != decimal.Truncate(scaledQuantity)
                || scaledQuantity > long.MaxValue)
                throw new SimulationContractException("OrderGenerationQuantityPrecisionExceeded");

            var totalUnits = decimal.ToInt64(scaledQuantity);
            if (totalUnits < orderCount)
                throw new SimulationContractException("OrderGenerationPositiveQuantityRequired");

            var baseUnits = totalUnits / orderCount;
            var remainderUnits = (int)(totalUnits % orderCount);
            var remainderOffset = SeededOffset(
                demandScenario.ScenarioSeed,
                segment.DemandSegmentStableId,
                orderCount);
            var receivesRemainder = new bool[orderCount];
            for (var index = 0; index < remainderUnits; index++)
                receivesRemainder[(remainderOffset + index) % orderCount] = true;

            var result = new List<도심마트주문SimulationData>(orderCount);
            var ordinal = 0;
            for (var tickOffset = 0; tickOffset < rule.OrdersPerTickPattern.Length; tickOffset++)
            {
                var countAtTick = rule.OrdersPerTickPattern[tickOffset];
                for (var indexAtTick = 0; indexAtTick < countAtTick; indexAtTick++)
                {
                    var requestedUnits = baseUnits + (receivesRemainder[ordinal] ? 1L : 0L);
                    var createdTick = segment.StartsAtTick + tickOffset;
                    result.Add(new 도심마트주문SimulationData
                    {
                        OrderStableId = "simulation-order:" + segment.DemandSegmentStableId
                            + ":" + (ordinal + 1).ToString("D3", CultureInfo.InvariantCulture),
                        SourceDemandSegmentStableId = segment.DemandSegmentStableId,
                        DemandSourceTypeCode = SimulationDemandSourceTypeCodes.BaseScenarioDemand,
                        ProductStableId = segment.ProductStableId,
                        RegionStableId = demandScenario.RegionStableId,
                        CreatedTick = createdTick,
                        FulfillmentDueTick = Math.Min(
                            segment.EndsAtTick,
                            createdTick + rule.FulfillmentWindowTicks),
                        RequestedQuantity = requestedUnits / factor,
                        AllocatedQuantity = 0m,
                        FulfilledQuantity = 0m,
                        UnfulfilledQuantity = 0m,
                        QuantityUnitCode = segment.QuantityUnitCode,
                        StateCode = SimulationOrderStateCodes.Pending,
                    });
                    ordinal++;
                }
            }

            return result;
        }

        private static void ValidateRule(도심마트기본방문주문생성SimulationRule rule)
        {
            ValidateStableId(rule.RuleStableId, "OrderGenerationRuleStableIdInvalid");
            RequireText(rule.RuleRevision, "OrderGenerationRuleRevisionMissing");
            if (!string.Equals(rule.QuantityBasisCode,
                    도심마트주문생성수량기준코드.ExpectedQuantity,
                    StringComparison.Ordinal))
                throw new SimulationContractException("OrderGenerationQuantityBasisInvalid");
            if (!string.Equals(rule.SplitStrategyCode,
                    도심마트주문분할전략코드.EvenSplitWithSeededRemainder,
                    StringComparison.Ordinal))
                throw new SimulationContractException("OrderGenerationSplitStrategyInvalid");
            if (rule.QuantityDecimalPlaces < 0 || rule.QuantityDecimalPlaces > 6)
                throw new SimulationContractException("OrderGenerationDecimalPlacesInvalid");
            if (rule.FulfillmentWindowTicks < 0)
                throw new SimulationContractException("OrderGenerationFulfillmentWindowInvalid");
            if (rule.OrdersPerTickPattern == null
                || rule.OrdersPerTickPattern.Length == 0
                || rule.OrdersPerTickPattern.Any(value => value < 0)
                || rule.OrdersPerTickPattern.Sum() <= 0)
                throw new SimulationContractException("OrderGenerationTickPatternInvalid");
            RequireText(rule.LimitationText, "OrderGenerationLimitationMissing");
        }

        private static decimal DecimalFactor(int decimalPlaces)
        {
            var factor = 1m;
            for (var index = 0; index < decimalPlaces; index++) factor *= 10m;
            return factor;
        }

        private static int SeededOffset(int seed, string stableId, int count)
        {
            var combined = (long)seed + StableHash(stableId);
            var remainder = combined % count;
            return (int)(remainder < 0 ? remainder + count : remainder);
        }

        private static uint StableHash(string value)
        {
            unchecked
            {
                var hash = 2166136261u;
                foreach (var character in value)
                {
                    hash ^= character;
                    hash *= 16777619u;
                }

                return hash;
            }
        }

        private static void ValidateStableId(string value, string errorCode)
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

    public static class 도심마트감자기본방문주문SimulationFixture
    {
        public static 도심마트기본방문주문생성SimulationRule Rule()
            => new 도심마트기본방문주문생성SimulationRule
            {
                RuleStableId = "order-generation-rule:potato-base-demand:1",
                RuleRevision = "potato-base-order-generation:1",
                QuantityBasisCode = 도심마트주문생성수량기준코드.ExpectedQuantity,
                SplitStrategyCode = 도심마트주문분할전략코드.EvenSplitWithSeededRemainder,
                QuantityDecimalPlaces = 3,
                FulfillmentWindowTicks = 2,
                OrdersPerTickPattern = new[] { 2, 2, 2, 2, 2, 2, 2 },
                LimitationText = "기본 방문 수요의 교육용 synthetic 주문이며 실제 개인·주소·운영 주문을 나타내지 않습니다.",
            };

        public static 도심마트주문SimulationDataSnapshot Create()
            => new 도심마트기본방문주문SimulationBuilder().Build(
                도심마트감자4주DemandSimulationFixture.CreateScenario(),
                Rule());
    }
}
