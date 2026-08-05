using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Unity.Data
{
    public static class 농업ScenarioHashCalculator
    {
        public static string Calculate(농업ScenarioPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            var builder = new StringBuilder();
            AppendManifest(builder, package.Manifest);
            AppendCrop(builder, package.Crop);
            AppendSoil(builder, package.Soil);

            foreach (var stage in package.GrowthStages.OrderBy(item => item.MinimumGrowthPoint).ThenBy(item => item.StageKey, StringComparer.Ordinal))
            {
                Append(builder, stage.StageKey);
                Append(builder, stage.MinimumGrowthPoint);
            }

            foreach (var weather in package.Weather.OrderBy(item => item.GameDay))
            {
                Append(builder, weather.GameDay);
                Append(builder, weather.MeanTemperatureC);
                Append(builder, weather.RainfallMm);
                AppendEvidence(builder, weather.TemperatureEvidence);
                AppendEvidence(builder, weather.RainfallEvidence);
            }

            foreach (var cost in package.Costs.OrderBy(item => item.CostKey, StringComparer.Ordinal))
            {
                Append(builder, cost.CostKey);
                Append(builder, cost.TriggerCode);
                Append(builder, cost.AmountKrw);
            }

            Append(builder, package.MarketObservation.ObservationKey);
            Append(builder, package.MarketObservation.CropKey);
            Append(builder, package.MarketObservation.PriceKrwPerKg);
            AppendEvidence(builder, package.MarketObservation.Evidence);

            foreach (var channel in package.SalesChannels.OrderBy(item => item.SalesChannelKey, StringComparer.Ordinal))
            {
                Append(builder, channel.SalesChannelKey);
                Append(builder, channel.PriceFactor);
                Append(builder, channel.SaleableQuantityFactor);
                Append(builder, channel.AdditionalCostKrw);
                AppendStrings(builder, channel.LaborDisclosureCodes);
                AppendStrings(builder, channel.RiskDisclosureCodes);
            }

            foreach (var mapping in package.ExternalMappings.OrderBy(item => item.MappingKey, StringComparer.Ordinal))
            {
                Append(builder, mapping.MappingKey);
                Append(builder, mapping.MappingVersion);
                Append(builder, mapping.ExternalSourceKey);
                Append(builder, mapping.ExternalCode);
                Append(builder, mapping.GameDataKey);
                Append(builder, mapping.QualityCode);
                Append(builder, mapping.EvidenceReference);
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

        private static void AppendManifest(StringBuilder builder, ScenarioManifest manifest)
        {
            Append(builder, manifest.ScenarioKey);
            Append(builder, manifest.ScenarioVersion);
            Append(builder, manifest.SchemaVersion);
            Append(builder, manifest.RuleSetKey);
            Append(builder, manifest.RuleSetVersion);
            Append(builder, manifest.DefaultRandomSeed);
            Append(builder, manifest.Mode);
        }

        private static void AppendCrop(StringBuilder builder, 작물Definition crop)
        {
            Append(builder, crop.CropKey);
            Append(builder, crop.VarietyKey);
            Append(builder, crop.BaseDailyGrowthPoint);
            Append(builder, crop.HarvestGrowthPoint);
            Append(builder, crop.BaseYieldKg);
            Append(builder, crop.OptimalTemperatureMinC);
            Append(builder, crop.OptimalTemperatureMaxC);
            Append(builder, crop.MinimumMoistureRatio);
            Append(builder, crop.MaximumMoistureRatio);
        }

        private static void AppendSoil(StringBuilder builder, 토양Definition soil)
        {
            Append(builder, soil.SoilKey);
            Append(builder, soil.InitialMoistureRatio);
            Append(builder, soil.RainRetentionRatioPerMillimeter);
            Append(builder, soil.DailyMoistureLossRatio);
        }

        private static void AppendEvidence(StringBuilder builder, 데이터근거Envelope evidence)
        {
            Append(builder, evidence.EvidenceId);
            Append(builder, evidence.SourceType);
            Append(builder, evidence.SourceKey);
            Append(builder, evidence.SourceRecordId);
            Append(builder, evidence.DatasetKey);
            Append(builder, evidence.DatasetVersion);
            Append(builder, evidence.ObservedAt);
            Append(builder, evidence.IngestedAt);
            Append(builder, evidence.RegionKey);
            Append(builder, evidence.MarketStageKey);
            Append(builder, evidence.OriginalValue);
            Append(builder, evidence.OriginalUnit);
            Append(builder, evidence.CurrencyCode);
            Append(builder, evidence.NormalizedValue);
            Append(builder, evidence.NormalizedUnit);
            Append(builder, evidence.QualityCode);
            Append(builder, evidence.FreshnessCode);
            Append(builder, evidence.LicenseOrTermsReference);
            AppendStrings(builder, evidence.Limitations);
            Append(builder, evidence.PayloadHash);
        }

        private static void AppendStrings(StringBuilder builder, string[]? values)
        {
            foreach (var value in (values ?? Array.Empty<string>()).OrderBy(item => item, StringComparer.Ordinal))
            {
                Append(builder, value);
            }
        }

        private static void Append(StringBuilder builder, DateTimeOffset value)
        {
            Append(builder, value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
        }

        private static void Append(StringBuilder builder, decimal value)
        {
            Append(builder, value.ToString("G29", CultureInfo.InvariantCulture));
        }

        private static void Append(StringBuilder builder, long value)
        {
            Append(builder, value.ToString(CultureInfo.InvariantCulture));
        }

        private static void Append(StringBuilder builder, int value)
        {
            Append(builder, value.ToString(CultureInfo.InvariantCulture));
        }

        private static void Append(StringBuilder builder, string? value)
        {
            var normalized = value ?? string.Empty;
            builder.Append(normalized.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(normalized);
            builder.Append('|');
        }
    }
}
