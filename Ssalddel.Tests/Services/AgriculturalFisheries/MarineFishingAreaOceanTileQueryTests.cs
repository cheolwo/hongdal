using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Extensions;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class MarineFishingAreaOceanTileQueryTests
{
    [Fact]
    public void 농수산정보Module은_공식수집기와바다TileUseCase를등록한다()
    {
        var services = new ServiceCollection();

        services.AddAgriculturalFisheriesInformationModule();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IMof어획구역CatalogSource)
            && descriptor.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(I해양수산Map바다Tile조회UseCase)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task 공식Cp949Csv를_한번수집하고_원문해시와함께Cache한다()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var csv = "영어약자,구역영어명,구역한글명,바다\r\n"
                  + "FAO61,Northwest Pacific,북서태평양,태평양\r\n"
                  + "FAO37,\"Mediterranean, Black Sea\",지중해/흑해,지중해\r\n";
        var requestCount = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            requestCount++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.GetEncoding(949).GetBytes(csv))
            };
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.data.go.kr/")
        };
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var source = new Mof어획구역CatalogSource(
            httpClient,
            cache,
            Options.Create(new PublicDataOptions()),
            TimeProvider.System);

        var first = await source.수집Async();
        var second = await source.수집Async();

        Assert.Equal(1, requestCount);
        Assert.Same(first, second);
        Assert.Equal(2, first.Records.Count);
        Assert.Equal("북서태평양", first.Records[0].KoreanName);
        Assert.Equal("Mediterranean, Black Sea", first.Records[1].EnglishName);
        Assert.Equal(64, first.ContentSha256.Length);
    }

    [Fact]
    public async Task 바다Tile조회는_공식바다분류만집계하고_좌표없는한계를반환한다()
    {
        var records = new[]
        {
            new Mof어획구역CatalogRecord("A", "Pacific A", "태평양 A", "태평양"),
            new Mof어획구역CatalogRecord("B", "Pacific B", "태평양 B", "태평양"),
            new Mof어획구역CatalogRecord("C", "Atlantic", "대서양 A", "대서양"),
            new Mof어획구역CatalogRecord("D", "Indian", "인도양 A", "인도양"),
            new Mof어획구역CatalogRecord("E", "Mediterranean", "지중해 A", "지중해"),
            new Mof어획구역CatalogRecord("F", "Bering", "베링해 A", "베링해"),
            new Mof어획구역CatalogRecord("G", "Arctic", "북극 A", "북국해"),
            new Mof어획구역CatalogRecord("H", "Antarctic", "남극 A", "남극수역"),
            new Mof어획구역CatalogRecord("TEST", "TEST", "TEST", "TEST"),
            new Mof어획구역CatalogRecord("", "Unknown", "불명", "")
        };
        var useCase = new 해양수산Map바다Tile조회UseCase(
            new FakeCatalogSource(records),
            Options.Create(new PublicDataOptions()));

        var response = await useCase.조회Async();

        Assert.Equal("mof-fishing-area-catalog", response.SourceKey);
        Assert.Equal(10, response.SourceRowCount);
        Assert.Equal(8, response.MappedFishingAreaCount);
        Assert.Equal(2, response.ExcludedRowCount);
        Assert.Equal(
            MarineFishingAreaGeometryBasisCodes.SchematicOceanCatalogLayout,
            response.GeometryBasisCode);
        Assert.Equal(7, response.Items.Count);
        Assert.Equal(2, response.Items.Single(tile => tile.TileKey == "pacific").FishingAreaCount);
        Assert.Contains("좌표·경계", response.Notices[0]);
        Assert.Contains("실제 조업 위치", response.Notices[1]);
    }

    private sealed class FakeCatalogSource(
        IReadOnlyList<Mof어획구역CatalogRecord> records) : IMof어획구역CatalogSource
    {
        public Task<Mof어획구역CatalogSnapshot> 수집Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(new Mof어획구역CatalogSnapshot(
                new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc),
                new string('a', 64),
                records));
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
