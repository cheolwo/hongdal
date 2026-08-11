using System.IO.Compression;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 대한민국법정동CodeIngestionTests
{
    [Fact]
    public void 출처등록은_공식전체자료와기준정보한계를기록한다()
    {
        var source = Assert.Single(new 대한민국법정동CodeSourceRegistration().GetDefinitions());

        Assert.Equal("mois-standard-codes", source.SourceId);
        Assert.Equal("korea-legal-dong-codes", source.DatasetId);
        Assert.Equal(ExternalDataAccessMethod.DownloadFile, source.AccessMethod);
        Assert.Equal(ExternalDataCredentialType.None, source.CredentialType);
        Assert.False(source.RequiresCredential);
        Assert.False(source.DefaultCollectionEnabled);
        Assert.False(source.RedistributionAllowed);
        Assert.Contains("경계", source.UsageLimitations, StringComparison.Ordinal);
        Assert.Contains("농장", source.UsageLimitations, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 수집기는_공식다운로드요청과원천형식을보존한다()
    {
        HttpMethod? capturedMethod = null;
        Uri? capturedUri = null;
        string? capturedBody = null;
        var archive = CreateArchive(
            "법정동코드\t법정동명\t폐지여부\n" +
            "1100000000\t서울특별시\t존재\n" +
            "1111000000\t서울특별시 종로구\t존재\n");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(archive),
        };
        response.Headers.Date = DateTimeOffset.Parse("2026-08-11T00:00:00Z");
        var collector = new 대한민국법정동CodeCollector(
            new HttpClient(new StubHttpMessageHandler(request =>
            {
                capturedMethod = request.Method;
                capturedUri = request.RequestUri;
                capturedBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                return response;
            })),
            Options.Create(new 대한민국법정동CodeOptions()),
            TimeProvider.System);

        await using var payload = await collector.CollectAsync(Source(), Request(), credential: null);

        Assert.Equal(HttpMethod.Post, capturedMethod);
        Assert.Equal("/etc/codeFullDown.do", capturedUri!.AbsolutePath);
        Assert.Equal("codeseId=%EB%B2%95%EC%A0%95%EB%8F%99%EC%BD%94%EB%93%9C",
            capturedBody);
        Assert.Equal("application/zip", payload.ContentType);
        Assert.Equal(2, payload.FetchedCount);
        Assert.Null(payload.EvidenceAsOfUtc);
        Assert.Equal("moi-standard-code:retrieved:2026-08-11", payload.SourceVersion);
    }

    [Fact]
    public async Task 정규화는_행정계층과상위법정동을명시한다()
    {
        var raw = Raw();
        var normalizer = new 대한민국법정동CodeNormalizer(
            Options.Create(new 대한민국법정동CodeOptions()));
        var archive = CreateArchive(
            "법정동코드\t법정동명\t폐지여부\n" +
            "1100000000\t서울특별시\t존재\n" +
            "1111000000\t서울특별시 종로구\t존재\n" +
            "1111010100\t서울특별시 종로구 청운동\t존재\n" +
            "1111010101\t서울특별시 종로구 청운동 시험리\t폐지\n" +
            "3611000000\t세종특별자치시\t존재\n");

        var result = await normalizer.NormalizeAsync(
            Source(),
            raw,
            new InMemoryRawStorage(archive));

        Assert.Equal(5, result.Records.Count);
        Assert.Equal(0, result.RejectedCount);
        var province = result.Records.Single(item => item.StableId.EndsWith("1100000000", StringComparison.Ordinal));
        Assert.Equal("province", province.SpatialPrecisionCode);
        Assert.Contains("status=active", province.DimensionKey, StringComparison.Ordinal);
        Assert.Contains("parent=", province.DimensionKey, StringComparison.Ordinal);
        var district = result.Records.Single(item => item.StableId.EndsWith("1111000000", StringComparison.Ordinal));
        Assert.Equal("city-county-district", district.SpatialPrecisionCode);
        Assert.Contains("parent=region:kr:bjd:1100000000", district.DimensionKey, StringComparison.Ordinal);
        var abolished = result.Records.Single(item => item.StableId.EndsWith("1111010101", StringComparison.Ordinal));
        Assert.Equal("village", abolished.SpatialPrecisionCode);
        Assert.Contains("status=abolished", abolished.DimensionKey, StringComparison.Ordinal);
        Assert.Equal("collection-time", abolished.TemporalPrecisionCode);
        Assert.Equal("source-effective-date-and-boundary-not-included", abolished.LimitationCode);
        var sejong = result.Records.Single(item => item.StableId.EndsWith("3611000000", StringComparison.Ordinal));
        Assert.Equal("province", sejong.SpatialPrecisionCode);
        Assert.Contains("parent=", sejong.DimensionKey, StringComparison.Ordinal);
        ExternalDataNormalizationValidator.Validate(Source(), raw, result);
    }

    [Fact]
    public async Task 정규화RecordKey는_수집시각이달라도법정동별로유지된다()
    {
        var archive = CreateArchive(
            "법정동코드\t법정동명\t폐지여부\n" +
            "4100000000\t경기도\t존재\n");
        var normalizer = new 대한민국법정동CodeNormalizer(
            Options.Create(new 대한민국법정동CodeOptions()));
        var first = await normalizer.NormalizeAsync(
            Source(), Raw(17, "2026-08-11T00:00:00Z"), new InMemoryRawStorage(archive));
        var second = await normalizer.NormalizeAsync(
            Source(), Raw(18, "2026-08-18T00:00:00Z"), new InMemoryRawStorage(archive));

        Assert.Equal(Assert.Single(first.Records).RecordKey, Assert.Single(second.Records).RecordKey);
        Assert.NotEqual(Assert.Single(first.Records).EvidenceAsOfUtc, Assert.Single(second.Records).EvidenceAsOfUtc);
    }

    [Theory]
    [InlineData("법정동코드\t법정동명\t폐지여부\n잘못된코드\t서울특별시\t존재\n")]
    [InlineData("법정동코드\t법정동명\t폐지여부\n1100000000\t서울특별시\t알수없음\n")]
    public async Task 정규화는_잘못된공식자료형식을거부한다(string text)
    {
        var normalizer = new 대한민국법정동CodeNormalizer(
            Options.Create(new 대한민국법정동CodeOptions()));

        var error = await Assert.ThrowsAsync<ExternalDataCollectionException>(() =>
            normalizer.NormalizeAsync(Source(), Raw(), new InMemoryRawStorage(CreateArchive(text))));

        Assert.Equal(ExternalDataCollectionErrorCode.InvalidPayload, error.ErrorCode);
    }

    [Fact]
    [Trait("Category", "LivePublicData")]
    public async Task 실제공식전체자료는_수집하고정규화할수있다()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("SSALDDEL_LIVE_PUBLIC_DATA"),
                "1",
                StringComparison.Ordinal))
            return;

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Ssalddel-Korea-PublicData-Test/1.0");
        var collector = new 대한민국법정동CodeCollector(
            httpClient,
            Options.Create(new 대한민국법정동CodeOptions()),
            TimeProvider.System);
        await using var payload = await collector.CollectAsync(Source(), Request(), credential: null);
        using var archive = new MemoryStream();
        await payload.Content.CopyToAsync(archive);
        var bytes = archive.ToArray();
        var raw = Raw();
        raw.SourceVersion = payload.SourceVersion;
        raw.ContentHashSha256 = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(bytes))
            .ToLowerInvariant();
        var normalized = await new 대한민국법정동CodeNormalizer(
                Options.Create(new 대한민국법정동CodeOptions()))
            .NormalizeAsync(Source(), raw, new InMemoryRawStorage(bytes));

        Assert.True(payload.FetchedCount > 20_000);
        Assert.Equal(payload.FetchedCount, normalized.Records.Count);
        Assert.Contains(normalized.Records, item =>
            item.StableId == "region:kr:bjd:4100000000"
            && item.TextValue == "경기도"
            && item.SpatialPrecisionCode == "province");
        ExternalDataNormalizationValidator.Validate(Source(), raw, normalized);
    }

    private static ExternalDataSourceDefinition Source()
        => Assert.Single(new 대한민국법정동CodeSourceRegistration().GetDefinitions());

    private static ExternalDataIngestionRequest Request() => new()
    {
        SourceId = 대한민국법정동CodeDataset.SourceId,
        DatasetId = 대한민국법정동CodeDataset.DatasetId,
        RunKey = "korea-legal-dong-test",
    };

    private static 외부데이터RawSnapshot Raw(
        long id = 17,
        string collectedAt = "2026-08-11T00:00:00Z") => new()
    {
        Id = id,
        SourceId = 대한민국법정동CodeDataset.SourceId,
        DatasetId = 대한민국법정동CodeDataset.DatasetId,
        SourceVersion = "moi-standard-code:retrieved:2026-08-11",
        ContentHashSha256 = new string('a', 64),
        CollectedAtUtc = DateTimeOffset.Parse(collectedAt),
    };

    private static byte[] CreateArchive(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("법정동코드 전체자료.txt");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, Encoding.GetEncoding(949));
            writer.Write(text);
        }
        return output.ToArray();
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }

    private sealed class InMemoryRawStorage(byte[] bytes) : IExternalDataRawStorage
    {
        public Task<ExternalDataRawStorageResult> StoreAsync(
            ExternalDataSourceDefinition source,
            ExternalDataCollectedPayload payload,
            DateTimeOffset collectedAtUtc,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Stream> OpenReadAsync(
            외부데이터RawSnapshot snapshot,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
    }
}
