using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityPageRoutesTests
{
    [Fact]
    public void 게시판과글쓰기경로는_선택문맥을encode한다()
    {
        Assert.Equal(
            "/community/boards?boardKey=free%20life",
            CommunityPageRoutes.BoardsFor(boardKey: "free life"));
        Assert.Equal(
            $"/community/write?board={Uri.EscapeDataString("자유·생활")}",
            CommunityPageRoutes.ComposeFor("자유·생활"));
        Assert.Equal(CommunityPageRoutes.Compose, CommunityPageRoutes.ComposeFor("전체"));
    }

    [Fact]
    public void 영속글과추천sample글은_서로다른상세route를사용한다()
    {
        Assert.Equal(
            $"/community/posts/42?board={Uri.EscapeDataString("업무 질문")}",
            CommunityPageRoutes.PostDetailFor(42, "업무 질문"));
        Assert.Equal(
            $"/community/posts/recommended/detail?seed={Uri.EscapeDataString("추천 글")}&board={Uri.EscapeDataString("자유")}",
            CommunityPageRoutes.RecommendedPostDetailFor("추천 글", "자유"));
    }

    [Fact]
    public void 다이어그램경로는_원장template문맥을보존한다()
    {
        Assert.Equal(
            "/community/workspace?diagram=true&ledgerTemplate=group-purchase",
            CommunityPageRoutes.DiagramFor("group-purchase"));
        Assert.Equal(
            "/community/ledgers/new?ledgerTemplate=group-purchase",
            CommunityPageRoutes.LedgerDraftFor("group-purchase"));
        Assert.Equal("/community/boards/manage", CommunityPageRoutes.BoardManagement);
    }

    [Fact]
    public void 공동구매단계는_stableCampaignId의_독립route를사용한다()
    {
        var campaignId = Guid.Parse("5b7d7c34-8e2c-49c7-9d17-fda506c53ce7");
        var root = $"/community/group-purchase/{campaignId:D}";

        Assert.Equal("/community/group-purchase", CommunityPageRoutes.GroupPurchase);
        Assert.Equal("/community/group-purchase/new", CommunityPageRoutes.GroupPurchaseCreate);
        Assert.Equal("/community/actions", CommunityPageRoutes.CollectiveActions);
        Assert.Equal(root, CommunityPageRoutes.GroupPurchaseDetailFor(campaignId));
        Assert.Equal($"{root}/participation", CommunityPageRoutes.GroupPurchaseParticipationFor(campaignId));
        Assert.Equal($"{root}/suppliers", CommunityPageRoutes.GroupPurchaseSuppliersFor(campaignId));
        Assert.Equal($"{root}/negotiation", CommunityPageRoutes.GroupPurchaseNegotiationFor(campaignId));
        Assert.Equal($"{root}/objections?stage=resolution", CommunityPageRoutes.GroupPurchaseObjectionsFor(campaignId, "resolution"));
        Assert.Equal($"{root}/resolution", CommunityPageRoutes.GroupPurchaseResolutionFor(campaignId));
        Assert.Equal($"{root}/signature", CommunityPageRoutes.GroupPurchaseSignatureFor(campaignId));
        Assert.Equal($"{root}/delivery-options", CommunityPageRoutes.GroupPurchaseDeliveryOptionsFor(campaignId));
        Assert.Equal($"{root}/fulfillment-draft", CommunityPageRoutes.GroupPurchaseFulfillmentDraftFor(campaignId));
    }

    [Fact]
    public void 공동구매route는_빈CampaignId를허용하지않는다()
        => Assert.Throws<ArgumentException>(() =>
            CommunityPageRoutes.GroupPurchaseDetailFor(Guid.Empty));
}
