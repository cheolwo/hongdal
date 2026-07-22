namespace Ssalddel.Contracts.Common.Community;

/// <summary>
/// Web과 모바일이 같은 커뮤니티 화면 의미와 딥링크를 공유하기 위한 경로 계약입니다.
/// </summary>
public static class CommunityPageRoutes
{
    public const string Home = "/community";
    public const string Boards = "/community/boards";
    public const string BoardManagement = "/community/boards/manage";
    public const string Compose = "/community/write";
    public const string Workspace = "/community/workspace";
    public const string LedgerDraft = "/community/ledgers/new";
    public const string RecommendedPosts = "/community/posts/recommended";
    public const string RecommendedPostDetail = "/community/posts/recommended/detail";
    public const string CollectiveActions = "/community/actions";
    public const string Bagua = "/community/bagua";
    public const string GroupPurchase = "/community/group-purchase";
    public const string GroupPurchaseCreate = "/community/group-purchase/new";
    public const string Personal = "/community/me";
    public const string Decorations = "/community/decorations";
    public const string DecorationProducts = $"{Decorations}/products";
    public const string DecorationProductTemplate = $"{DecorationProducts}/{{ProductKey}}";
    public const string DecorationCheckout = $"{Decorations}/checkout";
    public const string DecorationCheckoutTemplate = $"{DecorationCheckout}/{{ProductKey}}";
    public const string LegacyDecorationProductTemplate = $"{Decorations}/{{ProductKey}}";
    public const string LegacyDecorationCheckoutTemplate = $"{Decorations}/{{ProductKey}}/checkout";
    public const string Diagram = "/diagram";

    public static string BoardsFor(
        string? boardName = null,
        string? boardKey = null,
        string? workflowTag = null,
        string? roleTag = null,
        int? page = null,
        string? search = null,
        string? listFilter = null,
        string? viewMode = null,
        string? focusTarget = null)
        => WithQuery(
            Boards,
            (CommunityBoardNavigationQueryNames.BoardKey, boardKey),
            (CommunityBoardNavigationQueryNames.BoardName, boardName),
            (CommunityBoardNavigationQueryNames.WorkflowTag, workflowTag),
            (CommunityBoardNavigationQueryNames.RoleTag, roleTag),
            (CommunityBoardNavigationQueryNames.Page, CommunityBoardNavigationContext.NormalizePage(page) <= 1
                ? null
                : CommunityBoardNavigationContext.NormalizePage(page).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            (CommunityBoardNavigationQueryNames.Search, CommunityBoardNavigationContext.NormalizeSearch(search)),
            (CommunityBoardNavigationQueryNames.ListFilter, CommunityBoardNavigationContext.NormalizeFilter(listFilter) == "전체글"
                ? null
                : CommunityBoardNavigationContext.NormalizeFilter(listFilter)),
            (CommunityBoardNavigationQueryNames.ViewMode, CommunityBoardNavigationContext.NormalizeViewMode(viewMode) == CommunityBoardNavigationContext.ListViewMode
                ? null
                : CommunityBoardNavigationContext.CardViewMode),
            (PageNavigationQueryNames.FocusTarget, PageNavigationContext.NormalizeFocusTarget(focusTarget)));

    public static string ComposeFor(
        string? boardName = null,
        string? boardKey = null,
        string? returnPath = null)
        => WithQuery(
            Compose,
            (CommunityBoardNavigationQueryNames.BoardKey, boardKey),
            (CommunityBoardNavigationQueryNames.BoardName,
                string.IsNullOrWhiteSpace(boardName)
                || boardName.Equals("전체", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : boardName),
            (PageNavigationQueryNames.ReturnPath, PageNavigationContext.NormalizeReturnPath(returnPath)));

    public static string PostDetailFor(
        long postId,
        string? boardName = null,
        string? boardKey = null,
        string? returnPath = null)
        => WithQuery(
            $"/community/posts/{postId}",
            (CommunityBoardNavigationQueryNames.BoardKey, boardKey),
            (CommunityBoardNavigationQueryNames.BoardName, boardName),
            (PageNavigationQueryNames.ReturnPath, PageNavigationContext.NormalizeReturnPath(returnPath)));

    public static string RecommendedPostsFor(
        string? boardName = null,
        string? boardKey = null,
        string? returnPath = null)
        => WithQuery(
            RecommendedPosts,
            (CommunityBoardNavigationQueryNames.BoardKey, boardKey),
            (CommunityBoardNavigationQueryNames.BoardName, boardName),
            (PageNavigationQueryNames.ReturnPath, PageNavigationContext.NormalizeReturnPath(returnPath)));

    public static string RecommendedPostDetailFor(
        string seedPostTitle,
        string? boardName = null,
        string? boardKey = null,
        string? returnPath = null)
        => WithQuery(
            RecommendedPostDetail,
            ("seed", seedPostTitle),
            (CommunityBoardNavigationQueryNames.BoardKey, boardKey),
            (CommunityBoardNavigationQueryNames.BoardName, boardName),
            (PageNavigationQueryNames.ReturnPath, PageNavigationContext.NormalizeReturnPath(returnPath)));

    public static string DiagramFor(
        string? ledgerTemplateKey = null,
        string? selectedNode = null,
        int? zoomPercent = null,
        string? filter = null,
        string? returnPath = null)
        => WithQuery(
            Diagram,
            (CommunityDiagramNavigationQueryNames.LedgerTemplate, ledgerTemplateKey),
            (CommunityDiagramNavigationQueryNames.SelectedNode, selectedNode),
            (CommunityDiagramNavigationQueryNames.Zoom, zoomPercent is null
                ? null
                : CommunityDiagramNavigationContext.NormalizeZoom(zoomPercent).ToString(System.Globalization.CultureInfo.InvariantCulture)),
            (CommunityDiagramNavigationQueryNames.Filter, filter),
            (CommunityDiagramNavigationQueryNames.ReturnPath, string.IsNullOrWhiteSpace(returnPath)
                ? null
                : CommunityDiagramNavigationContext.NormalizeReturnPath(returnPath)));

    public static string LedgerDraftFor(string? ledgerTemplateKey = null)
        => WithQuery(LedgerDraft, ("ledgerTemplate", ledgerTemplateKey));

    public static string GroupPurchaseDetailFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId);

    public static string GroupPurchaseParticipationFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId, "participation");

    public static string GroupPurchaseSuppliersFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId, "suppliers");

    public static string GroupPurchaseNegotiationFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId, "negotiation");

    public static string GroupPurchaseObjectionsFor(Guid campaignId, string? stageCode = null)
        => WithQuery(
            GroupPurchaseCampaignRoute(campaignId, "objections"),
            ("stage", stageCode));

    public static string GroupPurchaseResolutionFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId, "resolution");

    public static string GroupPurchaseSignatureFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId, "signature");

    public static string GroupPurchaseDeliveryOptionsFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId, "delivery-options");

    public static string GroupPurchaseFulfillmentDraftFor(Guid campaignId)
        => GroupPurchaseCampaignRoute(campaignId, "fulfillment-draft");

    public static string DecorationProductFor(string productKey)
        => $"{DecorationProducts}/{EscapePathKey(productKey, nameof(productKey))}";

    public static string DecorationCheckoutFor(string productKey)
        => $"{DecorationCheckout}/{EscapePathKey(productKey, nameof(productKey))}";

    public static string LegacyDecorationProductFor(string productKey)
        => $"{Decorations}/{EscapePathKey(productKey, nameof(productKey))}";

    public static string LegacyDecorationCheckoutFor(string productKey)
        => $"{LegacyDecorationProductFor(productKey)}/checkout";

    private static string GroupPurchaseCampaignRoute(Guid campaignId, string? suffix = null)
    {
        if (campaignId == Guid.Empty)
        {
            throw new ArgumentException("공동구매 campaign ID는 비어 있을 수 없습니다.", nameof(campaignId));
        }

        var route = $"{GroupPurchase}/{campaignId:D}";
        return string.IsNullOrWhiteSpace(suffix)
            ? route
            : $"{route}/{suffix}";
    }

    private static string EscapePathKey(string value, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.IndexOfAny(['/', '\\', '?', '#']) >= 0)
        {
            throw new ArgumentException("경로 key는 비어 있지 않고 경로 구분자를 포함하지 않아야 합니다.", parameterName);
        }

        return Uri.EscapeDataString(normalized);
    }

    private static string WithQuery(
        string path,
        params (string Key, string? Value)[] values)
    {
        var query = values
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .Select(item => $"{item.Key}={Uri.EscapeDataString(item.Value!.Trim())}")
            .ToArray();

        return query.Length == 0
            ? path
            : $"{path}?{string.Join("&", query)}";
    }
}
