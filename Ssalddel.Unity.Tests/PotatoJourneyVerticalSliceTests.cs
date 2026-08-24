using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PotatoJourney;
using Ssalddel.Unity.PresentationContracts.LearningCards;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class PotatoJourneyVerticalSliceTests
{
    [Fact]
    public void Mapper는_ProductOnly가격과SourceLineage를_보존한다()
    {
        var result = new PotatoJourneyMapper().Map(ProductOnly());

        Assert.Equal("world-slice:potato-journey", result.StableId);
        Assert.Equal(PotatoJourneyLinkageStatusCodes.ProductOnly, result.LinkageStatusCode);
        Assert.Equal("product:potato", result.Product.ProductStableId);
        Assert.Equal("0701", result.Product.HsPrefix);
        Assert.Equal(2450m, result.DomesticPrice.Wholesale!.AverageKrwPerKg);
        Assert.Null(result.Farm);
        Assert.Null(result.CargoJourney);
        Assert.Single(result.SourceLineage);
    }

    [Fact]
    public void ProductOnly에_Farm을끼워넣으면_거부한다()
    {
        var source = ProductOnly();
        source.Farm = Farm(PotatoJourneyLinkageStatusCodes.Unverified);

        var error = Assert.Throws<InvalidOperationException>(() => new PotatoJourneyMapper().Map(source));

        Assert.Equal("PotatoProductOnlyFarmUnexpected", error.Message);
    }

    [Fact]
    public void OperationalSource가_SimulationLinked를_사칭하면거부한다()
    {
        var source = SimulationLinked();
        source.SourceModeCode = PotatoJourneySourceModeCodes.OperationalProjection;

        var error = Assert.Throws<InvalidOperationException>(() => new PotatoJourneyMapper().Map(source));

        Assert.Equal("PotatoJourneySimulationModeRequired", error.Message);
    }

    [Fact]
    public void Ready가격에_시장구간이없으면거부한다()
    {
        var source = ProductOnly();
        source.DomesticPrice.Wholesale = null;

        var error = Assert.Throws<InvalidOperationException>(() => new PotatoJourneyMapper().Map(source));

        Assert.Equal("PotatoPriceReadyRangeMissing", error.Message);
    }

    [Fact]
    public void 중복SourceStableId를_거부한다()
    {
        var source = ProductOnly();
        source.SourceLineage = new[] { source.SourceLineage[0], source.SourceLineage[0] };

        var error = Assert.Throws<InvalidOperationException>(() => new PotatoJourneyMapper().Map(source));

        Assert.StartsWith("PotatoJourneySourceLineageDuplicate", error.Message);
    }

    [Fact]
    public void ProductOnly상자는_상품가격근거3개Card와_Amber강조를만든다()
    {
        var snapshot = new PotatoJourneyMapper().Map(ProductOnly());
        var result = new PotatoJourneyInterpreter().Interpret(new PotatoJourneyInterpretationInput
        {
            Snapshot = snapshot,
            AnchorKindCode = PotatoJourneyAnchorKindCodes.FarmYardCargo,
            AnchorWorldObjectRef = Anchor("farm-yard-potato-box"),
        });

        Assert.Equal(PotatoJourneyVisualTokens.ProductOnly, result.HighlightToken);
        Assert.False(result.ShowFarmConditionMarker);
        Assert.False(result.ShowCargoRoute);
        Assert.Equal(3, result.CardDeck.Cards.Length);
        Assert.Equal(
            new[] { ConceptCardKindCodes.Concept, ConceptCardKindCodes.Status, ConceptCardKindCodes.Reason },
            result.CardDeck.Cards.Select(card => card.CardKindCode));
        Assert.Contains("₩2,450/kg", result.CardDeck.Cards[1].PrimaryValueText);
        Assert.Empty(result.ModeLabel);
    }

    [Fact]
    public void Unverified재배는_Farm상태를보이되_상품관계를확정하지않는다()
    {
        var source = ProductOnly();
        source.LinkageStatusCode = PotatoJourneyLinkageStatusCodes.Unverified;
        source.Farm = Farm(PotatoJourneyLinkageStatusCodes.Unverified);
        source.SourceLineage = new[]
        {
            FarmLineage(PotatoJourneySourceModeCodes.OperationalProjection),
            source.SourceLineage[0],
        };
        source.Limitations = new[] { "공통 ProductStableId가 없어 관계를 확정하지 않았습니다." };
        var snapshot = new PotatoJourneyMapper().Map(source);

        var result = new PotatoJourneyInterpreter().Interpret(new PotatoJourneyInterpretationInput
        {
            Snapshot = snapshot,
            AnchorKindCode = PotatoJourneyAnchorKindCodes.FarmPlot,
            AnchorWorldObjectRef = Anchor("farm-potato-plot"),
        });

        Assert.True(result.ShowFarmConditionMarker);
        Assert.Equal(PotatoJourneyVisualTokens.Unverified, result.HighlightToken);
        Assert.Contains("미확정", result.CardDeck.Cards[2].SummaryText);
        Assert.Contains(result.CardDeck.Cards[0].Cautions, value => value.Contains("ProductStableId"));
    }

    [Fact]
    public void SimulationLinked는_모든Card에_SIMULATION을표시한다()
    {
        var snapshot = new PotatoJourneyMapper().Map(SimulationLinked());
        var result = new PotatoJourneyInterpreter().Interpret(new PotatoJourneyInterpretationInput
        {
            Snapshot = snapshot,
            AnchorKindCode = PotatoJourneyAnchorKindCodes.FarmPlot,
            AnchorWorldObjectRef = Anchor("farm-potato-plot"),
        });

        Assert.Equal("SIMULATION", result.ModeLabel);
        Assert.All(result.CardDeck.Cards, card => Assert.Equal("SIMULATION", card.SimulationLabel));
        Assert.Equal("Simulation", result.CardDeck.ModeCode);
    }

    [Fact]
    public async Task Repository는_Api응답을검증한Snapshot으로만반환한다()
    {
        var client = new FakeClient(ProductOnly());
        var repository = new PotatoJourneyApiRepository(client, new PotatoJourneyMapper());

        var result = await repository.LoadAsync(null);

        Assert.Equal(PotatoJourneyApiRoutes.Read, "api/v1/common/world/slices/potato-journey");
        Assert.Equal("world-slice:potato-journey", result.StableId);
        Assert.Null(client.LastCultivationStableId);
    }

    private static WorldObjectRef Anchor(string value)
        => new(
            new WorldContextId("world:farm-potato-journey:1"),
            new WorldStableId("world-object:" + value));

    private static PotatoJourneyApiModel ProductOnly()
        => new()
        {
            StableId = "world-slice:potato-journey",
            Revision = "a87f04d0",
            GeneratedAt = new DateTimeOffset(2026, 8, 10, 2, 0, 0, TimeSpan.Zero),
            AuthorizedRoleCode = "Producer",
            ViewerScopeCode = "AuthorizedParty",
            AuthorizationDecisionId = "authorized-farm-producer:8.1",
            SourceModeCode = PotatoJourneySourceModeCodes.OperationalProjection,
            LinkageStatusCode = PotatoJourneyLinkageStatusCodes.ProductOnly,
            Product = new PotatoProductApiModel
            {
                ProductStableId = "product:potato",
                DisplayName = "감자",
                HsPrefix = "0701",
                MappingQualityCode = "ExactCommodity",
                MappingQualityLabel = "동일 품목",
                MappingEvidence = "감자의 국내 유통가격을 사용합니다.",
                InformationOnly = true,
            },
            DomesticPrice = Price(),
            SourceLineage = new[] { PriceLineage() },
            Limitations = new[] { "국내 가격은 정보용 시장 관측입니다." },
            IsReadOnly = true,
        };

    private static PotatoJourneyApiModel SimulationLinked()
    {
        var source = ProductOnly();
        source.SourceModeCode = PotatoJourneySourceModeCodes.SimulationFixture;
        source.LinkageStatusCode = PotatoJourneyLinkageStatusCodes.SimulationLinked;
        source.Farm = Farm(PotatoJourneyLinkageStatusCodes.SimulationLinked);
        source.SourceLineage = new[]
        {
            FarmLineage(PotatoJourneySourceModeCodes.SimulationFixture),
            PriceLineage(),
        };
        source.Limitations = new[] { "이 관계는 Simulation scenario 안에서만 유효합니다." };
        return source;
    }

    private static PotatoCultivationApiModel Farm(string linkage)
        => new()
        {
            FarmStableId = "farm:a",
            FarmRevision = 4,
            PlotStableId = "farm-plot:a.1",
            PlotRevision = 5,
            CultivationStableId = "cultivation:a.potato.2026",
            CultivationRevision = 6,
            CropName = "감자",
            CropReferenceStableId = "crop-reference-category:fc01",
            CropReferenceSourceKey = "nongsaro:crop-ebook",
            GrowthStatusCode = "Growing",
            ProductLinkageStatusCode = linkage,
            Sensors = new[]
            {
                new PotatoSensorApiModel
                {
                    StableId = "sensor:a.soil-moisture.1",
                    Revision = 7,
                    SensorTypeCode = "SoilMoisture",
                    StatusCode = "Active",
                    LatestObservation = new PotatoSensorObservationApiModel
                    {
                        Value = 18.5m,
                        UnitCode = "Percent",
                        ObservedAt = new DateTimeOffset(2026, 8, 10, 1, 0, 0, TimeSpan.Zero),
                        FreshnessStatusCode = "Fresh",
                        ConditionCode = "Dry",
                        AssessmentRuleRevision = "soil-water-rule:3",
                    },
                },
            },
        };

    private static PotatoPriceObservationApiModel Price()
        => new()
        {
            StatusCode = PotatoPriceObservationStatusCodes.Ready,
            HsCode = "0701",
            UnitCode = "KRW_PER_KG",
            CurrencyCode = "KRW",
            DataSource = "한국농수산식품유통공사(aT) 일별 도·소매 가격정보",
            StartDate = "20260801",
            EndDate = "20260809",
            Wholesale = new PotatoPriceRangeApiModel
            {
                MarketStageCode = "Wholesale",
                MarketStageLabel = "도매",
                AverageKrwPerKg = 2450m,
                MinimumKrwPerKg = 2200m,
                MaximumKrwPerKg = 2700m,
                SampleCount = 8,
                LatestSurveyDate = "20260809",
            },
            Notices = new[] { "정보 제공용 가격입니다." },
            InformationOnly = true,
        };

    private static PotatoJourneySourceLineageApiModel PriceLineage()
        => new()
        {
            SourceKey = "public-data:kamis-domestic-price",
            SourceStableId = "price-observation:potato.0701",
            SourceRevision = "20260801:20260809:Ready",
            ObservedAt = new DateTimeOffset(2026, 8, 9, 0, 0, 0, TimeSpan.Zero),
            SourceModeCode = PotatoJourneySourceModeCodes.OperationalProjection,
        };

    private static PotatoJourneySourceLineageApiModel FarmLineage(string sourceMode)
        => new()
        {
            SourceKey = "ssalddel:farm-producer-perspective",
            SourceStableId = "cultivation:a.potato.2026",
            SourceRevision = "6",
            SourceModeCode = sourceMode,
        };

    private sealed class FakeClient(PotatoJourneyApiModel response) : IPotatoJourneyApiClient
    {
        public string? LastCultivationStableId { get; private set; }

        public Task<PotatoJourneyApiModel> GetAsync(
            string? cultivationStableId,
            CancellationToken cancellationToken = default)
        {
            LastCultivationStableId = cultivationStableId;
            return Task.FromResult(response);
        }
    }
}
