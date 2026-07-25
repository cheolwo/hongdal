using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class DomesticAgriculturalAuctionPriceModuleTests
{
    [Fact]
    public async Task 농림부정산가격을_비식별경락가격으로변환한다()
    {
        Uri? requestedUri = null;
        var provider = CreateProvider(new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return JsonResponse(SampleResponse);
        }));

        var result = await provider.조회Async(new 국내농산물경락가격조회요청
        {
            SettlementDate = "2024-05-01",
            WholesaleMarketCode = "380303",
            CorporationCode = "38030302",
            Page = 2,
            PageSize = 100
        });

        Assert.True(result.Success);
        Assert.Equal(국내농산물경락가격조회상태Codes.완료, result.StatusCode);
        Assert.Equal(5, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal("양배추", item.ItemName);
        Assert.Equal("적양배추", item.VarietyName);
        Assert.Equal(17m, item.UnitWeight);
        Assert.Equal(35000m, item.AuctionPriceKrw);
        Assert.Equal(85m, item.TotalQuantity);
        Assert.Equal(175000m, item.TotalAmountKrw);
        Assert.NotNull(requestedUri);
        Assert.Equal(
            "/openapi/test-key/json/Grid_20240625000000000655_1/101/200",
            requestedUri!.AbsolutePath);
        Assert.Contains("SALEDATE=20240501", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("WHSALCD=380303", requestedUri.Query, StringComparison.Ordinal);
        Assert.Contains("CMPCD=38030302", requestedUri.Query, StringComparison.Ordinal);

        var publicJson = JsonSerializer.Serialize(item);
        Assert.DoesNotContain("김순학", publicJson, StringComparison.Ordinal);
        Assert.DoesNotContain("남밀양농협", publicJson, StringComparison.Ordinal);
        Assert.DoesNotContain("FARMNAME", publicJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task 조회Service는_잘못된정산일을_외부호출전에거부한다()
    {
        var provider = new StubProvider();
        var service = new 국내농산물경락가격조회Service([provider]);

        var response = await service.조회Async(new 국내농산물경락가격조회요청
        {
            SettlementDate = "20240501"
        });

        Assert.False(response.Success);
        Assert.Equal(국내농산물경락가격조회상태Codes.잘못된요청, response.StatusCode);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task Http원천은_명시적허용없이는_외부호출하지않는다()
    {
        var callCount = 0;
        var client = new HttpClient(new StubHttpMessageHandler(_ =>
        {
            callCount++;
            return JsonResponse(SampleResponse);
        }))
        {
            BaseAddress = new Uri("http://211.237.50.150:7080")
        };
        var provider = new Mafra공영도매시장경락가격공급자(
            client,
            Options.Create(new PublicDataOptions
            {
                DomesticAgriculturalAuctionPrices =
                    new DomesticAgriculturalAuctionPricesOptions
                    {
                        ApiKey = "test-key",
                        AllowInsecureHttp = false
                    }
            }),
            TimeProvider.System,
            NullLogger<Mafra공영도매시장경락가격공급자>.Instance);

        var result = await provider.조회Async(new 국내농산물경락가격조회요청
        {
            SettlementDate = "2024-05-01"
        });

        Assert.False(result.Success);
        Assert.Equal(국내농산물경락가격조회상태Codes.설정안됨, result.StatusCode);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task 같은거래를_재수집하면_중복하지않고_변경가격을갱신한다()
    {
        var options = new DbContextOptionsBuilder<AgriculturalFisheriesDbContext>()
            .UseInMemoryDatabase($"domestic-auction-{Guid.NewGuid():N}")
            .Options;
        await using var db = new AgriculturalFisheriesDbContext(options);
        var lookup = new StubLookupService();
        var service = new 국내농산물경락가격ArchiveService(
            db,
            lookup,
            Options.Create(new AgriculturalFisheriesBatchOptions
            {
                DomesticAuctionPageSize = 1000,
                DomesticAuctionMaxPagesPerRun = 2
            }),
            TimeProvider.System,
            NullLogger<국내농산물경락가격ArchiveService>.Instance);

        var first = await service.CollectAsync(new DateOnly(2024, 5, 1));
        lookup.AuctionPriceKrw = 36000m;
        var second = await service.CollectAsync(new DateOnly(2024, 5, 1));

        Assert.Equal(1, first.InsertedCount);
        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(1, second.UpdatedCount);
        var observation = Assert.Single(await db.DomesticAuctionPriceObservations.ToArrayAsync());
        Assert.Equal(36000m, observation.AuctionPriceKrw);
        Assert.Equal(2, await db.DomesticAuctionPriceCollectionRuns.CountAsync());

        var archive = await service.SearchAsync(new 국내농산물경락가격조회요청
        {
            SettlementDate = "2024-05-01",
            ItemName = "양배추"
        });
        Assert.True(archive.Success);
        Assert.Equal(36000m, Assert.Single(archive.Items).AuctionPriceKrw);
    }

    private static Mafra공영도매시장경락가격공급자 CreateProvider(
        HttpMessageHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://211.237.50.150:7080")
        };
        return new Mafra공영도매시장경락가격공급자(
            client,
            Options.Create(new PublicDataOptions
            {
                DomesticAgriculturalAuctionPrices =
                    new DomesticAgriculturalAuctionPricesOptions
                    {
                        ApiKey = "test-key",
                        AllowInsecureHttp = true
                    }
            }),
            TimeProvider.System,
            NullLogger<Mafra공영도매시장경락가격공급자>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }

    private sealed class StubProvider : I국내농산물경락가격공급자
    {
        public int CallCount { get; private set; }

        public string SourceKey
            => 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement;

        public 국내농산물경락가격원천응답 GetSource()
            => Source();

        public Task<국내농산물경락가격조회응답> 조회Async(
            국내농산물경락가격조회요청 request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new 국내농산물경락가격조회응답
            {
                Success = true,
                StatusCode = 국내농산물경락가격조회상태Codes.완료,
                Source = Source(),
                Query = request
            });
        }
    }

    private sealed class StubLookupService : I국내농산물경락가격조회Service
    {
        public decimal AuctionPriceKrw { get; set; } = 35000m;

        public IReadOnlyList<국내농산물경락가격원천응답> GetSources()
            => [Source()];

        public Task<국내농산물경락가격조회응답> 조회Async(
            국내농산물경락가격조회요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new 국내농산물경락가격조회응답
            {
                Success = true,
                StatusCode = 국내농산물경락가격조회상태Codes.완료,
                Source = Source(),
                Query = request,
                TotalCount = 1,
                LatestCollectedAtUtc = DateTimeOffset.UtcNow,
                Items =
                [
                    new 국내농산물경락가격항목
                    {
                        RecordKey = "record-1",
                        SourceKey = 국내농산물경락가격출처Keys
                            .MafraWholesaleMarketSettlement,
                        SettlementDate = new DateOnly(2024, 5, 1),
                        WholesaleMarketCode = "380303",
                        CorporationCode = "38030302",
                        SlipNumber = "534",
                        AuctionSequence1 = "10",
                        CorporationItemCode = "003003004003",
                        ItemName = "양배추",
                        VarietyName = "적양배추",
                        UnitWeight = 17m,
                        UnitCode = "12",
                        GradeCode = "10",
                        Quantity = 5m,
                        AuctionPriceKrw = AuctionPriceKrw,
                        TotalQuantity = 85m,
                        TotalAmountKrw = AuctionPriceKrw * 5m,
                        CollectedAtUtc = DateTimeOffset.UtcNow
                    }
                ]
            });
    }

    private static 국내농산물경락가격원천응답 Source()
        => new()
        {
            Key = 국내농산물경락가격출처Keys.MafraWholesaleMarketSettlement,
            Provider = "농림축산식품부",
            DisplayName = "전국 공영도매시장 경매원천 정산가격",
            UpdateCycle = "일간",
            IsConfigured = true
        };

    private const string SampleResponse =
        """
        {
          "Grid_20240625000000000655_1": {
            "totalCnt": 5,
            "result": {
              "message": "정상 처리되었습니다.",
              "code": "INFO-000"
            },
            "row": [
              {
                "SALEDATE": "20240501",
                "WHSALCD": "380303",
                "CMPCD": "38030302",
                "SEQ": "534",
                "NO1": "10",
                "NO2": "",
                "MMCD": "1",
                "LARGE": "13",
                "MID": "05",
                "SMALL": "01",
                "CMPGOOD": "003003004003",
                "PUMNAME": "양배추",
                "GOODNAME": "적양배추",
                "DANQ": "17",
                "DANCD": "12",
                "POJCD": "101",
                "SIZECD": "100",
                "LVCD": "10",
                "QTY": "5",
                "COST": "35000",
                "SANCD": "627600",
                "SANNAME": "경남 밀양시",
                "CHULNAME": "남밀양농협",
                "FARMNAME": "김순학",
                "TOTQTY": "85",
                "TOTAMT": "175000",
                "SBIDTIME": "071530"
              }
            ]
          }
        }
        """;
}
