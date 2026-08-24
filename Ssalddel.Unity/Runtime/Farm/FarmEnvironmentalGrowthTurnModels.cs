using System;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public static class 재배환경실행ModeCodes
    {
        public const string SimulationFixture = "SimulationFixture";
        public const string PublicObservationReference = "PublicObservationReference";

        internal static bool IsKnown(string value)
            => value == SimulationFixture || value == PublicObservationReference;
    }

    public static class 재배환경제한요인Codes
    {
        public const string None = "None";
        public const string Temperature = "Temperature";
        public const string Sunlight = "Sunlight";
        public const string Water = "Water";
    }

    public sealed class 재배환경구간Rule
    {
        public decimal AbsoluteMinimum { get; set; }
        public decimal OptimalMinimum { get; set; }
        public decimal OptimalMaximum { get; set; }
        public decimal AbsoluteMaximum { get; set; }
    }

    public sealed class 환경생육단계Definition
    {
        public string StageCode { get; set; } = string.Empty;
        public decimal MinimumAccumulatedGrowthPoint { get; set; }
    }

    public sealed class 작물환경생육RuleSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string CropVariantStableId { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public decimal BaseDailyGrowthPoint { get; set; }
        public decimal SoilWaterCapacityMm { get; set; }
        public decimal RainfallInfiltrationRatio { get; set; }
        public decimal ExcessWaterDrainageRatio { get; set; }
        public 재배환경구간Rule TemperatureC { get; set; } = new 재배환경구간Rule();
        public 재배환경구간Rule SolarRadiationMjPerSquareMeter { get; set; } = new 재배환경구간Rule();
        public 재배환경구간Rule SoilWaterMm { get; set; } = new 재배환경구간Rule();
        public 환경생육단계Definition[] GrowthStages { get; set; } = Array.Empty<환경생육단계Definition>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class 토양기준Snapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string SourceTypeCode { get; set; } = string.Empty;
        public string QualityCode { get; set; } = string.Empty;
        public DateTimeOffset ObservedAt { get; set; }
        public decimal CropSuitabilityFactor { get; set; }
        public string SpatialPrecisionCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 일별기후관측Snapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string SourceTypeCode { get; set; } = string.Empty;
        public string QualityCode { get; set; } = string.Empty;
        public DateTimeOffset ObservedOn { get; set; }
        public decimal? MeanTemperatureC { get; set; }
        public decimal? RainfallMm { get; set; }
        public decimal? SolarRadiationMjPerSquareMeter { get; set; }
        public decimal? RelativeHumidityPercent { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public decimal StationDistanceKm { get; set; }
        public string PayloadHash { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 재배환경일일Snapshot
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string ModeCode { get; set; } = string.Empty;
        public DateTimeOffset SimulationDate { get; set; }
        public string CultivationStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public string RuleStableId { get; set; } = string.Empty;
        public long RuleRevision { get; set; }
        public decimal PreviousSoilWaterMm { get; set; }
        public decimal IrrigationMm { get; set; }
        public decimal EvapotranspirationMm { get; set; }
        public 토양기준Snapshot Soil { get; set; } = new 토양기준Snapshot();
        public 일별기후관측Snapshot Climate { get; set; } = new 일별기후관측Snapshot();
    }

    public sealed class 재배환경스트레스State
    {
        public decimal Drought { get; set; }
        public decimal Waterlogging { get; set; }
        public decimal Cold { get; set; }
        public decimal Heat { get; set; }
        public decimal LowSunlight { get; set; }
        public decimal SoilMismatch { get; set; }
    }

    public sealed class 재배환경생육State
    {
        public string CultivationStableId { get; set; } = string.Empty;
        public string CanonicalProductStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public DateTimeOffset SimulationDate { get; set; }
        public int DaysAfterSowing { get; set; }
        public decimal SoilWaterMm { get; set; }
        public decimal AccumulatedGrowthPoint { get; set; }
        public string GrowthStageCode { get; set; } = string.Empty;
        public 재배환경스트레스State Stress { get; set; } = new 재배환경스트레스State();
    }

    public sealed class 재배환경하루판정Result
    {
        public 재배환경생육State State { get; set; } = new 재배환경생육State();
        public decimal EffectiveRainfallMm { get; set; }
        public decimal RunoffMm { get; set; }
        public decimal DrainageMm { get; set; }
        public decimal TemperatureFactor { get; set; }
        public decimal SunlightFactor { get; set; }
        public decimal WaterFactor { get; set; }
        public decimal SoilSuitabilityFactor { get; set; }
        public decimal DailyGrowthPoint { get; set; }
        public string LimitingFactorCode { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 작물환경생육RuleValidator
    {
        public void Validate(작물환경생육RuleSnapshot rule)
        {
            if (rule == null
                || !StableDataId.IsValid(rule.StableId)
                || rule.Revision <= 0
                || !StableDataId.IsValid(rule.CanonicalProductStableId)
                || !StableDataId.IsValid(rule.CropVariantStableId)
                || rule.SourceTypeCode != 데이터SourceTypes.Fixture
                || rule.BaseDailyGrowthPoint <= 0m
                || rule.SoilWaterCapacityMm <= 0m
                || !IsRatio(rule.RainfallInfiltrationRatio)
                || !IsRatio(rule.ExcessWaterDrainageRatio)
                || rule.SourceStableIds == null || rule.SourceStableIds.Length == 0
                || rule.SourceStableIds.Any(value => !StableDataId.IsValid(value))
                || rule.SourceStableIds.Distinct(StringComparer.Ordinal).Count() != rule.SourceStableIds.Length
                || rule.Limitations == null || rule.Limitations.Length == 0
                || rule.Limitations.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException("FarmEnvironmentalGrowthRuleInvalid");
            }

            ValidateRange(rule.TemperatureC, "Temperature");
            ValidateRange(rule.SolarRadiationMjPerSquareMeter, "Sunlight");
            ValidateRange(rule.SoilWaterMm, "SoilWater");
            ValidateStages(rule.GrowthStages);
            if (rule.SoilWaterMm.AbsoluteMinimum < 0m
                || rule.SoilWaterMm.AbsoluteMaximum > rule.SoilWaterCapacityMm)
            {
                throw new InvalidOperationException("FarmEnvironmentalSoilWaterRuleInvalid");
            }
        }

        private static void ValidateRange(재배환경구간Rule range, string name)
        {
            if (range == null
                || range.AbsoluteMinimum >= range.OptimalMinimum
                || range.OptimalMinimum > range.OptimalMaximum
                || range.OptimalMaximum >= range.AbsoluteMaximum)
            {
                throw new InvalidOperationException("FarmEnvironmental" + name + "RangeInvalid");
            }
        }

        private static void ValidateStages(환경생육단계Definition[] stages)
        {
            if (stages == null || stages.Length < 5
                || stages.Any(value => value == null
                    || !재배생육단계Codes.IsKnown(value.StageCode)
                    || value.StageCode == 재배생육단계Codes.Harvested
                    || value.MinimumAccumulatedGrowthPoint < 0m)
                || stages.Select(value => value.StageCode).Distinct(StringComparer.Ordinal).Count() != stages.Length
                || stages.Select(value => value.MinimumAccumulatedGrowthPoint).Distinct().Count() != stages.Length)
            {
                throw new InvalidOperationException("FarmEnvironmentalGrowthStagesInvalid");
            }

            var ordered = stages.OrderBy(value => value.MinimumAccumulatedGrowthPoint).ToArray();
            if (ordered[0].StageCode != 재배생육단계Codes.Sown
                || ordered[0].MinimumAccumulatedGrowthPoint != 0m
                || ordered[ordered.Length - 1].StageCode != 재배생육단계Codes.HarvestReady)
            {
                throw new InvalidOperationException("FarmEnvironmentalGrowthStagesInvalid");
            }
        }

        private static bool IsRatio(decimal value) => value >= 0m && value <= 1m;
    }

    public sealed class 재배환경생육TurnCalculator
    {
        private readonly 작물환경생육RuleValidator ruleValidator;

        public 재배환경생육TurnCalculator()
            : this(new 작물환경생육RuleValidator())
        {
        }

        public 재배환경생육TurnCalculator(작물환경생육RuleValidator validator)
        {
            ruleValidator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public 재배환경하루판정Result EvaluateDay(
            재배환경생육State current,
            재배환경일일Snapshot environment,
            작물환경생육RuleSnapshot rule)
        {
            ruleValidator.Validate(rule);
            ValidateInput(current, environment, rule);

            var rainfall = environment.Climate.RainfallMm!.Value;
            var effectiveRainfall = Round(rainfall * rule.RainfallInfiltrationRatio);
            var runoff = Round(rainfall - effectiveRainfall);
            var beforeDrainage = current.SoilWaterMm + effectiveRainfall
                + environment.IrrigationMm - environment.EvapotranspirationMm;
            var drainage = Round(Math.Max(0m, beforeDrainage - rule.SoilWaterCapacityMm)
                * rule.ExcessWaterDrainageRatio);
            var nextWater = Round(Clamp(beforeDrainage - drainage, 0m, rule.SoilWaterCapacityMm));

            var temperatureFactor = RangeFactor(environment.Climate.MeanTemperatureC!.Value, rule.TemperatureC);
            var sunlightFactor = RangeFactor(
                environment.Climate.SolarRadiationMjPerSquareMeter!.Value,
                rule.SolarRadiationMjPerSquareMeter);
            var waterFactor = RangeFactor(nextWater, rule.SoilWaterMm);
            var limitingFactor = Math.Min(temperatureFactor, Math.Min(sunlightFactor, waterFactor));
            var dailyGrowth = Round(rule.BaseDailyGrowthPoint
                * limitingFactor
                * environment.Soil.CropSuitabilityFactor);
            var accumulatedGrowth = Round(current.AccumulatedGrowthPoint + dailyGrowth);

            var stress = CopyStress(current.Stress);
            AccumulateTemperatureStress(stress, environment.Climate.MeanTemperatureC.Value,
                temperatureFactor, rule.TemperatureC);
            if (sunlightFactor < 1m)
            {
                stress.LowSunlight = Round(stress.LowSunlight + (1m - sunlightFactor));
            }

            if (waterFactor < 1m)
            {
                if (nextWater < rule.SoilWaterMm.OptimalMinimum)
                {
                    stress.Drought = Round(stress.Drought + (1m - waterFactor));
                }
                else if (nextWater > rule.SoilWaterMm.OptimalMaximum)
                {
                    stress.Waterlogging = Round(stress.Waterlogging + (1m - waterFactor));
                }
            }

            if (environment.Soil.CropSuitabilityFactor < 1m)
            {
                stress.SoilMismatch = Round(stress.SoilMismatch
                    + (1m - environment.Soil.CropSuitabilityFactor));
            }

            var nextState = new 재배환경생육State
            {
                CultivationStableId = current.CultivationStableId,
                CanonicalProductStableId = current.CanonicalProductStableId,
                Revision = current.Revision + 1,
                SimulationDate = environment.SimulationDate,
                DaysAfterSowing = current.DaysAfterSowing + 1,
                SoilWaterMm = nextWater,
                AccumulatedGrowthPoint = accumulatedGrowth,
                GrowthStageCode = ResolveStage(accumulatedGrowth, rule),
                Stress = stress,
            };

            return new 재배환경하루판정Result
            {
                State = nextState,
                EffectiveRainfallMm = effectiveRainfall,
                RunoffMm = runoff,
                DrainageMm = drainage,
                TemperatureFactor = temperatureFactor,
                SunlightFactor = sunlightFactor,
                WaterFactor = waterFactor,
                SoilSuitabilityFactor = environment.Soil.CropSuitabilityFactor,
                DailyGrowthPoint = dailyGrowth,
                LimitingFactorCode = ResolveLimitingFactor(
                    temperatureFactor, sunlightFactor, waterFactor),
                SourceStableIds = environment.Soil.SourceStableIds
                    .Concat(environment.Climate.SourceStableIds)
                    .Concat(rule.SourceStableIds)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
            };
        }

        private static void ValidateInput(
            재배환경생육State current,
            재배환경일일Snapshot environment,
            작물환경생육RuleSnapshot rule)
        {
            if (current == null || environment == null
                || !StableDataId.IsValid(current.CultivationStableId)
                || !StableDataId.IsValid(current.CanonicalProductStableId)
                || current.Revision <= 0
                || current.SimulationDate == default
                || current.DaysAfterSowing < 0
                || current.SoilWaterMm < 0m || current.SoilWaterMm > rule.SoilWaterCapacityMm
                || current.AccumulatedGrowthPoint < 0m
                || !재배생육단계Codes.IsKnown(current.GrowthStageCode)
                || current.GrowthStageCode == 재배생육단계Codes.Harvested
                || current.Stress == null)
            {
                throw new InvalidOperationException("FarmEnvironmentalGrowthStateInvalid");
            }

            if (!StableDataId.IsValid(environment.StableId)
                || environment.Revision <= 0
                || !재배환경실행ModeCodes.IsKnown(environment.ModeCode)
                || environment.SimulationDate != current.SimulationDate.AddDays(1)
                || environment.CultivationStableId != current.CultivationStableId
                || environment.CanonicalProductStableId != current.CanonicalProductStableId
                || environment.CanonicalProductStableId != rule.CanonicalProductStableId
                || environment.RuleStableId != rule.StableId
                || environment.RuleRevision != rule.Revision
                || environment.PreviousSoilWaterMm != current.SoilWaterMm
                || environment.IrrigationMm < 0m
                || environment.EvapotranspirationMm < 0m)
            {
                throw new InvalidOperationException("FarmEnvironmentalDailySnapshotInvalid");
            }

            ValidateSoil(environment.Soil, environment.ModeCode, environment.SimulationDate);
            ValidateClimate(environment.Climate, environment.ModeCode, environment.SimulationDate);
        }

        private static void ValidateSoil(
            토양기준Snapshot soil,
            string modeCode,
            DateTimeOffset simulationDate)
        {
            if (soil == null || !StableDataId.IsValid(soil.StableId)
                || soil.Revision <= 0 || soil.ObservedAt == default
                || soil.ObservedAt > simulationDate
                || soil.CropSuitabilityFactor < 0m || soil.CropSuitabilityFactor > 1m
                || string.IsNullOrWhiteSpace(soil.SpatialPrecisionCode)
                || soil.SourceStableIds == null || soil.SourceStableIds.Length == 0
                || soil.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
            {
                throw new InvalidOperationException("FarmEnvironmentalSoilSnapshotInvalid");
            }

            ValidateSourceQuality(modeCode, soil.SourceTypeCode, soil.QualityCode);
        }

        private static void ValidateClimate(
            일별기후관측Snapshot climate,
            string modeCode,
            DateTimeOffset simulationDate)
        {
            if (climate == null || !StableDataId.IsValid(climate.StableId)
                || climate.Revision <= 0 || climate.ObservedOn != simulationDate
                || climate.MeanTemperatureC == null
                || climate.RainfallMm == null || climate.RainfallMm < 0m
                || climate.SolarRadiationMjPerSquareMeter == null
                || climate.SolarRadiationMjPerSquareMeter < 0m
                || climate.RelativeHumidityPercent == null
                || climate.RelativeHumidityPercent < 0m || climate.RelativeHumidityPercent > 100m
                || string.IsNullOrWhiteSpace(climate.StationCode)
                || climate.StationDistanceKm < 0m
                || string.IsNullOrWhiteSpace(climate.PayloadHash)
                || climate.SourceStableIds == null || climate.SourceStableIds.Length == 0
                || climate.SourceStableIds.Any(value => !StableDataId.IsValid(value)))
            {
                throw new InvalidOperationException("FarmEnvironmentalObservationMissing");
            }

            ValidateSourceQuality(modeCode, climate.SourceTypeCode, climate.QualityCode);
        }

        private static void ValidateSourceQuality(
            string modeCode,
            string sourceTypeCode,
            string qualityCode)
        {
            var fixture = modeCode == 재배환경실행ModeCodes.SimulationFixture;
            if (fixture && (sourceTypeCode != 데이터SourceTypes.Fixture
                    || qualityCode != 데이터품질Codes.Fixture)
                || !fixture && (sourceTypeCode != 데이터SourceTypes.PublicObservation
                    || qualityCode != 데이터품질Codes.Valid))
            {
                throw new InvalidOperationException("FarmEnvironmentalSourceQualityMismatch");
            }
        }

        private static decimal RangeFactor(decimal value, 재배환경구간Rule range)
        {
            if (value <= range.AbsoluteMinimum || value >= range.AbsoluteMaximum)
            {
                return 0m;
            }

            if (value >= range.OptimalMinimum && value <= range.OptimalMaximum)
            {
                return 1m;
            }

            return value < range.OptimalMinimum
                ? Round((value - range.AbsoluteMinimum)
                    / (range.OptimalMinimum - range.AbsoluteMinimum))
                : Round((range.AbsoluteMaximum - value)
                    / (range.AbsoluteMaximum - range.OptimalMaximum));
        }

        private static string ResolveStage(decimal accumulatedGrowth, 작물환경생육RuleSnapshot rule)
            => rule.GrowthStages
                .Where(value => value.MinimumAccumulatedGrowthPoint <= accumulatedGrowth)
                .OrderByDescending(value => value.MinimumAccumulatedGrowthPoint)
                .ThenBy(value => value.StageCode, StringComparer.Ordinal)
                .First()
                .StageCode;

        private static string ResolveLimitingFactor(decimal temperature, decimal sunlight, decimal water)
        {
            var minimum = Math.Min(temperature, Math.Min(sunlight, water));
            if (minimum >= 1m)
            {
                return 재배환경제한요인Codes.None;
            }

            if (temperature == minimum)
            {
                return 재배환경제한요인Codes.Temperature;
            }

            return sunlight == minimum
                ? 재배환경제한요인Codes.Sunlight
                : 재배환경제한요인Codes.Water;
        }

        private static void AccumulateTemperatureStress(
            재배환경스트레스State stress,
            decimal temperature,
            decimal factor,
            재배환경구간Rule range)
        {
            if (factor >= 1m)
            {
                return;
            }

            if (temperature < range.OptimalMinimum)
            {
                stress.Cold = Round(stress.Cold + (1m - factor));
            }
            else if (temperature > range.OptimalMaximum)
            {
                stress.Heat = Round(stress.Heat + (1m - factor));
            }
        }

        private static 재배환경스트레스State CopyStress(재배환경스트레스State source)
            => new 재배환경스트레스State
            {
                Drought = source.Drought,
                Waterlogging = source.Waterlogging,
                Cold = source.Cold,
                Heat = source.Heat,
                LowSunlight = source.LowSunlight,
                SoilMismatch = source.SoilMismatch,
            };

        private static decimal Clamp(decimal value, decimal minimum, decimal maximum)
            => value < minimum ? minimum : value > maximum ? maximum : value;

        private static decimal Round(decimal value)
            => decimal.Round(value, 4, MidpointRounding.AwayFromZero);
    }

    public static class 감자재배환경SimulationFixture
    {
        public static 작물환경생육RuleSnapshot CreateRule()
            => new 작물환경생육RuleSnapshot
            {
                StableId = "farm-environment-rule:fixture.potato.open-field.1",
                Revision = 1,
                CanonicalProductStableId = "product:potato",
                CropVariantStableId = "crop-variant:fixture.potato.open-field.1",
                SourceTypeCode = 데이터SourceTypes.Fixture,
                BaseDailyGrowthPoint = 1m,
                SoilWaterCapacityMm = 100m,
                RainfallInfiltrationRatio = 0.8m,
                ExcessWaterDrainageRatio = 0.5m,
                TemperatureC = new 재배환경구간Rule
                {
                    AbsoluteMinimum = 0m,
                    OptimalMinimum = 15m,
                    OptimalMaximum = 22m,
                    AbsoluteMaximum = 35m,
                },
                SolarRadiationMjPerSquareMeter = new 재배환경구간Rule
                {
                    AbsoluteMinimum = 0m,
                    OptimalMinimum = 16m,
                    OptimalMaximum = 28m,
                    AbsoluteMaximum = 40m,
                },
                SoilWaterMm = new 재배환경구간Rule
                {
                    AbsoluteMinimum = 10m,
                    OptimalMinimum = 40m,
                    OptimalMaximum = 70m,
                    AbsoluteMaximum = 100m,
                },
                GrowthStages = new[]
                {
                    new 환경생육단계Definition { StageCode = 재배생육단계Codes.Sown, MinimumAccumulatedGrowthPoint = 0m },
                    new 환경생육단계Definition { StageCode = 재배생육단계Codes.Emerged, MinimumAccumulatedGrowthPoint = 1m },
                    new 환경생육단계Definition { StageCode = 재배생육단계Codes.Vegetative, MinimumAccumulatedGrowthPoint = 2m },
                    new 환경생육단계Definition { StageCode = 재배생육단계Codes.Bulking, MinimumAccumulatedGrowthPoint = 4m },
                    new 환경생육단계Definition { StageCode = 재배생육단계Codes.HarvestReady, MinimumAccumulatedGrowthPoint = 6m },
                },
                SourceStableIds = new[] { "simulation-assumption:potato.environment-growth.1" },
                Limitations = new[]
                {
                    "검증용 합성 규칙이며 농사로 권고, 실제 생리 모형 또는 수확량 예측이 아닙니다.",
                },
            };

        public static 재배환경생육State CreateState(decimal soilWaterMm = 55m)
            => new 재배환경생육State
            {
                CultivationStableId = "cultivation:sim.potato.environment.1",
                CanonicalProductStableId = "product:potato",
                Revision = 1,
                SimulationDate = UtcDate(2026, 4, 1),
                DaysAfterSowing = 0,
                SoilWaterMm = soilWaterMm,
                AccumulatedGrowthPoint = 0m,
                GrowthStageCode = 재배생육단계Codes.Sown,
                Stress = new 재배환경스트레스State(),
            };

        public static 재배환경일일Snapshot CreateEnvironment(
            재배환경생육State state,
            decimal rainfallMm = 5m,
            decimal solarRadiation = 20m,
            decimal meanTemperatureC = 18m,
            decimal evapotranspirationMm = 4m)
            => new 재배환경일일Snapshot
            {
                StableId = "farm-environment-daily:fixture.potato.day."
                    + (state.DaysAfterSowing + 1),
                Revision = 1,
                ModeCode = 재배환경실행ModeCodes.SimulationFixture,
                SimulationDate = state.SimulationDate.AddDays(1),
                CultivationStableId = state.CultivationStableId,
                CanonicalProductStableId = state.CanonicalProductStableId,
                RuleStableId = CreateRule().StableId,
                RuleRevision = 1,
                PreviousSoilWaterMm = state.SoilWaterMm,
                IrrigationMm = 0m,
                EvapotranspirationMm = evapotranspirationMm,
                Soil = new 토양기준Snapshot
                {
                    StableId = "soil-baseline:fixture.potato.plot.1",
                    Revision = 1,
                    SourceTypeCode = 데이터SourceTypes.Fixture,
                    QualityCode = 데이터품질Codes.Fixture,
                    ObservedAt = UtcDate(2026, 4, 1),
                    CropSuitabilityFactor = 1m,
                    SpatialPrecisionCode = "SimulationPlot",
                    SourceStableIds = new[] { "simulation-assumption:soil.potato.plot.1" },
                },
                Climate = new 일별기후관측Snapshot
                {
                    StableId = "daily-climate:fixture.potato.day."
                        + (state.DaysAfterSowing + 1),
                    Revision = 1,
                    SourceTypeCode = 데이터SourceTypes.Fixture,
                    QualityCode = 데이터품질Codes.Fixture,
                    ObservedOn = state.SimulationDate.AddDays(1),
                    MeanTemperatureC = meanTemperatureC,
                    RainfallMm = rainfallMm,
                    SolarRadiationMjPerSquareMeter = solarRadiation,
                    RelativeHumidityPercent = 60m,
                    StationCode = "FIXTURE-PLOT-1",
                    StationDistanceKm = 0m,
                    PayloadHash = "fixture-potato-environment-day-"
                        + (state.DaysAfterSowing + 1),
                    SourceStableIds = new[] { "simulation-assumption:climate.potato.day.1" },
                },
            };

        private static DateTimeOffset UtcDate(int year, int month, int day)
            => new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
    }
}
