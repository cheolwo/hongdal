using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 공공데이터포털업무ApiClientTests
{
    [Fact]
    public void 수협Client_로그인포털에서확인한11개Api를각각분리한다()
    {
        var sut = CreateFishClient(new StubHttpMessageHandler(_ => JsonResponse("{}")), "test-key");

        Assert.Equal(11, sut.Apis.Count);
        Assert.Equal(11, sut.Apis.Select(item => item.DefaultOperationPath).Distinct().Count());
        Assert.All(sut.Apis, item => Assert.StartsWith("/1192000/", item.DefaultOperationPath));
    }

    [Fact]
    public void 공동주택Client_10개Api와활성버전경로를제공한다()
    {
        var sut = CreateApartmentClient(new StubHttpMessageHandler(_ => JsonResponse("{}")), "test-key");

        Assert.Equal(10, sut.Apis.Count);
        Assert.Contains(sut.Apis, item =>
            item.Key == "complex-basic"
            && item.DefaultOperationPath == "/1613000/AptBasisInfoServiceV4/getAphusBassInfoV4");
        Assert.Contains(sut.Apis, item => item.Key == "maintenance-history");
        Assert.Contains(sut.Apis, item => item.Key == "energy-use");
    }

    [Fact]
    public async Task QueryAsync_공통키를추가하고호출자가주입한키를무시한다()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse("{\"response\":{\"body\":{}}}");
        });
        var sut = CreateApartmentClient(handler, "stored-key");

        var result = await sut.QueryAsync(new 공공데이터포털업무ApiRequest
        {
            ApiKey = "energy-use",
            OperationPath = "/1613000/ApHusEnergyUseInfoOfferServiceV2/getHsmpAvrgEnergyUseAmountInfoSearchV2",
            Parameters = new Dictionary<string, string?>
            {
                ["kaptCode"] = "A123",
                ["serviceKey"] = "caller-key"
            }
        });

        Assert.True(result.Success);
        Assert.NotNull(requestedUri);
        Assert.Contains("kaptCode=A123", requestedUri!.Query, StringComparison.Ordinal);
        Assert.Contains("serviceKey=stored-key", requestedUri.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("caller-key", requestedUri.Query, StringComparison.Ordinal);
        Assert.Equal("/1613000/ApHusEnergyUseInfoOfferServiceV2/getHsmpAvrgEnergyUseAmountInfoSearchV2", requestedUri.AbsolutePath);
    }

    [Fact]
    public async Task QueryAsync_다른업무모듈경로는외부호출전에차단한다()
    {
        var requested = false;
        var handler = new StubHttpMessageHandler(_ =>
        {
            requested = true;
            return JsonResponse("{}");
        });
        var sut = CreateFishClient(handler, "stored-key");

        await Assert.ThrowsAsync<ArgumentException>(() => sut.QueryAsync(new 공공데이터포털업무ApiRequest
        {
            ApiKey = "warehouse",
            OperationPath = "/1613000/AptListService3/getTotalAptList3"
        }));

        Assert.False(requested);
    }

    [Fact]
    public async Task QueryAsync_공통키가없으면외부호출전에실패한다()
    {
        var requested = false;
        var handler = new StubHttpMessageHandler(_ =>
        {
            requested = true;
            return JsonResponse("{}");
        });
        var sut = CreateFishClient(handler, string.Empty);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.QueryAsync(
            new 공공데이터포털업무ApiRequest { ApiKey = "local-cooperative" }));

        Assert.False(requested);
        Assert.Contains("DataGoKrServiceKey", exception.Message, StringComparison.Ordinal);
    }

    private static 수협유통공공데이터Client CreateFishClient(HttpMessageHandler handler, string serviceKey)
        => new(
            CreateHttpClient(handler),
            Options.Create(new PublicDataOptions { DataGoKrServiceKey = serviceKey }));

    private static 공동주택운영공공데이터Client CreateApartmentClient(
        HttpMessageHandler handler,
        string serviceKey)
        => new(
            CreateHttpClient(handler),
            Options.Create(new PublicDataOptions { DataGoKrServiceKey = serviceKey }));

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("https://apis.data.go.kr/") };

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
