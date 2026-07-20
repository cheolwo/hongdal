using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 재고현황페이지ViewModelTests
{
    [Fact]
    public async Task 초기화는_목록첫항목을자동선택하지않는다()
    {
        var service = new FakeService { ListResponse = new() { Items = [new() { InboundItemId = 11 }] } };
        var page = CreatePage(service);

        var loaded = await page.초기화Async();

        Assert.True(loaded);
        Assert.Null(page.상세.조회대상Id);
        Assert.Null(page.상세.항목);
        Assert.Empty(service.DetailRequests);
    }

    [Fact]
    public async Task 주소Id초기화는_같은입고상품Id의상세만요청한다()
    {
        var service = new FakeService { DetailResponse = new() { InboundItemId = 71 } };
        var page = CreatePage(service);

        var loaded = await page.초기화Async(71);

        Assert.True(loaded);
        Assert.Equal([71L], service.DetailRequests);
        Assert.Equal(71, page.상세.항목!.InboundItemId);
    }

    [Fact]
    public async Task 검색은_조건을정규화하고_첫페이지로조회한다()
    {
        var service = new FakeService();
        var page = CreatePage(service);
        page.목록.검색어 = "  감자  ";
        page.목록.조회상태 = 창고재고조회상태코드.가용;

        await page.검색Async();

        var request = Assert.Single(service.ListRequests);
        Assert.Equal("감자", request.Search);
        Assert.Equal(창고재고조회상태코드.가용, request.Status);
        Assert.Equal(0, request.Page);
    }

    private static 재고현황PageViewModel CreatePage(FakeService service)
        => new(new 재고현황목록ViewModel(service), new 재고현황상세ViewModel(service));

    private sealed class FakeService : I재고현황페이지Service
    {
        public 창고재고현황목록페이지응답 ListResponse { get; set; } = new();
        public 창고재고현황상세응답? DetailResponse { get; set; }
        public List<창고재고현황목록조회요청> ListRequests { get; } = [];
        public List<long> DetailRequests { get; } = [];

        public Task<창고재고현황목록페이지응답> 목록조회Async(창고재고현황목록조회요청 request, CancellationToken cancellationToken = default)
        {
            ListRequests.Add(request);
            return Task.FromResult(ListResponse);
        }

        public Task<창고재고현황상세응답?> 상세조회Async(long inboundItemId, CancellationToken cancellationToken = default)
        {
            DetailRequests.Add(inboundItemId);
            return Task.FromResult(DetailResponse);
        }
    }
}
