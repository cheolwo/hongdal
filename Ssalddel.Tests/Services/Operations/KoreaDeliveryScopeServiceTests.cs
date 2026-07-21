using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Services.External.Naver;
using Ssalddel.Services.Food;
using Ssalddel.Services.Operations;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.Operations;

public sealed class KoreaDeliveryScopeServiceTests
{
    [Fact]
    public async Task ResolveAsync_UsesAdministrativeCodeWithoutExposingDetailedAddress()
    {
        var address = new OperatingMarketAddressCandidate
        {
            MarketCode = OperatingMarketCodes.Korea,
            CountryCode = OperatingMarketCodes.Korea,
            FormattedAddress = "경기도 수원시 영통구 광교로 123",
            AdministrativeAreaCode = "4111710500",
            ProviderCode = OperatingAddressProviderCodes.KoreaRoadNameAddress
        };
        var sut = CreateSut(
            address,
            new Kakao주소정보(
                address.FormattedAddress,
                address.FormattedAddress,
                "경기도",
                "수원시 영통구",
                "이의동",
                37.294m,
                127.046m),
            new NaverDistrictRegion("경기도", "수원시 영통구"));

        var result = await sut.ResolveAsync(new OperatingMarketDeliveryScopeResolveRequest
        {
            MarketCode = OperatingMarketCodes.Korea,
            Address = address.FormattedAddress,
            ParticipantCount = 3
        });

        Assert.True(result.Success);
        Assert.Equal("kr-admin2:41117", result.RecommendedScopeKey);
        Assert.Equal(
            OperatingAddressProviderCodes.KoreaRoadNameAddress,
            result.ProviderCode);
        var recruitment = Assert.Single(
            result.Items,
            item => item.ScopeTypeCode ==
                    OperatingDeliveryScopeTypeCodes.AdministrativeLevel2Recruitment);
        Assert.True(recruitment.IsRecommendedRecruitmentScope);
        Assert.True(recruitment.CanPublishForParticipantCount);
        Assert.Equal("41117", recruitment.GeographicAreaCode);
        var delivery = Assert.Single(
            result.Items,
            item => item.ScopeTypeCode ==
                    OperatingDeliveryScopeTypeCodes.AdministrativeLevel3Delivery);
        Assert.Equal("kr-admin3:4111710500", delivery.ScopeKey);
        Assert.Equal(recruitment.ScopeKey, delivery.ParentScopeKey);
        Assert.DoesNotContain("광교로", string.Join('|', result.Items.Select(item =>
            $"{item.ScopeKey}:{item.DisplayName}")));
        Assert.DoesNotContain("123", string.Join('|', result.Items.Select(item =>
            $"{item.ScopeKey}:{item.DisplayName}")));
    }

    [Fact]
    public async Task ResolveAsync_FallsBackToCoarseAddressStructureWhenProvidersAreUnavailable()
    {
        var sut = CreateSut(address: null, kakaoAddress: null, naverDistrict: null);

        var result = await sut.ResolveAsync(new OperatingMarketDeliveryScopeResolveRequest
        {
            MarketCode = OperatingMarketCodes.Korea,
            Address = "서울특별시 중구 세종대로 110"
        });

        Assert.True(result.Success);
        Assert.Equal("kr-admin2:서울특별시-중구", result.RecommendedScopeKey);
        Assert.Equal("AddressStructureFallback", result.ProviderCode);
        Assert.All(result.Items, item => Assert.DoesNotContain("110", item.ScopeKey));
    }

    [Fact]
    public async Task ResolveAsync_RejectsCrossMarketRequest()
    {
        var sut = CreateSut(address: null, kakaoAddress: null, naverDistrict: null);

        var result = await sut.ResolveAsync(new OperatingMarketDeliveryScopeResolveRequest
        {
            MarketCode = OperatingMarketCodes.UnitedStates,
            Address = "4600 Silver Hill Rd"
        });

        Assert.False(result.Success);
        Assert.Empty(result.Items);
    }

    private static KoreaDeliveryScopeService CreateSut(
        OperatingMarketAddressCandidate? address,
        Kakao주소정보? kakaoAddress,
        NaverDistrictRegion? naverDistrict)
        => new(
            new OperatingMarketDeployment(OperatingMarketCodes.Korea),
            new StubAddressLookupService(address),
            new 주문자집단배송권조회Service(),
            new StubKakaoGeocodingService(kakaoAddress),
            new StubNaverReverseGeocodingService(naverDistrict));

    private sealed class StubAddressLookupService(
        OperatingMarketAddressCandidate? candidate) : IOperatingMarketAddressLookupService
    {
        public Task<OperatingMarketAddressSearchResult> SearchAsync(
            OperatingMarketAddressSearchRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new OperatingMarketAddressSearchResult
            {
                Success = candidate is not null,
                ProviderConfigured = candidate is not null,
                MarketCode = OperatingMarketCodes.Korea,
                ProviderCode = OperatingAddressProviderCodes.KoreaRoadNameAddress,
                Items = candidate is null ? [] : [candidate]
            });
    }

    private sealed class StubKakaoGeocodingService(
        Kakao주소정보? address) : IKakao좌표변환Service
    {
        public Task<(double 위도, double 경도)?> 도로명주소좌표변환Async(
            string 주소,
            CancellationToken cancellationToken = default)
            => Task.FromResult<(double 위도, double 경도)?>(null);

        public Task<Kakao주소정보?> 주소정보조회Async(
            string 주소,
            CancellationToken cancellationToken = default)
            => Task.FromResult(address);

        public Task<Kakao지역정보?> 좌표지역정보조회Async(
            decimal 위도,
            decimal 경도,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Kakao지역정보?>(null);
    }

    private sealed class StubNaverReverseGeocodingService(
        NaverDistrictRegion? district) : INaverMapsReverseGeocodingService
    {
        public Task<NaverDistrictRegion?> ResolveDistrictAsync(
            decimal latitude,
            decimal longitude,
            CancellationToken cancellationToken = default)
            => Task.FromResult(district);
    }
}
