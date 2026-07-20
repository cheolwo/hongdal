using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 운송의뢰초안페이지ViewModelTests
{
    [Fact]
    public async Task 명시한출고예정Id만조회하고_초안에연결한다()
    {
        var service = new FakeService { Detail = ReadyPlan(17) };
        var page = Create(service);

        Assert.True(await page.초기화Async(17));
        Assert.Equal([17L], service.DetailIds);
        Assert.Equal(17, page.초안.원장!.OutboundPlanId);
        Assert.True(page.초안.작성가능);
    }

    [Fact]
    public async Task 검토조건을통과하지못한원장은_입력검토를차단한다()
    {
        var service = new FakeService { Detail = ReadyPlan(3, canStart: false) };
        var page = Create(service);
        await page.초기화Async(3);

        Assert.False(page.초안.입력값검토());
        Assert.Contains("검토 조건", page.초안.검증오류);
        Assert.Null(page.초안.검토결과);
    }

    [Fact]
    public async Task 유효한입력은_서버저장없이로컬검토결과만만든다()
    {
        var service = new FakeService { Detail = ReadyPlan(9) };
        var page = Create(service);
        await page.초기화Async(9);
        page.초안.하차지요약 = "서울 동부 공동 수령지";
        page.초안.희망상차일 = new DateTime(2026, 7, 21);
        page.초안.희망상차시각 = new TimeSpan(9, 0, 0);
        page.초안.희망도착일 = new DateTime(2026, 7, 21);
        page.초안.희망도착시각 = new TimeSpan(11, 30, 0);
        page.초안.차량유형 = "1톤 냉장탑차";
        page.초안.상품수량확인 = true;

        Assert.True(page.초안.입력값검토());
        Assert.Equal("OUT-000009-LOCAL", page.초안.검토결과!.DraftReference);
        Assert.Equal("서울 동부 공동 수령지", page.초안.검토결과.DestinationSummary);
        Assert.Equal([9L], service.DetailIds);
    }

    [Fact]
    public void 도착일시가상차일시보다이르면_검토를거부한다()
    {
        var draft = new 운송의뢰초안작성ViewModel();
        draft.원장설정(ReadyPlan(1));
        draft.하차지요약 = "서울 동부 공동 수령지";
        draft.희망상차일 = new DateTime(2026, 7, 21);
        draft.희망상차시각 = new TimeSpan(12, 0, 0);
        draft.희망도착일 = new DateTime(2026, 7, 21);
        draft.희망도착시각 = new TimeSpan(11, 0, 0);
        draft.차량유형 = "1톤 냉장탑차";
        draft.상품수량확인 = true;

        Assert.False(draft.입력값검토());
        Assert.Contains("뒤여야", draft.검증오류);
    }

    private static 운송의뢰초안PageViewModel Create(FakeService service)
        => new(new(service), new());

    private static 출고예정검토상세응답 ReadyPlan(long id, bool canStart = true)
        => new()
        {
            OutboundPlanId = id,
            ProductName = "냉장 감자",
            Quantity = 9,
            CanStartTransportRequestDraft = canStart,
            ReviewStatus = canStart ? "초안 입력 가능" : "원장 보완 필요"
        };

    private sealed class FakeService : I출고예정검토페이지Service
    {
        public 출고예정검토상세응답? Detail { get; set; }
        public List<long> DetailIds { get; } = [];
        public Task<출고예정검토목록페이지응답> 목록조회Async(출고예정검토목록조회요청 request, CancellationToken cancellationToken = default)
            => Task.FromResult(new 출고예정검토목록페이지응답());
        public Task<출고예정검토상세응답?> 상세조회Async(long outboundPlanId, CancellationToken cancellationToken = default)
        {
            DetailIds.Add(outboundPlanId);
            return Task.FromResult(Detail);
        }
    }
}
