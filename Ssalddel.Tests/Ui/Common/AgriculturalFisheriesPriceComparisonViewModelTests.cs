using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class AgriculturalFisheriesPriceComparisonViewModelTests
{
    [Fact]
    public async Task Initialize_ComposesCountryCatalogsAndOfficialSources()
    {
        var client = new FakePublicDataClient();
        using var viewModel = CreateViewModel(client);

        var initialized = await viewModel.초기화Async();

        Assert.True(initialized);
        Assert.True(viewModel.초기화됨);
        Assert.Null(viewModel.InitializationMessage);
        Assert.Contains(viewModel.국내.품목, item => item.HsPrefix == "080810");
        Assert.Contains(viewModel.국내.품목, item => item.HsPrefix == "1006");
        Assert.Equal("호주 과일", viewModel.호주.Catalog.Indexes.Single().Label);
        Assert.Equal("official-source", viewModel.VisibleSources.Single().Key);
    }

    [Fact]
    public async Task LoadComparison_QueriesAllCountriesWithoutFlatteningTheirUnits()
    {
        var client = new FakePublicDataClient();
        using var viewModel = CreateViewModel(client);
        await viewModel.초기화Async();
        viewModel.국내.선택HsCode = "080810";
        viewModel.미국.품목명 = " apples ";
        viewModel.호주.선택IndexCode = 호주식품가격지수Codes.Fruit;

        await viewModel.LoadComparisonAsync();

        Assert.Equal("080810", client.DomesticHsCode);
        Assert.Equal("APPLES", client.UnitedStatesCommodity);
        Assert.Equal(호주식품가격지수Codes.Fruit, client.AustraliaRequest?.IndexCode);
        Assert.Equal(12_000m, viewModel.국내.응답?.Price?.Retail?.AverageKrwPerKg);
        Assert.Equal("DOLLARS / CWT", viewModel.미국.응답?.Items.Single().Unit);
        Assert.False(viewModel.호주.응답?.IsActualUnitPrice);
        Assert.Equal("INDEX POINTS", viewModel.호주.응답?.Items.Single().UnitLabel);
    }

    [Fact]
    public async Task Initialize_WhenOverviewIsUnavailable_UsesFallbackSourcesWithoutFailingPage()
    {
        var client = new FakePublicDataClient
        {
            FailOverview = true
        };
        using var viewModel = CreateViewModel(client);

        var initialized = await viewModel.초기화Async();

        Assert.True(initialized);
        Assert.True(viewModel.초기화됨);
        Assert.NotNull(viewModel.InitializationMessage);
        Assert.Equal(3, viewModel.VisibleSources.Count);
        Assert.Contains(
            viewModel.VisibleSources,
            source => source.Key == 호주농수산식품가격출처Keys.AbsConsumerPriceIndex);
    }

    [Fact]
    public async Task LoadComparison_WhenOneCountryFails_PreservesOtherCountryResults()
    {
        var client = new FakePublicDataClient
        {
            FailUnitedStatesPrice = true
        };
        using var viewModel = CreateViewModel(client);
        await viewModel.초기화Async();

        await viewModel.LoadComparisonAsync();

        Assert.NotNull(viewModel.국내.응답);
        Assert.Null(viewModel.미국.응답);
        Assert.Equal("미국 가격 API에 연결하지 못했습니다.", viewModel.미국.오류메시지);
        Assert.NotNull(viewModel.호주.응답);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task DomesticComparison_ConvertsKamisStagesToSelectedGramBasis()
    {
        var client = new FakePublicDataClient();
        using var viewModel = new 국내농수산가격조회ViewModel(client);

        await viewModel.조회Async();

        Assert.Equal("100g", viewModel.비교기준Label);
        Assert.Collection(
            viewModel.비교항목,
            auction =>
            {
                Assert.Equal("경락가", auction.StageLabel);
                Assert.False(auction.IsAvailable);
                Assert.Contains("공식 Open API", auction.AvailabilityNote);
            },
            wholesale =>
            {
                Assert.Equal("중도매가", wholesale.StageLabel);
                Assert.Equal(900m, wholesale.DisplayPriceKrw);
                Assert.Equal(0m, wholesale.DifferenceFromLowestKrw);
            },
            retail =>
            {
                Assert.Equal("소매가", retail.StageLabel);
                Assert.Equal(1_200m, retail.DisplayPriceKrw);
                Assert.Equal(300m, retail.DifferenceFromLowestKrw);
                Assert.Equal(33.3m, retail.DifferencePercentFromLowest!.Value, 1);
            });
    }

    [Fact]
    public async Task DomesticComparison_SupportsKilogramAndRepresentativeItemWeight()
    {
        var client = new FakePublicDataClient();
        using var viewModel = new 국내농수산가격조회ViewModel(client);
        await viewModel.조회Async();

        viewModel.비교단위 = 국내가격비교단위.킬로그램;

        Assert.Equal("1kg", viewModel.비교기준Label);
        Assert.Equal(9_000m, viewModel.비교항목[1].DisplayPriceKrw);
        Assert.Equal(12_000m, viewModel.비교항목[2].DisplayPriceKrw);

        viewModel.비교단위 = 국내가격비교단위.개수;
        viewModel.대표개당그램 = 250m;

        Assert.Equal("1개 · 대표 250g", viewModel.비교기준Label);
        Assert.Equal(2_250m, viewModel.비교항목[1].DisplayPriceKrw);
        Assert.Equal(3_000m, viewModel.비교항목[2].DisplayPriceKrw);
        Assert.Contains("추정값", viewModel.개수환산안내);
    }

    [Theory]
    [InlineData(농수산가격비교Section.비교, "한국·미국·호주 가격 비교")]
    [InlineData(농수산가격비교Section.국내, "한국 농수산물 가격")]
    [InlineData(농수산가격비교Section.미국, "미국 농수산물 가격")]
    [InlineData(농수산가격비교Section.호주, "호주 식품 가격지수")]
    [InlineData(농수산가격비교Section.출처, "공식 데이터 출처")]
    public void SelectSection_UsesPlainPriceSections(
        농수산가격비교Section section,
        string expectedTitle)
    {
        using var viewModel = CreateViewModel(new FakePublicDataClient());

        viewModel.SelectSection(section);

        Assert.Equal(section, viewModel.ActiveSection);
        Assert.Equal(expectedTitle, viewModel.CurrentSectionTitle);
    }

    private static 농수산가격비교PageViewModel CreateViewModel(
        I농수산공공데이터Client client)
        => new(
            client,
            new 국내농수산가격조회ViewModel(client),
            new 미국농수산가격조회ViewModel(client),
            new 호주농수산가격조회ViewModel(client));

    private sealed class FakePublicDataClient : I농수산공공데이터Client
    {
        public bool FailOverview { get; init; }

        public bool FailUnitedStatesPrice { get; init; }

        public string? DomesticHsCode { get; private set; }

        public string? UnitedStatesCommodity { get; private set; }

        public 호주농수산식품가격조회요청? AustraliaRequest { get; private set; }

        public Task<AgriculturalFisheriesInformationOverviewResponse> 개요조회Async(
            CancellationToken cancellationToken = default)
        {
            if (FailOverview)
            {
                throw new HttpRequestException("Overview unavailable.");
            }

            return Task.FromResult(new AgriculturalFisheriesInformationOverviewResponse
            {
                DataSources =
                [
                    new AgriculturalFisheriesDataSourceResponse
                    {
                        Key = "official-source",
                        Provider = "공식 기관",
                        DisplayName = "공식 가격 자료"
                    }
                ]
            });
        }

        public Task<AgriculturalFisheriesItemSearchResponse> 국내품목조회Async(
            string? query = null,
            string? categoryCode = null,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new AgriculturalFisheriesItemSearchResponse
            {
                Items =
                [
                    new AgriculturalFisheriesItemResponse
                    {
                        HsPrefix = "080810",
                        ProductName = "사과",
                        CategoryLabel = "과일류"
                    }
                ]
            });

        public Task<AgriculturalFisheriesDomesticPriceResponse> 국내가격조회Async(
            string hsCode,
            int lookbackDays = 14,
            CancellationToken cancellationToken = default)
        {
            DomesticHsCode = hsCode;
            return Task.FromResult(new AgriculturalFisheriesDomesticPriceResponse
            {
                Success = true,
                HsCode = hsCode,
                Price = new AtDomesticFoodPriceLookupResult
                {
                    Success = true,
                    Wholesale = new AtDomesticFoodPriceAggregate
                    {
                        PriceTypeLabel = "중도매",
                        AverageKrwPerKg = 9_000m,
                        LatestSurveyDate = "2026-07-17",
                        SampleCount = 4
                    },
                    Retail = new AtDomesticFoodPriceAggregate
                    {
                        PriceTypeLabel = "소매",
                        AverageKrwPerKg = 12_000m,
                        LatestSurveyDate = "2026-07-17",
                        SampleCount = 6
                    }
                }
            });
        }

        public Task<미국농수산가격조회응답> 미국가격조회Async(
            string commodity,
            string program,
            int yearFrom,
            int yearTo,
            int maxItems = 100,
            CancellationToken cancellationToken = default)
        {
            if (FailUnitedStatesPrice)
            {
                throw new HttpRequestException("USDA unavailable.");
            }

            UnitedStatesCommodity = commodity;
            return Task.FromResult(new 미국농수산가격조회응답
            {
                Success = true,
                Query = new 미국농수산가격조회요청
                {
                    Commodity = commodity,
                    YearFrom = yearFrom,
                    YearTo = yearTo
                },
                Items =
                [
                    new 미국농수산가격항목
                    {
                        Commodity = commodity,
                        RawValue = "54.20",
                        NumericValue = 54.20m,
                        Unit = "DOLLARS / CWT",
                        Year = "2026"
                    }
                ]
            });
        }

        public Task<호주농수산식품가격Catalog응답> 호주가격원천Catalog조회Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult(new 호주농수산식품가격Catalog응답
            {
                Indexes =
                [
                    new 호주식품가격지수선택항목
                    {
                        Code = 호주식품가격지수Codes.Fruit,
                        Label = "호주 과일"
                    }
                ],
                Measures =
                [
                    new 호주식품가격지수선택항목
                    {
                        Code = 호주식품가격지수측정Codes.IndexNumber,
                        Label = "가격지수"
                    }
                ],
                Regions =
                [
                    new 호주식품가격지수선택항목
                    {
                        Code = 호주식품가격지수지역Codes.Australia,
                        Label = "호주"
                    }
                ]
            });

        public Task<호주농수산식품가격조회응답> 호주식품가격지수조회Async(
            호주농수산식품가격조회요청 request,
            CancellationToken cancellationToken = default)
        {
            AustraliaRequest = request;
            return Task.FromResult(new 호주농수산식품가격조회응답
            {
                Success = true,
                Query = request,
                IsActualUnitPrice = false,
                Items =
                [
                    new 호주농수산식품가격항목
                    {
                        IndexCode = request.IndexCode,
                        RawValue = "141.7",
                        NumericValue = 141.7m,
                        UnitLabel = "INDEX POINTS",
                        ReferencePeriod = "2026-06"
                    }
                ]
            });
        }

        public Task<FoodPriceComparisonResponse> 식품가격비교Async(
            FoodPriceComparisonRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<HsCountryImportUnitPriceSimulationResult> 수입평균단가조회Async(
            HsCountryMonthlyTradeUnitPriceRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
