using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class GroupPurchasePracticeViewModelTests
{
    [Fact]
    public async Task 시작과다음라운드는_같은무저장세션을이어간다()
    {
        var client = new RecordingPracticeClient();
        var viewModel = new GroupPurchasePracticeViewModel(client);

        await viewModel.InitializeAsync();
        await viewModel.StartAsync();
        await viewModel.AdvanceAsync();

        Assert.Equal(2, client.Requests.Count);
        Assert.Equal(0, client.Requests[0].라운드);
        Assert.Equal(1, client.Requests[1].라운드);
        Assert.Equal("practice-session", client.Requests[1].세션Id);
        Assert.True(viewModel.HasStarted);
        Assert.True(viewModel.CanAdvance);
    }

    [Fact]
    public async Task 시나리오를바꾸면_이전결과를버리고기본수량으로돌아간다()
    {
        var client = new RecordingPracticeClient();
        var viewModel = new GroupPurchasePracticeViewModel(client);

        await viewModel.InitializeAsync();
        await viewModel.StartAsync();
        viewModel.SelectScenario("scenario-2");

        Assert.Null(viewModel.Result);
        Assert.Equal(4, viewModel.DesiredQuantity);
    }

    private sealed class RecordingPracticeClient : I공동구매체험Client
    {
        public List<공동구매체험요청> Requests { get; } = [];

        public Task<IReadOnlyList<공동구매체험시나리오응답>> 시나리오목록Async(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<공동구매체험시나리오응답>>(
            [
                Scenario("scenario-1", 2),
                Scenario("scenario-2", 4)
            ]);

        public Task<공동구매체험응답?> 시뮬레이션Async(
            공동구매체험요청 request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(new 공동구매체험요청
            {
                세션Id = request.세션Id,
                시나리오Id = request.시나리오Id,
                내희망수량 = request.내희망수량,
                라운드 = request.라운드,
                대화주제코드 = request.대화주제코드
            });
            return Task.FromResult<공동구매체험응답?>(new()
            {
                세션Id = "practice-session",
                현재라운드 = request.라운드,
                최대라운드 = 3,
                완료여부 = false
            });
        }

        private static 공동구매체험시나리오응답 Scenario(string id, decimal quantity)
            => new()
            {
                시나리오Id = id,
                기본희망수량 = quantity
            };
    }
}
