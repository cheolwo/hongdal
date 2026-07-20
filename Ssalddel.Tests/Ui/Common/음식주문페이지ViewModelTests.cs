using Ssalddel.Contracts.Food;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 음식주문페이지ViewModelTests
{
    [Fact]
    public async Task 인증ViewModel은_익명복원뒤로그인사용자세션을반영한다()
    {
        var service = new FakeAuthenticationService();
        var viewModel = new 주문자앱인증ViewModel(service);

        Assert.True(await viewModel.복원Async());
        Assert.True(viewModel.초기화됨);
        Assert.False(viewModel.로그인됨);

        Assert.True(await viewModel.로그인Async("orderer", "password"));
        Assert.True(viewModel.로그인됨);
        Assert.Equal("주문자", viewModel.현재사용자표시);
    }

    [Fact]
    public async Task 목록ViewModel은_검색상태와페이지를서버요청에전달한다()
    {
        var service = new FakeFoodOrderService
        {
            ListResponse = new 주문자음식주문목록응답
            {
                Items = [new 주문자음식주문요약응답 { 주문번호 = "FOOD-2" }],
                TotalCount = 13,
                Page = 2,
                PageSize = 12
            }
        };
        var viewModel = new 주문자음식주문목록ViewModel(service)
        {
            검색어 = "김밥",
            상태필터 = 음식주문상태코드.조리중
        };

        Assert.True(await viewModel.페이지조회Async(2));

        Assert.Equal("김밥", service.LastRequest!.검색어);
        Assert.Equal(음식주문상태코드.조리중, service.LastRequest.상태);
        Assert.Equal(2, service.LastRequest.Page);
        Assert.Equal(13, viewModel.전체건수);
        Assert.Equal(2, viewModel.총페이지수);
    }

    [Fact]
    public async Task 정확한상세가없어도다른주문으로대체하지않는다()
    {
        var viewModel = new 주문자음식주문상세ViewModel(
            new FakeFoodOrderService { DetailResponse = null });

        var succeeded = await viewModel.조회Async("FOOD-MISSING");

        Assert.True(succeeded);
        Assert.Equal("FOOD-MISSING", viewModel.요청OrderNo);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    private sealed class FakeAuthenticationService : I주문자앱인증Service
    {
        public Task<주문자앱인증결과> 복원Async(CancellationToken cancellationToken = default)
            => Task.FromResult(new 주문자앱인증결과(주문자앱세션상태.익명));

        public Task<주문자앱인증결과> 로그인Async(string userNameOrEmail, string password, CancellationToken cancellationToken = default)
            => Task.FromResult(new 주문자앱인증결과(new 주문자앱세션상태(true, "user-1", "주문자")));

        public Task 로그아웃Async(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeFoodOrderService : I주문자음식주문읽기Service
    {
        public 주문자음식주문목록응답 ListResponse { get; init; } = new();
        public 주문자음식주문상세응답? DetailResponse { get; init; }
        public 주문자음식주문목록조회요청? LastRequest { get; private set; }

        public Task<주문자음식주문목록응답> 목록Async(주문자음식주문목록조회요청 request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(ListResponse);
        }

        public Task<주문자음식주문상세응답?> 상세Async(string orderNo, CancellationToken cancellationToken = default)
            => Task.FromResult(DetailResponse);
    }
}
