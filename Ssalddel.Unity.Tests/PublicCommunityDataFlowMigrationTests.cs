using Ssalddel.Unity.Community;
using Ssalddel.Unity.PublicData;

namespace Ssalddel.Tests.UnityData;

public sealed class PublicCommunityDataFlowMigrationTests
{
    [Fact]
    public void PublicData는_출처Metric을보존한뒤_World와MarkerPresentation으로변환한다()
    {
        var data = new PublicWorldMapDataMapper().Map(PublicSnapshot("revision-1"));
        var world = new PublicWorldMapInterpreter().Interpret(data);
        var load = new PublicDataHallLoadResult
        {
            StateCode = PublicDataHallLoadStateCodes.Success,
            Snapshot = world,
            Changes = new PublicWorldMapReconciler().Reconcile(
                Array.Empty<PublicWorldMapObservation>(), world.Observations),
        };
        var presentation = new PublicDataHallPresenter().Present(load);

        Assert.Equal("KAMIS", data.Observations[0].SourceName);
        Assert.Equal("kg", Assert.Single(data.Observations[0].Metrics).Unit);
        Assert.StartsWith("interpretation:", world.Lineage!.InterpretationRevision);
        Assert.StartsWith("presentation:", presentation.PresentationRevision);
        Assert.Equal("감자 가격\nKAMIS", Assert.Single(presentation.Observations).MarkerLabelText);
    }

    [Fact]
    public async Task PublicDataFlow갱신실패는_마지막성공World를유지한다()
    {
        var coordinator = new PublicDataHallDataFlowLoadCoordinator(
            new PublicWorldMapDataFlowQueryUseCase(
                new PublicWorldMapApiDataRepository(
                    new PublicClient(PublicSnapshot("revision-1"), new InvalidOperationException("offline")),
                    new PublicWorldMapDataMapper()),
                new PublicWorldMapInterpreter()),
            new PublicWorldMapReconciler());

        var success = await coordinator.LoadAsync(new PublicWorldMapQuery());
        var failure = await coordinator.LoadAsync(new PublicWorldMapQuery());

        Assert.Equal(PublicDataHallLoadStateCodes.Success, success.StateCode);
        Assert.Equal(PublicDataHallLoadStateCodes.RefreshError, failure.StateCode);
        Assert.Same(success.Snapshot, failure.Snapshot);
    }

    [Fact]
    public void CommunityData는_공개사실배열과_World관계와_ItemPresentation을분리한다()
    {
        var data = new CommunitySquareDataMapper().Map(CommunitySnapshot("revision-1"));
        var world = new CommunitySquareWorldInterpreter().Interpret(data);
        var load = new CommunityMarketSquareLoadResult
        {
            StateCode = CommunityMarketSquareLoadStateCodes.Success,
            Snapshot = world,
            Changes = new CommunityMarketSquareReconciler().Reconcile(
                Array.Empty<CommunitySquareWorldItem>(), world.Items),
        };
        var presentation = new CommunitySquarePresenter().Present(load);

        Assert.Single(data.Boards);
        Assert.Single(data.Posts);
        Assert.Single(data.Activities);
        Assert.Single(data.Ledgers);
        Assert.StartsWith("interpretation:", world.Lineage!.InterpretationRevision);
        Assert.StartsWith("presentation:", presentation.PresentationRevision);
        Assert.Contains(presentation.Items, value => value.KindCode == "Ledger" && value.DetailHref == "/community/posts/101");
    }

    [Fact]
    public async Task CommunityDataFlow갱신실패는_마지막성공World를유지한다()
    {
        var coordinator = new CommunitySquareDataFlowLoadCoordinator(
            new CommunitySquareDataFlowQueryUseCase(
                new CommunitySquareApiDataRepository(
                    new CommunityClient(CommunitySnapshot("revision-1"), new InvalidOperationException("offline")),
                    new CommunitySquareDataMapper()),
                new CommunitySquareWorldInterpreter()),
            new CommunityMarketSquareReconciler());

        var success = await coordinator.LoadAsync();
        var failure = await coordinator.LoadAsync();

        Assert.Equal(CommunityMarketSquareLoadStateCodes.Success, success.StateCode);
        Assert.Equal(CommunityMarketSquareLoadStateCodes.RefreshError, failure.StateCode);
        Assert.Same(success.Snapshot, failure.Snapshot);
    }

