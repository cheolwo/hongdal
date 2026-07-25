using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Contracts.Common.Community;

public static class CommunityActivitySourceKinds
{
    public const string Command = "command";
    public const string Event = "event";

    public static string DisplayName(string sourceKind)
        => string.Equals(sourceKind, Command, StringComparison.OrdinalIgnoreCase)
            ? "Command"
            : "Event";
}

public static class CommunityActivityBoardKeys
{
    public const string FoundationEvidence = "work-foundation-evidence";
    public const string IndividualDemand = "work-individual-demand";
    public const string CollectiveLedger = "work-collective-ledger";
    public const string HsClassification = "work-hs-classification";
    public const string CustomsDelegation = "work-customs-delegation";
    public const string CustomsProcess = "work-customs-process";
    public const string TransportRequest = "work-transport-request";
    public const string DispatchDecision = "work-dispatch-decision";
    public const string LoadingJourney = "work-loading-journey";
    public const string DeliveryHandover = "work-delivery-handover";
    public const string SellerWarehouseReceipt = "work-seller-warehouse-receipt";
    public const string WarehouseInbound = "work-warehouse-inbound";
    public const string PickingHandover = "work-picking-handover";
    public const string FoodOrderAcceptance = "work-food-order-acceptance";
    public const string FoodDeliveryHandoff = "work-food-delivery-handoff";
    public const string MartFulfillment = "work-mart-fulfillment";

    // 배포 전 사용하던 버전별 키는 해당 버전의 첫 업무 게시판 별칭으로만 보존합니다.
    public const string LegacyFoundation = "activity-foundation";
    public const string LegacyGroupPurchase = "activity-group-purchase";
    public const string LegacyTradeReadiness = "activity-trade-readiness";
    public const string LegacyTransport = "activity-transport";
    public const string LegacyFulfillment = "activity-fulfillment";
    public const string LegacyFoodDelivery = "activity-food-delivery";
    public const string LegacyMart = "activity-mart";
}

public static class CommunityActivityProductNames
{
    public const string CultureTransport = "문화교통";
    public const string Ssalddel = "살뜰";

    public static string ForVersion(string productVersion)
        => productVersion is
            SsalddelProductRoadmapCatalog.FoundationVersion
            or SsalddelProductRoadmapCatalog.IndividualOrderVersion
            or SsalddelProductRoadmapCatalog.GroupPurchaseVersion
            or SsalddelProductRoadmapCatalog.TradeReadinessVersion
                ? CultureTransport
                : Ssalddel;
}

public sealed record CommunityActivityPageDefinition(
    string Surface,
    string PageName,
    string Route,
    string Responsibility,
    bool IsWebRoute)
{
    public bool IsRouteTemplate
        => Route.Contains('{');

    public bool CanNavigateFromCommunityWeb
        => IsWebRoute
           && !IsRouteTemplate
           && Route.StartsWith("/", StringComparison.Ordinal);
}

public sealed record CommunityActivityBoardDefinition(
    string SourceKind,
    string SourceName,
    string ActivityDisplayName,
    string ProductVersion,
    string PublicActivitySummary,
    bool PublishesActivityPost,
    CommunityBoardDefinition Board)
{
    public SsalddelProductRoadmapStage RoadmapStage
        => SsalddelProductRoadmapCatalog.Find(ProductVersion);

    public string ProductName
        => CommunityActivityProductNames.ForVersion(ProductVersion);

    public string RoadmapDisplayName
        => $"{ProductName} {ProductVersion} · {RoadmapStage.DisplayName}";

    public string SourceKindDisplayName
        => CommunityActivitySourceKinds.DisplayName(SourceKind);

    public string PublicationLabel
        => PublishesActivityPost ? "게시 투영" : "업무 관계";
}

public sealed record CommunityActivityBoardBundleDefinition(
    string ProductVersion,
    CommunityBoardDefinition Board,
    IReadOnlyList<CommunityActivityBoardDefinition> Activities,
    IReadOnlyList<CommunityActivityPageDefinition> Pages)
{
    public const string MountainSymbol = "☶";
    public const string MountainName = "간";

    public SsalddelProductRoadmapStage RoadmapStage
        => SsalddelProductRoadmapCatalog.Find(ProductVersion);

    public string ProductName
        => CommunityActivityProductNames.ForVersion(ProductVersion);

    public string RoadmapDisplayName
        => $"{ProductName} {ProductVersion} · {RoadmapStage.DisplayName}";

    public int CommandCount
        => Activities.Count(activity => activity.SourceKind == CommunityActivitySourceKinds.Command);

    public int EventCount
        => Activities.Count(activity => activity.SourceKind == CommunityActivitySourceKinds.Event);

    public int PublishedActivityCount
        => Activities.Count(activity => activity.PublishesActivityPost);
}

/// <summary>
/// 버전은 보조 태그로만 유지하고, Command·Event·Page를 깊게 점검할 수 있는 업무단위 게시판으로 묶습니다.
/// </summary>
public static class CommunityActivityBoardCatalog
{
    public const string SurfaceMappingBoundary =
        "0.0~3.5의 Command·Event·페이지는 버전 게시판이 아니라 독립된 업무단위 게시판에 연결합니다.";

    public const string PrivacyBoundary =
        "사용자·업체 식별자, 연락처, 상세 주소, 위치, 금액, 결제 정보, 첨부와 원본 payload는 공개하지 않습니다.";

    public static IReadOnlyList<CommunityActivityBoardBundleDefinition> Bundles
        => CommunityWorkBoardCatalog.Bundles;

    public static IReadOnlyList<CommunityActivityBoardDefinition> All { get; } =
        Bundles.SelectMany(bundle => bundle.Activities).ToArray();

    public static IReadOnlyList<CommunityBoardDefinition> Boards { get; } =
        Bundles.Select(bundle => bundle.Board).ToArray();

    public static CommunityActivityBoardDefinition? FindSource(
        string? sourceKind,
        string? sourceName)
        => string.IsNullOrWhiteSpace(sourceName)
            ? null
            : All.FirstOrDefault(definition =>
                string.Equals(definition.SourceKind, sourceKind?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(definition.SourceName, sourceName.Trim(), StringComparison.Ordinal));

    public static CommunityActivityBoardBundleDefinition? FindBundle(string? boardKeyOrName)
        => string.IsNullOrWhiteSpace(boardKeyOrName)
            ? null
            : Bundles.FirstOrDefault(bundle =>
                IsSame(bundle.Board.Key, boardKeyOrName)
                || IsSame(bundle.Board.DisplayName, boardKeyOrName)
                || bundle.Board.LegacyCategoryNames.Any(alias => IsSame(alias, boardKeyOrName)));

    public static CommunityActivityBoardDefinition? FindBoard(string? boardKeyOrName)
        => FindBundle(boardKeyOrName)?.Activities.FirstOrDefault();

    public static bool IsActivityBoard(string? boardKeyOrName)
        => FindBundle(boardKeyOrName) is not null;

    private static bool IsSame(string? left, string? right)
        => !string.IsNullOrWhiteSpace(left)
           && !string.IsNullOrWhiteSpace(right)
           && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
}
