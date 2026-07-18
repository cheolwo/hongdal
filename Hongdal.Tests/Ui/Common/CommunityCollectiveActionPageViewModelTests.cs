using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class CommunityCollectiveActionPageViewModelTests
{
    [Fact]
    public void 알수없는PageKey는_마음모으기로정규화한다()
    {
        var campaignId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        Assert.Equal(
            CommunityCollectiveActionPageKeys.Gathering,
            CommunityCollectiveActionPageKeys.Normalize("unknown"));
        Assert.Equal(
            $"/community/actions/in-progress?campaignId={campaignId:D}",
            CommunityCollectiveActionRoutes.Build(CommunityCollectiveActionPageKeys.InProgress, campaignId));
        Assert.Equal("/community/posts/31", CommunityCollectiveActionRoutes.BuildSourcePost(31));
        Assert.Equal("/community", CommunityCollectiveActionRoutes.BuildSourcePost(null));
    }

    [Fact]
    public void 추가참여ViewModel은_모든필수근거의최솟값으로_확인여력을계산한다()
    {
        var snapshot = CommunityCollectiveActionPreviewCatalog.Create()
            .Single(item => item.CurrentPageKey == CommunityCollectiveActionPageKeys.InProgress);
        var viewModel = new CommunityActionExecutionViewModel();

        viewModel.Apply(snapshot);
        viewModel.AdditionalQuantity = 8m;

        Assert.True(viewModel.AllRequiredCapacityConfirmed);
        Assert.Equal(52m, viewModel.ConfirmedMaximumTotalQuantity);
        Assert.Equal(8m, viewModel.ConfirmedRemainingQuantity);
        Assert.True(viewModel.SelectedQuantityFitsConfirmedCapacity);

        viewModel.AdditionalQuantity = 9m;

        Assert.False(viewModel.SelectedQuantityFitsConfirmedCapacity);
    }

    [Fact]
    public void 확인대기근거가있으면_추가수량을확정여력으로표시하지않는다()
    {
        var viewModel = new CommunityActionExecutionViewModel();
        viewModel.Apply(new CommunityCollectiveActionSnapshot
        {
            CurrentCommittedQuantity = 10m,
            CurrentPotentialQuantity = 12m,
            QuantityUnit = "상자",
            CapacityEvidence =
            [
                new("supply", "공급", "공급자", CommunityCapacityEvidenceStatus.Confirmed, 20m, "확인"),
                new("transport", "운송", "운송사", CommunityCapacityEvidenceStatus.Pending, 18m, "확인 중")
            ]
        });

        Assert.False(viewModel.AllRequiredCapacityConfirmed);
        Assert.Null(viewModel.ConfirmedMaximumTotalQuantity);
        Assert.Equal(0m, viewModel.ConfirmedRemainingQuantity);
        Assert.Equal(6m, viewModel.EstimatedRemainingQuantity);
        Assert.Contains("확인하고", viewModel.CapacityHeadline);
    }

    [Fact]
    public async Task PageViewModel은_하위ViewModel을조립하고_빈Feed에서는둘러보기상태를연다()
    {
        using var page = CreatePage(new FakeSource([]));
        page.Configure(CommunityCollectiveActionPageKeys.InProgress, null);

        var initialized = await page.초기화Async();

        Assert.True(initialized);
        Assert.True(page.IsPreview);
        Assert.Equal(CommunityCollectiveActionPageKeys.InProgress, page.CurrentPage.Key);
        Assert.NotNull(page.SelectedAction);
        Assert.Equal(CommunityCollectiveActionPageKeys.InProgress, page.SelectedAction!.CurrentPageKey);
        Assert.Equal(8m, page.Execution.ConfirmedRemainingQuantity);
    }

    [Fact]
    public async Task 실제Campaign이있으면_예시가아닌실제Feed로표시한다()
    {
        var campaign = new CommunityVoteResponse
        {
            Id = Guid.NewGuid(),
            SourcePostId = 902,
            Title = "동네 쌀 공동구매",
            Description = "필요한 가구가 함께 수량을 모읍니다.",
            CommunityScope = "서울",
            Status = CommunityVoteStatusCodes.Open,
            TotalVoteCount = 5,
            Options =
            [
                new CommunityVoteOptionResponse
                {
                    OptionId = "rice",
                    Text = "쌀 10kg",
                    RequestedQuantity = 8,
                    QuantityUnit = "포"
                }
            ],
            GroupPurchase = new CommunityGroupPurchaseVoteResponse
            {
                MinimumTotalQuantity = 20,
                TotalRequestedQuantity = 8,
                QuantityUnit = "포",
                ShipFromCountryCode = "KR",
                DeliveryCountryCode = "KR"
            }
        };
        using var page = CreatePage(new FakeSource([campaign]));
        page.Configure(CommunityCollectiveActionPageKeys.Gathering, campaign.Id);

        await page.초기화Async();

        Assert.False(page.IsPreview);
        Assert.Equal(campaign.Id, page.SelectedAction?.Id);
        Assert.Equal(902, page.SelectedAction?.SourcePostId);
        Assert.Equal("동네 쌀 공동구매", page.SelectedAction?.Title);
        Assert.Equal(8m, page.Execution.CurrentCommittedQuantity);
    }

    private static CommunityCollectiveActionPageViewModel CreatePage(
        ICommunityCollectiveActionSource source)
        => new(
            source,
            new CommunityActionJourneyNavigationViewModel(),
            new CommunityActionCollectionViewModel(),
            new CommunityActionConditionsViewModel(),
            new CommunityActionPartyViewModel(),
            new CommunityActionReadinessViewModel(),
            new CommunityActionExecutionViewModel(),
            new CommunityActionOutcomeViewModel());

    private sealed class FakeSource(IReadOnlyList<CommunityVoteResponse> items)
        : ICommunityCollectiveActionSource
    {
        public Task<IReadOnlyList<CommunityVoteResponse>> LoadAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(items);
    }
}
