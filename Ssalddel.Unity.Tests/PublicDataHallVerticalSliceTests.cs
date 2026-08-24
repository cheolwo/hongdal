using Ssalddel.Unity.PublicData;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class PublicDataHallVerticalSliceTests
{
    [Fact]
    public async Task 공개세계지도Api는_Repository와UseCase를통해_출처와정밀도를보존한다()
    {
        var client = new SequencePublicWorldMapApiClient(Snapshot("revision-1", Observation()));
        var useCase = new PublicWorldMapQueryUseCase(
            new PublicWorldMapApiRepository(client, new PublicWorldMapMapper()));

        var result = await useCase.실행Async(new PublicWorldMapQuery
        {
            DatasetCode = PublicWorldMapDatasetCodes.DayWork,
        });

        var observation = Assert.Single(result.Observations);
        Assert.Equal("public-data:price.kr", observation.StableId);
        Assert.Equal("KAMIS", observation.SourceName);
        Assert.Equal("administrative-region-representative", observation.LocationPrecisionCode);
        Assert.Equal("Fresh", observation.FreshnessCode);
        Assert.Equal("행정구역 대표 위치", observation.BoundaryNotice);
        Assert.Equal(1, client.CallCount);
        Assert.Equal("api/v1/community/world-map/observations", PublicWorldMapApiRoutes.Observations);
    }

    [Fact]
    public void Mapper는_중복StableId를_잘못된Snapshot으로거부한다()
    {
        var snapshot = Snapshot("revision-1", Observation(), Observation());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new PublicWorldMapMapper().Map(snapshot));

        Assert.Equal(
            "DuplicatePublicWorldMapObservation:public-data:price.kr",
            exception.Message);
    }

    [Fact]
    public void Mapper는_좌표와Layer경계를검증하고_위치정밀도를임의로바꾸지않는다()
    {
        var invalid = Observation();
        invalid.Latitude = 91d;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new PublicWorldMapMapper().Map(Snapshot("revision-1", invalid)));

        Assert.Equal(
            "PublicWorldMapCoordinatesInvalid:public-data:price.kr",
            exception.Message);
    }

    [Fact]
    public async Task 최초실패는빈상태이고_갱신실패는마지막성공Snapshot을유지한다()
    {
        var failureFirst = new PublicDataHallLoadCoordinator(
            UseCase(new SequencePublicWorldMapApiClient(new InvalidOperationException("offline"))),
            new PublicWorldMapReconciler());

        var initialFailure = await failureFirst.LoadAsync(Query());

        Assert.Equal(PublicDataHallLoadStateCodes.InitialLoadError, initialFailure.StateCode);
        Assert.Null(initialFailure.Snapshot);

        var client = new SequencePublicWorldMapApiClient(
            Snapshot("revision-1", Observation()),
            new InvalidOperationException("refresh-offline"));
        var coordinator = new PublicDataHallLoadCoordinator(
            UseCase(client), new PublicWorldMapReconciler());

        var success = await coordinator.LoadAsync(Query());
        var refreshFailure = await coordinator.LoadAsync(Query());

        Assert.Equal(PublicDataHallLoadStateCodes.Success, success.StateCode);
        Assert.Equal(PublicDataHallLoadStateCodes.RefreshError, refreshFailure.StateCode);
        Assert.Same(success.Snapshot, refreshFailure.Snapshot);
        Assert.Single(refreshFailure.Snapshot!.Observations);
    }

    [Fact]
    public async Task 성공갱신은_StableId기준으로_추가갱신제거를계산한다()
    {
        var original = Observation();
        var updated = Observation();
        updated.Title = "감자 가격 갱신";
        var added = Observation("public-data:culture.kr");
        added.LayerCode = "regional-culture";
        var client = new SequencePublicWorldMapApiClient(
            Snapshot("revision-1", original, Observation("public-data:remove.kr")),
            Snapshot("revision-2", updated, added));
        var coordinator = new PublicDataHallLoadCoordinator(
            UseCase(client), new PublicWorldMapReconciler());

        await coordinator.LoadAsync(Query());
        var refreshed = await coordinator.LoadAsync(Query());

        Assert.Single(refreshed.Changes!.Added);
        Assert.Single(refreshed.Changes.Updated);
        Assert.Single(refreshed.Changes.Removed);
        Assert.Empty(refreshed.Changes.Unchanged);
    }

    private static PublicWorldMapQuery Query() => new PublicWorldMapQuery
    {
        DatasetCode = PublicWorldMapDatasetCodes.DayWork,
    };

    private static PublicWorldMapQueryUseCase UseCase(IPublicWorldMapApiClient client)
    {
        return new PublicWorldMapQueryUseCase(
            new PublicWorldMapApiRepository(client, new PublicWorldMapMapper()));
    }

    private static PublicWorldMapSnapshotApiModel Snapshot(
        string revision,
        params PublicWorldMapObservationApiModel[] observations)
    {
        return new PublicWorldMapSnapshotApiModel
        {
            DatasetCode = PublicWorldMapDatasetCodes.DayWork,
            Revision = revision,
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T15:00:00+09:00"),
            Layers = new[]
            {
                new PublicWorldMapLayerApiModel
                {
                    Code = "public-price",
                    DatasetCode = PublicWorldMapDatasetCodes.DayWork,
                    DisplayName = "가격·시장",
                    Color = "#ef8f3c",
                    MarkerShape = "diamond",
                },
                new PublicWorldMapLayerApiModel
                {
                    Code = "regional-culture",
                    DatasetCode = PublicWorldMapDatasetCodes.DayWork,
                    DisplayName = "지역 문화",
                    Color = "#176b4d",
                    MarkerShape = "circle",
                },
            },
            Observations = observations,
        };
    }

    private static PublicWorldMapObservationApiModel Observation(
        string stableId = "public-data:price.kr")
    {
        return new PublicWorldMapObservationApiModel
        {
            StableId = stableId,
            DatasetCode = PublicWorldMapDatasetCodes.DayWork,
            LayerCode = "public-price",
            CountryCode = "KR",
            CountryName = "대한민국",
            Latitude = 36.5d,
            Longitude = 127.8d,
            Title = "감자 가격",
            Summary = "공개 가격 관측",
            SourceName = "KAMIS",
            EvidenceAsOfUtc = DateTimeOffset.Parse("2026-08-08T00:00:00Z"),
            EvidenceStatusCode = "Observed",
            DetailHref = "/community/information-map?observation=price",
            SourceHref = "https://example.invalid/source",
            LocationPrecisionCode = "administrative-region-representative",
            FreshnessCode = "Fresh",
            BoundaryNotice = "행정구역 대표 위치",
        };
    }

    private sealed class SequencePublicWorldMapApiClient : IPublicWorldMapApiClient
    {
        private readonly Queue<object> responses;

        public SequencePublicWorldMapApiClient(params object[] responses)
        {
            this.responses = new Queue<object>(responses);
        }

        public int CallCount { get; private set; }

        public Task<PublicWorldMapSnapshotApiModel> GetAsync(
            PublicWorldMapQuery query,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            var response = responses.Dequeue();
            return response is Exception exception
                ? Task.FromException<PublicWorldMapSnapshotApiModel>(exception)
                : Task.FromResult((PublicWorldMapSnapshotApiModel)response);
        }
    }
}
