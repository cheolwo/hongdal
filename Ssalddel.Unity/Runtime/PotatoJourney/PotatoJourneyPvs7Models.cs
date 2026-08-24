using System;
using System.Globalization;

namespace Ssalddel.Unity.PotatoJourney
{
    public static class PotatoJourneyCityQuantityMeaningCodes
    {
        public const string ProjectedSaleAvailability = "ProjectedSaleAvailability";
    }

    public sealed class PotatoJourneyCityPresentationModel
    {
        public bool IsVisible { get; set; }
        public string PublicProductStableId { get; set; } = string.Empty;
        public string ObservedPriceText { get; set; } = string.Empty;
        public string SalePriceText { get; set; } = string.Empty;
        public string AvailabilityText { get; set; } = string.Empty;
        public string QuantityMeaningCode { get; set; } = string.Empty;
        public string PriceSeparationText { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
        public string ModeLabel { get; set; } = string.Empty;
        public string BlockReasonCode { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoJourneyCityProjector
    {
        public PotatoJourneyCityPresentationModel Project(PotatoJourneySnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var linked = source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.CanonicalLinked
                         || source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.SimulationLinked;
            if (!linked || source.CargoJourney == null || source.Market == null)
            {
                return new PotatoJourneyCityPresentationModel
                {
                    SourceModeCode = source.SourceModeCode,
                    BlockReasonCode = !linked
                        ? "PotatoJourneyCityLinkageNotVerified"
                        : source.CargoJourney == null
                            ? "PotatoJourneyCityCargoRelationshipMissing"
                            : "PotatoJourneyCityPublicProductMissing",
                };
            }

            var market = source.Market;
            var observed = source.DomesticPrice.Wholesale;
            return new PotatoJourneyCityPresentationModel
            {
                IsVisible = true,
                PublicProductStableId = market.PublicProductStableId,
                ObservedPriceText = observed == null
                    ? "KAMIS wholesale observation unavailable"
                    : observed.AverageKrwPerKg.ToString("N0", CultureInfo.InvariantCulture)
                      + " KRW/kg · observed wholesale",
                SalePriceText = market.SalePrice.ToString("N0", CultureInfo.InvariantCulture)
                                + " " + market.CurrencyCode + " / " + market.SaleUnit,
                AvailabilityText = market.AvailableQuantity.ToString(CultureInfo.InvariantCulture)
                                   + " " + market.QuantityUnit + " · projected sale availability",
                QuantityMeaningCode = market.QuantityMeaningCode,
                PriceSeparationText = "KAMIS observation is not this store's sale price.",
                SourceModeCode = source.SourceModeCode,
                ModeLabel = source.SourceModeCode == PotatoJourneySourceModeCodes.SimulationFixture
                    ? "SIMULATION"
                    : string.Empty,
            };
        }
    }
}
