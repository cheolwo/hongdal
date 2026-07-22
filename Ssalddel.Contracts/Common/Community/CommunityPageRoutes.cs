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
    public const string GroupPurchase = "/community/group-purchase";
    public const string GroupPurchaseCreate = "/community/group-purchase/new";

    public static string BoardsFor(string? boardName = null, string? boardKey = null)
        => WithQuery(
            Boards,
            ("boardKey", boardKey),
            ("board", boardName));

    public static string ComposeFor(string? boardName = null)
        => string.IsNullOrWhiteSpace(boardName)
           || boardName.Equals("전체", StringComparison.OrdinalIgnoreCase)
            ? Compose
            : WithQuery(Compose, ("board", boardName));

    public static string PostDetailFor(
        long postId,
        string? boardName = null,
        string? boardKey = null)
        => WithQuery(
            $"/community/posts/{postId}",
            ("boardKey", boardKey),
            ("board", boardName));

    public static string RecommendedPostDetailFor(string seedPostTitle, string? boardName = null)
        => WithQuery(
            RecommendedPostDetail,
            ("seed", seedPostTitle),
            ("board", boardName));

    public static string DiagramFor(string? ledgerTemplateKey = null)
        => WithQuery(
            Workspace,
            ("diagram", "true"),
            ("ledgerTemplate", ledgerTemplateKey));

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
