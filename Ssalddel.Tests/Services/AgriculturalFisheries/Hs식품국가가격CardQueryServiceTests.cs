using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Hs식품국가가격CardQueryServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 3, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 한국시장가격과_미국일본중국수입통계단가를_카드한건으로반환한다()
    {
        var trade = new StubTradePriceService();
        var service = CreateService(trade);

        var result = await service.GetAsync(
            "070200",
            new Hs식품국가가격CardQuery
            {
                Month = "202607",
                LookbackMonths = 3
            });

        Assert.Equal(Hs식품국가가격Card상태Codes.완료, result.StatusCode);
        Assert.Equal("070200", result.HsCode);
        Assert.Equal("HS6", result.HsCodeScheme);
        Assert.Equal("토마토", result.ProductName);
        Assert.Equal("https://example.test/hs-070200.jpg", result.RepresentativeImageUrl);
        Assert.Equal(["KR", "US", "JP", "CN"], result.Countries.Select(item => item.CountryCode));

        var korea = result.Countries[0];
        Assert.Equal(2, korea.PriceObservations.Count);
        Assert.All(korea.PriceObservations, observation =>
        {
            Assert.Equal(Hs식품국가가격맥락Codes.국내시장조사가격, observation.PriceContextCode);
            Assert.Equal("KRW/kg", observation.Unit);
            Assert.DoesNotContain("Kcs", observation.ComparisonGroupCode, StringComparison.Ordinal);
        });

        var imports = result.Countries.Skip(1).ToArray();
        Assert.All(imports, country =>
        {
            var observation = Assert.Single(country.PriceObservations);
            Assert.Equal(Hs식품국가가격맥락Codes.수입통계단가, observation.PriceContextCode);
            Assert.Equal("USD/kg", observation.Unit);
            Assert.StartsWith("KcsHs6ImportCifUsdPerKg-", observation.ComparisonGroupCode);
            Assert.True(observation.AllowsComparisonWithinGroup);
        });
        Assert.Equal(["US", "JP", "CN"], trade.RequestedCountries);
        Assert.All(trade.Requests, request =>
        {
            Assert.Equal("070200", request.HsCode);
            Assert.Equal("202607", request.Month);
            Assert.Equal(3, request.LookbackMonths);
        });
        Assert.Contains(result.ComparisonBoundaries, note =>
            note.Contains("직접 차액이나 순위", StringComparison.Ordinal));
        Assert.True(result.InformationOnly);
    }

    [Fact]
    public async Task 한나라수입실적이없어도_다른나라가격은유지한다()
    {
        var trade = new StubTradePriceService
        {
            NoDataCountryCode = "JP"
        };
        var service = CreateService(trade);

        var result = await service.GetAsync(
            "070200",
            new Hs식품국가가격CardQuery { Month = "202607" });

        Assert.Equal(Hs식품국가가격Card상태Codes.일부자료, result.StatusCode);
        var japan = Assert.Single(result.Countries, item => item.CountryCode == "JP");
        Assert.Equal(Hs식품국가가격관측상태Codes.자료없음, japan.DataStatusCode);
        Assert.Empty(japan.PriceObservations);
        Assert.Equal(
            Hs식품국가가격관측상태Codes.관측됨,
            Assert.Single(result.Countries, item => item.CountryCode == "US").DataStatusCode);
        Assert.Equal(
            Hs식품국가가격관측상태Codes.관측됨,
            Assert.Single(result.Countries, item => item.CountryCode == "CN").DataStatusCode);
    }

    [Fact]
    public async Task 등록되지않은Hs6은_외부가격을조회하지않는다()
    {
        var trade = new StubTradePriceService();
        var service = new Hs식품국가가격CardQueryService(
            new StubCatalogReader(null),
            new StubDomesticPriceService(),
            trade,
            new FixedTimeProvider(Now));

        var result = await service.GetAsync(
            "999999",
            new Hs식품국가가격CardQuery { Month = "202607" });

        Assert.Equal(Hs식품국가가격Card상태Codes.품목없음, result.StatusCode);
        Assert.Empty(result.Countries);
        Assert.Empty(trade.Requests);
    }

    private static Hs식품국가가격CardQueryService CreateService(
        StubTradePriceService trade)
        => new(
            new StubCatalogReader(new Hs식품가격CardCatalog항목(
                "070200",
                "토마토",
                "https://example.test/hs-070200.jpg",
                "Unreviewed")),
            new StubDomesticPriceService(),
            trade,
            new FixedTimeProvider(Now));

    private sealed class StubCatalogReader(Hs식품가격CardCatalog항목? item)
        : IHs식품가격CardCatalogReader
    {
        public Task<Hs식품가격CardCatalog항목?> FindAsync(
            string hsCode,
            CancellationToken cancellationToken = default)
            => Task.FromResult(item?.HsCode == hsCode ? item : null);
    }

    private sealed class StubDomesticPriceService
        : IAgriculturalFisheriesInformationService
    {
        public Task<AgriculturalFisheriesDomesticPriceResponse> GetDomesticPriceAsync(
            AgriculturalFisheriesDomesticPriceRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new AgriculturalFisheriesDomesticPriceResponse
            {
                Success = true,
                StatusCode = "CompleteFromArchive",
                HsCode = request.HsCode,
                Summary = "저장된 KAMIS 가격",
                Price = new AtDomesticFoodPriceLookupResult
                {
                    Success = true,
                    Wholesale = new AtDomesticFoodPriceAggregate
                    {
                        PriceTypeCode = "Wholesale",
                        PriceTypeLabel = "도매",
                        LatestSurveyDate = "20260801",
                        AverageKrwPerKg = 4200m,
                        MinimumKrwPerKg = 3800m,
                        MaximumKrwPerKg = 4600m,
                        SampleCount = 5
                    },
                    Retail = new AtDomesticFoodPriceAggregate
                    {
                        PriceTypeCode = "Retail",
                        PriceTypeLabel = "소매",
                        LatestSurveyDate = "20260801",
                        AverageKrwPerKg = 5900m,
                        MinimumKrwPerKg = 5400m,
                        MaximumKrwPerKg = 6400m,
                        SampleCount = 4
                    }
                }
            });

        public AgriculturalFisheriesInformationOverviewResponse GetOverview()
            => throw new NotSupportedException();

        public AgriculturalFisheriesItemSearchResponse SearchItems(
            string? query,
            string? categoryCode,
            int page,
            int pageSize)
            => throw new NotSupportedException();

        public AgriculturalFisheriesItemResponse? FindItem(string? hsCode)
            => throw new NotSupportedException();

        public 농수산시세정보원목록응답 GetMarketPriceSources(
            string? countryCode,
            string? marketStageCode)
            => throw new NotSupportedException();

        public 농수산시세비교판정응답 AssessMarketPriceComparability(
            string? leftSourceKey,
            string? rightSourceKey)
            => throw new NotSupportedException();
    }

    private sealed class StubTradePriceService : IHsCountryTradeUnitPriceLookupService
    {
        public string? NoDataCountryCode { get; init; }

        public List<HsCountryMonthlyTradeUnitPriceRequest> Requests { get; } = [];

        public string[] RequestedCountries => Requests
            .Select(item => item.CountryCode)
            .ToArray();

        public Task<HsCountryImportUnitPriceSimulationResult> SimulateImportUnitPriceAsync(
            HsCountryMonthlyTradeUnitPriceRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (request.CountryCode == NoDataCountryCode)
            {
                return Task.FromResult(new HsCountryImportUnitPriceSimulationResult
                {
                    Success = false,
                    ErrorMessage = "No import statistics were returned for the HS code, country, and month range.",
                    CountryCode = request.CountryCode,
                    HsCode = request.HsCode
                });
            }

            var price = request.CountryCode switch
            {
                "US" => 1.8m,
                "JP" => 2.4m,
                _ => 1.5m
            };
            return Task.FromResult(new HsCountryImportUnitPriceSimulationResult
            {
                Success = true,
                HsCode = request.HsCode,
                CountryCode = request.CountryCode,
                StartMonth = "202605",
                EndMonth = "202607",
                AverageImportUnitValueUsdPerKg = price,
                MonthlyItems =
                [
                    new HsCountryMonthlyTradeUnitPriceItem
                    {
                        CountryCode = request.CountryCode,
                        Month = "202606",
                        ImportWeightKg = 100m,
                        ImportValueUsd = price * 100m,
                        AverageImportUnitValueUsdPerKg = price
                    },
                    new HsCountryMonthlyTradeUnitPriceItem
                    {
                        CountryCode = request.CountryCode,
                        Month = "202607",
                        ImportWeightKg = 120m,
                        ImportValueUsd = price * 120m,
                        AverageImportUnitValueUsdPerKg = price
                    }
                ]
            });
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
