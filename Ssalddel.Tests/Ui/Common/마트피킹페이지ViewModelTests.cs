using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 마트피킹페이지ViewModelTests
{
    [Fact]
    public async Task 목록은_검색창고상태와페이지를서버요청에전달한다()
    {
        var service = new FakePickingService
        {
            ListResponse = new 마트피킹주문목록응답
            {
                TotalCount = 13,
                Page = 2,
                PageSize = 12,
                Items = [new 마트피킹주문요약응답 { 주문Id = 4, 주문참조번호 = "MART-4" }]
            }
        };
        var viewModel = new 마트피킹주문목록ViewModel(service)
        {
            검색어 = "생수",
            창고Id = 17,
            작업상태 = 마트피킹작업상태코드.진행중
        };

        Assert.True(await viewModel.조회Async());
        Assert.True(await viewModel.페이지조회Async(2));

        Assert.Equal("생수", service.LastRequest!.검색어);
        Assert.Equal(17, service.LastRequest.창고Id);
        Assert.Equal(마트피킹작업상태코드.진행중, service.LastRequest.작업상태);
        Assert.Equal(2, service.LastRequest.Page);
        Assert.Equal(13, viewModel.전체건수);
        Assert.Equal(2, viewModel.총페이지수);
    }

    [Fact]
    public async Task 정확한상세가없어도다른주문이나첫주문으로대체하지않는다()
    {
        var viewModel = new 마트피킹주문상세ViewModel(
            new FakePickingService { DetailResponse = null });

        var succeeded = await viewModel.조회Async(73);

        Assert.True(succeeded);
        Assert.Equal(73, viewModel.요청OrderId);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    [Fact]
    public async Task PageViewModel은_목록후명시한OrderId상세만조회한다()
    {
        var service = new FakePickingService
        {
            ListResponse = new 마트피킹주문목록응답(),
            DetailResponse = new 마트피킹주문상세응답 { 주문Id = 31 }
        };
        var page = new 마트피킹작업PageViewModel(
            new 마트피킹주문목록ViewModel(service),
            new 마트피킹주문상세ViewModel(service));

        var succeeded = await page.초기화Async(31);

        Assert.True(succeeded);
        Assert.Equal(31, service.LastOrderId);
        Assert.Equal(31, page.상세.상세?.주문Id);
    }

    private sealed class FakePickingService : I마트피킹읽기Service
    {
        public 마트피킹주문목록응답 ListResponse { get; init; } = new();
        public 마트피킹주문상세응답? DetailResponse { get; init; }
        public 마트피킹주문목록조회요청? LastRequest { get; private set; }
        public long? LastOrderId { get; private set; }

        public Task<마트피킹주문목록응답> 목록Async(
            마트피킹주문목록조회요청 request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(ListResponse);
        }

        public Task<마트피킹주문상세응답?> 상세Async(
            long orderId,
            CancellationToken cancellationToken = default)
        {
            LastOrderId = orderId;
            return Task.FromResult(DetailResponse);
        }
    }
}
