using Ssalddel.Unity.PotatoJourney;
using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.Tests;

public sealed class PotatoJourneyPvs6GateTests
{
    [Fact]
    public void RequestBuilder는_선택StableId와조회범위를_안전하게인코딩한다()
    {
        var route = PotatoJourneyApiRequestBuilder.Build("cultivation:a/potato 2026", "20260810", 21);

        Assert.Equal(
            "api/v1/common/world/slices/potato-journey?lookbackDays=21&cultivationStableId=cultivation%3Aa%2Fpotato%202026&referenceDate=20260810",
            route);
    }

    [Fact]
    public void ProductOnly에_화물을끼워넣으면_Hub이동전에거부한다()
    {
        var source = Fixture(PotatoJourneySourceModeCodes.OperationalProjection,
            PotatoJourneyLinkageStatusCodes.ProductOnly, includeCargo: true);

        var error = Assert.Throws<InvalidOperationException>(() => new PotatoJourneyMapper().Map(source));

        Assert.Equal("PotatoJourneyUnverifiedOperationalBlockUnexpected", error.Message);
    }

    [Fact]
    public void SimulationLinked와명시적화물만_SIMULATION_HubRoute를연다()
    {
        var source = Fixture(PotatoJourneySourceModeCodes.SimulationFixture,
            PotatoJourneyLinkageStatusCodes.SimulationLinked, includeCargo: true);
        var snapshot = new PotatoJourneyMapper().Map(source);

        var result = new PotatoJourneyHubRouteProjector().Project(snapshot);

        Assert.True(result.IsVisible);
        Assert.Equal("SIMULATION", result.ModeLabel);
        Assert.Equal("cargo:simulation-potato-1", result.CargoStableId);
        Assert.Equal("hub.inbound-dock", result.DestinationWaypointKey);
    }

    [Fact]
    public void Canonical관계라도_화물StableId가없으면_HubRoute는닫힌다()
    {
        var source = Fixture(PotatoJourneySourceModeCodes.OperationalProjection,
            PotatoJourneyLinkageStatusCodes.CanonicalLinked, includeCargo: false);
        var snapshot = new PotatoJourneyMapper().Map(source);

        var result = new PotatoJourneyHubRouteProjector().Project(snapshot);

        Assert.False(result.IsVisible);
        Assert.Equal("PotatoJourneyCargoRelationshipMissing", result.BlockReasonCode);
    }

    [Fact]
    public void CARGO1Adapter는_LoadedCargo의수량과전체Lineage를HubRoute에보존한다()
    {
        var loaded = LoadedCargo();

        var result = new PotatoHarvestCargoHubRouteAdapter(
            new 감자수확CargoSimulationValidator()).Project(loaded);

        Assert.True(result.IsVisible);
        Assert.Equal("SIMULATION", result.ModeLabel);
        Assert.Equal(수확CargoStateCodes.Loaded, result.HandoffStateCode);
        Assert.Equal(loaded.Cargo!.StableId, result.CargoStableId);
        Assert.Equal(loaded.HarvestLot.StableId, result.HarvestLotStableId);
        Assert.Equal(loaded.PackageLot!.StableId, result.PackageLotStableId);
        Assert.Equal(15, result.PackageCount);
        Assert.Equal(300m, result.Quantity);
        Assert.Equal(400m, result.VehicleCapacityKg);
        Assert.Contains(loaded.Cargo.StableId, result.LineageText);
    }

    [Fact]
    public void CARGO1Adapter는_포장만된Cargo를이동경로에노출하지않는다()
    {
        var source = CargoSnapshot();
        var engine = new 감자수확CargoSimulationEngine(new 감자수확CargoSimulationValidator());
        var packed = engine.Tick(source, engine.Confirm(source, engine.PreviewPacking(source)));

        var result = new PotatoHarvestCargoHubRouteAdapter(
            new 감자수확CargoSimulationValidator()).Project(packed);

        Assert.False(result.IsVisible);
        Assert.Equal("PotatoHarvestCargoNotLoaded", result.BlockReasonCode);
    }

    [Fact]
    public async Task ReadSession은_stale과실패시마지막성공본을_구분한다()
    {
        var client = new SequenceClient(
            Fixture(PotatoJourneySourceModeCodes.OperationalProjection,
                PotatoJourneyLinkageStatusCodes.ProductOnly, includeCargo: false));
        var session = new PotatoJourneyReadSession(new PotatoJourneyQueryUseCase(
            new PotatoJourneyApiRepository(client, new PotatoJourneyMapper())));
        var now = new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

        var stale = await session.RefreshAsync(null, now, TimeSpan.FromHours(2));
        client.Fail = true;
        var failed = await session.RefreshAsync(null, now, TimeSpan.FromHours(2));

        Assert.Equal(PotatoJourneyReadStateCodes.Stale, stale.StateCode);
        Assert.Equal(PotatoJourneyReadStateCodes.Error, failed.StateCode);
        Assert.True(failed.IsShowingLastSuccess);
        Assert.Same(stale.Snapshot, failed.Snapshot);
        Assert.Equal("PotatoJourneyHttpUnauthorized", failed.ErrorCode);
    }

