using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class RegionalAgriculturalMapViewModelTests
{
    [Fact]
    public async Task 초기화와검색뒤_국가와관계레이어를바꾸고_첫지역을선택한다()
    {
        var client = new FakePublicDataClient();
        using var viewModel = new 지역농수산MapPageViewModel(client);

        Assert.True(await viewModel.초기화Async());
        Assert.Equal(RegionalAgriculturalMapCountryCodes.Korea, client.Query?.CountryCode);
        Assert.Equal("kr-seoul", viewModel.SelectedMarker?.RegionKey);

        viewModel.ProductName = "  사과  ";
        Assert.True(await viewModel.검색Async());

        Assert.Equal("사과", client.Query?.ProductName);
        Assert.Equal(200, client.Query?.MaxItems);
        Assert.Single(viewModel.Markers);
        Assert.Contains("실제 농장 위치가 아닙니다.", viewModel.Notices);

        Assert.True(await viewModel.국가선택Async(RegionalAgriculturalMapCountryCodes.UnitedStates));
        Assert.Equal(RegionalAgriculturalMapCountryCodes.UnitedStates, client.Query?.CountryCode);
        Assert.Equal("us-ca", viewModel.SelectedMarker?.RegionKey);
        Assert.Equal("미국", viewModel.CountryName);

        Assert.True(await viewModel.관계선택Async(
            RegionalAgriculturalMapRelationTypeCodes.MarketObservation));
        Assert.Equal(
            RegionalAgriculturalMapRelationTypeCodes.MarketObservation,
            client.Query?.RelationTypeCode);
    }

    [Fact]
    public void 초기국가설정은_지원하지않는국가를_한국으로되돌린다()
    {
        using var viewModel = new 지역농수산MapPageViewModel(new FakePublicDataClient());

        viewModel.초기국가설정(" us ");
        Assert.Equal(RegionalAgriculturalMapCountryCodes.UnitedStates, viewModel.CountryCode);

        viewModel.초기국가설정("CN");
        Assert.Equal(RegionalAgriculturalMapCountryCodes.Korea, viewModel.CountryCode);
    }

    private sealed class FakePublicDataClient : I농수산공공데이터Client
    {
        public RegionalAgriculturalMapMarkerQuery? Query { get; private set; }

        public Task<RegionalAgriculturalMapMarkerListResponse> 지역MapMarker조회Async(
            RegionalAgriculturalMapMarkerQuery query,
            CancellationToken cancellationToken = default)
        {
            Query = query;
            var isUnitedStates = query.CountryCode == RegionalAgriculturalMapCountryCodes.UnitedStates;
            var source = new RegionalAgriculturalMapMarkerSourceDto(
                isUnitedStates ? "usda-ams-market-news" : "mafra-wholesale-market-settlement",
                isUnitedStates
                    ? RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesPostalState
                    : RegionalAgriculturalMapCodeSchemeCodes.KoreaMafraOrigin,
                isUnitedStates ? "CA" : "11",
                isUnitedStates ? "California" : "서울",
                RegionalAgriculturalMapConfidenceCodes.OfficialCodeCrosswalk,
                new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                7,
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 31));
            var marker = new RegionalAgriculturalMapMarkerDto(
                isUnitedStates ? "us-ca:MarketObservation" : "kr-seoul:ConfirmedOrigin",
                isUnitedStates ? "us-ca" : "kr-seoul",
                query.CountryCode,
                RegionalAgriculturalMapRegionTypeCodes.StateProvince,
                isUnitedStates ? "캘리포니아" : "서울특별시",
                isUnitedStates ? "California" : "Seoul",
                isUnitedStates ? "California" : "서울특별시",
                isUnitedStates ? 36.7783m : 37.5665m,
                isUnitedStates ? -119.4179m : 126.9780m,
                isUnitedStates ? "US-CENSUS-GEOID" : "KR-SGIS-HADM",
                "2025",
                "https://example.test/official-region",
                new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                isUnitedStates
                    ? RegionalAgriculturalMapRelationTypeCodes.MarketObservation
                    : RegionalAgriculturalMapRelationTypeCodes.ConfirmedOrigin,
                7,
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 31),
                [source]);

            return Task.FromResult(new RegionalAgriculturalMapMarkerListResponse(
                query.CountryCode,
                query.RelationTypeCode is null
                    ? RegionalAgriculturalMapRelationTypeCodes.All
                    : [query.RelationTypeCode],
                query.ProductName,
                query.FromDate,
                query.ToDate,
                1,
                1,
                0,
                0,
                ["실제 농장 위치가 아닙니다."],
                [marker]));
        }

        public Task<AgriculturalFisheriesInformationOverviewResponse> 개요조회Async(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<AgriculturalFisheriesItemSearchResponse> 국내품목조회Async(
            string? query = null,
            string? categoryCode = null,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<AgriculturalFisheriesDomesticPriceResponse> 국내가격조회Async(
            string hsCode,
            int lookbackDays = 14,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<미국농수산가격조회응답> 미국가격조회Async(
            string commodity,
            string program,
            int yearFrom,
            int yearTo,
            int maxItems = 100,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<호주농수산식품가격Catalog응답> 호주가격원천Catalog조회Async(
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<호주농수산식품가격조회응답> 호주식품가격지수조회Async(
            호주농수산식품가격조회요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

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
