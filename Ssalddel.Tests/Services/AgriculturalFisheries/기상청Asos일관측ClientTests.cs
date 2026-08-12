using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class 기상청Asos일관측ClientTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 11, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 일관측값과_단위_원문Hash를_보존한다()
    {
        const string payload = """
            {"response":{"header":{"resultCode":"00","resultMsg":"NORMAL_SERVICE"},"body":{"items":{"item":[{"tm":"2026-08-10","stnId":"108","stnNm":"서울","avgTa":"27.1","minTa":"23.4","maxTa":"31.2","sumRn":"4.5","sumGsr":"18.63","sumSsHr":"7.1","ssDur":"13.6","avgRhm":"71.3"}]},"totalCount":1}}}
            """;
        var sut = CreateClient(payload, out var handler);

        var result = await sut.조회Async(new 기상청Asos일관측Query(
            new DateOnly(2026, 8, 10),
            "108"));

        Assert.Equal("weather-observation:kma.asos.108.20260810", result.StableId);
        Assert.Equal(기상관측품질Codes.Valid, result.QualityCode);
        Assert.True(result.CanUseForSimulation);
        Assert.Equal(27.1m, result.MeanTemperatureC);
        Assert.Equal(4.5m, result.DailyPrecipitationMm);
        Assert.Equal(18.63m, result.TotalSolarRadiationMjPerSquareMeter);
        Assert.Equal("MJ/m²", result.Units.SolarRadiation);
        Assert.Equal(
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant(),
            result.RawPayloadHashSha256);
        Assert.Contains("serviceKey=secret-key", handler.RequestUri!.Query);
        Assert.Contains("stnIds=108", handler.RequestUri.Query);
    }

    [Fact]
    public async Task 결측은_0으로_바꾸지_않고_사용을_차단한다()
    {
        const string payload = """
            {"response":{"header":{"resultCode":"00"},"body":{"items":{"item":{"tm":"20260810","stnId":108,"stnNm":"서울","avgTa":"27.1","minTa":"23.4","maxTa":"31.2","sumRn":"","sumGsr":null,"sumSsHr":"7.1","ssDur":"13.6","avgRhm":"71.3"}},"totalCount":1}}}
            """;
        var sut = CreateClient(payload, out _);

        var result = await sut.조회Async(new 기상청Asos일관측Query(
            new DateOnly(2026, 8, 10),
            "108"));

        Assert.Null(result.DailyPrecipitationMm);
        Assert.Null(result.TotalSolarRadiationMjPerSquareMeter);
        Assert.Equal(기상관측품질Codes.Incomplete, result.QualityCode);
        Assert.False(result.CanUseForSimulation);
        Assert.Equal(["sumRn", "sumGsr"], result.MissingFieldCodes);
    }

    [Fact]
    public async Task 농장과_관측소_좌표가_있으면_지점거리를_기록한다()
    {
        var sut = CreateClient(CompletePayload(), out _);
        var spatialContext = new 기상청Asos공간Context(
            "farm-field:seoul.demo.001",
            37.5665m,
            126.9780m,
            37.5714m,
            126.9658m,
            "https://example.test/official-station-metadata");

        var result = await sut.조회Async(new 기상청Asos일관측Query(
            new DateOnly(2026, 8, 10),
            "108",
            spatialContext));

        Assert.Equal("farm-field:seoul.demo.001", result.TargetLocationStableId);
        Assert.InRange(result.StationDistanceKm!.Value, 1.1m, 1.3m);
        Assert.Contains(result.Limitations, item =>
            item.Contains(spatialContext.StationMetadataSourceHref, StringComparison.Ordinal));
    }

    [Fact]
    public async Task 요청한_날짜와_관측소의_단일_기록만_선택한다()
    {
        const string payload = """
            {"response":{"header":{"resultCode":"00"},"body":{"items":{"item":[
              {"tm":"2026-08-09","stnId":"108","stnNm":"서울","avgTa":"1","minTa":"1","maxTa":"1","sumRn":"1","sumGsr":"1","sumSsHr":"1","ssDur":"1","avgRhm":"1"},
              {"tm":"2026-08-10","stnId":"108","stnNm":"서울","avgTa":"27.1","minTa":"23.4","maxTa":"31.2","sumRn":"4.5","sumGsr":"18.63","sumSsHr":"7.1","ssDur":"13.6","avgRhm":"71.3"},
              {"tm":"2026-08-10","stnId":"159","stnNm":"부산","avgTa":"2","minTa":"2","maxTa":"2","sumRn":"2","sumGsr":"2","sumSsHr":"2","ssDur":"2","avgRhm":"2"}
            ]},"totalCount":3}}}
            """;
        var sut = CreateClient(payload, out _);

        var result = await sut.조회Async(new 기상청Asos일관측Query(
            new DateOnly(2026, 8, 10),
            "108"));

        Assert.Equal(27.1m, result.MeanTemperatureC);
        Assert.Equal("서울", result.StationName);
    }

    [Fact]
    public async Task 현재일_또는_미래_관측은_외부호출_전에_차단한다()
    {
        var sut = CreateClient(CompletePayload(), out var handler);

        var error = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.조회Async(new 기상청Asos일관측Query(
                new DateOnly(2026, 8, 11),
                "108")));

        Assert.Contains("KmaAsosObservationDateUnavailable", error.Message);
        Assert.Null(handler.RequestUri);
    }

    [Fact]
    public async Task 공식Api_오류를_샘플값으로_숨기지_않는다()
    {
        const string payload = """
            {"response":{"header":{"resultCode":"30","resultMsg":"SERVICE_KEY_IS_NOT_REGISTERED_ERROR"},"body":{}}}
            """;
        var sut = CreateClient(payload, out _);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.조회Async(new 기상청Asos일관측Query(
                new DateOnly(2026, 8, 10),
                "108")));

        Assert.Equal("KmaAsosRemoteFailure:30", error.Message);
    }

    private static 기상청Asos일관측Client CreateClient(
        string payload,
        out RecordingHandler handler)
    {
        handler = new RecordingHandler(payload);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://apis.data.go.kr")
        };
        var options = Options.Create(new PublicDataOptions
        {
            DataGoKrServiceKey = "secret-key"
        });
        return new 기상청Asos일관측Client(
            httpClient,
            options,
            new FixedTimeProvider(Now));
    }

    private static string CompletePayload() => """
        {"response":{"header":{"resultCode":"00"},"body":{"items":{"item":[{"tm":"2026-08-10","stnId":"108","stnNm":"서울","avgTa":"27.1","minTa":"23.4","maxTa":"31.2","sumRn":"4.5","sumGsr":"18.63","sumSsHr":"7.1","ssDur":"13.6","avgRhm":"71.3"}]},"totalCount":1}}}
        """;

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class RecordingHandler(string payload) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        }
    }
}
