using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.Community;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Community;

public sealed class 선택공공데이터MapProjectionTests
{
    private static readonly DateTimeOffset CollectedAt =
        new(2026, 8, 3, 1, 2, 3, TimeSpan.Zero);

    [Fact]
    public async Task 세원천을_좌표_품목범위_단위있는KosisSnapshot으로각각갱신한다()
    {
        var store = new 선택공공데이터MapSnapshotStore();
        var sut = CreateRefresher(
            store,
            TourismResponse(),
            OnlinePriceResponse(),
            KosisListResponse(),
            KosisDetailResponse(),
            new SelectedPublicDataMapOptions
            {
                TourismEnabled = true,
                OnlinePriceEnabled = true,
                KosisEnabled = true,
                TourismMarkerLimit = 50
            });

        var result = await sut.RefreshAsync();

        Assert.True(result.TourismRefreshed);
        Assert.True(result.OnlinePriceRefreshed);
        Assert.True(result.KosisRefreshed);
        var snapshot = store.Read();
        Assert.Equal(48925, snapshot.Tourism?.TotalCount);
        Assert.Equal(2, snapshot.Tourism?.Items.Count);
        Assert.All(snapshot.Tourism!.Items, item =>
        {
            Assert.InRange(item.Latitude, 33, 39);
            Assert.InRange(item.Longitude, 124, 132);
        });
        Assert.Equal(402, snapshot.OnlinePrice?.ItemCatalogCount);
        Assert.Equal("58", snapshot.Kosis?.IndicatorId);
        Assert.Equal("202605", snapshot.Kosis?.Period);
        Assert.Equal(119.92m, snapshot.Kosis?.Value);
        Assert.Equal("2020=100", snapshot.Kosis?.Unit);
    }

    [Fact]
    public async Task 관광갱신실패시_이전성공Snapshot을유지한다()
    {
        var previousTourism = new 국문관광정보MapSnapshot(
            CollectedAt.AddHours(-2),
            1,
            [new("previous", "12", "이전 관광지", 37.5, 127, null)]);
        var previous = new 선택공공데이터MapSnapshot(previousTourism, null, null);
        var store = new 선택공공데이터MapSnapshotStore();
        store.Replace(previous);
        var sut = CreateRefresher(
            store,
            FailureResponse("area-based"),
            OnlinePriceResponse(),
            KosisListResponse(),
            KosisDetailResponse(),
            new SelectedPublicDataMapOptions { TourismEnabled = true });

        var result = await sut.RefreshAsync();

        Assert.False(result.TourismRefreshed);
        Assert.Same(previousTourism, store.Read().Tourism);
    }

