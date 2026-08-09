using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PublicData;

namespace Ssalddel.Tests.UnityData;

public sealed class PublicWorldSurfaceTests
{
    [Fact]
    public void SharedWorld는_WireVisualMetadata를제외하고_공개사실과의미만보존한다()
    {
        var world = Shared(Api("revision-1"));

        Assert.Single(world.Layers);
        Assert.Null(typeof(PublicLayerWorldState).GetProperty("Color"));
        Assert.Null(typeof(PublicLayerWorldState).GetProperty("MarkerShape"));
        var observation = Assert.Single(world.Observations);
        Assert.Equal("Observed", observation.SemanticStatusCode);
        Assert.Equal("35000", Assert.Single(observation.Metrics).Value.ToString());
        Assert.StartsWith("interpretation:", world.Lineage.InterpretationRevision);
    }

    [Fact]
    public void Perspective는_SharedWorld에없는Focus를새로추론하지않는다()
    {
        var world = Shared(Api("revision-1"));
        var context = Perspective(new WorldStableId("public-observation:unknown-1"));

        var error = Assert.Throws<InvalidOperationException>(() =>
            new PublicWorldPerspectiveInterpreter().Interpret(world, context));

        Assert.Equal("PublicPerspectiveFocusUnknown:public-observation:unknown-1", error.Message);
    }

    [Fact]
    public void Projector는_MarkerLegendHeatmapDetail을독립Surface로만든다()
    {
        var world = Shared(Api("revision-1"));
        var focus = Assert.Single(world.Observations).StableId;
        var perspective = new PublicWorldPerspectiveInterpreter().Interpret(world, Perspective(focus));
        var result = Projector().Project(perspective, Presentation());

        var marker = Assert.Single(result.Markers);
        var legend = Assert.Single(result.Legends);
        var heatmap = Assert.Single(result.Heatmaps);
        var detail = Assert.Single(result.Details);
        Assert.StartsWith("public-map-marker:", marker.StableId.Value);
        Assert.Equal("Green", marker.ColorCode);
        Assert.Equal("Circle", marker.ShapeCode);
        Assert.Equal("Green", legend.ColorCode);
        Assert.False(heatmap.IsAvailable);
        Assert.Equal("RegionGeometryMissing", heatmap.LimitationCode);
        Assert.Equal("가격 35000 KRW/kg", detail.MetricText);
    }

    [Fact]
    public void Marker내용만바뀌면_Legend와HeatmapInstance는유지한다()
    {
        var first = Surface(Api("revision-1"));
        var changedApi = Api("revision-2");
        changedApi.Observations[0].Title = "감자 가격 갱신";
        var second = Surface(changedApi);

        var changes = new PublicDataHallSurfaceChangeSetCalculator().Calculate(first, second);

        Assert.Single(changes.Markers.Updated);
        Assert.Single(changes.Legends.Unchanged);
        Assert.Single(changes.Heatmaps.Unchanged);
        Assert.Empty(changes.Details.Updated);
    }

    [Fact]
    public void Focus가없으면_DetailSurface를생성하지않는다()
    {
        var result = Surface(Api("revision-1"));

        Assert.Empty(result.Details);
    }

    [Fact]
    public async Task SurfaceRuntime은_Data부터독립SurfaceChangeSet까지한번에조율한다()
    {
        var repository = new StubPublicWorldMapDataRepository(
            new PublicWorldMapDataMapper().Map(Api("revision-1")));
        var coordinator = new PublicDataHallSurfaceRuntimeCoordinator(
            new PublicWorldMapRuntimeDataQuery(repository),
            new PublicSharedWorldInterpreter(),
            new PublicWorldPerspectiveInterpreter(),
            Projector(),
            new PublicDataHallSurfaceChangeSetCalculator());

        var result = await coordinator.RefreshDataAsync(
            new PublicWorldMapQuery { DatasetCode = PublicWorldMapDatasetCodes.DayWork },
            new PublicWorldInterpretationContext(),
            Perspective(),
            Presentation(),
            WorldDataQueryContext.Global(
                PublicWorldMapDatasetCodes.DayWork,
                DataRuntimeMode.Operational));

        Assert.Equal(ZoneRuntimeStateCode.Ready, result.Status.StateCode);
        Assert.NotNull(result.SharedWorld);
        Assert.NotNull(result.PerspectiveWorld);
        Assert.Single(result.Changes!.Markers.Added);
        Assert.Single(result.Changes.Legends.Added);
        Assert.Single(result.Changes.Heatmaps.Added);
        Assert.Empty(result.Changes.Details.Added);
    }

