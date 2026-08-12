using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PresentationContracts.LearningCards;

namespace Ssalddel.Unity.PotatoJourney
{
    public static class PotatoJourneyApiRoutes
    {
        public const string Read = "api/v1/common/world/slices/potato-journey";
    }

    public static class PotatoJourneySourceModeCodes
    {
        public const string OperationalProjection = "OperationalProjection";
        public const string SimulationFixture = "SimulationFixture";
    }

    public static class PotatoJourneyLinkageStatusCodes
    {
        public const string CanonicalLinked = "CanonicalLinked";
        public const string SimulationLinked = "SimulationLinked";
        public const string ProductOnly = "ProductOnly";
        public const string Unverified = "Unverified";
        public const string Unavailable = "Unavailable";
    }

    public static class PotatoPriceObservationStatusCodes
    {
        public const string Ready = "Ready";
        public const string MappingRequired = "MappingRequired";
        public const string DataUnavailable = "DataUnavailable";
    }

    public static class PotatoJourneyAnchorKindCodes
    {
        public const string FarmPlot = "FarmPlot";
        public const string FarmYardCargo = "FarmYardCargo";
    }

    public static class PotatoJourneyVisualTokens
    {
        public const string ProductOnly = "PotatoProductAmber";
        public const string Unverified = "PotatoUnverifiedGold";
        public const string Simulation = "PotatoSimulationBlue";
        public const string Unavailable = "PotatoUnavailableGray";
    }

    public sealed class PotatoJourneySourceLineageApiModel
    {
        public string SourceKey { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public DateTimeOffset? ObservedAt { get; set; }
        public string SourceModeCode { get; set; } = string.Empty;
    }

    public sealed class PotatoProductApiModel
    {
        public string ProductStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HsPrefix { get; set; } = string.Empty;
        public string MappingQualityCode { get; set; } = string.Empty;
        public string MappingQualityLabel { get; set; } = string.Empty;
        public string MappingEvidence { get; set; } = string.Empty;
        public bool InformationOnly { get; set; }
    }

    public sealed class PotatoPriceRangeApiModel
    {
        public string MarketStageCode { get; set; } = string.Empty;
        public string MarketStageLabel { get; set; } = string.Empty;
        public decimal AverageKrwPerKg { get; set; }
        public decimal MinimumKrwPerKg { get; set; }
        public decimal MaximumKrwPerKg { get; set; }
        public int SampleCount { get; set; }
        public string LatestSurveyDate { get; set; } = string.Empty;
    }

    public sealed class PotatoPriceObservationApiModel
    {
        public string StatusCode { get; set; } = string.Empty;
        public string HsCode { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public PotatoPriceRangeApiModel? Wholesale { get; set; }
        public PotatoPriceRangeApiModel? Retail { get; set; }
        public string[] Notices { get; set; } = Array.Empty<string>();
        public bool InformationOnly { get; set; }
    }

    public sealed class PotatoSensorObservationApiModel
    {
        public decimal Value { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public DateTimeOffset ObservedAt { get; set; }
        public string FreshnessStatusCode { get; set; } = string.Empty;
        public string ConditionCode { get; set; } = string.Empty;
        public string AssessmentRuleRevision { get; set; } = string.Empty;
        public string? EvidenceCardId { get; set; }
        public string? ConfidenceCode { get; set; }
        public string? Limitation { get; set; }
    }

    public sealed class PotatoSensorApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string SensorTypeCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public PotatoSensorObservationApiModel? LatestObservation { get; set; }
    }

    public sealed class PotatoCultivationApiModel
    {
        public string FarmStableId { get; set; } = string.Empty;
        public long FarmRevision { get; set; }
        public string PlotStableId { get; set; } = string.Empty;
        public long PlotRevision { get; set; }
        public string CultivationStableId { get; set; } = string.Empty;
        public long CultivationRevision { get; set; }
        public string CropName { get; set; } = string.Empty;
        public string? CropReferenceStableId { get; set; }
        public string? CropReferenceSourceKey { get; set; }
        public string GrowthStatusCode { get; set; } = string.Empty;
        public string? PlantedOn { get; set; }
        public string? ExpectedHarvestOn { get; set; }
        public string ProductLinkageStatusCode { get; set; } = string.Empty;
        public PotatoSensorApiModel[] Sensors { get; set; } = Array.Empty<PotatoSensorApiModel>();
    }

