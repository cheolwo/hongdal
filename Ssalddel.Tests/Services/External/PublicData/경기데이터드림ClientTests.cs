using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 경기데이터드림ClientTests
{
    [Fact]
    public async Task ApiClient_저장된키와표준페이지매개변수를사용하고호출자키를무시한다()
    {
        Uri? requestedUri = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse("{\"LivestockBreeding\":[]}");
        });
        var sut = CreateApiClient(handler, "stored-key");

        var result = await sut.QueryAsync(new 경기데이터드림ApiRequest
        {
            DatasetPath = "LivestockBreeding",
            Page = 2,
            PageSize = 50,
            Parameters = new Dictionary<string, string?>
            {
                ["KEY"] = "caller-key",
                ["Type"] = "xml",
                ["SIGUN_CD"] = "41110"
            }
        });

        Assert.True(result.Success);
        Assert.NotNull(requestedUri);
        Assert.Equal("/LivestockBreeding", requestedUri!.AbsolutePath);
        Assert.Contains("KEY=stored-key", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("Type=json", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("pIndex=2", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("pSize=50", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("SIGUN_CD=41110", requestedUri.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("caller-key", requestedUri.Query, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("../secret")]
    [InlineData("https://example.com/api")]
    [InlineData("Livestock-Breeding")]
    public async Task ApiClient_외부경로또는허용되지않은DatasetPath를거부한다(string datasetPath)
    {
        var called = false;
        var sut = CreateApiClient(new StubHttpMessageHandler(_ =>
        {
            called = true;
            return JsonResponse("{}");
        }), "stored-key");

        var result = await sut.QueryAsync(new 경기데이터드림ApiRequest
        {
            DatasetPath = datasetPath
        });

        Assert.False(result.Success);
        Assert.False(called);
    }

    [Fact]
    public async Task CatalogClient_API항목만수집하고InfId중복을제거해분류한다()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            var page = request.RequestUri!.Query.Contains("page=2", StringComparison.Ordinal) ? 2 : 1;
            return JsonResponse(page == 1
                ? CatalogPage(1, 2,
                    CatalogRow("livestock", "가축 사육업체 현황", "축산 농가", 3),
                    CatalogRow("sheet-only", "농촌 사진", "API 없음", null))
                : CatalogPage(2, 2,
                    CatalogRow("livestock", "가축 사육업체 현황", "중복", 3),
                    CatalogRow("fish", "수산물 안전성 검사결과", "양식 수산물 검사", 2)));
        });
        var sut = new 경기데이터드림CatalogClient(
            CreateHttpClient(handler, "https://data.gg.go.kr/"),
            TimeProvider.System);

        var result = await sut.GetAgricultureLivestockFisheriesAsync();

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(10, result.Modules.Count);
        Assert.Contains(result.Items, item =>
            item.InfId == "livestock"
            && item.ModuleKey == 경기데이터드림ModuleKeys.축산농장사육);
        Assert.Contains(result.Items, item =>
            item.InfId == "fish"
            && item.ModuleKey == 경기데이터드림ModuleKeys.수산양식안전);
        Assert.DoesNotContain(result.Items, item => item.InfId == "sheet-only");
    }

    [Fact]
    public void CatalogClient_기관명의농수산표현만으로수산모듈에넣지않는다()
    {
        var moduleKey = 경기데이터드림CatalogClient.Classify(
            "경기도농수산진흥원 도민텃밭현황",
            "도민에게 제공하는 농업 체험용 텃밭 현황");

        Assert.Equal(경기데이터드림ModuleKeys.농산생산인증유통, moduleKey);
    }

    [Fact]
    public async Task 가축사육집계Client_민감필드를버리고시군과상태만집계한다()
    {
        const string body = """
            {
              "LivestockBreeding": [
                { "head": [{ "list_total_count": 3 }] },
                { "row": [
                  { "SIGUN_CD": "41110", "SIGUN_NM": "수원시", "BSN_STATE_NM": "영업", "BIZPLC_NM": "농장A", "REFINE_ROADNM_ADDR": "비공개 주소", "REFINE_WGS84_LAT": "37.1" },
                  { "SIGUN_CD": "41110", "SIGUN_NM": "수원시", "BSN_STATE_NM": "영업", "BIZPLC_NM": "농장B", "LOCPLC_FACLT_TELNO": "010-0000-0000" },
                  { "SIGUN_CD": "41280", "SIGUN_NM": "고양시", "BSN_STATE_NM": "폐업", "STOCKRS_IDNTFY_NO": "secret-id" }
                ] }
              ]
            }
            """;
        var source = new Stub경기데이터드림ApiClient(new 경기데이터드림ApiResponse
        {
            Success = true,
            HttpStatusCode = 200,
            Body = body,
            ObservedAt = DateTimeOffset.Parse("2026-08-03T00:00:00Z")
        });
        var sut = new 경기데이터드림가축사육집계Client(source);

        var result = await sut.QueryAsync();

        Assert.True(result.Success);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, item =>
            item.RegionName == "수원시"
            && item.BusinessStatus == "영업"
            && item.BusinessCount == 2);
        var serialized = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.DoesNotContain("농장A", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("비공개 주소", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-id", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 가축사육집계Client_전체페이지를조회하고_시군미제공원문을경기도범위로집계한다()
    {
        var source = new PagedStub경기데이터드림ApiClient(request =>
            new 경기데이터드림ApiResponse
            {
                Success = true,
                HttpStatusCode = 200,
                Body = request.Page == 1
                    ? """
                      { "LivestockBreeding": [
                        { "head": [{ "list_total_count": 2 }] },
                        { "row": [{ "SIGUN_CD": "", "BSN_STATE_NM": "영업", "BIZPLC_NM": "discard-a", "LOCPLC_FACLT_TELNO": "discard-phone" }] }
                      ] }
                      """
                    : """
                      { "LivestockBreeding": [
                        { "head": [{ "list_total_count": 2 }] },
                        { "row": [{ "SIGUN_CD": "", "BSN_STATE_NM": "폐업", "RIGHT_MAINBD_IDNTFY_NO": "discard-id", "X_CRDNT_VL": "discard-x" }] }
                      ] }
                      """,
                ObservedAt = DateTimeOffset.Parse("2026-08-03T00:00:00Z")
            });
        var sut = new 경기데이터드림가축사육집계Client(source);

        var result = await sut.QueryAsync();

        Assert.True(result.Success);
        Assert.Equal([1, 2], source.RequestedPages);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item =>
        {
            Assert.Equal(경기데이터드림가축사육집계Client.DatasetScopeRegionCode, item.RegionCode);
            Assert.Equal(경기데이터드림가축사육집계Client.DatasetScopeRegionName, item.RegionName);
            Assert.Equal(1, item.BusinessCount);
        });
        var serialized = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.DoesNotContain("discard", serialized, StringComparison.Ordinal);
    }

    private static 경기데이터드림ApiClient CreateApiClient(
        HttpMessageHandler handler,
        string apiKey)
        => new(
            CreateHttpClient(handler, "https://openapi.gg.go.kr/"),
            Options.Create(new PublicDataOptions
            {
                GyeonggiDataDream = new GyeonggiDataDreamOptions
                {
                    ApiKey = apiKey,
                    PageSize = 1000
                }
            }),
            TimeProvider.System);

    private static HttpClient CreateHttpClient(HttpMessageHandler handler, string baseUrl)
        => new(handler) { BaseAddress = new Uri(baseUrl) };

    private static HttpResponseMessage JsonResponse(string body)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static string CatalogPage(int currentPage, int totalPages, params string[] rows)
        => $$"""
            {
              "result": {
                "pageInfo": { "totalPages": {{totalPages}}, "currentPage": {{currentPage}} },
                "contents": [{{string.Join(',', rows)}}]
              }
            }
            """;

    private static string CatalogRow(
        string infId,
        string displayName,
        string description,
        int? apiInfSeq)
        => $$"""
            {
              "infId": "{{infId}}",
              "infSeq": 1,
              "acolInfSeq": {{(apiInfSeq?.ToString() ?? "null")}},
              "infNm": "{{displayName}}",
              "infExp": "{{description}}",
              "topCateNm2": "농업·농촌",
              "regDttm": "2026-08-01",
              "updDttm": "2026-08-02"
            }
            """;

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }

    private sealed class Stub경기데이터드림ApiClient : I경기데이터드림ApiClient
    {
        private readonly 경기데이터드림ApiResponse _response;

        public Stub경기데이터드림ApiClient(경기데이터드림ApiResponse response)
        {
            _response = response;
        }

        public Task<경기데이터드림ApiResponse> QueryAsync(
            경기데이터드림ApiRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_response);
    }

    private sealed class PagedStub경기데이터드림ApiClient(
        Func<경기데이터드림ApiRequest, 경기데이터드림ApiResponse> responseFactory)
        : I경기데이터드림ApiClient
    {
        public List<int> RequestedPages { get; } = [];

        public Task<경기데이터드림ApiResponse> QueryAsync(
            경기데이터드림ApiRequest request,
            CancellationToken cancellationToken = default)
        {
            RequestedPages.Add(request.Page);
            return Task.FromResult(responseFactory(request));
        }
    }
}
