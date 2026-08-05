using System;

namespace Ssalddel.Unity.Data
{
    public sealed class 시장가격관측ApiModel
    {
        public string SchemaVersion { get; set; } = string.Empty;

        public string ObservationKey { get; set; } = string.Empty;

        public string ExternalItemCode { get; set; } = string.Empty;

        public string SourceKey { get; set; } = string.Empty;

        public string SourceRecordId { get; set; } = string.Empty;

        public string DatasetKey { get; set; } = string.Empty;

        public string DatasetVersion { get; set; } = string.Empty;

        public DateTimeOffset ObservedAt { get; set; }

        public DateTimeOffset IngestedAt { get; set; }

        public string RegionKey { get; set; } = string.Empty;

        public string MarketStageKey { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = string.Empty;

        public string QualityCode { get; set; } = string.Empty;

        public string FreshnessCode { get; set; } = string.Empty;

        public string LicenseOrTermsReference { get; set; } = string.Empty;

        public string[] Limitations { get; set; } = Array.Empty<string>();

        public string PayloadHash { get; set; } = string.Empty;
    }
}