    [Fact]
    public void 저장된Snapshot은_재시작뒤같은Version과마지막성공본으로복원된다()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "ssalddel-selected-map-snapshot-tests",
            Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "snapshot.v1.json");
        try
        {
            var firstStore = new 선택공공데이터MapSnapshotStore(
                path,
                new FixedTimeProvider(CollectedAt.AddMinutes(1)));
            firstStore.Replace(new 선택공공데이터MapSnapshot(
                new 국문관광정보MapSnapshot(
                    CollectedAt,
                    1,
                    [new("1001", "12", "관광지 A", 37.5, 127.1, CollectedAt.AddDays(-1))]),
                null,
                null));

            var persisted = firstStore.Read();
            var restartedStore = new 선택공공데이터MapSnapshotStore(path);
            var restored = restartedStore.Read();

            Assert.StartsWith("selected-public-data-map.v1:sha256:", persisted.SnapshotVersion);
            Assert.Equal(persisted.SnapshotVersion, restored.SnapshotVersion);
            Assert.Equal(CollectedAt.AddMinutes(1), restored.PersistedAtUtc);
            Assert.Equal("1001", restored.Tourism?.Items.Single().ContentId);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task 지도투영은_관광주소연락처를제외하고온라인가격과Kosis단위를분리한다()
    {
        var store = new 선택공공데이터MapSnapshotStore();
        var refresher = CreateRefresher(
            store,
            TourismResponse(),
            OnlinePriceResponse(),
            KosisListResponse(),
            KosisDetailResponse(),
            new SelectedPublicDataMapOptions
            {
                TourismEnabled = true,
                OnlinePriceEnabled = true,
                KosisEnabled = true
            });
        await refresher.RefreshAsync();
        var sut = new 선택공공데이터MapMarkerReader(
            store,
            new FixedTimeProvider(CollectedAt.AddHours(1)));

        var observations = await sut.공개Marker조회Async();

        Assert.Equal(4, observations.Count);
        Assert.Equal(2, observations.Count(item =>
            item.LayerCode == 커뮤니티세계지도LayerCodes.TourismPublicEvidence));
        var tourism = observations.First(item =>
            item.LayerCode == 커뮤니티세계지도LayerCodes.TourismPublicEvidence);
        Assert.Equal(커뮤니티세계지도위치정밀도Codes.OfficialPoint, tourism.LocationPrecisionCode);
        Assert.DoesNotContain("주소", tourism.Summary, StringComparison.Ordinal);
        Assert.DoesNotContain("전화", tourism.Summary, StringComparison.Ordinal);
        var online = observations.Single(item =>
            item.LayerCode == 커뮤니티세계지도LayerCodes.OnlinePricePublicEvidence);
        Assert.Equal(402m, online.Metrics?.Single().Value);
        Assert.Contains("판매단위가 없어", online.Summary, StringComparison.Ordinal);
        var kosis = observations.Single(item =>
            item.LayerCode == 커뮤니티세계지도LayerCodes.KosisStatisticalContext);
        Assert.Equal(119.92m, kosis.Metrics?.Single().Value);
        Assert.Equal("2020=100", kosis.Metrics?.Single().Unit);
        Assert.Contains("2026년 5월", kosis.Summary, StringComparison.Ordinal);
        Assert.Contains("직접 가격차", kosis.Summary, StringComparison.Ordinal);
        Assert.All(observations, observation =>
            Assert.Equal(커뮤니티세계지도FreshnessCodes.Fresh, observation.FreshnessCode));
        Assert.All(observations, observation =>
            Assert.StartsWith("selected-public-data-map.v1:sha256:", observation.SourceVersion));
    }

    private static 선택공공데이터MapProjectionRefresher CreateRefresher(
        I선택공공데이터MapSnapshotStore store,
        공공데이터포털업무ApiResponse tourism,
        공공데이터포털업무ApiResponse onlinePrice,
        공공데이터포털업무ApiResponse kosisList,
        공공데이터포털업무ApiResponse kosisDetail,
        SelectedPublicDataMapOptions mapOptions)
        => new(
            new StubTourismClient(tourism),
            new StubOnlinePriceClient(onlinePrice),
            new StubKosisClient(kosisList, kosisDetail),
            store,
            Options.Create(new PublicDataOptions { SelectedPublicDataMap = mapOptions }));

    private static 공공데이터포털업무ApiResponse TourismResponse()
        => SuccessResponse(
            "area-based",
            """
            {
              "response": {
                "header": { "resultCode": "0000", "resultMsg": "OK" },
                "body": {
                  "totalCount": 48925,
                  "items": { "item": [
                    { "contentid": "1001", "contenttypeid": "12", "title": "관광지 A", "mapx": "127.1", "mapy": "37.5", "modifiedtime": "20260802030405", "addr1": "비공개 주소", "tel": "비공개 전화" },
                    { "contentid": "1002", "contenttypeid": "14", "title": "관광지 B", "mapx": "129.1", "mapy": "35.1", "modifiedtime": "20260801000000" }
                  ] }
                }
              }
            }
            """);

    private static 공공데이터포털업무ApiResponse OnlinePriceResponse()
        => SuccessResponse(
            "item-list",
            """
            { "body": { "items": { "item": [{ "ic": "A01101", "in": "쌀", "ed": "2024-12-19" }] }, "totalCount": 402 } }
            """);

    private static 공공데이터포털업무ApiResponse KosisListResponse()
        => SuccessResponse(
            "indicator-by-name",
            """
            {
              "response": {
                "header": { "resultCode": "00", "resultMsg": "NORMAL SERVICE." },
                "body": { "items": { "item": [
                  { "statJipyoId": "58", "statJipyoNm": "소비자물가지수", "unit": "2020=100", "endPrdDe": "202605", "prdSeName": "월" },
                  { "statJipyoId": "59", "statJipyoNm": "생활물가지수", "unit": "2020=100" }
                ] } }
              }
            }
            """);

    private static 공공데이터포털업무ApiResponse KosisDetailResponse()
        => SuccessResponse(
            "indicator-detail-by-name",
            """
            {
              "response": {
                "header": { "resultCode": "00", "resultMsg": "NORMAL SERVICE." },
                "body": { "items": { "item": [
                  { "itmNm": "전국", "prdDe": "202604", "prdSe": "M", "statJipyoId": "58", "statJipyoNm": "소비자물가지수", "val": "119.37" },
                  { "itmNm": "전국", "prdDe": "202605", "prdSe": "M", "statJipyoId": "58", "statJipyoNm": "소비자물가지수", "val": "119.92" },
                  { "itmNm": "경기", "prdDe": "202605", "prdSe": "M", "statJipyoId": "434", "statJipyoNm": "소비자물가지수(월)", "val": "118.90" }
                ] } }
              }
            }
            """);

    private static 공공데이터포털업무ApiResponse SuccessResponse(string apiKey, string body)
        => new()
        {
            Success = true,
            ApiKey = apiKey,
            HttpStatusCode = 200,
            Body = body,
            ObservedAt = CollectedAt
        };

    private static 공공데이터포털업무ApiResponse FailureResponse(string apiKey)
        => new()
        {
            Success = false,
            ApiKey = apiKey,
            HttpStatusCode = 503,
            ObservedAt = CollectedAt
        };

    private sealed class StubTourismClient(공공데이터포털업무ApiResponse response)
        : I국문관광정보공공데이터Client
    {
        public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => [];

        public Task<공공데이터포털업무ApiResponse> QueryAsync(
            공공데이터포털업무ApiRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class StubOnlinePriceClient(공공데이터포털업무ApiResponse response)
        : I온라인가격공공데이터Client
    {
        public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => [];

        public Task<공공데이터포털업무ApiResponse> QueryAsync(
            공공데이터포털업무ApiRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class StubKosisClient(
        공공데이터포털업무ApiResponse listResponse,
        공공데이터포털업무ApiResponse detailResponse)
        : IKosis비교자료공공데이터Client
    {
        public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => [];

        public Task<공공데이터포털업무ApiResponse> QueryAsync(
            공공데이터포털업무ApiRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(request.ApiKey == "indicator-by-name"
                ? listResponse
                : detailResponse);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
