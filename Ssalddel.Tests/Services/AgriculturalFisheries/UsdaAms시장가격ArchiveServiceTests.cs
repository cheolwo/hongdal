using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class UsdaAms시장가격ArchiveServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 29, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Client는_Basic인증으로보고서와상세가격범위를읽는다()
    {
        var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://marsapi.ams.usda.gov")
        };
        var client = new UsdaAmsMarketNewsClient(
            httpClient,
            Options.Create(new PublicDataOptions
            {
                UsdaAmsMarketNews = new UsdaAmsMarketNewsOptions
                {
                    ApiKey = "test-api-key"
                }
            }));

        var reports = await client.GetReportsAsync();
        var detail = await client.GetReportDetailsAsync(
            "2277",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 29));

        var report = Assert.Single(reports);
        Assert.Equal("2277", report.SlugId);
        Assert.Contains("Terminal", report.MarketTypes);
        var row = Assert.Single(detail.Rows);
        Assert.Equal("Apples", row.Commodity);
        Assert.Equal("Fuji", row.Variety);
        Assert.Equal("40 lb cartons", row.Package);
        Assert.Equal("large", row.ItemSize);
        Assert.Equal("Fruits", row.Group);
        Assert.Equal("good", row.Condition);
        Assert.Equal("fair", row.Appearance);
        Assert.Equal("controlled atmosphere", row.Environment);
        Assert.Equal("18.00", row.LowPrice);
        Assert.Equal(1, detail.TotalRows);
        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal("Basic", request.Authorization?.Scheme);
            Assert.Equal(
                Convert.ToBase64String(Encoding.ASCII.GetBytes("test-api-key:")),
                request.Authorization?.Parameter);
            Assert.DoesNotContain("test-api-key", request.Uri.ToString());
        });
        Assert.Contains(
            "report_begin_date=07%2F01%2F2026%3A07%2F29%2F2026",
            handler.Requests[1].Uri.Query);
    }

    [Fact]
    public async Task 세시장단계의_2026년관측을저장하고_재수집해도중복하지않는다()
    {
        await using var db = CreateDb();
        var client = new FakeClient();
        var service = CreateService(client, db);
        var request = new UsdaAms시장가격수집요청
        {
            Year = 2026,
            DateTo = "2026-01-31"
        };

        var first = await service.CollectAsync(request);
        var second = await service.CollectAsync(request);

        Assert.Equal(3, first.DiscoveredReportCount);
        Assert.Equal(3, first.InsertedCount);
        Assert.Equal(0, first.ExistingCount);
        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(3, second.ExistingCount);
        Assert.Equal(3, await db.UsdaAmsMarketPriceObservations.CountAsync());
        Assert.Equal(2, await db.UsdaAmsMarketPriceCollectionRuns.CountAsync());
        var catalog = Assert.Single(
            await db.UsdaAmsYearCommodityCatalog.ToArrayAsync());
        Assert.Equal(2026, catalog.Year);
        Assert.Equal("Apples", catalog.Commodity);
        Assert.Equal(new DateOnly(2026, 1, 15), catalog.FirstObservedDate);
        Assert.Equal(new DateOnly(2026, 1, 15), catalog.LastObservedDate);
        Assert.Contains(
            await db.UsdaAmsMarketPriceObservations.ToArrayAsync(),
            item =>
                item.SourceKey == 농수산시세정보원Keys.UsdaAms도매터미널가격
                && item.MarketStageCode == 농수산시세시장단계Codes.도매터미널
                && item.LowPrice == 18m
                && item.HighPrice == 22m
                && item.OriginalUnit.Contains("package: 40 lb cartons"));
        Assert.Contains(
            await db.UsdaAmsMarketPriceObservations.ToArrayAsync(),
            item =>
                item.SourceKey == 농수산시세정보원Keys.UsdaAms소매광고가격
                && item.WeightedAveragePrice == 2.99m
                && item.StoreCount == 120);
    }

    [Fact]
    public async Task Archive조회는_품목과연도를검색하고_원가격범위를보존한다()
    {
        await using var db = CreateDb();
        var service = CreateService(new FakeClient(), db);
        await service.CollectAsync(new UsdaAms시장가격수집요청
        {
            Year = 2026,
            DateTo = "2026-01-31",
            MarketTypes = ["Terminal"]
        });

        var result = await service.GetArchiveAsync(new UsdaAms시장가격ArchiveQuery
        {
            Commodity = "Apple",
            Year = 2026
        });

        Assert.Equal(UsdaAms시장가격상태Codes.완료, result.StatusCode);
        var item = Assert.Single(result.Items);
        Assert.Equal("USD", item.CurrencyCode);
        Assert.Equal(18m, item.LowPrice);
        Assert.Equal(22m, item.HighPrice);
        Assert.Equal("Washington", item.Origin);
    }

    [Fact]
    public async Task 보고서목록실패는_실패Run으로기록한다()
    {
        await using var db = CreateDb();
        var service = CreateService(new FailingClient(), db);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.CollectAsync(new UsdaAms시장가격수집요청
            {
                Year = 2026,
                DateTo = "2026-01-31"
            }));

        var run = await db.UsdaAmsMarketPriceCollectionRuns.SingleAsync();
        Assert.Equal(UsdaAms시장가격Archive상태Codes.실패, run.StatusCode);
        Assert.Contains("official source unavailable", run.ErrorMessage);
        Assert.Empty(await db.UsdaAmsMarketPriceObservations.ToArrayAsync());
    }

    private static UsdaAms시장가격ArchiveService CreateService(
        IUsdaAmsMarketNewsClient client,
        AgriculturalFisheriesDbContext db)
        => new(
            client,
            db,
            new FixedTimeProvider(Now),
            NullLogger<UsdaAms시장가격ArchiveService>.Instance);

    private static AgriculturalFisheriesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AgriculturalFisheriesDbContext(options);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class FakeClient : IUsdaAmsMarketNewsClient
    {
        public Task<IReadOnlyList<UsdaAms보고서Descriptor>> GetReportsAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UsdaAms보고서Descriptor>>(
            [
                Report("100", "Terminal"),
                Report("200", "Shipping Point"),
                Report("300", "Retail - Specialty Crops"),
                new(
                    "400",
                    "ORNAMENTAL",
                    "Ornamental Terminal Prices",
                    new DateOnly(2026, 1, 31),
                    ["Terminal"],
                    ["Report Details"])
            ]);

        public Task<UsdaAms시장가격Slice> GetReportDetailsAsync(
            string slugId,
            DateOnly dateFrom,
            DateOnly dateTo,
            CancellationToken cancellationToken = default)
        {
            var marketType = slugId switch
            {
                "100" => "Terminal",
                "200" => "Shipping Point",
                _ => "Retail - Specialty Crops"
            };
            var row = new UsdaAms시장가격Row
            {
                ReportDate = "01/15/2026",
                ReportBeginDate = "01/15/2026",
                ReportEndDate = "01/15/2026",
                PublishedDate = "01/15/2026 10:00:00",
                OfficeName = "Test Office",
                OfficeState = "GA",
                MarketType = marketType,
                MarketLocationName = "Atlanta",
                MarketLocationState = "GA",
                SlugId = slugId,
                SlugName = $"AMS_{slugId}",
                ReportTitle = $"Test {marketType}",
                Commodity = "Apples",
                Variety = "Fuji",
                Package = marketType == "Retail - Specialty Crops"
                    ? string.Empty
                    : "40 lb cartons",
                UnitSales = "per carton",
                ItemSize = "large",
                Grade = "U.S. One",
                Origin = "Washington",
                LowPrice = marketType == "Retail - Specialty Crops" ? string.Empty : "18.00",
                HighPrice = marketType == "Retail - Specialty Crops" ? string.Empty : "22.00",
                WeightedAveragePrice =
                    marketType == "Retail - Specialty Crops" ? "2.99" : string.Empty,
                StoreCount =
                    marketType == "Retail - Specialty Crops" ? "120" : string.Empty,
                Region = marketType == "Retail - Specialty Crops" ? "NATIONAL" : string.Empty,
                RawJson =
                    $$"""{"slug_id":"{{slugId}}","market_type":"{{marketType}}","commodity":"Apples","report_begin_date":"01/15/2026"}"""
            };
            return Task.FromResult(new UsdaAms시장가격Slice(
                1,
                1,
                100_000,
                [row]));
        }

        private static UsdaAms보고서Descriptor Report(
            string slugId,
            string marketType)
            => new(
                slugId,
                $"AMS_{slugId}",
                $"Test {marketType}",
                new DateOnly(2026, 1, 31),
                [marketType],
                ["Report Details"]);
    }

    private sealed class FailingClient : IUsdaAmsMarketNewsClient
    {
        public Task<IReadOnlyList<UsdaAms보고서Descriptor>> GetReportsAsync(
            CancellationToken cancellationToken = default)
            => throw new HttpRequestException("official source unavailable");

        public Task<UsdaAms시장가격Slice> GetReportDetailsAsync(
            string slugId,
            DateOnly dateFrom,
            DateOnly dateTo,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new CapturedRequest(
                request.RequestUri!,
                request.Headers.Authorization));
            var isCatalog = request.RequestUri!.AbsolutePath.EndsWith(
                "/reports",
                StringComparison.Ordinal);
            var json = isCatalog
                ? """
                  [
                    {
                      "slug_id": "2277",
                      "slug_name": "AJ_FV010",
                      "report_title": "Atlanta Terminal Market Fruit Prices",
                      "report_date": "07/28/2026",
                      "market_types": ["Terminal"],
                      "sectionNames": ["Report Header", "Report Details"]
                    }
                  ]
                  """
                : """
                  {
                    "reportSection": "Report Details",
                    "reportSections": ["Report Header", "Report Details"],
                    "stats": {
                      "returnedRows": 1,
                      "userAllowedRows": 100000,
                      "totalRows": 1
                    },
                    "results": [
                      {
                        "report_date": "07/28/2026",
                        "report_begin_date": "07/28/2026",
                        "report_end_date": "07/28/2026",
                        "slug_id": "2277",
                        "slug_name": "AJ_FV010",
                        "market_type": "Terminal",
                        "grp": "Fruits",
                        "commodity": "Apples",
                        "var": "Fuji",
                        "pkg": "40 lb cartons",
                        "item_size": "large",
                        "cond": "good",
                        "appear": "fair",
                        "env": "controlled atmosphere",
                        "low_price": "18.00",
                        "high_price": "22.00"
                      }
                    ]
                  }
                  """;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed record CapturedRequest(
        Uri Uri,
        AuthenticationHeaderValue? Authorization);
}
