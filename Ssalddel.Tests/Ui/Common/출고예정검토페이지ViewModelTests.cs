using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 출고예정검토페이지ViewModelTests
{
    [Fact]
    public async Task 초기화는_첫출고예정을자동선택하지않는다()
    {
        var service = new FakeService { List = new() { Items = [new() { OutboundPlanId = 1 }] } };
        var page = Create(service);

        Assert.True(await page.초기화Async());
        Assert.Null(page.상세.조회대상Id);
        Assert.Empty(service.DetailIds);
    }

    [Fact]
    public async Task 명시한출고예정Id만상세조회한다()
    {
        var service = new FakeService { Detail = new() { OutboundPlanId = 17, ReviewStatus = "초안 입력 가능" } };
        var page = Create(service);

        Assert.True(await page.초기화Async(17));
        Assert.Equal([17L], service.DetailIds);
        Assert.Equal(17, page.상세.항목!.OutboundPlanId);
    }

    [Fact]
    public async Task 검색은_상태와검색어를첫페이지로전달한다()
    {
        var service = new FakeService();
        var page = Create(service);
        page.목록.검색어 = " 감자 ";
        page.목록.조회상태 = 출고예정검토조회상태코드.운송연결;

        await page.검색Async();

        var request = Assert.Single(service.Requests);
        Assert.Equal("감자", request.Search);
        Assert.Equal(출고예정검토조회상태코드.운송연결, request.Status);
        Assert.Equal(0, request.Page);
    }

    private static 출고예정검토PageViewModel Create(FakeService service)
        => new(new(service), new(service));

    private sealed class FakeService : I출고예정검토페이지Service
    {
        public 출고예정검토목록페이지응답 List { get; set; } = new();
        public 출고예정검토상세응답? Detail { get; set; }
        public List<출고예정검토목록조회요청> Requests { get; } = [];
        public List<long> DetailIds { get; } = [];

        public Task<출고예정검토목록페이지응답> 목록조회Async(출고예정검토목록조회요청 request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(List);
        }

        public Task<출고예정검토상세응답?> 상세조회Async(long outboundPlanId, CancellationToken cancellationToken = default)
        {
            DetailIds.Add(outboundPlanId);
            return Task.FromResult(Detail);
        }

        public Task<출고운송인계완료응답> 인계완료Async(
            long outboundPlanId,
            출고운송인계완료요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
