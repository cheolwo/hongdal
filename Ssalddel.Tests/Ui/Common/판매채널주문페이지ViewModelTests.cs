using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 판매채널주문페이지ViewModelTests
{
    [Fact]
    public async Task 목록ViewModel은_검색필터와사용자페이지를서버요청으로변환한다()
    {
        var service = new StubOrderService
        {
            ListResponse = new 판매채널주문목록응답
            {
                Items = [new 판매채널주문요약응답 { OrderId = 41 }],
                TotalCount = 51,
                Page = 1,
                PageSize = 25
            }
        };
        var viewModel = new 판매채널주문목록PageViewModel(service)
        {
            검색어 = "캠핑",
            국내외구분 = CommerceChannelOrderSyncScopes.Overseas,
            출고상태 = "출고예정"
        };

        var succeeded = await viewModel.페이지조회Async(2);

        Assert.True(succeeded);
        Assert.NotNull(service.LastRequest);
        Assert.Equal(1, service.LastRequest.Page);
        Assert.Equal(25, service.LastRequest.PageSize);
        Assert.Equal("캠핑", service.LastRequest.Search);
        Assert.Equal(CommerceChannelOrderSyncScopes.Overseas, service.LastRequest.SyncScope);
        Assert.Equal("출고예정", service.LastRequest.Status);
        Assert.Equal(41, Assert.Single(viewModel.주문목록).OrderId);
        Assert.Equal(3, viewModel.총페이지수);
    }

    [Fact]
    public async Task 목록ViewModel은_원장없음과검색결과없음을구분한다()
    {
        var service = new StubOrderService { ListResponse = new 판매채널주문목록응답() };
        var empty = new 판매채널주문목록PageViewModel(service);
        var filtered = new 판매채널주문목록PageViewModel(service) { 검색어 = "없는 주문" };

        Assert.True(await empty.조회Async());
        Assert.True(await filtered.조회Async());

        Assert.True(empty.원장없음);
        Assert.False(empty.검색결과없음);
        Assert.True(filtered.검색결과없음);
        Assert.False(filtered.원장없음);
    }

    [Fact]
    public async Task 상세ViewModel은_요청한OrderId가없어도_다른주문으로대체하지않는다()
    {
        var service = new StubOrderService { DetailResponse = null };
        var viewModel = new 판매채널주문상세PageViewModel(service);

        var succeeded = await viewModel.조회Async(77);

        Assert.True(succeeded);
        Assert.Equal(77, service.LastDetailId);
        Assert.Equal(77, viewModel.요청OrderId);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    private sealed class StubOrderService : I판매채널주문읽기Service
    {
        public 판매채널주문목록응답 ListResponse { get; init; } = new();
        public 판매채널주문상세응답? DetailResponse { get; init; }
        public 판매채널주문목록조회요청? LastRequest { get; private set; }
        public long? LastDetailId { get; private set; }

        public Task<판매채널주문목록응답> 목록조회Async(
            판매채널주문목록조회요청 request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(ListResponse);
        }

        public Task<판매채널주문상세응답?> 상세조회Async(
            long orderId,
            CancellationToken cancellationToken = default)
        {
            LastDetailId = orderId;
            return Task.FromResult(DetailResponse);
        }
    }
}
