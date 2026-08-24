using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class 지역잠재수요InterpretationRule
    {
        public string RuleRevision { get; set; } = string.Empty;
        public long MediumHouseholdThreshold { get; set; }
        public long HighHouseholdThreshold { get; set; }
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class 지역잠재수요WorldState
    {
        public string StableId { get; set; } = string.Empty;
        public string RegionStableId { get; set; } = string.Empty;
        public string PopulationBasisRevision { get; set; } = string.Empty;
        public string PotentialDemandBandCode { get; set; } = string.Empty;
        public string BasisMetricCode { get; set; } = string.Empty;
        public long BasisMetricValue { get; set; }
        public string InterpretationRuleRevision { get; set; } = string.Empty;
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class 지역잠재수요Interpreter
    {
        public 지역잠재수요WorldState Interpret(
            지역인구SimulationBasisDataSnapshot data,
            지역잠재수요InterpretationRule rule)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            ValidateStableId(data.SnapshotStableId, "PopulationSnapshotStableIdInvalid");
            ValidateStableId(data.RegionStableId, "PopulationRegionStableIdInvalid");
            RequireText(data.SourceKey, "PopulationSourceKeyMissing");
            RequireText(data.SpatialPrecisionCode, "PopulationSpatialPrecisionMissing");
            RequireText(data.QualityStatusCode, "PopulationQualityStatusMissing");
            RequireText(data.DataRevision, "PopulationDataRevisionMissing");
            if (!data.IsPublicAggregate || data.IsSuppressed)
                throw new SimulationContractException("PopulationBasisUnavailable");
            if (!data.RegisteredPopulation.HasValue || data.RegisteredPopulation.Value < 0
                || !data.RegisteredHouseholdCount.HasValue || data.RegisteredHouseholdCount.Value < 0)
                throw new SimulationContractException("PopulationMetricMissingOrInvalid");
            RequireText(rule.RuleRevision, "PotentialDemandRuleRevisionMissing");
            RequireText(rule.LimitationText, "PotentialDemandLimitationMissing");
            if (rule.MediumHouseholdThreshold < 0
                || rule.HighHouseholdThreshold <= rule.MediumHouseholdThreshold)
                throw new SimulationContractException("PotentialDemandThresholdInvalid");

            var households = data.RegisteredHouseholdCount.Value;
            var band = households >= rule.HighHouseholdThreshold
                ? "HighPotentialBasis"
                : households >= rule.MediumHouseholdThreshold
                    ? "MediumPotentialBasis"
                    : "LowPotentialBasis";
            return new 지역잠재수요WorldState
            {
                StableId = "potential-demand:" + data.RegionStableId,
                RegionStableId = data.RegionStableId,
                PopulationBasisRevision = data.DataRevision,
                PotentialDemandBandCode = band,
                BasisMetricCode = "RegisteredHouseholdCount",
                BasisMetricValue = households,
                InterpretationRuleRevision = rule.RuleRevision,
                LimitationText = rule.LimitationText,
            };
        }

        private static void ValidateStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(errorCode);
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException(errorCode);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class 도심마트4주DemandScenarioBuilder
    {
        public 도심마트수요시나리오DataSnapshot Build(
            string sessionStableId,
            string scenarioStableId,
            int scenarioSeed,
            지역잠재수요WorldState potentialDemand,
            도심마트4주DemandScenarioAssumptions assumptions)
        {
            ValidateStableId(sessionStableId, "DemandSessionStableIdInvalid");
            ValidateStableId(scenarioStableId, "DemandScenarioStableIdInvalid");
            if (potentialDemand == null) throw new ArgumentNullException(nameof(potentialDemand));
            if (assumptions == null) throw new ArgumentNullException(nameof(assumptions));
            ValidateStableId(potentialDemand.StableId, "PotentialDemandStableIdInvalid");
            ValidateStableId(potentialDemand.RegionStableId, "PotentialDemandRegionStableIdInvalid");
            RequireText(potentialDemand.PopulationBasisRevision, "PotentialDemandPopulationRevisionMissing");
            RequireText(potentialDemand.InterpretationRuleRevision, "PotentialDemandRuleRevisionMissing");
            ValidateStableId(assumptions.AssumptionStableId, "DemandAssumptionStableIdInvalid");
            RequireText(assumptions.AssumptionRevision, "DemandAssumptionRevisionMissing");
            ValidateStableId(assumptions.ProductStableId, "DemandAssumptionProductStableIdInvalid");
            RequireRate(assumptions.ProductSelectionRate, "DemandAssumptionProductSelectionRateInvalid");
            RequireRate(assumptions.SimulationMarketShareRate, "DemandAssumptionMarketShareRateInvalid");
            RequireText(assumptions.SeasonAssumptionCode, "DemandAssumptionSeasonMissing");
            RequireText(assumptions.EventAssumptionCode, "DemandAssumptionEventMissing");
            RequireText(assumptions.QuantityUnitCode, "DemandAssumptionQuantityUnitMissing");
            RequireText(assumptions.LimitationText, "DemandAssumptionLimitationMissing");
            if (assumptions.WeeklyDemand == null || assumptions.WeeklyDemand.Length != 4
                || assumptions.WeeklyDemand.Select(value => value.WeekIndex).Distinct().Count() != 4
                || assumptions.WeeklyDemand.Any(value => value.WeekIndex < 0 || value.WeekIndex > 3))
                throw new SimulationContractException("DemandAssumptionFourWeeksRequired");

            var segments = assumptions.WeeklyDemand
                .OrderBy(value => value.WeekIndex)
                .Select(value => Segment(scenarioStableId, assumptions, value))
                .ToArray();
            var snapshot = new 도심마트수요시나리오DataSnapshot
            {
                SnapshotStableId = "demand-snapshot:" + scenarioStableId + ":" + assumptions.AssumptionRevision,
                SessionStableId = sessionStableId,
                ScenarioStableId = scenarioStableId,
                DataRevision = "demand-scenario:" + potentialDemand.PopulationBasisRevision
                    + ":" + assumptions.AssumptionRevision,
                AsOfTick = 0,
                ModeCode = SimulationModeCodes.Simulation,
                IsOperationalState = false,
                RegionStableId = potentialDemand.RegionStableId,
                PopulationBasisRevision = potentialDemand.PopulationBasisRevision,
                DemandRuleRevision = assumptions.AssumptionRevision,
                ScenarioSeed = scenarioSeed,
                ProductSelectionRate = assumptions.ProductSelectionRate,
                SimulationMarketShareRate = assumptions.SimulationMarketShareRate,
                SeasonAssumptionCode = assumptions.SeasonAssumptionCode,
                EventAssumptionCode = assumptions.EventAssumptionCode,
                LimitationText = assumptions.LimitationText,
                SourceLineage = new[]
                {
                    new SimulationDataLineage
                    {
                        SourceStableId = potentialDemand.StableId,
                        SourceDataRevision = potentialDemand.PopulationBasisRevision,
                        RuleRevision = potentialDemand.InterpretationRuleRevision,
                    },
                    new SimulationDataLineage
                    {
                        SourceStableId = assumptions.AssumptionStableId,
                        SourceDataRevision = assumptions.AssumptionRevision,
                        RuleRevision = assumptions.AssumptionRevision,
                    },
                },
                DemandSegments = segments,
            };
            도심마트공급경영SimulationDataValidator.Validate(snapshot);
            return snapshot;
        }

        private static 도심마트기간별수요SimulationData Segment(
            string scenarioStableId,
            도심마트4주DemandScenarioAssumptions assumptions,
            도심마트주간수요SimulationAssumption weekly)
        {
            if (weekly.MinimumQuantity < 0
                || weekly.ExpectedQuantity < weekly.MinimumQuantity
                || weekly.MaximumQuantity < weekly.ExpectedQuantity)
                throw new SimulationContractException("DemandAssumptionQuantityRangeInvalid");
            return new 도심마트기간별수요SimulationData
            {
                DemandSegmentStableId = "demand-segment:" + scenarioStableId + ":week-" + (weekly.WeekIndex + 1),
                ProductStableId = assumptions.ProductStableId,
                StartsAtTick = weekly.WeekIndex * 7,
                EndsAtTick = weekly.WeekIndex * 7 + 6,
                MinimumQuantity = weekly.MinimumQuantity,
                ExpectedQuantity = weekly.ExpectedQuantity,
                MaximumQuantity = weekly.MaximumQuantity,
                QuantityUnitCode = assumptions.QuantityUnitCode,
            };
        }

        private static void RequireRate(decimal value, string errorCode)
        {
            if (value < 0 || value > 1) throw new SimulationContractException(errorCode);
        }

        private static void ValidateStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(errorCode);
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException(errorCode);
        }
    }

    public static class 도심마트감자4주DemandSimulationFixture
    {
        public static 지역인구SimulationBasisDataSnapshot PopulationBasis(long households = 18000)
            => new 지역인구SimulationBasisDataSnapshot
            {
                SnapshotStableId = "population-basis:region-sample:2026-07",
                RegionStableId = "region:kr:seoul:sample",
                RegisteredPopulation = 42000,
                RegisteredHouseholdCount = households,
                SourceKey = "simulation-public-population-basis-fixture",
                EvidenceAsOfUtc = new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero),
                SpatialPrecisionCode = "DistrictSimulationFixture",
                QualityStatusCode = "FixtureNotOfficialObservation",
                DataRevision = "population-basis-fixture:2026-07:households-" + households + ":1",
                IsPublicAggregate = true,
                IsSuppressed = false,
            };

        public static 지역잠재수요InterpretationRule PotentialDemandRule()
            => new 지역잠재수요InterpretationRule
            {
                RuleRevision = "potential-demand-band-rule:1",
                MediumHouseholdThreshold = 10000,
                HighHouseholdThreshold = 30000,
                LimitationText = "세대수 band는 잠재 소비 기반이며 상품 주문량 또는 매출 예측이 아닙니다.",
            };

        public static 도심마트4주DemandScenarioAssumptions Assumptions()
            => new 도심마트4주DemandScenarioAssumptions
            {
                AssumptionStableId = "demand-assumption:potato-4w:1",
                AssumptionRevision = "potato-demand-assumption:1",
                ProductStableId = 도심마트감자공급SimulationFixture.ProductStableId,
                ProductSelectionRate = 0.20m,
                SimulationMarketShareRate = 0.05m,
                SeasonAssumptionCode = "NormalSeason",
                EventAssumptionCode = "NoEvent",
                QuantityUnitCode = 도심마트감자공급SimulationFixture.QuantityUnitCode,
                LimitationText = "교육용 4주 수요 가정이며 공공 인구와 실제 주문을 직접 환산하지 않습니다.",
                WeeklyDemand = new[]
                {
                    Week(0, 360m, 420m, 480m),
                    Week(1, 380m, 440m, 500m),
                    Week(2, 350m, 410m, 470m),
                    Week(3, 390m, 450m, 520m),
                },
            };

        public static 도심마트수요시나리오DataSnapshot CreateScenario(long households = 18000)
        {
            var potential = new 지역잠재수요Interpreter().Interpret(PopulationBasis(households), PotentialDemandRule());
            return new 도심마트4주DemandScenarioBuilder().Build(
                "simulation-session:potato-fixture",
                도심마트감자공급SimulationFixture.ScenarioStableId,
                240809,
                potential,
                Assumptions());
        }

        private static 도심마트주간수요SimulationAssumption Week(
            int index, decimal minimum, decimal expected, decimal maximum)
            => new 도심마트주간수요SimulationAssumption
            {
                WeekIndex = index,
                MinimumQuantity = minimum,
                ExpectedQuantity = expected,
                MaximumQuantity = maximum,
            };
    }
}