    public sealed class PotatoCargoApiModel
    {
        public string CargoStableId { get; set; } = string.Empty;
        public string TransportTaskStableId { get; set; } = string.Empty;
        public string InboundTaskStableId { get; set; } = string.Empty;
        public string HandoffStateCode { get; set; } = string.Empty;
    }

    public sealed class PotatoWarehouseApiModel
    {
        public string WarehouseStableId { get; set; } = string.Empty;
        public string InventoryStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public int? AuthorizedQuantity { get; set; }
    }

    public sealed class PotatoMarketApiModel
    {
        public string PublicProductStableId { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public string SaleUnit { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
        public string QuantityMeaningCode { get; set; } = string.Empty;
        public bool IsSaleAvailable { get; set; }
        public DateTimeOffset InventoryObservedAt { get; set; }
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
    }

    public sealed class PotatoJourneyApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string AuthorizedRoleCode { get; set; } = string.Empty;
        public string ViewerScopeCode { get; set; } = string.Empty;
        public string AuthorizationDecisionId { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
        public string LinkageStatusCode { get; set; } = string.Empty;
        public PotatoProductApiModel Product { get; set; } = new PotatoProductApiModel();
        public PotatoCultivationApiModel? Farm { get; set; }
        public PotatoPriceObservationApiModel DomesticPrice { get; set; } = new PotatoPriceObservationApiModel();
        public PotatoCargoApiModel? CargoJourney { get; set; }
        public PotatoWarehouseApiModel? Warehouse { get; set; }
        public PotatoMarketApiModel? Market { get; set; }
        public PotatoJourneySourceLineageApiModel[] SourceLineage { get; set; } = Array.Empty<PotatoJourneySourceLineageApiModel>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
        public bool IsReadOnly { get; set; }
    }

    public sealed class PotatoJourneySourceLineageSnapshot
    {
        public string SourceKey { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public DateTimeOffset? ObservedAt { get; set; }
        public string SourceModeCode { get; set; } = string.Empty;
    }

    public sealed class PotatoProductSnapshot
    {
        public string ProductStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HsPrefix { get; set; } = string.Empty;
        public string MappingQualityCode { get; set; } = string.Empty;
        public string MappingQualityLabel { get; set; } = string.Empty;
        public string MappingEvidence { get; set; } = string.Empty;
    }

    public sealed class PotatoPriceRangeSnapshot
    {
        public string MarketStageCode { get; set; } = string.Empty;
        public string MarketStageLabel { get; set; } = string.Empty;
        public decimal AverageKrwPerKg { get; set; }
        public decimal MinimumKrwPerKg { get; set; }
        public decimal MaximumKrwPerKg { get; set; }
        public int SampleCount { get; set; }
        public string LatestSurveyDate { get; set; } = string.Empty;
    }

    public sealed class PotatoPriceObservationSnapshot
    {
        public string StatusCode { get; set; } = string.Empty;
        public string HsCode { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public PotatoPriceRangeSnapshot? Wholesale { get; set; }
        public PotatoPriceRangeSnapshot? Retail { get; set; }
        public string[] Notices { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoCultivationSnapshot
    {
        public string FarmStableId { get; set; } = string.Empty;
        public long FarmRevision { get; set; }
        public string PlotStableId { get; set; } = string.Empty;
        public long PlotRevision { get; set; }
        public string CultivationStableId { get; set; } = string.Empty;
        public long CultivationRevision { get; set; }
        public string CropName { get; set; } = string.Empty;
        public string? CropReferenceStableId { get; set; }
        public string? CropReferenceSourceKey { get; set; }
        public string GrowthStatusCode { get; set; } = string.Empty;
        public string ProductLinkageStatusCode { get; set; } = string.Empty;
        public PotatoSensorApiModel[] Sensors { get; set; } = Array.Empty<PotatoSensorApiModel>();
    }

    public sealed class PotatoJourneySnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string AuthorizedRoleCode { get; set; } = string.Empty;
        public string ViewerScopeCode { get; set; } = string.Empty;
        public string AuthorizationDecisionId { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
        public string LinkageStatusCode { get; set; } = string.Empty;
        public PotatoProductSnapshot Product { get; set; } = new PotatoProductSnapshot();
        public PotatoCultivationSnapshot? Farm { get; set; }
        public PotatoPriceObservationSnapshot DomesticPrice { get; set; } = new PotatoPriceObservationSnapshot();
        public PotatoCargoApiModel? CargoJourney { get; set; }
        public PotatoWarehouseApiModel? Warehouse { get; set; }
        public PotatoMarketApiModel? Market { get; set; }
        public PotatoJourneySourceLineageSnapshot[] SourceLineage { get; set; } = Array.Empty<PotatoJourneySourceLineageSnapshot>();
        public string[] Limitations { get; set; } = Array.Empty<string>();
    }

    public sealed class PotatoJourneyMapper
    {
        public PotatoJourneySnapshot Map(PotatoJourneyApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            StableDataId.EnsureValid(source.StableId, nameof(source.StableId));
            Require(source.Revision, "PotatoJourneyRevisionMissing");
            if (source.GeneratedAt == default
                || source.AuthorizedRoleCode != "Producer"
                || source.ViewerScopeCode != "AuthorizedParty"
                || string.IsNullOrWhiteSpace(source.AuthorizationDecisionId)
                || !source.IsReadOnly)
                throw new InvalidOperationException("PotatoJourneyAuthorizationInvalid");
            EnsureSourceAndLinkage(source.SourceModeCode, source.LinkageStatusCode);
            ValidateProduct(source.Product);
            ValidatePrice(source.DomesticPrice);
            ValidateRelationship(source);
            var lineage = MapLineage(source.SourceLineage);

            return new PotatoJourneySnapshot
            {
                StableId = source.StableId.Trim(),
                Revision = source.Revision.Trim(),
                GeneratedAt = source.GeneratedAt,
                AuthorizedRoleCode = source.AuthorizedRoleCode,
                ViewerScopeCode = source.ViewerScopeCode,
                AuthorizationDecisionId = source.AuthorizationDecisionId.Trim(),
                SourceModeCode = source.SourceModeCode,
                LinkageStatusCode = source.LinkageStatusCode,
                Product = new PotatoProductSnapshot
                {
                    ProductStableId = source.Product.ProductStableId.Trim(),
                    DisplayName = source.Product.DisplayName.Trim(),
                    HsPrefix = source.Product.HsPrefix.Trim(),
                    MappingQualityCode = source.Product.MappingQualityCode.Trim(),
                    MappingQualityLabel = source.Product.MappingQualityLabel.Trim(),
                    MappingEvidence = source.Product.MappingEvidence.Trim(),
                },
                Farm = source.Farm == null ? null : MapFarm(source.Farm),
                DomesticPrice = MapPrice(source.DomesticPrice),
                CargoJourney = source.CargoJourney,
                Warehouse = source.Warehouse,
                Market = source.Market,
                SourceLineage = lineage,
                Limitations = Normalize(source.Limitations),
            };
        }

        private static void ValidateProduct(PotatoProductApiModel source)
        {
            if (source == null || !source.InformationOnly)
                throw new InvalidOperationException("PotatoProductInformationBoundaryInvalid");
            StableDataId.EnsureValid(source.ProductStableId, nameof(source.ProductStableId));
            if (source.ProductStableId != "product:potato"
                || source.HsPrefix != "0701"
                || string.IsNullOrWhiteSpace(source.DisplayName)
                || string.IsNullOrWhiteSpace(source.MappingQualityCode)
                || string.IsNullOrWhiteSpace(source.MappingQualityLabel)
                || string.IsNullOrWhiteSpace(source.MappingEvidence))
                throw new InvalidOperationException("PotatoProductInvalid");
        }

        private static void ValidatePrice(PotatoPriceObservationApiModel source)
        {
            if (source == null || !source.InformationOnly
                || !IsPriceStatus(source.StatusCode)
                || source.UnitCode != "KRW_PER_KG"
                || source.CurrencyCode != "KRW"
                || string.IsNullOrWhiteSpace(source.DataSource)
                || source.Notices == null)
                throw new InvalidOperationException("PotatoPriceObservationInvalid");
            if (source.StatusCode == PotatoPriceObservationStatusCodes.Ready
                && source.Wholesale == null && source.Retail == null)
                throw new InvalidOperationException("PotatoPriceReadyRangeMissing");
            ValidateRange(source.Wholesale);
            ValidateRange(source.Retail);
        }

        private static void ValidateRange(PotatoPriceRangeApiModel? source)
        {
            if (source == null) return;
            if (string.IsNullOrWhiteSpace(source.MarketStageCode)
                || string.IsNullOrWhiteSpace(source.MarketStageLabel)
                || source.AverageKrwPerKg < 0
                || source.MinimumKrwPerKg < 0
                || source.MaximumKrwPerKg < source.MinimumKrwPerKg
                || source.SampleCount < 0)
                throw new InvalidOperationException("PotatoPriceRangeInvalid");
        }

        private static void ValidateRelationship(PotatoJourneyApiModel source)
        {
            var hasFarm = source.Farm != null;
            if (source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.ProductOnly && hasFarm)
                throw new InvalidOperationException("PotatoProductOnlyFarmUnexpected");
            if ((source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.Unverified
                 || source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.CanonicalLinked
                 || source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.SimulationLinked)
                && !hasFarm)
                throw new InvalidOperationException("PotatoJourneyFarmRequired");
            if (source.LinkageStatusCode != PotatoJourneyLinkageStatusCodes.CanonicalLinked
                && source.LinkageStatusCode != PotatoJourneyLinkageStatusCodes.SimulationLinked
                && (source.CargoJourney != null || source.Warehouse != null || source.Market != null))
                throw new InvalidOperationException("PotatoJourneyUnverifiedOperationalBlockUnexpected");
            if ((source.Warehouse != null || source.Market != null) && source.CargoJourney == null)
                throw new InvalidOperationException("PotatoJourneyCargoRequiredForDownstreamState");
            ValidateCargo(source.CargoJourney);
            ValidateMarket(source.Market);
        }

        private static void ValidateCargo(PotatoCargoApiModel? source)
        {
            if (source == null) return;
            StableDataId.EnsureValid(source.CargoStableId, nameof(source.CargoStableId));
            StableDataId.EnsureValid(source.TransportTaskStableId, nameof(source.TransportTaskStableId));
            StableDataId.EnsureValid(source.InboundTaskStableId, nameof(source.InboundTaskStableId));
            if (string.IsNullOrWhiteSpace(source.HandoffStateCode))
                throw new InvalidOperationException("PotatoJourneyCargoHandoffStateMissing");
        }

        private static void ValidateMarket(PotatoMarketApiModel? source)
        {
            if (source == null) return;
            StableDataId.EnsureValid(source.PublicProductStableId, nameof(source.PublicProductStableId));
            StableDataId.EnsureValid(source.SourceStableId, nameof(source.SourceStableId));
            if (source.SalePrice < 0 || source.AvailableQuantity < 0
                || source.InventoryObservedAt == default
                || string.IsNullOrWhiteSpace(source.SaleUnit)
                || source.CurrencyCode != "KRW"
                || string.IsNullOrWhiteSpace(source.QuantityUnit)
                || source.QuantityMeaningCode != PotatoJourneyCityQuantityMeaningCodes.ProjectedSaleAvailability
                || string.IsNullOrWhiteSpace(source.SourceRevision))
                throw new InvalidOperationException("PotatoJourneyMarketProjectionInvalid");
            if (source.IsSaleAvailable != (source.AvailableQuantity > 0))
                throw new InvalidOperationException("PotatoJourneyMarketAvailabilityMismatch");
        }

        private static PotatoCultivationSnapshot MapFarm(PotatoCultivationApiModel source)
        {
            StableDataId.EnsureValid(source.FarmStableId, nameof(source.FarmStableId));
            StableDataId.EnsureValid(source.PlotStableId, nameof(source.PlotStableId));
            StableDataId.EnsureValid(source.CultivationStableId, nameof(source.CultivationStableId));
            if (source.FarmRevision < 0 || source.PlotRevision < 0 || source.CultivationRevision < 0
                || string.IsNullOrWhiteSpace(source.CropName)
                || string.IsNullOrWhiteSpace(source.GrowthStatusCode)
                || source.Sensors == null)
                throw new InvalidOperationException("PotatoCultivationInvalid");
            if (source.ProductLinkageStatusCode != PotatoJourneyLinkageStatusCodes.Unverified
                && source.ProductLinkageStatusCode != PotatoJourneyLinkageStatusCodes.CanonicalLinked
                && source.ProductLinkageStatusCode != PotatoJourneyLinkageStatusCodes.SimulationLinked)
                throw new InvalidOperationException("PotatoCultivationLinkageInvalid");

            return new PotatoCultivationSnapshot
            {
                FarmStableId = source.FarmStableId.Trim(),
                FarmRevision = source.FarmRevision,
                PlotStableId = source.PlotStableId.Trim(),
                PlotRevision = source.PlotRevision,
                CultivationStableId = source.CultivationStableId.Trim(),
                CultivationRevision = source.CultivationRevision,
                CropName = source.CropName.Trim(),
                CropReferenceStableId = source.CropReferenceStableId?.Trim(),
                CropReferenceSourceKey = source.CropReferenceSourceKey?.Trim(),
                GrowthStatusCode = source.GrowthStatusCode.Trim(),
                ProductLinkageStatusCode = source.ProductLinkageStatusCode,
                Sensors = source.Sensors,
            };
        }

        private static PotatoPriceObservationSnapshot MapPrice(PotatoPriceObservationApiModel source)
            => new PotatoPriceObservationSnapshot
            {
                StatusCode = source.StatusCode,
                HsCode = source.HsCode?.Trim() ?? string.Empty,
                UnitCode = source.UnitCode,
                CurrencyCode = source.CurrencyCode,
                DataSource = source.DataSource.Trim(),
                StartDate = source.StartDate?.Trim() ?? string.Empty,
                EndDate = source.EndDate?.Trim() ?? string.Empty,
                Wholesale = MapRange(source.Wholesale),
                Retail = MapRange(source.Retail),
                Notices = Normalize(source.Notices),
            };

        private static PotatoPriceRangeSnapshot? MapRange(PotatoPriceRangeApiModel? source)
            => source == null ? null : new PotatoPriceRangeSnapshot
            {
                MarketStageCode = source.MarketStageCode.Trim(),
                MarketStageLabel = source.MarketStageLabel.Trim(),
                AverageKrwPerKg = source.AverageKrwPerKg,
                MinimumKrwPerKg = source.MinimumKrwPerKg,
                MaximumKrwPerKg = source.MaximumKrwPerKg,
                SampleCount = source.SampleCount,
                LatestSurveyDate = source.LatestSurveyDate?.Trim() ?? string.Empty,
            };

        private static PotatoJourneySourceLineageSnapshot[] MapLineage(
            PotatoJourneySourceLineageApiModel[] source)
        {
            if (source == null || source.Length == 0)
                throw new InvalidOperationException("PotatoJourneySourceLineageMissing");
            var duplicate = source.GroupBy(value => value?.SourceStableId ?? string.Empty, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("PotatoJourneySourceLineageDuplicate:" + duplicate.Key);
            return source.Select(value =>
            {
                if (value == null) throw new InvalidOperationException("PotatoJourneySourceLineageItemMissing");
                StableDataId.EnsureValid(value.SourceStableId, nameof(value.SourceStableId));
                return new PotatoJourneySourceLineageSnapshot
                {
                    SourceKey = Require(value.SourceKey, "PotatoJourneySourceKeyMissing"),
                    SourceStableId = value.SourceStableId.Trim(),
                    SourceRevision = Require(value.SourceRevision, "PotatoJourneySourceRevisionMissing"),
                    ObservedAt = value.ObservedAt,
                    SourceModeCode = Require(value.SourceModeCode, "PotatoJourneyLineageModeMissing"),
                };
            }).OrderBy(value => value.SourceStableId, StringComparer.Ordinal).ToArray();
        }

        private static void EnsureSourceAndLinkage(string sourceMode, string linkage)
        {
            if (sourceMode != PotatoJourneySourceModeCodes.OperationalProjection
                && sourceMode != PotatoJourneySourceModeCodes.SimulationFixture)
                throw new InvalidOperationException("PotatoJourneySourceModeInvalid");
            if (linkage != PotatoJourneyLinkageStatusCodes.CanonicalLinked
                && linkage != PotatoJourneyLinkageStatusCodes.SimulationLinked
                && linkage != PotatoJourneyLinkageStatusCodes.ProductOnly
                && linkage != PotatoJourneyLinkageStatusCodes.Unverified
                && linkage != PotatoJourneyLinkageStatusCodes.Unavailable)
                throw new InvalidOperationException("PotatoJourneyLinkageInvalid");
            if (linkage == PotatoJourneyLinkageStatusCodes.SimulationLinked
                && sourceMode != PotatoJourneySourceModeCodes.SimulationFixture)
                throw new InvalidOperationException("PotatoJourneySimulationModeRequired");
            if (linkage == PotatoJourneyLinkageStatusCodes.CanonicalLinked
                && sourceMode != PotatoJourneySourceModeCodes.OperationalProjection)
                throw new InvalidOperationException("PotatoJourneyOperationalModeRequired");
        }

        private static bool IsPriceStatus(string value)
            => value == PotatoPriceObservationStatusCodes.Ready
               || value == PotatoPriceObservationStatusCodes.MappingRequired
               || value == PotatoPriceObservationStatusCodes.DataUnavailable;

        private static string[] Normalize(IEnumerable<string>? values)
            => (values ?? Array.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim()).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

        private static string Require(string value, string error)
            => string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException(error) : value.Trim();
    }

    public interface IPotatoJourneyApiClient
    {
        Task<PotatoJourneyApiModel> GetAsync(
            string? cultivationStableId,
            CancellationToken cancellationToken = default);
    }

    public interface IPotatoJourneyRepository
    {
        Task<PotatoJourneySnapshot> LoadAsync(
            string? cultivationStableId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PotatoJourneyApiRepository : IPotatoJourneyRepository
    {
        private readonly IPotatoJourneyApiClient client;
        private readonly PotatoJourneyMapper mapper;

        public PotatoJourneyApiRepository(IPotatoJourneyApiClient apiClient, PotatoJourneyMapper dataMapper)
        {
            client = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            mapper = dataMapper ?? throw new ArgumentNullException(nameof(dataMapper));
        }

        public async Task<PotatoJourneySnapshot> LoadAsync(
            string? cultivationStableId,
            CancellationToken cancellationToken = default)
            => mapper.Map(await client.GetAsync(cultivationStableId, cancellationToken).ConfigureAwait(false));
    }

    public sealed class PotatoJourneyQueryUseCase
    {
        private readonly IPotatoJourneyRepository repository;

        public PotatoJourneyQueryUseCase(IPotatoJourneyRepository dataRepository)
            => repository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));

        public Task<PotatoJourneySnapshot> ExecuteAsync(
            string? cultivationStableId,
            CancellationToken cancellationToken = default)
            => repository.LoadAsync(cultivationStableId, cancellationToken);
    }

    public sealed class PotatoJourneyInterpretationInput
    {
        public PotatoJourneySnapshot Snapshot { get; set; } = new PotatoJourneySnapshot();
        public string AnchorKindCode { get; set; } = string.Empty;
        public WorldObjectRef AnchorWorldObjectRef { get; set; }
    }

    public sealed class PotatoJourneyPresentationModel
    {
        public string SnapshotStableId { get; set; } = string.Empty;
        public string SnapshotRevision { get; set; } = string.Empty;
        public string AnchorKindCode { get; set; } = string.Empty;
        public string LinkageStatusCode { get; set; } = string.Empty;
        public string HighlightToken { get; set; } = string.Empty;
        public bool ShowFarmConditionMarker { get; set; }
        public bool ShowCargoRoute { get; set; }
        public string ModeLabel { get; set; } = string.Empty;
        public ConceptCardDeckPresentationModel CardDeck { get; set; } = null!;
    }

    public sealed class PotatoJourneyInterpreter
    {
        public const string ContractVersion = "potato-journey-interpretation-v1";
        public const string RuleRevision = "potato-journey-rule:1";

        public PotatoJourneyPresentationModel Interpret(PotatoJourneyInterpretationInput input)
        {
            if (input == null || input.Snapshot == null)
                throw new ArgumentNullException(nameof(input));
            var source = input.Snapshot;
            if (input.AnchorKindCode != PotatoJourneyAnchorKindCodes.FarmPlot
                && input.AnchorKindCode != PotatoJourneyAnchorKindCodes.FarmYardCargo)
                throw new InvalidOperationException("PotatoJourneyAnchorKindInvalid");
            if (input.AnchorKindCode == PotatoJourneyAnchorKindCodes.FarmPlot && source.Farm == null)
                throw new InvalidOperationException("PotatoJourneyFarmAnchorSourceMissing");

            var revisions = new DataRevisionSet(source.SourceLineage.Select(value =>
                new DataRevisionReference(
                    value.SourceStableId,
                    value.SourceRevision,
                    value.ObservedAt,
                    PriceQuality(source.DomesticPrice.StatusCode))));
            var interpretationRevision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                revisions,
                ContractVersion,
                RuleRevision,
                input.AnchorKindCode + ":" + source.LinkageStatusCode);
            var lineage = source.SourceLineage.Select(value => new ConceptCardSourceLineageItem
            {
                SourceStableId = value.SourceStableId,
                Revision = value.SourceRevision,
                EvidenceAsOfUtc = value.ObservedAt,
                QualityCode = PriceQuality(source.DomesticPrice.StatusCode),
            }).ToArray();
            var worldIds = new List<WorldStableId> { new WorldStableId(source.Product.ProductStableId) };
            if (source.Farm != null) worldIds.Add(new WorldStableId(source.Farm.CultivationStableId));
            var priceSource = source.SourceLineage.FirstOrDefault(value =>
                value.SourceKey == "public-data:kamis-domestic-price")?.SourceStableId ?? string.Empty;
            var drafts = BuildCards(source, worldIds.ToArray(), lineage, priceSource);
            var mode = source.SourceModeCode == PotatoJourneySourceModeCodes.SimulationFixture
                ? DataRuntimeMode.Simulation
                : DataRuntimeMode.Operational;
            var deck = new ConceptCardDeckProjector().Project(new ConceptCardDeckProjectionInput
            {
                DeckStableId = "concept-card-deck:potato-journey",
                AnchorWorldObjectRef = input.AnchorWorldObjectRef,
                RoleCode = source.AuthorizedRoleCode,
                IntentCode = "ReviewPotatoJourney",
                Mode = mode,
                SourceRevision = StableRevision(source.Revision),
                InterpretationRevision = interpretationRevision,
                IsRoleAuthorized = true,
                Cards = drafts,
            }) ?? throw new InvalidOperationException("PotatoJourneyCardDeckMissing");

            return new PotatoJourneyPresentationModel
            {
                SnapshotStableId = source.StableId,
                SnapshotRevision = source.Revision,
                AnchorKindCode = input.AnchorKindCode,
                LinkageStatusCode = source.LinkageStatusCode,
                HighlightToken = Highlight(source.LinkageStatusCode),
                ShowFarmConditionMarker = source.Farm != null,
                ShowCargoRoute = source.CargoJourney != null
                    && (source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.CanonicalLinked
                        || source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.SimulationLinked),
                ModeLabel = mode == DataRuntimeMode.Simulation ? "SIMULATION" : string.Empty,
                CardDeck = deck,
            };
        }

        private static ConceptCardDraft[] BuildCards(
            PotatoJourneySnapshot source,
            WorldStableId[] worldIds,
            ConceptCardSourceLineageItem[] lineage,
            string priceSource)
        {
            var price = source.DomesticPrice;
            var primaryRange = price.Wholesale ?? price.Retail;
            var priceValue = price.StatusCode == PotatoPriceObservationStatusCodes.Ready && primaryRange != null
                ? string.Format(CultureInfo.InvariantCulture, "{0} 평균 ₩{1:N0}/kg", primaryRange.MarketStageLabel, primaryRange.AverageKrwPerKg)
                : "가격 자료 없음";
            var evidence = new List<ConceptCardEvidenceDraft>();
            if (primaryRange != null && !string.IsNullOrWhiteSpace(priceSource))
            {
                evidence.Add(new ConceptCardEvidenceDraft
                {
                    LabelText = "범위",
                    ValueText = string.Format(CultureInfo.InvariantCulture, "₩{0:N0}–₩{1:N0}/kg · 표본 {2}", primaryRange.MinimumKrwPerKg, primaryRange.MaximumKrwPerKg, primaryRange.SampleCount),
                    CalculationRoleCode = ConceptCardCalculationRoleCodes.Result,
                    SourceStableId = priceSource,
                    RuleRevision = RuleRevision,
                });
                evidence.Add(new ConceptCardEvidenceDraft
                {
                    LabelText = "조사일",
                    ValueText = primaryRange.LatestSurveyDate,
                    CalculationRoleCode = ConceptCardCalculationRoleCodes.Input,
                    SourceStableId = priceSource,
                    RuleRevision = RuleRevision,
                });
            }

            return new[]
            {
                new ConceptCardDraft
                {
                    Sequence = 1,
                    StableId = "concept-card:potato-product",
                    CardKindCode = ConceptCardKindCodes.Concept,
                    ConceptStableId = "concept:potato-product",
                    TitleText = source.Product.DisplayName,
                    SummaryText = "HS " + source.Product.HsPrefix + " · " + source.Product.MappingQualityLabel,
                    PrimaryValueText = source.Product.MappingQualityCode,
                    SimulationLabel = source.SourceModeCode == PotatoJourneySourceModeCodes.SimulationFixture ? "SIMULATION" : string.Empty,
                    SourceWorldIds = worldIds,
                    Cautions = source.Limitations,
                    SourceLineage = lineage,
                },
                new ConceptCardDraft
                {
                    Sequence = 2,
                    StableId = "concept-card:potato-domestic-price",
                    CardKindCode = ConceptCardKindCodes.Status,
                    ConceptStableId = "concept:potato-domestic-price",
                    TitleText = "국내 가격 관측",
                    SummaryText = price.DataSource + " · " + price.StartDate + "–" + price.EndDate,
                    PrimaryValueText = priceValue,
                    SimulationLabel = source.SourceModeCode == PotatoJourneySourceModeCodes.SimulationFixture ? "SIMULATION" : string.Empty,
                    SourceWorldIds = worldIds,
                    EvidenceRows = evidence.ToArray(),
                    Cautions = price.Notices,
                    SourceLineage = lineage,
                },
                new ConceptCardDraft
                {
                    Sequence = 3,
                    StableId = "concept-card:potato-linkage-reason",
                    CardKindCode = ConceptCardKindCodes.Reason,
                    ConceptStableId = "concept:potato-linkage",
                    TitleText = "데이터 연결 상태",
                    SummaryText = LinkageSummary(source.LinkageStatusCode),
                    PrimaryValueText = source.LinkageStatusCode,
                    SimulationLabel = source.SourceModeCode == PotatoJourneySourceModeCodes.SimulationFixture ? "SIMULATION" : string.Empty,
                    SourceWorldIds = worldIds,
                    EvidenceRows = new[]
                    {
                        new ConceptCardEvidenceDraft
                        {
                            LabelText = "연결 기준",
                            ValueText = "stable ID와 source lineage",
                            CalculationRoleCode = ConceptCardCalculationRoleCodes.Limitation,
                            SourceStableId = lineage[0].SourceStableId,
                            RuleRevision = RuleRevision,
                        },
                    },
                    Cautions = source.Limitations,
                    SourceLineage = lineage,
                },
            };
        }

        private static string LinkageSummary(string value)
        {
            if (value == PotatoJourneyLinkageStatusCodes.ProductOnly)
                return "상품·가격만 확인됐으며 재배·화물·창고 관계는 연결하지 않았습니다.";
            if (value == PotatoJourneyLinkageStatusCodes.Unverified)
                return "재배작기를 선택했지만 공통 ProductStableId가 없어 상품 관계는 미확정입니다.";
            if (value == PotatoJourneyLinkageStatusCodes.SimulationLinked)
                return "이 연결은 Simulation scenario 안에서만 유효합니다.";
            if (value == PotatoJourneyLinkageStatusCodes.CanonicalLinked)
                return "서버 원장의 stable ID 관계로 연결됐습니다.";
            return "현재 연결 상태를 확인할 수 없습니다.";
        }

        private static string Highlight(string value)
        {
            if (value == PotatoJourneyLinkageStatusCodes.ProductOnly) return PotatoJourneyVisualTokens.ProductOnly;
            if (value == PotatoJourneyLinkageStatusCodes.Unverified) return PotatoJourneyVisualTokens.Unverified;
            if (value == PotatoJourneyLinkageStatusCodes.SimulationLinked) return PotatoJourneyVisualTokens.Simulation;
            if (value == PotatoJourneyLinkageStatusCodes.CanonicalLinked) return PotatoJourneyVisualTokens.Simulation;
            return PotatoJourneyVisualTokens.Unavailable;
        }

        private static string PriceQuality(string status)
            => status == PotatoPriceObservationStatusCodes.Ready ? DataQualityCodes.Observed : DataQualityCodes.Missing;

        private static long StableRevision(string value)
        {
            unchecked
            {
                long hash = 1469598103934665603L;
                foreach (var character in value ?? string.Empty)
                    hash = (hash ^ character) * 1099511628211L;
                return hash == long.MinValue ? long.MaxValue : Math.Abs(hash);
            }
        }
    }
}
