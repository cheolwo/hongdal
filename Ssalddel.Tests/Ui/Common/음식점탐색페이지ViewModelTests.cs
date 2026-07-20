using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Restaurants;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 음식점탐색페이지ViewModelTests
{
    [Fact]
    public async Task 탐색기준은_서버정책과권역을읽지만첫권역을자동선택하지않는다()
    {
        var restaurantService = new FakeRestaurantService
        {
            Scopes = [new 음식점탐색권역응답 { 배달권키 = "scope-1", 표시명 = "첫 권역" }]
        };
        var viewModel = new 음식점탐색기준ViewModel(new FakePolicyService(), restaurantService);

        var succeeded = await viewModel.준비Async();

        Assert.True(succeeded);
        Assert.True(viewModel.초기화됨);
        Assert.Equal(7d, viewModel.반경Km);
        Assert.Null(viewModel.선택배달권키);
        Assert.Null(viewModel.선택권역);
        Assert.False(viewModel.조회가능);
    }

    [Fact]
    public async Task 목록은_선택한권역반경검색과페이지를서버요청에전달한다()
    {
        var service = new FakeRestaurantService
        {
            ListResponse = new 음식점공개목록응답
            {
                TotalCount = 13,
                Page = 2,
                PageSize = 12,
                Items = [new 음식점공개요약응답 { Id = 4, 상호명 = "분식집" }]
            }
        };
        var viewModel = new 음식점공개목록ViewModel(service)
        {
            검색어 = "분식",
            주문가능만 = true
        };

        Assert.True(await viewModel.조회Async("scope-1", 7.5d));
        Assert.True(await viewModel.페이지조회Async(2));

        Assert.Equal("scope-1", service.LastRequest!.배달권키);
        Assert.Equal(7.5m, service.LastRequest.반경Km);
        Assert.Equal("분식", service.LastRequest.검색어);
        Assert.True(service.LastRequest.주문가능만);
        Assert.Equal(2, service.LastRequest.Page);
        Assert.Equal(13, viewModel.전체건수);
        Assert.Equal(2, viewModel.총페이지수);
    }

    [Fact]
    public async Task 정확한상세가없어도다른음식점으로대체하지않는다()
    {
        var viewModel = new 음식점공개상세ViewModel(new FakeRestaurantService { DetailResponse = null });

        var succeeded = await viewModel.조회Async(31);

        Assert.True(succeeded);
        Assert.Equal(31, viewModel.요청RestaurantId);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    private sealed class FakePolicyService : I음식점탐색정책읽기Service
    {
        public Task<RestaurantSearchPolicyDto> 조회Async(CancellationToken cancellationToken = default)
            => Task.FromResult(new RestaurantSearchPolicyDto
            {
                DefaultRadiusKm = 7,
                MinRadiusKm = 1,
                MaxRadiusKm = 10,
                RadiusStepKm = .5,
                QuickRadiusOptions = [3, 5, 7, 10]
            });
    }

    private sealed class FakeRestaurantService : I음식점공개읽기Service
    {
        public IReadOnlyList<음식점탐색권역응답> Scopes { get; init; } = [];
        public 음식점공개목록응답 ListResponse { get; init; } = new();
        public 음식점공개상세응답? DetailResponse { get; init; }
        public 음식점공개목록조회요청? LastRequest { get; private set; }

        public Task<IReadOnlyList<음식점탐색권역응답>> 권역목록Async(CancellationToken cancellationToken = default)
            => Task.FromResult(Scopes);

        public Task<음식점공개목록응답> 목록Async(음식점공개목록조회요청 request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(ListResponse);
        }

        public Task<음식점공개상세응답?> 상세Async(long restaurantId, CancellationToken cancellationToken = default)
            => Task.FromResult(DetailResponse);
    }
}
