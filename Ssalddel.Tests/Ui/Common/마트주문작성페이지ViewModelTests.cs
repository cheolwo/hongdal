using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 마트주문작성페이지ViewModelTests
{
    [Fact]
    public async Task 작성은_안내확인전에는호출하지않고확인후멱등요청Id를전달한다()
    {
        var service = new FakeMartOrderRequestService();
        var viewModel = new 마트주문작성ViewModel(service) { 수량 = 3 };
        var initialRequestId = viewModel.클라이언트요청Id;

        Assert.False(await viewModel.등록Async(41));
        Assert.Null(service.LastSubmitRequest);

        viewModel.비구속주문요청확인 = true;
        Assert.True(await viewModel.등록Async(41));

        Assert.Equal(initialRequestId, service.LastSubmitRequest!.클라이언트요청Id);
        Assert.Equal(41, service.LastSubmitRequest.공개상품Id);
        Assert.Equal(3, service.LastSubmitRequest.수량);
        Assert.Equal(마트주문요청안내.현재버전, service.LastSubmitRequest.안내버전);
    }

    [Fact]
    public async Task 정확한요청이없어도다른요청으로대체하지않는다()
    {
        var requestId = Guid.NewGuid();
        var viewModel = new 마트주문요청상세ViewModel(
            new FakeMartOrderRequestService { DetailResponse = null });

        var succeeded = await viewModel.조회Async(requestId);

        Assert.True(succeeded);
        Assert.Equal(requestId, viewModel.요청Id);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    [Fact]
    public void 새요청준비는_작성상태와멱등요청Id를함께갱신한다()
    {
        var viewModel = new 마트주문작성ViewModel(new FakeMartOrderRequestService())
        {
            수량 = 4,
            비구속주문요청확인 = true
        };
        var previous = viewModel.클라이언트요청Id;

        viewModel.새요청준비();

        Assert.Equal(1, viewModel.수량);
        Assert.False(viewModel.비구속주문요청확인);
        Assert.NotEqual(previous, viewModel.클라이언트요청Id);
    }

    [Fact]
    public async Task 등록실패후_같은멱등요청Id로_다시시도할수있다()
    {
        var service = new FakeMartOrderRequestService { FailNextSubmit = true };
        var viewModel = new 마트주문작성ViewModel(service)
        {
            수량 = 2,
            비구속주문요청확인 = true
        };
        var requestId = viewModel.클라이언트요청Id;

        Assert.False(await viewModel.등록Async(41));
        Assert.True(viewModel.오류발생);
        Assert.Equal(requestId, service.LastSubmitRequest!.클라이언트요청Id);

        Assert.True(await viewModel.등록Async(41));
        Assert.Equal(2, service.SubmitCallCount);
        Assert.Equal(requestId, service.LastSubmitRequest!.클라이언트요청Id);
    }

    private sealed class FakeMartOrderRequestService : I마트주문요청Service
    {
        public 마트주문요청등록요청? LastSubmitRequest { get; private set; }
        public 마트주문요청응답? DetailResponse { get; init; }
        public bool FailNextSubmit { get; set; }
        public int SubmitCallCount { get; private set; }

        public Task<마트주문요청응답> 등록Async(
            마트주문요청등록요청 request,
            CancellationToken cancellationToken = default)
        {
            SubmitCallCount++;
            LastSubmitRequest = request;
            if (FailNextSubmit)
            {
                FailNextSubmit = false;
                throw new InvalidOperationException("일시적인 저장 실패");
            }

            return Task.FromResult(new 마트주문요청응답 { 주문요청Id = Guid.NewGuid() });
        }

        public Task<마트주문요청응답?> 상세Async(
            Guid orderRequestId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(DetailResponse);
    }
}
