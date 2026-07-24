using Ssalddel.Contracts.Driver.Recommendation;
using Ssalddel.WebApp.Services;
using Ssalddel.WebApp.ViewModels;

namespace Ssalddel.Tests.WebApp;

public sealed class DriverRecommendationDecisionPageViewModelTests
{
    [Fact]
    public async Task InitializeAsync_선택_캐시에_없으면_서버_추천에서만_찾아_선택한다()
    {
        기사추천수신항목? selected = null;
        var fixture = new OperationsFixture
        {
            GetSelected = _ => null,
            LoadAll = _ => Task.FromResult<IReadOnlyList<기사추천수신항목>>(
                [Recommendation("REQ-20")]),
            Select = (item, _) => selected = item
        };
        using var viewModel = new DriverRecommendationDecisionPageViewModel(
            fixture.CreateOperations());

        await viewModel.InitializeAsync(" REQ-20 ");

        Assert.Equal("REQ-20", viewModel.Recommendation?.의뢰Id);
        Assert.Equal("REQ-20", selected?.의뢰Id);
        Assert.Equal(
            DriverRecommendationDecisionMessageTone.Info,
            viewModel.StatusTone);
    }

    [Fact]
    public async Task 만료는_표시만_하고_자동_거절이나_수락을_실행하지_않는다()
    {
        var fixture = new OperationsFixture();
        fixture.GetSelectedDeadline = () => fixture.Now.AddSeconds(-1);
        using var viewModel = new DriverRecommendationDecisionPageViewModel(
            fixture.CreateOperations());

        await viewModel.InitializeAsync("REQ-20");
        await viewModel.AcceptAsync();

        Assert.True(viewModel.IsExpired);
        Assert.Equal(0, fixture.AcceptCount);
        Assert.Equal(0, fixture.RejectCount);
        Assert.Equal(
            DriverRecommendationDecisionMessageTone.Warning,
            viewModel.StatusTone);
    }

    [Fact]
    public async Task RejectAsync_기사의_명시적_사유를_전송하고_목록으로_이동한다()
    {
        var fixture = new OperationsFixture();
        using var viewModel = new DriverRecommendationDecisionPageViewModel(
            fixture.CreateOperations());
        await viewModel.InitializeAsync("REQ-20");
        viewModel.RejectReason = "운행 종료 예정";

        await viewModel.RejectAsync();

        Assert.Equal(1, fixture.RejectCount);
        Assert.Equal("REQ-20", fixture.LastRequestId);
        Assert.Equal("운행 종료 예정", fixture.LastRejectReason);
        Assert.Equal("REQ-20", fixture.ClearedRequestId);
        Assert.Equal(DriverRoutes.Recommendations, fixture.NavigatedTo);
    }

    [Fact]
    public async Task AcceptAsync_추천_ID를_전송하고_현재_운송_조회로_이동한다()
    {
        var fixture = new OperationsFixture();
        using var viewModel = new DriverRecommendationDecisionPageViewModel(
            fixture.CreateOperations());
        await viewModel.InitializeAsync("REQ-20");

        await viewModel.AcceptAsync();

        Assert.Equal(1, fixture.AcceptCount);
        Assert.Equal("REQ-20", fixture.LastRequestId);
        Assert.Equal("REQ-20", fixture.ClearedRequestId);
        Assert.Equal(
            DriverRoutes.CurrentTransportFor("REQ-20"),
            fixture.NavigatedTo);
    }

    private static 기사추천수신항목 Recommendation(string requestId)
        => new()
        {
            의뢰Id = requestId,
            화물종류 = "공동구매 식자재",
            픽업지 = "서울 공동창고",
            하차지 = "인천 공동수령점",
            차량적합여부 = true
        };

    private sealed class OperationsFixture
    {
        public DateTimeOffset Now { get; } =
            new(2026, 7, 23, 3, 0, 0, TimeSpan.Zero);

        public int AcceptCount { get; private set; }
        public int RejectCount { get; private set; }
        public string? LastRequestId { get; private set; }
        public string? LastRejectReason { get; private set; }
        public string? ClearedRequestId { get; private set; }
        public string? NavigatedTo { get; private set; }

        public Func<string, 기사추천수신항목?> GetSelected { get; set; }
            = requestId => Recommendation(requestId);

        public Func<CancellationToken, Task<IReadOnlyList<기사추천수신항목>>> LoadAll { get; set; }
            = _ => Task.FromResult<IReadOnlyList<기사추천수신항목>>([]);

        public Action<기사추천수신항목, string> Select { get; set; }
            = (_, _) => { };

        public Func<DateTimeOffset?> GetSelectedDeadline { get; set; } =
            () => new DateTimeOffset(2026, 7, 23, 3, 1, 0, TimeSpan.Zero);

        public DriverRecommendationDecisionOperations CreateOperations()
            => new(
                GetSelected,
                LoadAll,
                Select,
                GetSelectedDeadline,
                () => 60,
                (requestId, _) =>
                {
                    AcceptCount++;
                    LastRequestId = requestId;
                    return Task.FromResult(
                        new 기사추천처리결과(
                            requestId,
                            "Accepted",
                            "수락"));
                },
                (requestId, reason, _) =>
                {
                    RejectCount++;
                    LastRequestId = requestId;
                    LastRejectReason = reason;
                    return Task.FromResult(
                        new 기사추천처리결과(
                            requestId,
                            "Rejected",
                            "거절"));
                },
                requestId => ClearedRequestId = requestId,
                href => NavigatedTo = href,
                () => Now,
                (_, cancellationToken) =>
                    Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken));
    }
}
