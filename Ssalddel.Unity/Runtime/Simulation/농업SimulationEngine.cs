using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Unity.Data
{
    public sealed class 농업SimulationEngine
    {
        private readonly 농업ScenarioValidator _validator;

        public 농업SimulationEngine()
            : this(new 농업ScenarioValidator())
        {
        }

        public 농업SimulationEngine(농업ScenarioValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public 농업SimulationRunResult Run(
            농업ScenarioPackage package,
            IEnumerable<농업SimulationCommand> commands)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (commands == null)
            {
                throw new ArgumentNullException(nameof(commands));
            }

            var validation = _validator.Validate(package);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(
                    "유효하지 않은 시나리오는 실행할 수 없습니다: "
                    + string.Join(", ", validation.Issues.Select(item => item.Code)));
            }

            var state = CreateInitialState(package);
            var events = new List<농업SimulationEvent>();
            var seenCommandIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var command in commands)
            {
                if (command == null)
                {
                    throw new InvalidOperationException("Command는 null일 수 없습니다.");
                }

                StableDataId.EnsureValid(command.CommandId, nameof(command.CommandId));
                if (!seenCommandIds.Add(command.CommandId))
                {
                    throw new InvalidOperationException("중복 CommandId는 재적용할 수 없습니다: " + command.CommandId);
                }

                if (command.GameDay != state.CurrentGameDay)
                {
                    throw new InvalidOperationException("Command의 GameDay가 현재 state와 일치하지 않습니다: " + command.CommandId);
                }

                Apply(package, state, command, events);
            }

            var eventArray = events.ToArray();
            return new 농업SimulationRunResult
            {
                State = state,
                Events = eventArray,
                FinalStateHash = CalculateFinalStateHash(package, state, eventArray),
            };
        }

        private static 농업SimulationState CreateInitialState(농업ScenarioPackage package)
        {
            var firstStage = package.GrowthStages
                .OrderBy(item => item.MinimumGrowthPoint)
                .ThenBy(item => item.StageKey, StringComparer.Ordinal)
                .First();

            return new 농업SimulationState
            {
                ScenarioKey = package.Manifest.ScenarioKey,
                ScenarioVersion = package.Manifest.ScenarioVersion,
                CurrentGameDay = 0,
                MoistureRatio = package.Soil.InitialMoistureRatio,
                GrowthStageKey = firstStage.StageKey,
            };
        }

        private static void Apply(
            농업ScenarioPackage package,
            농업SimulationState state,
            농업SimulationCommand command,
            ICollection<농업SimulationEvent> events)
        {
            switch (command.CommandCode)
            {
                case 농업CommandCodes.PlantCrop:
                    Plant(package, state, events);
                    break;
                case 농업CommandCodes.WaterTile:
                    Water(package, state, command, events);
                    break;
                case 농업CommandCodes.AdvanceDay:
                    AdvanceDay(package, state, events);
                    break;
                case 농업CommandCodes.HarvestCrop:
                    Harvest(package, state, events);
                    break;
                case 농업CommandCodes.CompareSales:
                    CompareSales(package, state, events);
                    break;
                default:
                    throw new InvalidOperationException("지원하지 않는 CommandCode입니다: " + command.CommandCode);
            }
        }

        private static void Plant(
            농업ScenarioPackage package,
            농업SimulationState state,
            ICollection<농업SimulationEvent> events)
        {
            if (state.IsPlanted || state.IsHarvested)
            {
                throw new InvalidOperationException("이미 작물을 심었거나 수확한 state입니다.");
            }

            state.IsPlanted = true;
            AddEvent(state, events, 농업EventCodes.CropPlanted, 1, "crop", "crop-planted");
            AccrueCost(package, state, events, 비용발생Codes.Planting);
        }

        private static void Water(
            농업ScenarioPackage package,
            농업SimulationState state,
            농업SimulationCommand command,
            ICollection<농업SimulationEvent> events)
        {
            if (!state.IsPlanted || state.IsHarvested)
            {
                throw new InvalidOperationException("재배 중인 작물에만 물을 줄 수 있습니다.");
            }

            if (command.Amount <= 0 || command.Amount > 1)
            {
                throw new InvalidOperationException("물주기 Amount는 0보다 크고 1 이하여야 합니다.");
            }

            state.MoistureRatio = ClampRatio(state.MoistureRatio + command.Amount);
            AddEvent(state, events, 농업EventCodes.TileWatered, command.Amount, "ratio", "tile-watered");
            AccrueCost(package, state, events, 비용발생Codes.Watering);
        }

        private static void AdvanceDay(
            농업ScenarioPackage package,
            농업SimulationState state,
            ICollection<농업SimulationEvent> events)
        {
            var nextDay = state.CurrentGameDay + 1;
            var weather = package.Weather.SingleOrDefault(item => item.GameDay == nextDay);
            if (weather == null)
            {
                throw new InvalidOperationException("다음 날짜의 날씨 snapshot이 없습니다: " + nextDay.ToString(CultureInfo.InvariantCulture));
            }

            state.CurrentGameDay = nextDay;
            state.MoistureRatio = ClampRatio(
                state.MoistureRatio
                + (weather.RainfallMm * package.Soil.RainRetentionRatioPerMillimeter)
                - package.Soil.DailyMoistureLossRatio);

            AddEvent(state, events, 농업EventCodes.DayAdvanced, 1, "game-day", "day-advanced");

            if (!state.IsPlanted || state.IsHarvested)
            {
                return;
            }

            AccrueCost(package, state, events, 비용발생Codes.DailyCare);

            var temperatureFactor = weather.MeanTemperatureC >= package.Crop.OptimalTemperatureMinC
                && weather.MeanTemperatureC <= package.Crop.OptimalTemperatureMaxC
                ? 1m
                : 0.5m;
            var moistureFactor = state.MoistureRatio >= package.Crop.MinimumMoistureRatio
                && state.MoistureRatio <= package.Crop.MaximumMoistureRatio
                ? 1m
                : 0.6m;
            var growth = package.Crop.BaseDailyGrowthPoint * temperatureFactor * moistureFactor;
            state.GrowthPoint = decimal.Round(state.GrowthPoint + growth, 4, MidpointRounding.AwayFromZero);

            AddEvent(state, events, 농업EventCodes.CropGrowthAdvanced, growth, "growth-point", "growth-from-weather-and-moisture");

            var nextStage = package.GrowthStages
                .Where(item => item.MinimumGrowthPoint <= state.GrowthPoint)
                .OrderByDescending(item => item.MinimumGrowthPoint)
                .ThenBy(item => item.StageKey, StringComparer.Ordinal)
                .First();
            if (!string.Equals(nextStage.StageKey, state.GrowthStageKey, StringComparison.Ordinal))
            {
                state.GrowthStageKey = nextStage.StageKey;
                AddEvent(state, events, 농업EventCodes.GrowthStageChanged, state.GrowthPoint, "growth-point", nextStage.StageKey);
            }
        }

        private static void Harvest(
            농업ScenarioPackage package,
            농업SimulationState state,
            ICollection<농업SimulationEvent> events)
        {
            if (!state.IsPlanted || state.IsHarvested)
            {
                throw new InvalidOperationException("수확 가능한 재배 state가 아닙니다.");
            }

            if (state.GrowthPoint < package.Crop.HarvestGrowthPoint)
            {
                throw new InvalidOperationException("수확 성장 기준에 도달하지 않았습니다.");
            }

            var yieldFactor = state.GrowthPoint / package.Crop.HarvestGrowthPoint;
            if (yieldFactor > 1)
            {
                yieldFactor = 1;
            }

            state.HarvestQuantityKg = decimal.Round(
                package.Crop.BaseYieldKg * yieldFactor,
                3,
                MidpointRounding.AwayFromZero);
            state.IsHarvested = true;

            AccrueCost(package, state, events, 비용발생Codes.Harvest);
            AddEvent(state, events, 농업EventCodes.HarvestCompleted, state.HarvestQuantityKg, "kg", "harvest-completed");
        }

        private static void CompareSales(
            농업ScenarioPackage package,
            농업SimulationState state,
            ICollection<농업SimulationEvent> events)
        {
            if (!state.IsHarvested || state.HarvestQuantityKg <= 0)
            {
                throw new InvalidOperationException("수확 완료 뒤에만 판매 방식을 비교할 수 있습니다.");
            }

            var results = package.SalesChannels
                .OrderBy(item => item.SalesChannelKey, StringComparer.Ordinal)
                .Select(rule => CalculateSales(package, state, rule))
                .ToArray();
            state.SalesComparisons = results;

            AddEvent(state, events, 농업EventCodes.SalesCompared, results.Length, "sales-channel", "simulated-sales-compared");
        }

        private static 판매비교Result CalculateSales(
            농업ScenarioPackage package,
            농업SimulationState state,
            판매방식Rule rule)
        {
            var expectedQuantity = decimal.Round(
                state.HarvestQuantityKg * rule.SaleableQuantityFactor,
                3,
                MidpointRounding.AwayFromZero);
            var unitPrice = ToLong(package.MarketObservation.PriceKrwPerKg * rule.PriceFactor);
            var revenue = ToLong(expectedQuantity * unitPrice);

            return new 판매비교Result
            {
                SalesChannelKey = rule.SalesChannelKey,
                HarvestQuantityKg = state.HarvestQuantityKg,
                ExpectedSaleQuantityKg = expectedQuantity,
                ExpectedUnitPriceKrwPerKg = unitPrice,
                ExpectedRevenueKrw = revenue,
                ProductionCostKrw = state.ProductionCostKrw,
                AdditionalSalesCostKrw = rule.AdditionalCostKrw,
                ExpectedProfitKrw = revenue - state.ProductionCostKrw - rule.AdditionalCostKrw,
                LaborDisclosureCodes = rule.LaborDisclosureCodes.ToArray(),
                RiskDisclosureCodes = rule.RiskDisclosureCodes.ToArray(),
                Lineage = new 파생값Lineage
                {
                    DerivedValueId = "derived:sales:" + rule.SalesChannelKey.ToLowerInvariant(),
                    InputEvidenceIds = new[] { package.MarketObservation.Evidence.EvidenceId },
                    RuleSetKey = package.Manifest.RuleSetKey,
                    RuleSetVersion = package.Manifest.RuleSetVersion,
                    RuleParametersHash = package.Manifest.ExpectedDataHash,
                    RandomSeed = package.Manifest.DefaultRandomSeed,
                    CalculatedAtGameTime = state.CurrentGameDay,
                    ExplanationCodes = new[]
                    {
                        "observed-price-is-not-a-quote",
                        "sales-result-is-simulated",
                    },
                },
            };
        }

        private static void AccrueCost(
            농업ScenarioPackage package,
            농업SimulationState state,
            ICollection<농업SimulationEvent> events,
            string triggerCode)
        {
            var amount = package.Costs
                .Where(item => string.Equals(item.TriggerCode, triggerCode, StringComparison.Ordinal))
                .Sum(item => item.AmountKrw);
            if (amount == 0)
            {
                return;
            }

            state.ProductionCostKrw += amount;
            AddEvent(state, events, 농업EventCodes.CostAccrued, amount, "KRW", triggerCode);
        }

        private static void AddEvent(
            농업SimulationState state,
            ICollection<농업SimulationEvent> events,
            string eventCode,
            decimal amount,
            string unit,
            string explanationCode)
        {
            state.LastEventSequence++;
            events.Add(new 농업SimulationEvent
            {
                Sequence = state.LastEventSequence,
                EventCode = eventCode,
                GameDay = state.CurrentGameDay,
                Amount = amount,
                Unit = unit,
                ExplanationCode = explanationCode,
            });
        }

        private static decimal ClampRatio(decimal value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value > 1 ? 1 : value;
        }

        private static long ToLong(decimal value)
        {
            return decimal.ToInt64(decimal.Round(value, 0, MidpointRounding.AwayFromZero));
        }

        private static string CalculateFinalStateHash(
            농업ScenarioPackage package,
            농업SimulationState state,
            IEnumerable<농업SimulationEvent> events)
        {
            var builder = new StringBuilder();
            builder.Append(package.Manifest.ExpectedDataHash);
            builder.Append('|').Append(state.ScenarioKey);
            builder.Append('|').Append(state.ScenarioVersion);
            builder.Append('|').Append(state.CurrentGameDay.ToString(CultureInfo.InvariantCulture));
            builder.Append('|').Append(state.IsPlanted ? "1" : "0");
            builder.Append('|').Append(state.IsHarvested ? "1" : "0");
            builder.Append('|').Append(state.MoistureRatio.ToString("G29", CultureInfo.InvariantCulture));
            builder.Append('|').Append(state.GrowthPoint.ToString("G29", CultureInfo.InvariantCulture));
            builder.Append('|').Append(state.GrowthStageKey);
            builder.Append('|').Append(state.HarvestQuantityKg.ToString("G29", CultureInfo.InvariantCulture));
            builder.Append('|').Append(state.ProductionCostKrw.ToString(CultureInfo.InvariantCulture));

            foreach (var comparison in state.SalesComparisons.OrderBy(item => item.SalesChannelKey, StringComparer.Ordinal))
            {
                builder.Append('|').Append(comparison.SalesChannelKey);
                builder.Append('|').Append(comparison.ExpectedSaleQuantityKg.ToString("G29", CultureInfo.InvariantCulture));
                builder.Append('|').Append(comparison.ExpectedUnitPriceKrwPerKg.ToString(CultureInfo.InvariantCulture));
                builder.Append('|').Append(comparison.ExpectedRevenueKrw.ToString(CultureInfo.InvariantCulture));
                builder.Append('|').Append(comparison.ExpectedProfitKrw.ToString(CultureInfo.InvariantCulture));
            }

            foreach (var simulationEvent in events.OrderBy(item => item.Sequence))
            {
                builder.Append('|').Append(simulationEvent.Sequence.ToString(CultureInfo.InvariantCulture));
                builder.Append('|').Append(simulationEvent.EventCode);
                builder.Append('|').Append(simulationEvent.GameDay.ToString(CultureInfo.InvariantCulture));
                builder.Append('|').Append(simulationEvent.Amount.ToString("G29", CultureInfo.InvariantCulture));
                builder.Append('|').Append(simulationEvent.Unit);
                builder.Append('|').Append(simulationEvent.ExplanationCode);
            }

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
                var result = new StringBuilder(hash.Length * 2);
                foreach (var item in hash)
                {
                    result.Append(item.ToString("x2", CultureInfo.InvariantCulture));
                }

                return result.ToString();
            }
        }
    }
}
