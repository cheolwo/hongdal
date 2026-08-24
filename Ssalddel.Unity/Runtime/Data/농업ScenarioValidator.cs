using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Unity.Data
{
    public sealed class 데이터검증Issue
    {
        public 데이터검증Issue(string code, string path, string message)
        {
            Code = code;
            Path = path;
            Message = message;
        }

        public string Code { get; }

        public string Path { get; }

        public string Message { get; }
    }

    public sealed class 데이터검증Result
    {
        public 데이터검증Result(데이터검증Issue[] issues)
        {
            Issues = issues ?? Array.Empty<데이터검증Issue>();
        }

        public 데이터검증Issue[] Issues { get; }

        public bool IsValid => Issues.Length == 0;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 농업ScenarioValidator
    {
        public const string SupportedSchemaVersion = "1.0.0";

        public 데이터검증Result Validate(농업ScenarioPackage? package)
        {
            var issues = new List<데이터검증Issue>();
            if (package == null)
            {
                issues.Add(Issue("PackageMissing", "$", "시나리오 package가 필요합니다."));
                return new 데이터검증Result(issues.ToArray());
            }

            ValidateManifest(package.Manifest, issues);
            ValidateCrop(package.Crop, issues);
            ValidateSoil(package.Soil, issues);
            ValidateStages(package, issues);
            ValidateWeather(package, issues);
            ValidateCosts(package.Costs, issues);
            ValidateMarket(package, issues);
            ValidateSalesChannels(package.SalesChannels, issues);
            ValidateMappings(package, issues);

            if (issues.Count == 0)
            {
                var actualHash = 농업ScenarioHashCalculator.Calculate(package);
                if (!string.Equals(package.Manifest.ExpectedDataHash, actualHash, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(Issue(
                        "DataHashMismatch",
                        "$.manifest.expectedDataHash",
                        "시나리오 data hash가 manifest와 일치하지 않습니다. actual=" + actualHash));
                }
            }

            return new 데이터검증Result(issues.ToArray());
        }

        private static void ValidateManifest(ScenarioManifest manifest, ICollection<데이터검증Issue> issues)
        {
            RequireStableId(manifest.ScenarioKey, "$.manifest.scenarioKey", issues);
            RequireStableId(manifest.RuleSetKey, "$.manifest.ruleSetKey", issues);
            RequireText(manifest.ScenarioVersion, "$.manifest.scenarioVersion", issues);
            RequireText(manifest.RuleSetVersion, "$.manifest.ruleSetVersion", issues);

            if (!string.Equals(manifest.SchemaVersion, SupportedSchemaVersion, StringComparison.Ordinal))
            {
                issues.Add(Issue("UnsupportedSchema", "$.manifest.schemaVersion", "지원하지 않는 schema version입니다."));
            }

            if (!string.Equals(manifest.Mode, "SIMULATED", StringComparison.Ordinal))
            {
                issues.Add(Issue("OperationalModeForbidden", "$.manifest.mode", "초기 Unity package는 SIMULATED mode만 허용합니다."));
            }

            if (manifest.ExpectedDataHash == null || manifest.ExpectedDataHash.Length != 64)
            {
                issues.Add(Issue("DataHashInvalid", "$.manifest.expectedDataHash", "SHA-256 data hash가 필요합니다."));
            }
        }

        private static void ValidateCrop(작물Definition crop, ICollection<데이터검증Issue> issues)
        {
            RequireStableId(crop.CropKey, "$.crop.cropKey", issues);
            RequireStableId(crop.VarietyKey, "$.crop.varietyKey", issues);

            if (crop.BaseDailyGrowthPoint <= 0 || crop.HarvestGrowthPoint <= 0 || crop.BaseYieldKg <= 0)
            {
                issues.Add(Issue("CropRangeInvalid", "$.crop", "성장량, 수확 기준과 수확량은 0보다 커야 합니다."));
            }

            if (crop.OptimalTemperatureMinC >= crop.OptimalTemperatureMaxC)
            {
                issues.Add(Issue("TemperatureRangeInvalid", "$.crop", "최적 온도 범위가 올바르지 않습니다."));
            }

            if (!IsRatio(crop.MinimumMoistureRatio)
                || !IsRatio(crop.MaximumMoistureRatio)
                || crop.MinimumMoistureRatio >= crop.MaximumMoistureRatio)
            {
                issues.Add(Issue("MoistureRangeInvalid", "$.crop", "작물 수분 비율 범위가 올바르지 않습니다."));
            }
        }

        private static void ValidateSoil(토양Definition soil, ICollection<데이터검증Issue> issues)
        {
            RequireStableId(soil.SoilKey, "$.soil.soilKey", issues);
            if (!IsRatio(soil.InitialMoistureRatio)
                || soil.RainRetentionRatioPerMillimeter < 0
                || !IsRatio(soil.DailyMoistureLossRatio))
            {
                issues.Add(Issue("SoilRangeInvalid", "$.soil", "토양 수분·보수·손실 값이 허용 범위를 벗어났습니다."));
            }
        }

        private static void ValidateStages(농업ScenarioPackage package, ICollection<데이터검증Issue> issues)
        {
            var stages = package.GrowthStages ?? Array.Empty<성장단계Definition>();
            if (stages.Length < 4)
            {
                issues.Add(Issue("GrowthStagesInsufficient", "$.growthStages", "최소 네 개 성장 단계가 필요합니다."));
                return;
            }

            if (stages.GroupBy(item => item.StageKey, StringComparer.Ordinal).Any(group => group.Count() > 1))
            {
                issues.Add(Issue("GrowthStageDuplicate", "$.growthStages", "성장 단계 key가 중복되었습니다."));
            }

            foreach (var stage in stages)
            {
                RequireStableId(stage.StageKey, "$.growthStages[].stageKey", issues);
                if (stage.MinimumGrowthPoint < 0 || stage.MinimumGrowthPoint > package.Crop.HarvestGrowthPoint)
                {
                    issues.Add(Issue("GrowthStageRangeInvalid", "$.growthStages[]", "성장 단계 기준이 수확 범위를 벗어났습니다."));
                }
            }

            if (stages.Min(item => item.MinimumGrowthPoint) != 0)
            {
                issues.Add(Issue("GrowthStageStartInvalid", "$.growthStages", "첫 성장 단계는 0에서 시작해야 합니다."));
            }
        }

        private static void ValidateWeather(농업ScenarioPackage package, ICollection<데이터검증Issue> issues)
        {
            var weather = package.Weather ?? Array.Empty<일별날씨Snapshot>();
            if (weather.Length == 0)
            {
                issues.Add(Issue("WeatherMissing", "$.weather", "최소 하루의 날씨 snapshot이 필요합니다."));
                return;
            }

            if (weather.GroupBy(item => item.GameDay).Any(group => group.Count() > 1))
            {
                issues.Add(Issue("WeatherDayDuplicate", "$.weather", "게임 날짜별 날씨가 중복되었습니다."));
            }

            var orderedDays = weather.Select(item => item.GameDay).OrderBy(item => item).ToArray();
            for (var index = 0; index < orderedDays.Length; index++)
            {
                if (orderedDays[index] != index + 1)
                {
                    issues.Add(Issue("WeatherDayGap", "$.weather", "날씨 snapshot은 1일부터 연속되어야 합니다."));
                    break;
                }
            }

            foreach (var day in weather)
            {
                if (day.GameDay <= 0 || day.RainfallMm < 0)
                {
                    issues.Add(Issue("WeatherRangeInvalid", "$.weather[]", "날짜와 강수량이 허용 범위를 벗어났습니다."));
                }

                ValidateEvidence(day.TemperatureEvidence, "$.weather[].temperatureEvidence", "degC", issues);
                ValidateEvidence(day.RainfallEvidence, "$.weather[].rainfallEvidence", "mm", issues);
                if (day.TemperatureEvidence.NormalizedValue != day.MeanTemperatureC)
                {
                    issues.Add(Issue("TemperatureEvidenceMismatch", "$.weather[]", "기온과 evidence 정규화값이 일치하지 않습니다."));
                }

                if (day.RainfallEvidence.NormalizedValue != day.RainfallMm)
                {
                    issues.Add(Issue("RainfallEvidenceMismatch", "$.weather[]", "강수량과 evidence 정규화값이 일치하지 않습니다."));
                }
            }
        }

        private static void ValidateCosts(비용항목Definition[]? costs, ICollection<데이터검증Issue> issues)
        {
            var items = costs ?? Array.Empty<비용항목Definition>();
            if (items.GroupBy(item => item.CostKey, StringComparer.Ordinal).Any(group => group.Count() > 1))
            {
                issues.Add(Issue("CostDuplicate", "$.costs", "비용 항목 key가 중복되었습니다."));
            }

            foreach (var item in items)
            {
                RequireStableId(item.CostKey, "$.costs[].costKey", issues);
                RequireText(item.TriggerCode, "$.costs[].triggerCode", issues);
                if (item.AmountKrw < 0)
                {
                    issues.Add(Issue("CostRangeInvalid", "$.costs[].amountKrw", "비용은 음수일 수 없습니다."));
                }
            }
        }

        private static void ValidateMarket(농업ScenarioPackage package, ICollection<데이터검증Issue> issues)
        {
            var market = package.MarketObservation;
            RequireStableId(market.ObservationKey, "$.marketObservation.observationKey", issues);
            if (!string.Equals(market.CropKey, package.Crop.CropKey, StringComparison.Ordinal))
            {
                issues.Add(Issue("MarketCropMismatch", "$.marketObservation.cropKey", "시장 관측의 작물 key가 시나리오 작물과 다릅니다."));
            }

            if (market.PriceKrwPerKg <= 0)
            {
                issues.Add(Issue("MarketPriceInvalid", "$.marketObservation.priceKrwPerKg", "시장 관측가격은 0보다 커야 합니다."));
            }

            ValidateEvidence(market.Evidence, "$.marketObservation.evidence", "KRW/kg", issues);
            if (market.Evidence.NormalizedValue != market.PriceKrwPerKg)
            {
                issues.Add(Issue("MarketPriceEvidenceMismatch", "$.marketObservation", "정규화 가격과 evidence 값이 일치하지 않습니다."));
            }
        }

        private static void ValidateSalesChannels(판매방식Rule[]? channels, ICollection<데이터검증Issue> issues)
        {
            var items = channels ?? Array.Empty<판매방식Rule>();
            var keys = items.Select(item => item.SalesChannelKey).ToArray();
            if (!keys.Contains(판매방식Codes.General) || !keys.Contains(판매방식Codes.Collective))
            {
                issues.Add(Issue("SalesChannelsMissing", "$.salesChannels", "일반판매와 공동판매 rule이 모두 필요합니다."));
            }

            if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
            {
                issues.Add(Issue("SalesChannelDuplicate", "$.salesChannels", "판매 방식 key가 중복되었습니다."));
            }

            foreach (var item in items)
            {
                if (item.PriceFactor <= 0
                    || item.PriceFactor > 2
                    || !IsRatio(item.SaleableQuantityFactor)
                    || item.AdditionalCostKrw < 0)
                {
                    issues.Add(Issue("SalesChannelRangeInvalid", "$.salesChannels[]", "판매 방식 계산 값이 허용 범위를 벗어났습니다."));
                }
            }
        }

        private static void ValidateMappings(농업ScenarioPackage package, ICollection<데이터검증Issue> issues)
        {
            var mappings = package.ExternalMappings ?? Array.Empty<ExternalCodeMapping>();
            if (mappings.Length == 0)
            {
                issues.Add(Issue("ExternalMappingMissing", "$.externalMappings", "공공데이터 품목과 game 작물의 명시적 mapping이 필요합니다."));
                return;
            }

            foreach (var mapping in mappings)
            {
                RequireStableId(mapping.MappingKey, "$.externalMappings[].mappingKey", issues);
                if (!string.Equals(mapping.GameDataKey, package.Crop.CropKey, StringComparison.Ordinal))
                {
                    issues.Add(Issue("ExternalMappingCropMismatch", "$.externalMappings[].gameDataKey", "mapping 대상이 시나리오 작물과 다릅니다."));
                }

                if (!string.Equals(mapping.QualityCode, 데이터품질Codes.Valid, StringComparison.Ordinal))
                {
                    issues.Add(Issue("ExternalMappingNotApproved", "$.externalMappings[].qualityCode", "검토 완료된 Valid mapping만 사용할 수 있습니다."));
                }

                if (!string.Equals(mapping.EvidenceReference, package.MarketObservation.Evidence.EvidenceId, StringComparison.Ordinal))
                {
                    issues.Add(Issue("ExternalMappingEvidenceMismatch", "$.externalMappings[].evidenceReference", "mapping 근거가 시장 관측 evidence와 연결되지 않았습니다."));
                }
            }
        }

        private static void ValidateEvidence(
            데이터근거Envelope evidence,
            string path,
            string expectedNormalizedUnit,
            ICollection<데이터검증Issue> issues)
        {
            RequireStableId(evidence.EvidenceId, path + ".evidenceId", issues);
            RequireStableId(evidence.SourceKey, path + ".sourceKey", issues);
            RequireStableId(evidence.DatasetKey, path + ".datasetKey", issues);
            RequireText(evidence.DatasetVersion, path + ".datasetVersion", issues);

            if (evidence.ObservedAt == default || evidence.IngestedAt == default)
            {
                issues.Add(Issue("EvidenceTimeMissing", path, "관측 시각과 수집 시각이 필요합니다."));
            }
            else if (evidence.ObservedAt > evidence.IngestedAt)
            {
                issues.Add(Issue("EvidenceTimeOrderInvalid", path, "수집 시각은 관측 시각보다 빠를 수 없습니다."));
            }

            RequireStableId(evidence.RegionKey, path + ".regionKey", issues);
            if (!string.IsNullOrWhiteSpace(evidence.MarketStageKey))
            {
                RequireStableId(evidence.MarketStageKey, path + ".marketStageKey", issues);
            }

            if (!string.Equals(evidence.NormalizedUnit, expectedNormalizedUnit, StringComparison.Ordinal))
            {
                issues.Add(Issue("EvidenceUnitMismatch", path + ".normalizedUnit", "정규화 단위가 계산 계약과 다릅니다."));
            }

            if (!string.Equals(evidence.QualityCode, 데이터품질Codes.Valid, StringComparison.Ordinal)
                && !string.Equals(evidence.QualityCode, 데이터품질Codes.Fixture, StringComparison.Ordinal))
            {
                issues.Add(Issue("EvidenceQualityRejected", path + ".qualityCode", "Valid 또는 Fixture evidence만 시나리오에 사용할 수 있습니다."));
            }

            if (string.Equals(evidence.QualityCode, 데이터품질Codes.Fixture, StringComparison.Ordinal)
                && !string.Equals(evidence.SourceType, 데이터SourceTypes.Fixture, StringComparison.Ordinal))
            {
                issues.Add(Issue("EvidenceSourceTypeMismatch", path + ".sourceType", "Fixture 품질은 Fixture source type으로 표시해야 합니다."));
            }

            if (string.Equals(evidence.QualityCode, 데이터품질Codes.Valid, StringComparison.Ordinal)
                && !string.Equals(evidence.SourceType, 데이터SourceTypes.PublicObservation, StringComparison.Ordinal))
            {
                issues.Add(Issue("EvidenceSourceTypeMismatch", path + ".sourceType", "Valid 공공 관측은 PublicObservation source type이어야 합니다."));
            }

            if (string.IsNullOrWhiteSpace(evidence.LicenseOrTermsReference))
            {
                issues.Add(Issue("EvidenceTermsMissing", path + ".licenseOrTermsReference", "출처 이용조건 참조가 필요합니다."));
            }

            if (string.IsNullOrWhiteSpace(evidence.OriginalUnit)
                || string.IsNullOrWhiteSpace(evidence.FreshnessCode)
                || string.IsNullOrWhiteSpace(evidence.PayloadHash))
            {
                issues.Add(Issue("EvidenceMetadataMissing", path, "원 단위, freshness와 payload hash가 필요합니다."));
            }

            if (evidence.Limitations == null || evidence.Limitations.Length == 0)
            {
                issues.Add(Issue("EvidenceLimitationMissing", path + ".limitations", "관측값의 제한을 하나 이상 표시해야 합니다."));
            }
        }

        private static bool IsRatio(decimal value)
        {
            return value >= 0 && value <= 1;
        }

        private static void RequireStableId(string value, string path, ICollection<데이터검증Issue> issues)
        {
            if (!StableDataId.IsValid(value))
            {
                issues.Add(Issue("StableIdInvalid", path, "stable ID 형식이 올바르지 않습니다."));
            }
        }

        private static void RequireText(string value, string path, ICollection<데이터검증Issue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                issues.Add(Issue("RequiredValueMissing", path, "필수 값이 없습니다."));
            }
        }

        private static 데이터검증Issue Issue(string code, string path, string message)
        {
            return new 데이터검증Issue(code, path, message);
        }
    }
}