    private static PublicDataHallSurfaceSnapshot Surface(PublicWorldMapSnapshotApiModel api)
    {
        var world = Shared(api);
        var perspective = new PublicWorldPerspectiveInterpreter().Interpret(world, Perspective());
        return Projector().Project(perspective, Presentation());
    }

    private static PublicWorldState Shared(PublicWorldMapSnapshotApiModel api)
        => new PublicSharedWorldInterpreter().Interpret(
            new PublicWorldMapDataMapper().Map(api),
            new PublicWorldInterpretationContext { EvaluationTimeUtc = api.GeneratedAtUtc });

    private static InterpretationPerspectiveContext Perspective(WorldStableId? focus = null)
        => new(
            "PublicObserver",
            "ExplorePublicData",
            "PublicDataHall",
            WorldInterpretationMode.Operational,
            focus);

    private static PublicDataHallPresentationContext Presentation()
        => new() { LocaleCode = "ko-KR", QualityTierCode = "Primitive" };

    private static PublicDataHallSurfaceProjector Projector()
        => new(new PublicDataHallVisualPolicy());

    private static PublicWorldMapSnapshotApiModel Api(string revision)
        => new()
        {
            DatasetCode = PublicWorldMapDatasetCodes.DayWork,
            Revision = revision,
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T09:00:00Z"),
            Layers =
            [
                new PublicWorldMapLayerApiModel
                {
                    Code = "price-market",
                    DatasetCode = PublicWorldMapDatasetCodes.DayWork,
                    DisplayName = "가격·시장",
                    Description = "공개 가격 관측",
                    Color = "#ff0000",
                    MarkerShape = "star",
                },
            ],
            Observations =
            [
                new PublicWorldMapObservationApiModel
                {
                    StableId = "public-observation:potato-1",
                    DatasetCode = PublicWorldMapDatasetCodes.DayWork,
                    LayerCode = "price-market",
                    CountryCode = "KR",
                    CountryName = "대한민국",
                    Latitude = 37.5,
                    Longitude = 127.0,
                    Title = "감자 가격",
                    Summary = "공개 관측",
                    SourceName = "KAMIS",
                    EvidenceAsOfUtc = DateTimeOffset.Parse("2026-08-08T08:30:00Z"),
                    EvidenceStatusCode = "Observed",
                    DetailHref = "/public-data/price/potato",
                    SourceHref = "https://example.invalid/kamis",
                    LocationPrecisionCode = "Region",
                    MarkerStatusCode = "Observed",
                    FreshnessCode = "Fresh",
                    BoundaryNotice = "단계간 가격차는 마진이 아님",
                    SourceVersion = revision,
                    Metrics =
                    [
                        new PublicWorldMapMetricApiModel
                        {
                            Code = "price",
                            DisplayName = "가격",
                            Value = 35000m,
                            Unit = "KRW/kg",
                        },
                    ],
                },
            ],
        };

    private sealed class StubPublicWorldMapDataRepository : IPublicWorldMapDataRepository
    {
        private readonly PublicWorldMapDataSnapshot snapshot;

        public StubPublicWorldMapDataRepository(PublicWorldMapDataSnapshot snapshot)
            => this.snapshot = snapshot;

        public Task<PublicWorldMapDataSnapshot> 조회Async(
            PublicWorldMapQuery query,
            CancellationToken cancellationToken = default)
            => Task.FromResult(snapshot);
    }
}
