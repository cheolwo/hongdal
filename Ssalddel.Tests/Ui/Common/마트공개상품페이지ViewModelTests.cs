using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 마트공개상품페이지ViewModelTests
{
    [Fact]
    public async Task 목록은_검색과판매가능조건및페이지를서버요청에전달한다()
    {
        var service = new FakeMartProductService
        {
            ListResponse = new 마트공개상품목록응답
            {
                TotalCount = 13,
                Page = 2,
                PageSize = 12,
                Items = [new 마트공개상품요약응답 { Id = 4, 상품명 = "생수" }]
            }
        };
        var viewModel = new 마트공개상품목록ViewModel(service)
        {
            검색어 = "생수",
            판매가능만 = true
        };

        Assert.True(await viewModel.조회Async());
        Assert.True(await viewModel.페이지조회Async(2));

        Assert.Equal("생수", service.LastRequest!.검색어);
        Assert.True(service.LastRequest.판매가능만);
        Assert.Equal(2, service.LastRequest.Page);
        Assert.Equal(13, viewModel.전체건수);
        Assert.Equal(2, viewModel.총페이지수);
    }

    [Fact]
    public async Task 정확한상세가없어도다른상품이나첫상품으로대체하지않는다()
    {
        var viewModel = new 마트공개상품상세ViewModel(
            new FakeMartProductService { DetailResponse = null });

        var succeeded = await viewModel.조회Async(41);

        Assert.True(succeeded);
        Assert.Equal(41, viewModel.요청ProductId);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    [Fact]
    public async Task 접근ViewModel은_마트기능비활성을별도상태로유지한다()
    {
        var viewModel = new 마트페이지접근ViewModel(new FakeAccessService(false));

        var succeeded = await viewModel.확인Async();

        Assert.True(succeeded);
        Assert.True(viewModel.기능비활성);
        Assert.False(viewModel.사용가능);
    }

    private sealed class FakeAccessService(bool enabled) : I마트페이지접근Service
    {
        public Task<bool> 기능활성여부Async(CancellationToken cancellationToken = default)
            => Task.FromResult(enabled);
    }

    private sealed class FakeMartProductService : I마트공개상품읽기Service
    {
        public 마트공개상품목록응답 ListResponse { get; init; } = new();
        public 마트공개상품상세응답? DetailResponse { get; init; }
        public 마트공개상품목록조회요청? LastRequest { get; private set; }

        public Task<마트공개상품목록응답> 목록Async(
            마트공개상품목록조회요청 request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(ListResponse);
        }

        public Task<마트공개상품상세응답?> 상세Async(
            long productId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(DetailResponse);
    }
}
