using System.IO.Compression;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class 국제농수산가격ArchiveServiceTests
{
    private static readonly DateTime CollectedAtUtc =
        new(2026, 7, 29, 1, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void 정보원목록은_키없는캐나다소매와Eurostat생산자가격을구분한다()
    {
        using var db = CreateDb();
        var service = CreateService(
            db,
            new FakeProvider(
                국제농수산가격SourceKeys.StatCan소비자평균소매가격,
                () => []),
            new FakeProvider(
                국제농수산가격SourceKeys.Eurostat농산물절대생산자가격,
                () => []));

        var sources = service.GetSources();

        Assert.Equal(2, sources.Count);
        Assert.Contains(sources, source =>
            source.SourceKey == 국제농수산가격SourceKeys.StatCan소비자평균소매가격
            && source.CountryScopeCode == "CA"
            && source.FrequencyCode == "Monthly"
            && !source.RequiresCredential);
        Assert.Contains(sources, source =>
            source.SourceKey == 국제농수산가격SourceKeys.Eurostat농산물절대생산자가격
            && source.CountryScopeCode == "EU"
            && source.FrequencyCode == "Annual"
            && !source.RequiresCredential);
    }

    [Fact]
    public async Task 같은공식관측을재수집하면_RecordKey기준으로중복저장하지않는다()
    {
        await using var db = CreateDb();
        var provider = new FakeProvider(
            국제농수산가격SourceKeys.StatCan소비자평균소매가격,
            () =>
            [
                CreateObservation(
                    "statcan:18100245:v1:202605",
                    국제농수산가격SourceKeys.StatCan소비자평균소매가격,
                    "18100245",
                    "CA",
                    new DateOnly(2026, 5, 1),
                    18.25m,
                    "CAD",
                    "per kilogram")
            ]);
        var service = CreateService(db, provider);
        var request = new 국제농수산가격수집요청
        {
            SourceKey = provider.SourceKey,
            YearFrom = 2026,
            YearTo = 2026
        };

        var first = await service.CollectAsync(request);
        var second = await service.CollectAsync(request);
        var archive = await service.GetArchiveAsync(new 국제농수산가격ArchiveQuery
        {
            SourceKey = provider.SourceKey,
            YearFrom = 2026,
            YearTo = 2026
        });

        Assert.Equal(1, first.InsertedCount);
        Assert.Equal(0, first.ExistingCount);
        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(1, second.ExistingCount);
        Assert.Single(await db.InternationalPriceObservations.ToArrayAsync());
        Assert.Equal(2, await db.InternationalPriceCollectionRuns.CountAsync());
        Assert.Equal(국제농수산가격상태Codes.완료, archive.StatusCode);
        var item = Assert.Single(archive.Items);
        Assert.Equal("CAD", item.CurrencyCode);
        Assert.Equal("per kilogram", item.OriginalUnit);
        Assert.Equal(농수산시세시장단계Codes.소비자평균소매, item.MarketStageCode);
    }

    [Fact]
    public async Task 공급자실패는_실패Run만남기고_관측을부분저장하지않는다()
    {
        await using var db = CreateDb();
        var provider = new FakeProvider(
            국제농수산가격SourceKeys.Eurostat농산물절대생산자가격,
            () => throw new InvalidOperationException("official source unavailable"));
        var service = CreateService(db, provider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CollectAsync(new 국제농수산가격수집요청
            {
                SourceKey = provider.SourceKey,
                YearFrom = 2024,
                YearTo = 2024
            }));

        Assert.Contains("official source unavailable", exception.Message);
        Assert.Empty(await db.InternationalPriceObservations.ToArrayAsync());
        var run = await db.InternationalPriceCollectionRuns.SingleAsync();
        Assert.Equal(국제농수산가격Archive상태Codes.실패, run.StatusCode);
        Assert.Contains("official source unavailable", run.ErrorMessage);
    }

    [Fact]
    public async Task StatCan공급자는_ZipCsv를읽고_식품아닌4개품목군을제외한다()
    {
        var csv = """
                  REF_DATE,GEO,DGUID,Products,UOM,VECTOR,COORDINATE,VALUE,STATUS,SYMBOL,TERMINATED,DECIMALS
                  2026-05,Canada,2021A000011124,"Beef stewing cuts, per kilogram",dollars,v123,1.1.0,18.25,,,,2
                  2026-05,Canada,2021A000011124,"Deodorant, 85 grams",dollars,v999,1.75.0,7.40,,,,2
                  2025-12,Canada,2021A000011124,"Beef stewing cuts, per kilogram",dollars,v123,1.1.0,17.50,,,,2
                  """;
        var zipBytes = CreateZip(csv);
        var handler = new DelegateHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith(
                    "/getFullTableDownloadCSV/18100245/en",
                    StringComparison.Ordinal))
            {
                return JsonResponse(
                    """{"status":"SUCCESS","object":"https://download.test/18100245-eng.zip"}""");
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(zipBytes)
            };
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www150.statcan.gc.ca/")
        };
        var provider = new StatCan평균소매가격공급자(httpClient);

        var result = await provider.CollectAsync(
            2026,
            2026,
            CollectedAtUtc);

        var observation = Assert.Single(result.Observations);
        Assert.Equal("statcan:18100245:v123:202605", observation.RecordKey);
        Assert.Equal("1", observation.OfficialProductCode);
        Assert.Equal("CAD", observation.CurrencyCode);
        Assert.Equal("per kilogram", observation.OriginalUnit);
        Assert.Equal(18.25m, observation.Price);
        Assert.Contains(result.SourceMessages, message =>
            message.Contains("1개를 제외", StringComparison.Ordinal));
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Eurostat공급자는_두절대가격Dataset의_국가값만원단위로보존한다()
    {
        var handler = new DelegateHandler(request =>
        {
            var isAnimal = request.RequestUri!.AbsolutePath.EndsWith(
                "/apri_ap_anouta",
                StringComparison.Ordinal);
            return JsonResponse(CreateEurostatResponse(
                isAnimal ? "prod_ani" : "prod_veg",
                isAnimal ? "01110000" : "01600000",
                isAnimal
                    ? "Cattle - prices per 100 kg live weight"
                    : "Soft wheat - prices per 100 kg"));
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ec.europa.eu/")
        };
        var provider = new Eurostat농산물절대가격공급자(httpClient);

        var result = await provider.CollectAsync(
            2024,
            2024,
            CollectedAtUtc);

        Assert.Equal(2, result.Observations.Count);
        Assert.All(result.Observations, observation =>
        {
            Assert.Equal("DE", observation.CountryCode);
            Assert.Equal("EUR", observation.CurrencyCode);
            Assert.Equal(new DateOnly(2024, 1, 1), observation.ReferenceDate);
            Assert.Equal(농수산시세시장단계Codes.생산자수취, observation.MarketStageCode);
            Assert.StartsWith("per 100 kg", observation.OriginalUnit);
        });
        Assert.DoesNotContain(result.Observations, observation =>
            observation.GeographyCode == "EU27_2020");
        Assert.All(handler.Requests, request =>
        {
            Assert.Contains("currency=EUR", request.RequestUri!.Query);
            Assert.Contains("sinceTimePeriod=2024", request.RequestUri.Query);
            Assert.DoesNotContain("key=", request.RequestUri.Query, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static 국제농수산가격ArchiveService CreateService(
        AgriculturalFisheriesDbContext db,
        params I국제농수산가격공급자[] providers)
        => new(
            providers,
            db,
            new FixedTimeProvider(new DateTimeOffset(CollectedAtUtc)),
            NullLogger<국제농수산가격ArchiveService>.Instance);

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private static 국제농수산가격관측 CreateObservation(
        string recordKey,
        string sourceKey,
        string datasetCode,
        string countryCode,
        DateOnly referenceDate,
        decimal price,
        string currencyCode,
        string originalUnit)
        => new()
        {
            RecordKey = recordKey,
            SourceKey = sourceKey,
            DatasetCode = datasetCode,
            CountryCode = countryCode,
            CountryName = "Canada",
            GeographyCode = countryCode,
            GeographyName = "Canada",
            MarketStageCode = 농수산시세시장단계Codes.소비자평균소매,
            OfficialSeriesCode = "v1",
            OfficialProductCode = "1",
            ProductNameOriginal = "Beef stewing cuts",
            ReferenceDate = referenceDate,
            FrequencyCode = "Monthly",
            ValueRaw = price.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Price = price,
            CurrencyCode = currencyCode,
            OriginalUnit = originalUnit,
            SourceUrl = "https://official.test/data",
            FirstCollectedAtUtc = CollectedAtUtc,
            LastSeenAtUtc = CollectedAtUtc
        };

    private static byte[] CreateZip(string csv)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("18100245.csv");
            using var writer = new StreamWriter(
                entry.Open(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(csv);
        }

        return stream.ToArray();
    }

    private static HttpResponseMessage JsonResponse(string body)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static string CreateEurostatResponse(
        string productDimension,
        string productCode,
        string productLabel)
        => $$"""
             {
               "id": ["freq", "currency", "{{productDimension}}", "geo", "time"],
               "size": [1, 1, 1, 2, 1],
               "dimension": {
                 "freq": {
                   "category": {
                     "index": {"A": 0},
                     "label": {"A": "Annual"}
                   }
                 },
                 "currency": {
                   "category": {
                     "index": {"EUR": 0},
                     "label": {"EUR": "Euro"}
                   }
                 },
                 "{{productDimension}}": {
                   "category": {
                     "index": {"{{productCode}}": 0},
                     "label": {"{{productCode}}": "{{productLabel}}"}
                   }
                 },
                 "geo": {
                   "category": {
                     "index": {"DE": 0, "EU27_2020": 1},
                     "label": {"DE": "Germany", "EU27_2020": "European Union"}
                   }
                 },
                 "time": {
                   "category": {
                     "index": {"2024": 0},
                     "label": {"2024": "2024"}
                   }
                 }
               },
               "value": {"0": 22.5, "1": 99.0}
             }
             """;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class FakeProvider(
        string sourceKey,
        Func<IReadOnlyList<국제농수산가격관측>> observationsFactory)
        : I국제농수산가격공급자
    {
        public string SourceKey { get; } = sourceKey;

        public Task<국제농수산가격공급결과> CollectAsync(
            int yearFrom,
            int yearTo,
            DateTime collectedAtUtc,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new 국제농수산가격공급결과(
                SourceKey,
                "https://official.test/data",
                observationsFactory(),
                ["test"]));
    }

    private sealed class DelegateHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(responseFactory(request));
        }
    }
}
