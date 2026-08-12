using Ssalddel.Unity.PotatoJourney;

namespace Ssalddel.Unity.Tests;

public sealed class PotatoJourneyPvs7CityGateTests
{
    [Fact]
    public void KAMIS관측가와마트판매가는_단위와의미를분리한다()
    {
        var model = new PotatoJourneyCityProjector().Project(Snapshot(includeMarket: true));

        Assert.True(model.IsVisible);
        Assert.Equal("2,450 KRW/kg · observed wholesale", model.ObservedPriceText);
        Assert.Equal("35,000 KRW / 20kg box", model.SalePriceText);
        Assert.Contains("not this store's sale price", model.PriceSeparationText);
    }

    [Fact]
    public void 공개수량은_물리재고가아닌_ProjectedSaleAvailability로표시한다()
    {
        var model = new PotatoJourneyCityProjector().Project(Snapshot(includeMarket: true));

        Assert.Equal(PotatoJourneyCityQuantityMeaningCodes.ProjectedSaleAvailability,
            model.QuantityMeaningCode);
        Assert.Equal("12 boxes · projected sale availability", model.AvailabilityText);
    }

    [Fact]
    public void MarketStableId가없으면_CityAnchor를열지않는다()
    {
        var model = new PotatoJourneyCityProjector().Project(Snapshot(includeMarket: false));

        Assert.False(model.IsVisible);
        Assert.Equal("PotatoJourneyCityPublicProductMissing", model.BlockReasonCode);
    }

    [Fact]
    public void 공개수량의미가물리재고처럼오면_Mapper가거부한다()
    {
        var api = ApiFixture();
        api.Market!.QuantityMeaningCode = "PhysicalShelfStock";

        var error = Assert.Throws<InvalidOperationException>(() => new PotatoJourneyMapper().Map(api));

        Assert.Equal("PotatoJourneyMarketProjectionInvalid", error.Message);
    }

    private static PotatoJourneySnapshot Snapshot(bool includeMarket)
    {
        var api = ApiFixture();
        if (!includeMarket) api.Market = null;
        return new PotatoJourneyMapper().Map(api);
    }

    private static PotatoJourneyApiModel ApiFixture()
        => new()
        {
            StableId = "world-slice:potato-journey",
            Revision = "pvs7:1",
            GeneratedAt = new DateTimeOffset(2026, 8, 10, 9, 0, 0, TimeSpan.Zero),
            AuthorizedRoleCode = "Producer",
            ViewerScopeCode = "AuthorizedParty",
            AuthorizationDecisionId = "authorized:producer-a",
            SourceModeCode = PotatoJourneySourceModeCodes.SimulationFixture,
            LinkageStatusCode = PotatoJourneyLinkageStatusCodes.SimulationLinked,
            Product = new PotatoProductApiModel
            {
                ProductStableId = "product:potato", DisplayName = "감자", HsPrefix = "0701",
                MappingQualityCode = "ExactCommodity", MappingQualityLabel = "동일 품목",
                MappingEvidence = "HS 0701", InformationOnly = true,
            },
            Farm = new PotatoCultivationApiModel
            {
                FarmStableId = "farm:a", PlotStableId = "farm-plot:a.1",
                CultivationStableId = "cultivation:a.potato.2026", CropName = "감자",
                GrowthStatusCode = "Harvested",
                ProductLinkageStatusCode = PotatoJourneyLinkageStatusCodes.SimulationLinked,
            },
            DomesticPrice = new PotatoPriceObservationApiModel
            {
                StatusCode = PotatoPriceObservationStatusCodes.Ready, HsCode = "0701",
                UnitCode = "KRW_PER_KG", CurrencyCode = "KRW", DataSource = "KAMIS",
                Wholesale = new PotatoPriceRangeApiModel
                {
                    MarketStageCode = "Wholesale", MarketStageLabel = "도매",
                    AverageKrwPerKg = 2450, MinimumKrwPerKg = 2200,
                    MaximumKrwPerKg = 2700, SampleCount = 8,
                },
                InformationOnly = true,
            },
            CargoJourney = new PotatoCargoApiModel
            {
                CargoStableId = "cargo:simulation-potato-city-1",
                TransportTaskStableId = "transport-task:simulation-potato-city-1",
                InboundTaskStableId = "inbound-task:simulation-potato-city-1",
                HandoffStateCode = "AvailableAtMarket",
            },
            Market = new PotatoMarketApiModel
            {
                PublicProductStableId = "mart-product:simulation-potato-20kg",
                SalePrice = 35000, SaleUnit = "20kg box", CurrencyCode = "KRW",
                AvailableQuantity = 12, QuantityUnit = "boxes",
                QuantityMeaningCode = PotatoJourneyCityQuantityMeaningCodes.ProjectedSaleAvailability,
                IsSaleAvailable = true,
                InventoryObservedAt = new DateTimeOffset(2026, 8, 10, 8, 55, 0, TimeSpan.Zero),
                SourceStableId = "market:urban-demo-001", SourceRevision = "simulation:1",
            },
            SourceLineage = new[]
            {
                new PotatoJourneySourceLineageApiModel
                {
                    SourceKey = "fixture:potato-city", SourceStableId = "source:potato-pvs7",
                    SourceRevision = "1", SourceModeCode = PotatoJourneySourceModeCodes.SimulationFixture,
                },
            },
            Limitations = new[] { "Simulation only" },
            IsReadOnly = true,
        };
}
