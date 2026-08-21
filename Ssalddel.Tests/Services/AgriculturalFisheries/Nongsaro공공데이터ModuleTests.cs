using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Nongsaro공공데이터ModuleTests
{
    [Fact]
    public async Task QueryAsync_출처와기준시각을보존하고Item을구조화한다()
    {
        var handler = new RecordingHandler(_ => XmlResponse(
            """
            <response>
              <header><resultCode>00</resultCode><resultMsg>정상</resultMsg></header>
              <body><items><item><mainCategoryCode>FC</mainCategoryCode><mainCategoryNm>식량작물</mainCategoryNm></item></items></body>
            </response>
            """));
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 8, 6, 12, 0, 0, TimeSpan.Zero));
        var sut = CreateClient(handler, timeProvider);

        var result = await sut.QueryAsync(
            Nongsaro공공데이터Catalog.작목기술Service,
            Nongsaro공공데이터Catalog.작목기술주분류Operation);

        var item = Assert.Single(result.Items);
        Assert.Equal("FC", item.Get("mainCategoryCode"));
        Assert.Equal("식량작물", item.Get("mainCategoryNm"));
        Assert.Equal(timeProvider.GetUtcNow(), result.RetrievedAtUtc);
        Assert.Equal(Nongsaro공공데이터Catalog.DocumentationUrl, result.SourceDocumentationUrl);
        Assert.Equal(64, result.RawContentHashSha256.Length);
        Assert.All(result.RawContentHashSha256, value => Assert.True(Uri.IsHexDigit(value)));
        Assert.Contains("apiKey=test-key", handler.RequestUri?.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 농작업일정Module_품목Code를서버요청으로전달한다()
    {
        var handler = new RecordingHandler(_ => XmlResponse(
            """
            <response><header><resultCode>00</resultCode></header><body><items /></body></response>
            """));
        var client = CreateClient(handler, TimeProvider.System);
        var sut = new 농사로농작업일정Module(client);

        await sut.일정조회Async("210004");

        Assert.Equal(
            "/service/farmWorkingPlanNew/workScheduleLst",
            handler.RequestUri?.AbsolutePath);
        Assert.Contains("kidofcomdtySeCode=210004", handler.RequestUri?.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task QueryAsync_승인되지않은Operation은외부호출전에차단한다()
    {
        var handler = new RecordingHandler(_ => XmlResponse("<response />"));
        var sut = CreateClient(handler, TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.QueryAsync("unknown", "operation"));

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task QueryAsync_Api오류에인증키를포함하지않는다()
    {
        var handler = new RecordingHandler(_ => XmlResponse(
            """
            <response><header><resultCode>30</resultCode><resultMsg>등록되지 않은 키</resultMsg></header></response>
            """));
        var sut = CreateClient(handler, TimeProvider.System);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.QueryAsync(
                Nongsaro공공데이터Catalog.농작물재해예방Service,
                Nongsaro공공데이터Catalog.농작물재해예방연도Operation));

        Assert.DoesNotContain("test-key", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Code=30", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Catalog_다운로드매뉴얼별StableKey와실행경계를구분한다()
    {
        Assert.Equal(
            Nongsaro공공데이터Catalog.Modules.Count,
            Nongsaro공공데이터Catalog.Modules
                .Select(module => module.StableKey)
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Contains(
            Nongsaro공공데이터Catalog.Modules,
            module => module.DisplayName == "지역특산물" && module.Executable);
        Assert.Contains(
            Nongsaro공공데이터Catalog.Modules,
            module => module.DisplayName == "농약판매가격" && !module.Executable);
        Assert.All(
            Nongsaro공공데이터Catalog.Modules,
            module => Assert.Contains("재고", module.Boundary, StringComparison.Ordinal));
    }

    [Fact]
    public async Task 지역문화Module_특산물과계절음식을서로다른서비스로호출한다()
    {
        var handler = new RecordingHandler(_ => XmlResponse(
            """
            <response><header><resultCode>00</resultCode></header><body><items /></body></response>
            """));
        var client = CreateClient(handler, TimeProvider.System);
        var sut = new 농사로지역문화Module(client);

        await sut.지역특산물시도조회Async();
        Assert.Equal(
            "/service/localSpcprd/selectAreaSidoLst",
            handler.RequestUri?.AbsolutePath);

        await sut.이달음식연도조회Async();
        Assert.Equal(
            "/service/monthFd/monthFdYearLst",
            handler.RequestUri?.AbsolutePath);
    }

    private static NongsaroOpenApiClient CreateClient(
        HttpMessageHandler handler,
        TimeProvider timeProvider)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test")
        };
        var options = Options.Create(new PublicDataOptions
        {
            Nongsaro = new NongsaroOpenApiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "https://api.example.test"
            }
        });
        return new NongsaroOpenApiClient(httpClient, options, timeProvider);
    }

    private static HttpResponseMessage XmlResponse(string xml)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(xml, Encoding.UTF8, "text/xml")
        };

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            RequestUri = request.RequestUri;
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
