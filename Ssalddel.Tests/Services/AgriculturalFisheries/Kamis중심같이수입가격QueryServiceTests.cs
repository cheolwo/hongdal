using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Services.AgriculturalFisheries.ImportReadiness;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class Kamis중심같이수입가격QueryServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 29, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task KAMIS품목에_HS후보와관세청CIF통계단가를함께반환한다()
    {
        var marketService = new FakeMarketPriceService(
            MarketResponse(MarketItem("411", "사과", ["Apples"])));
        var publicDataService = new FakeHsPublicDataService();
        var service = CreateService(marketService, publicDataService);

        var result = await service.GetAsync(
            new Kamis중심같이수입가격Query
            {
                Year = 2026,
                ItemCode = "411",
                CountryCode = "US",
                ReferenceMonth = "202606",
                ImportLookbackMonths = 3,
                FxRateKrwPerUsd = 1_380m
            });

        var item = Assert.Single(result.Items);
        Assert.Equal("411", item.MarketPrice.KamisItemCode);
        var candidate = Assert.Single(item.HsImportPriceCandidates);
        Assert.Equal("080810", candidate.HsCode);
        Assert.Equal("HS6", candidate.HsCodeScheme);
        Assert.True(candidate.RequiresProfessionalReview);
        Assert.True(candidate.IsImportPriceLookupSelected);
        Assert.Equal(Hs공공데이터수집상태Codes.성공, candidate.ImportPrice?.StatusCode);
        Assert.Equal(2.5m, candidate.ImportPrice?.AverageCifUsdPerKg);
        Assert.Equal(3_450m, candidate.ImportPrice?.AverageCifKrwPerKg);
        Assert.Equal("US", result.CountryCode);
        Assert.Equal("202606", result.ReferenceMonth);
        Assert.Equal(1, result.ExternalLookupCount);

        var request = Assert.Single(publicDataService.Requests);
        Assert.Equal("080810", request.HsCode);
        Assert.Equal("US", request.CountryCode);
        Assert.Equal("202606", request.ReferenceMonth);
        Assert.Equal(
            [Hs공공데이터출처Keys.수입평균단가],
            request.SourceKeys);
    }

    [Fact]
    public async Task 품목의전체HS후보를표시하고_요청개수만외부조회한다()
    {
        var marketService = new FakeMarketPriceService(
            MarketResponse(MarketItem("214", "상추", ["Lettuce"])));
        var publicDataService = new FakeHsPublicDataService();
        var service = CreateService(marketService, publicDataService);

        var result = await service.GetAsync(
            new Kamis중심같이수입가격Query
            {
                Year = 2026,
                ItemCode = "214",
                CountryCode = "CN",
                ReferenceMonth = "202606",
                HsPriceCandidatesPerItem = 1
            });

        var candidates = Assert.Single(result.Items).HsImportPriceCandidates;
        Assert.Equal(["070511", "070519"], candidates.Select(item => item.HsCode));
        Assert.True(candidates[0].IsImportPriceLookupSelected);
        Assert.NotNull(candidates[0].ImportPrice);
        Assert.False(candidates[1].IsImportPriceLookupSelected);
        Assert.Null(candidates[1].ImportPrice);
        Assert.Equal(
            Kamis중심Hs수입가격조회상태Codes.후보제한,
            candidates[1].LookupOmissionReasonCode);
        Assert.Single(publicDataService.Requests);
        Assert.Equal(1, result.SkippedLookupCount);
    }

    [Fact]
    public async Task 검토된HSK코드를입력하면_그코드로KAMIS품목을찾아조회한다()
    {
        var marketService = new FakeMarketPriceService(
            MarketResponse(MarketItem("411", "사과", ["Apples"])));
        var publicDataService = new FakeHsPublicDataService();
        var service = CreateService(marketService, publicDataService);

        var result = await service.GetAsync(
            new Kamis중심같이수입가격Query
            {
                Year = 2026,
                HsCode = "0808.10-0000",
                CountryCode = "US",
                ReferenceMonth = "202606"
            });

        Assert.Equal("411", marketService.LastQuery?.ItemCode);
        var candidate = Assert.Single(Assert.Single(result.Items).HsImportPriceCandidates);
        Assert.Equal("0808100000", candidate.HsCode);
        Assert.Equal("HSK10", candidate.HsCodeScheme);
        Assert.Equal(
            "0808100000",
            Assert.Single(publicDataService.Requests).HsCode);
    }

    [Fact]
    public async Task HS코드와KAMIS품목연결이불일치하면외부조회전에거절한다()
    {
        var marketService = new FakeMarketPriceService(
            MarketResponse(MarketItem("152", "감자", ["Potatoes"])));
        var publicDataService = new FakeHsPublicDataService();
        var service = CreateService(marketService, publicDataService);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAsync(new Kamis중심같이수입가격Query
            {
                Year = 2026,
                ItemCode = "152",
                HsCode = "080810",
                CountryCode = "US",
                ReferenceMonth = "202606"
            }));
        Assert.Null(marketService.LastQuery);
        Assert.Empty(publicDataService.Requests);
    }

    private static Kamis중심같이수입가격QueryService CreateService(
        IKamis중심UsdaAms가격비교QueryService marketService,
        IHs공공데이터수집Service publicDataService)
        => new(
            marketService,
            new FoodPriceCrosswalkCatalog(),
            publicDataService,
            new FixedTimeProvider(Now));

    private static Kamis중심UsdaAms가격비교응답 MarketResponse(
        params Kamis중심UsdaAms품목가격응답[] items)
        => new(
            Kamis중심UsdaAms가격비교상태Codes.완료,
            Now.UtcDateTime,
            2026,
            items.Length,
            items.Length,
            items.Length,
            0,
            1,
            10,
            items,
            ["원 거래단위를 보존합니다."]);

    private static Kamis중심UsdaAms품목가격응답 MarketItem(
        string itemCode,
        string itemName,
        IReadOnlyList<string> amsCommodities)
        => new(
            itemCode.StartsWith('4') ? "400" : "200",
            itemCode.StartsWith('4') ? "과일류" : "채소류",
            itemCode,
            itemName,
            new DateOnly(2026, 7, 28),
            Kamis중심UsdaAms매핑상태Codes.후보있음,
            Kamis중심UsdaAms매핑품질Codes.동일품목후보,
            "동일 품목 후보",
            amsCommodities,
            "단위 확인 필요",
            AllowsDirectPriceDifference: false,
            [],
            [],
            new Kamis중심상품코드연결응답(
                $"kamis:{(itemCode.StartsWith('4') ? "400" : "200")}:{itemCode}",
                itemCode.StartsWith('4') ? "400" : "200",
                itemCode,
                Kamis중심상품코드연결상태Codes.확인됨,
                amsCommodities,
                Kamis중심상품코드연결상태Codes.후보,
                [],
                []),
            []);

    private sealed class FakeMarketPriceService(
        Kamis중심UsdaAms가격비교응답 response)
        : IKamis중심UsdaAms가격비교QueryService
    {
        public Kamis중심UsdaAms가격비교Query? LastQuery { get; private set; }

        public Task<Kamis중심UsdaAms가격비교응답> GetAsync(
            Kamis중심UsdaAms가격비교Query query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(response);
        }
    }

    private sealed class FakeHsPublicDataService : IHs공공데이터수집Service
    {
        private readonly object _gate = new();

        public List<Hs공공데이터수집요청> Requests { get; } = [];

        public Task<Hs공공데이터묶음응답> 수집Async(
            Hs공공데이터수집요청 request,
            CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                Requests.Add(request);
            }

            return Task.FromResult(new Hs공공데이터묶음응답
            {
                HsCode = request.HsCode,
                CountryCode = request.CountryCode,
                ReferenceMonth = request.ReferenceMonth,
                CollectedAtUtc = Now.UtcDateTime,
                SuccessSourceCount = 1,
                Sources =
                [
                    new Hs공공데이터출처응답
                    {
                        SourceKey = Hs공공데이터출처Keys.수입평균단가,
                        Provider = "관세청",
                        DisplayName = "품목별 국가별 수입실적",
                        StatusCode = Hs공공데이터수집상태Codes.성공,
                        Summary = "3개월 가중평균 CIF 통계단가입니다.",
                        DocumentationUrl =
                            "https://www.data.go.kr/data/15100475/openapi.do",
                        CollectedAtUtc = Now.UtcDateTime,
                        Items =
                        [
                            new Hs공공데이터정보항목
                            {
                                ItemKey = request.HsCode,
                                Title = "국가별 수입 가중평균 단가",
                                Fields = new Dictionary<string, string?>
                                {
                                    ["startMonth"] = "202604",
                                    ["endMonth"] = "202606",
                                    ["totalImportWeightKg"] = "1000",
                                    ["totalImportValueUsd"] = "2500",
                                    ["averageImportUnitValueUsdPerKg"] = "2.5",
                                    ["averageImportUnitValueKrwPerKg"] =
                                        request.ExpectedFxRateKrwPerUsd.HasValue
                                            ? "3450"
                                            : null
                                }
                            }
                        ]
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