    private static PotatoJourneyApiModel Fixture(string sourceMode, string linkage, bool includeCargo)
    {
        var linked = linkage is PotatoJourneyLinkageStatusCodes.CanonicalLinked
            or PotatoJourneyLinkageStatusCodes.SimulationLinked;
        return new PotatoJourneyApiModel
        {
            StableId = "world-slice:potato-journey",
            Revision = "pvs6:1",
            GeneratedAt = new DateTimeOffset(2026, 8, 10, 8, 0, 0, TimeSpan.Zero),
            AuthorizedRoleCode = "Producer",
            ViewerScopeCode = "AuthorizedParty",
            AuthorizationDecisionId = "authorized:producer-a",
            SourceModeCode = sourceMode,
            LinkageStatusCode = linkage,
            Product = new PotatoProductApiModel
            {
                ProductStableId = "product:potato",
                DisplayName = "감자",
                HsPrefix = "0701",
                MappingQualityCode = "ExactCommodity",
                MappingQualityLabel = "동일 품목",
                MappingEvidence = "HS 0701 정보용 관측",
                InformationOnly = true,
            },
            Farm = linked ? new PotatoCultivationApiModel
            {
                FarmStableId = "farm:a",
                PlotStableId = "farm-plot:a.1",
                CultivationStableId = "cultivation:a.potato.2026",
                CropName = "감자",
                GrowthStatusCode = "Growing",
                ProductLinkageStatusCode = linkage,
            } : null,
            DomesticPrice = new PotatoPriceObservationApiModel
            {
                StatusCode = PotatoPriceObservationStatusCodes.Ready,
                HsCode = "0701",
                UnitCode = "KRW_PER_KG",
                CurrencyCode = "KRW",
                DataSource = "aT",
                Wholesale = new PotatoPriceRangeApiModel
                {
                    MarketStageCode = "Wholesale",
                    MarketStageLabel = "도매",
                    AverageKrwPerKg = 2450,
                    MinimumKrwPerKg = 2200,
                    MaximumKrwPerKg = 2700,
                    SampleCount = 8,
                },
                InformationOnly = true,
            },
            CargoJourney = includeCargo ? new PotatoCargoApiModel
            {
                CargoStableId = "cargo:simulation-potato-1",
                TransportTaskStableId = "transport-task:simulation-potato-1",
                InboundTaskStableId = "inbound-task:simulation-potato-1",
                HandoffStateCode = "InTransit",
            } : null,
            SourceLineage = new[]
            {
                new PotatoJourneySourceLineageApiModel
                {
                    SourceKey = "fixture:potato",
                    SourceStableId = "source:potato-pvs6",
                    SourceRevision = "1",
                    SourceModeCode = sourceMode,
                },
            },
            Limitations = new[] { "Information only" },
            IsReadOnly = true,
        };
    }

    private static 감자수확CargoSimulationSnapshot LoadedCargo()
    {
        var packed = CargoSnapshot();
        var engine = new 감자수확CargoSimulationEngine(new 감자수확CargoSimulationValidator());
        packed = engine.Tick(packed, engine.Confirm(packed, engine.PreviewPacking(packed)));
        return engine.Tick(packed, engine.Confirm(packed, engine.PreviewLoading(packed)));
    }

    private static 감자수확CargoSimulationSnapshot CargoSnapshot()
    {
        var lifecycleValidator = new 감자재배LifecycleSimulationValidator(
            new FarmSoilTileSimulationValidator(), new 재배달력ProfileValidator());
        var lifecycle = new 감자재배LifecycleSimulationEngine(lifecycleValidator);
        var source = 감자재배LifecycleSimulationFixture.Create();
        var tile = source.Soil.Tiles.First(value =>
            value.CultivationStateCode == FarmSoilTileCultivationStateCodes.Tilled);
        var sown = lifecycle.Tick(source,
            lifecycle.Confirm(source, lifecycle.PreviewSowing(source, tile.StableId)));
        var ready = lifecycle.Tick(sown, lifecycle.CreateAdvanceDaysCommand(sown, 6));
        var harvested = lifecycle.Tick(ready,
            lifecycle.Confirm(ready, lifecycle.PreviewHarvest(ready)));
        return 감자수확CargoSimulationFixture.Create(harvested.HarvestLot!);
    }

    private sealed class SequenceClient(PotatoJourneyApiModel response) : IPotatoJourneyApiClient
    {
        public bool Fail { get; set; }

        public Task<PotatoJourneyApiModel> GetAsync(
            string? cultivationStableId,
            CancellationToken cancellationToken = default)
            => Fail
                ? Task.FromException<PotatoJourneyApiModel>(
                    new InvalidOperationException("PotatoJourneyHttpUnauthorized:401"))
                : Task.FromResult(response);
    }
}