    private static PublicWorldMapSnapshotApiModel PublicSnapshot(string revision)
        => new()
        {
            DatasetCode = PublicWorldMapDatasetCodes.DayWork,
            Revision = revision,
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            Layers = new[]
            {
                new PublicWorldMapLayerApiModel
                {
                    Code = "public-price", DatasetCode = PublicWorldMapDatasetCodes.DayWork,
                    DisplayName = "가격", Color = "#ef8f3c", MarkerShape = "diamond",
                },
            },
            Observations = new[]
            {
                new PublicWorldMapObservationApiModel
                {
                    StableId = "public-data:price.kr", DatasetCode = PublicWorldMapDatasetCodes.DayWork,
                    LayerCode = "public-price", CountryCode = "KR", Latitude = 36.5, Longitude = 127.8,
                    Title = "감자 가격", SourceName = "KAMIS", EvidenceStatusCode = "Observed",
                    DetailHref = "/community/information-map?observation=price", FreshnessCode = "Fresh",
                    LocationPrecisionCode = "administrative-region-representative",
                    Metrics = new[]
                    {
                        new PublicWorldMapMetricApiModel { Code = "price", DisplayName = "가격", Value = 35000, Unit = "kg" },
                    },
                },
            },
        };

    private static CommunityMarketSquareSnapshotApiModel CommunitySnapshot(string revision)
        => new()
        {
            StableId = "community-market-square:public", Revision = revision,
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            Boards = new[]
            {
                new CommunitySquareBoardApiModel
                {
                    StableId = "community-board:sales-supply", DisplayName = "판매와 공급",
                    Description = "공개 게시판", PostingAccessCode = "Member", PostCount = 1,
                },
            },
            Posts = new[]
            {
                new CommunitySquarePostApiModel
                {
                    StableId = "community-post:101", Title = "감자 수요", Excerpt = "공개 요약",
                    Category = "수요", DetailHref = "/community/posts/101",
                    PublishedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            },
            ActivitySignals = new[]
            {
                new CommunitySquareActivityApiModel
                {
                    StableId = "community-activity:signal-1", Title = "참여 신호", Summary = "비식별 집계",
                    ActivityKind = "Participation", AggregationCount = 4,
                    OccurredAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                },
            },
            Ledgers = new[]
            {
                new CommunitySquareLedgerApiModel
                {
                    StableId = "community-ledger-summary:post-101", SourcePostStableId = "community-post:101",
                    Title = "감자 수요", TemplateName = "공동행동 준비", State = "관심모집",
                    CurrentStage = "수요확인", DetailAvailable = true, DetailHref = "/community/posts/101",
                },
            },
        };

    private sealed class PublicClient(params object[] responses) : IPublicWorldMapApiClient
    {
        private readonly Queue<object> values = new(responses);
        public Task<PublicWorldMapSnapshotApiModel> GetAsync(
            PublicWorldMapQuery query, CancellationToken cancellationToken = default)
        {
            var value = values.Dequeue();
            return value is Exception error
                ? Task.FromException<PublicWorldMapSnapshotApiModel>(error)
                : Task.FromResult((PublicWorldMapSnapshotApiModel)value);
        }
    }

    private sealed class CommunityClient(params object[] responses) : ICommunityMarketSquareApiClient
    {
        private readonly Queue<object> values = new(responses);
        public Task<CommunityMarketSquareSnapshotApiModel> GetPublicSnapshotAsync(CancellationToken cancellationToken = default)
        {
            var value = values.Dequeue();
            return value is Exception error
                ? Task.FromException<CommunityMarketSquareSnapshotApiModel>(error)
                : Task.FromResult((CommunityMarketSquareSnapshotApiModel)value);
        }
    }
}
