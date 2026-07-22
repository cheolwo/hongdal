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
    public void 다이어그램경로는_선택확대필터출발문맥을보존한다()
    {
        Assert.Equal(
            $"/diagram?ledgerTemplate=group-purchase&node={Uri.EscapeDataString("수요 모집")}&zoom=125&filter={Uri.EscapeDataString("수요")}&from={Uri.EscapeDataString("/community/boards?board=공동구매")}",
            CommunityPageRoutes.DiagramFor(
                "group-purchase",
                "수요 모집",
                125,
                "수요",
                "/community/boards?board=공동구매"));
        Assert.Equal("/diagram?ledgerTemplate=group-purchase", CommunityPageRoutes.DiagramFor("group-purchase"));
        Assert.Equal(
            "/community/ledgers/new?ledgerTemplate=group-purchase",
            CommunityPageRoutes.LedgerDraftFor("group-purchase"));
        Assert.Equal("/community/boards/manage", CommunityPageRoutes.BoardManagement);
    }

    [Fact]
    public void 다이어그램문맥은_zoom과외부returnPath를안전한범위로정규화한다()
    {
        Assert.Equal(
            $"/diagram?zoom=150&from={Uri.EscapeDataString(CommunityPageRoutes.Workspace)}",
            CommunityPageRoutes.DiagramFor(
                zoomPercent: 900,
                returnPath: "https://outside.example/redirect"));
        Assert.Equal(75, CommunityDiagramNavigationContext.NormalizeZoom(10));
        Assert.Equal(
            CommunityPageRoutes.Workspace,
            CommunityDiagramNavigationContext.NormalizeReturnPath("//outside.example"));
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
