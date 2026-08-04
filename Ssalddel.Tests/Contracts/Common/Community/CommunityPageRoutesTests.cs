using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityPageRoutesTests
{
    [Fact]
    public void 세계지도관측경로는_국가_레이어_StableId와게시당시Version을보존한다()
    {
        var route = CommunityPageRoutes.WorldMapFor(
            CommunityPageRoutes.WorldMapDayWorkDataset,
            "kr",
            커뮤니티세계지도LayerCodes.KosisStatisticalContext,
            "kosis:cpi:kr",
            "kosis:cpi:kr",
            "snapshot-revision-1",
            "source-version-1");

        Assert.Equal(
            "/community/home?country=KR"
            + "&layers=kosis-statistical-context"
            + "&marker=kosis%3Acpi%3Akr"
            + "&observation=kosis%3Acpi%3Akr"
            + "&snapshot=snapshot-revision-1"
            + "&sourceVersion=source-version-1",
            route);
    }

    [Fact]
    public void 커뮤니티홈은_전체피드와게시판탐색상태를보존한다()
    {
        Assert.Equal(
            "/community?view=feed",
            CommunityPageRoutes.HomeFor(CommunityPageRoutes.HomeFeedView));
        Assert.Equal(
            "/community?view=boards&mode=work",
            CommunityPageRoutes.HomeFor(CommunityPageRoutes.HomeBoardView, "work"));
        Assert.Equal(CommunityPageRoutes.Home, CommunityPageRoutes.HomeFor("unknown", "life"));
    }

    [Fact]
    public void 게시판과글쓰기경로는_선택문맥을encode한다()
    {
        Assert.Equal("/community/boards/directory", CommunityPageRoutes.BoardDirectory);
        Assert.Equal(
            "/community/boards?boardKey=free%20life",
            CommunityPageRoutes.BoardsFor(boardKey: "free life"));
        Assert.Equal(
            $"/community/write?board={Uri.EscapeDataString("자유·생활")}",
            CommunityPageRoutes.ComposeFor("자유·생활"));
        Assert.Equal(CommunityPageRoutes.Compose, CommunityPageRoutes.ComposeFor("전체"));
    }

    [Fact]
    public void 게시판경로는_검색필터보기focus를한문맥으로보존한다()
    {
        var focus = CommunityBoardNavigationContext.FocusForPost(42);
        var route = CommunityPageRoutes.BoardsFor(
            boardKey: "free life",
            workflowTag: "생활 협업",
            roleTag: "이웃",
            page: 3,
            search: "창고 경험",
            listFilter: "추천글",
            viewMode: CommunityBoardNavigationContext.CardViewMode,
            focusTarget: focus);

        Assert.Equal(
            $"/community/boards?boardKey=free%20life&workflowTag={Uri.EscapeDataString("생활 협업")}&roleTag={Uri.EscapeDataString("이웃")}&page=3&q={Uri.EscapeDataString("창고 경험")}&filter={Uri.EscapeDataString("추천글")}&view=cards&focus={focus}",
            route);
        Assert.Equal(42, CommunityBoardNavigationContext.PostIdFromFocus(focus));
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
    public void 상세와글쓰기는_정확한로컬목록returnPath를encode한다()
    {
        var returnPath = "/community/boards?boardKey=free&q=창고&filter=추천글&focus=community-post-42";

        Assert.Equal(
            $"/community/posts/42?boardKey=free&from={Uri.EscapeDataString(returnPath)}",
            CommunityPageRoutes.PostDetailFor(42, boardKey: "free", returnPath: returnPath));
        Assert.Equal(
            $"/community/write?boardKey=free&from={Uri.EscapeDataString(returnPath)}",
            CommunityPageRoutes.ComposeFor(boardKey: "free", returnPath: returnPath));
        Assert.Equal(
            $"/shipper/request?source=diagram-node&from={Uri.EscapeDataString("/diagram?node=수요 모집&zoom=120")}",
            PageNavigationContext.WithReturnPath(
                "/shipper/request?source=diagram-node",
                "/diagram?node=수요 모집&zoom=120"));
    }

    [Theory]
    [InlineData("https://outside.example/path")]
    [InlineData("//outside.example/path")]
    [InlineData("/\\outside")]
    [InlineData("/%5C%5Coutside")]
    public void 공용returnPath는_외부또는역슬래시경로를거부한다(string unsafePath)
    {
        Assert.Null(PageNavigationContext.NormalizeReturnPath(unsafePath));
        Assert.Equal(
            CommunityPageRoutes.Boards,
            PageNavigationContext.ResolveReturnPath(unsafePath, CommunityPageRoutes.Boards));
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
        Assert.Equal("/community/group-import", CommunityPageRoutes.GroupImport);
        Assert.Equal("/community/group-purchase/new", CommunityPageRoutes.GroupPurchaseCreate);
        Assert.Equal("/community/orders/new", CommunityPageRoutes.IndividualOrderStart);
        Assert.Equal("/community/orders", CommunityPageRoutes.IndividualOrders);
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

    [Fact]
    public void 꾸미기상점은_목록_상품_checkout의_canonicalRoute를분리한다()
    {
        const string productKey = "market-theme-한글";
        var escapedKey = Uri.EscapeDataString(productKey);

        Assert.Equal("/community/me", CommunityPageRoutes.Personal);
        Assert.Equal("/community/decorations", CommunityPageRoutes.Decorations);
        Assert.Equal(
            $"/community/decorations/products/{escapedKey}",
            CommunityPageRoutes.DecorationProductFor(productKey));
        Assert.Equal(
            $"/community/decorations/checkout/{escapedKey}",
            CommunityPageRoutes.DecorationCheckoutFor(productKey));
        Assert.Equal(
            $"/community/decorations/{escapedKey}",
            CommunityPageRoutes.LegacyDecorationProductFor(productKey));
        Assert.Equal(
            $"/community/decorations/{escapedKey}/checkout",
            CommunityPageRoutes.LegacyDecorationCheckoutFor(productKey));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("pack/child")]
    [InlineData("pack?next=checkout")]
    [InlineData("pack#preview")]
    [InlineData("pack\\child")]
    public void 꾸미기상품key는_빈값과경로구분자를거부한다(string productKey)
    {
        Assert.Throws<ArgumentException>(() => CommunityPageRoutes.DecorationProductFor(productKey));
        Assert.Throws<ArgumentException>(() => CommunityPageRoutes.DecorationCheckoutFor(productKey));
    }
}
