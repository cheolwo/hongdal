using Ssalddel.Unity.PotatoJourney;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class PotatoProductionDistributionWorldMemoryTests
{
    [Fact]
    public async Task 게임시작Bootstrap은_서버Snapshot을StableId메모리와VisualKey로올린다()
    {
        var client = new FixedClient(Api("bootstrap:1", At(9)));
        var store = new PotatoProductionDistributionWorldMemoryStore();
        var loader = new PotatoProductionDistributionWorldBootstrapLoader(
            new PotatoJourneyQueryUseCase(new PotatoJourneyApiRepository(client, new PotatoJourneyMapper())),
            store);

        var result = await loader.LoadAsync("cultivation:a.potato.2026", "session:a|role:producer");

        Assert.Equal(5, store.Nodes.Count);
        Assert.Equal(5, result.Changes.Added.Length);
        Assert.True(store.TryGet("cargo:simulation-potato-city-1", out var cargo));
        Assert.Equal(PotatoProductionDistributionVisualKeys.DeliveryVan, cargo!.VisualKey);
        Assert.All(store.Nodes, node =>
        {
            Assert.DoesNotContain("Assets/", node.VisualKey, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".prefab", node.VisualKey, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public void 같은Snapshot은_기존메모리Instance를유지한다()
    {
        var store = new PotatoProductionDistributionWorldMemoryStore();
        var snapshot = Map(Api("bootstrap:1", At(9)));
        store.Load(snapshot, "scope:a");
        store.TryGet("product:potato", out var before);

        var changes = store.Load(Map(Api("bootstrap:1", At(9))), "scope:a");
        store.TryGet("product:potato", out var after);

        Assert.Empty(changes.Added);
        Assert.Empty(changes.Updated);
        Assert.Equal(5, changes.Unchanged.Length);
        Assert.Same(before, after);
    }

    [Fact]
    public void 낮은GeneratedAt은_현재메모리를바꾸기전에거부한다()
    {
        var store = new PotatoProductionDistributionWorldMemoryStore();
        store.Load(Map(Api("bootstrap:2", At(10))), "scope:a");

        var error = Assert.Throws<InvalidOperationException>(() =>
            store.Load(Map(Api("bootstrap:1", At(9))), "scope:a"));

        Assert.Equal("PotatoProductionDistributionLowerGeneratedAt", error.Message);
        Assert.Equal("bootstrap:2", store.Current!.Revision);
    }

    [Fact]
    public void AuthorizationBoundary가바뀌면_이전Instance를재사용하지않는다()
    {
        var store = new PotatoProductionDistributionWorldMemoryStore();
        var snapshot = Map(Api("bootstrap:1", At(9)));
        store.Load(snapshot, "session:a|role:producer");
        store.TryGet("product:potato", out var before);

        var changes = store.Load(snapshot, "session:a|role:observer");
        store.TryGet("product:potato", out var after);

        Assert.Equal(5, changes.Added.Length);
        Assert.Empty(changes.Unchanged);
        Assert.NotSame(before, after);
    }

    [Fact]
    public void 공개상품이사라지면_Market노드두개를Removed로계산한다()
    {
        var store = new PotatoProductionDistributionWorldMemoryStore();
        store.Load(Map(Api("bootstrap:1", At(9))), "scope:a");
        var withoutMarket = Api("bootstrap:2", At(10));
        withoutMarket.Market = null;

        var changes = store.Load(Map(withoutMarket), "scope:a");

        Assert.Equal(2, changes.Removed.Length);
        Assert.Contains(changes.Removed, value =>
            value.ObjectKindCode == PotatoProductionDistributionObjectKindCodes.Market);
        Assert.Contains(changes.Removed, value =>
            value.ObjectKindCode == PotatoProductionDistributionObjectKindCodes.PublicMarketProduct);
    }

    private static PotatoJourneySnapshot Map(PotatoJourneyApiModel source)
        => new PotatoJourneyMapper().Map(source);

    private static DateTimeOffset At(int hour)
        => new(2026, 8, 10, hour, 0, 0, TimeSpan.Zero);

    private static PotatoJourneyApiModel Api(string revision, DateTimeOffset generatedAt)
        => new()
        {
            StableId = "world-slice:potato-journey",
            Revision = revision,
            GeneratedAt = generatedAt,
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
                FarmStableId = "farm:a", FarmRevision = 1,
                PlotStableId = "farm-plot:a.1", PlotRevision = 1,
                CultivationStableId = "cultivation:a.potato.2026", CultivationRevision = 1,
                CropName = "감자", GrowthStatusCode = "Harvested",
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
                IsSaleAvailable = true, InventoryObservedAt = generatedAt,
                SourceStableId = "market:urban-demo-001", SourceRevision = revision,
            },
            SourceLineage = new[]
            {
                new PotatoJourneySourceLineageApiModel
                {
                    SourceKey = "fixture:potato-bootstrap",
                    SourceStableId = "source:potato-bootstrap",
                    SourceRevision = revision,
                    SourceModeCode = PotatoJourneySourceModeCodes.SimulationFixture,
                },
            },
            Limitations = new[] { "Simulation only" },
            IsReadOnly = true,
        };

    private sealed class FixedClient(PotatoJourneyApiModel response) : IPotatoJourneyApiClient
    {
        public int CallCount { get; private set; }

        public Task<PotatoJourneyApiModel> GetAsync(
            string? cultivationStableId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(response);
        }
    }
}
