using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 공동구매공개조회ViewModelTests
{
    [Fact]
    public async Task 목록ViewModel은_국내공동구매만_최신순으로관리한다()
    {
        var older = DomesticCampaign(1, new DateTime(2026, 7, 18, 0, 0, 0, DateTimeKind.Utc));
        var newer = DomesticCampaign(2, new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc));
        var groupImport = DomesticCampaign(3, new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc));
        groupImport.GroupPurchase!.TradeRouteCode = CommunityGroupPurchaseTradeRouteCodes.InboundGroupImportCandidate;
        var general = DomesticCampaign(4, new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc));
        general.VoteKind = CommunityVoteKindCodes.General;
        var service = new FakePublicReadService
        {
            ListResponse = new CommunityVoteListResponse
            {
                Items = [older, groupImport, general, newer]
            }
        };
        var viewModel = new 공동구매공개목록ViewModel(service);

        var result = await viewModel.조회Async();

        Assert.True(result);
        Assert.Equal([newer.Id, older.Id], viewModel.모집목록.Select(item => item.Id));
        Assert.True(viewModel.초기화됨);
        Assert.False(viewModel.비어있음);
    }

    [Fact]
    public async Task 상세ViewModel은_요청한campaignId와_연결의견만조회한다()
    {
        var campaign = DomesticCampaign(8, DateTime.UtcNow);
        campaign.SourcePostId = 71;
        var comment = new PlatformCommunityPostCommentResponse
        {
            Id = 99,
            Body = "공개 의견"
        };
        var service = new FakePublicReadService
        {
            DetailResponse = campaign,
            Comments = [comment]
        };
        var viewModel = new 공동구매공개상세ViewModel(service);

        var result = await viewModel.조회Async(campaign.Id);

        Assert.True(result);
        Assert.Equal(campaign.Id, service.RequestedCampaignId);
        Assert.Equal(71, service.RequestedPostId);
        Assert.Same(campaign, viewModel.공동구매);
        Assert.Same(comment, Assert.Single(viewModel.의견목록));
        Assert.False(viewModel.찾을수없음);
    }

    [Fact]
    public async Task 상세ViewModel은_없는명시Id를_첫목록항목으로대체하지않는다()
    {
        var requestedId = Guid.NewGuid();
        var service = new FakePublicReadService { DetailResponse = null };
        var viewModel = new 공동구매공개상세ViewModel(service);

        var result = await viewModel.조회Async(requestedId);

        Assert.True(result);
        Assert.Equal(requestedId, viewModel.요청CampaignId);
        Assert.Null(viewModel.공동구매);
        Assert.Empty(viewModel.의견목록);
        Assert.True(viewModel.찾을수없음);
        Assert.False(viewModel.오류발생);
    }

    [Fact]
    public async Task 상세ViewModel은_공동수입campaign을_국내공동구매로노출하지않는다()
    {
        var campaign = DomesticCampaign(11, DateTime.UtcNow);
        campaign.GroupPurchase!.TradeRouteCode = CommunityGroupPurchaseTradeRouteCodes.InboundGroupImportCandidate;
        var service = new FakePublicReadService { DetailResponse = campaign };
        var viewModel = new 공동구매공개상세ViewModel(service);

        Assert.True(await viewModel.조회Async(campaign.Id));
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.공동구매);
        Assert.Null(service.RequestedPostId);
    }

    private static CommunityVoteResponse DomesticCampaign(int seed, DateTime createdAtUtc)
        => new()
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{seed:D12}"),
            AppKey = "OrdererApp",
            VoteKind = CommunityVoteKindCodes.GroupPurchaseDemand,
            Title = $"공동구매 {seed}",
            CreatedAtUtc = createdAtUtc,
            GroupPurchase = new CommunityGroupPurchaseVoteResponse
            {
                TradeRouteCode = CommunityGroupPurchaseTradeRouteCodes.Domestic
            }
        };

    private sealed class FakePublicReadService : I공동구매공개조회Service
    {
        public CommunityVoteListResponse ListResponse { get; init; } = new();
        public CommunityVoteResponse? DetailResponse { get; init; }
        public IReadOnlyList<PlatformCommunityPostCommentResponse> Comments { get; init; } = [];
        public Guid? RequestedCampaignId { get; private set; }
        public long? RequestedPostId { get; private set; }

        public Task<CommunityVoteListResponse> 목록조회Async(
            string? communityScope = null,
            string? hsCode = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(ListResponse);

        public Task<CommunityVoteResponse?> 상세조회Async(
            Guid campaignId,
            CancellationToken cancellationToken = default)
        {
            RequestedCampaignId = campaignId;
            return Task.FromResult(DetailResponse);
        }

        public Task<IReadOnlyList<PlatformCommunityPostCommentResponse>> 의견조회Async(
            long postId,
            CancellationToken cancellationToken = default)
        {
            RequestedPostId = postId;
            return Task.FromResult(Comments);
        }
    }
}
