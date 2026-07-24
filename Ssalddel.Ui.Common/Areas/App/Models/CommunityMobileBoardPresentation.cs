using MudBlazor;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.Models;

public sealed record CommunityMobileLifeBoardPresentation(
    string BoardKey,
    string DisplayName,
    string Description,
    string Icon,
    string Tone);

public sealed record CommunityMobileWorkGroupPresentation(
    string Key,
    string ScreenCode,
    string DisplayName,
    string ShortLabel,
    string Description,
    string Boundary,
    string Icon,
    string Tone,
    IReadOnlyList<string> BoardKeys,
    bool IsCrossCutting = false)
{
    public string Href => $"/community/work/{Uri.EscapeDataString(Key)}";
}

public static class CommunityMobileBoardPresentation
{
    public const string WorkModeQuery = "/community?mode=work";
    public const string CommunityModeQuery = "/community";
    public const string AllWorkGroupsPath = "/community/work/all";

    public static IReadOnlyList<CommunityMobileLifeBoardPresentation> LifeBoards { get; } =
    [
        new(
            CommunityBoardKeys.Vow,
            "서원",
            "마음을 모아 시작하는 제안과 이야기",
            Icons.Material.Filled.FavoriteBorder,
            "mint"),
        new(
            CommunityBoardKeys.FreeLife,
            "자유 · 생활",
            "일상에서 발견한 생각과 질문",
            Icons.Material.Filled.Forum,
            "indigo"),
        new(
            CommunityBoardKeys.InformationPrices,
            "정보 · 가격",
            "공공자료와 가격 비교를 함께 확인",
            Icons.Material.Filled.QueryStats,
            "indigo"),
        new(
            CommunityBoardKeys.Participation,
            "동네 나눔 · 모임",
            "가까운 이웃과 나누고 만나는 공간",
            Icons.Material.Filled.Groups,
            "indigo")
    ];

    public static IReadOnlyList<CommunityMobileWorkGroupPresentation> WorkGroups { get; } =
    [
        new(
            "group-purchase",
            "01A.08",
            "공동구매 · 주문",
            "모으기",
            "수요와 수량, 가격 근거를 모아 함께 살 조건을 생각합니다.",
            "자동 가입이나 주문 확정 없이 비구속 원함과 공동 원장 후보를 구분합니다.",
            Icons.Material.Filled.Groups,
            "violet",
            [
                CommunityActivityBoardKeys.FoundationEvidence,
                CommunityActivityBoardKeys.IndividualDemand,
                CommunityActivityBoardKeys.CollectiveLedger,
                CommunityActivityBoardKeys.FoodOrderAcceptance
            ]),
        new(
            "trade",
            "01A.09",
            "수출입 · 공급",
            "공급",
            "생산자·판매자·수입자 관점에서 공급 여정을 구체화합니다.",
            "HS 후보와 통관 자료는 검토 근거이며 전문 판단이나 계약 확정이 아닙니다.",
            Icons.Material.Filled.Public,
            "blue",
            [
                CommunityActivityBoardKeys.HsClassification,
                CommunityActivityBoardKeys.CustomsDelegation,
                CommunityActivityBoardKeys.CustomsProcess
            ]),
        new(
            "transport",
            "01A.10",
            "운송 · 배송",
            "이동",
            "구간과 시간창, 차량·인계 조건을 공개 정보로 함께 검토합니다.",
            "게시글과 참여 의사는 정보 공유의 시작이며 자동 배차나 운송 계약이 아닙니다.",
            Icons.Material.Filled.LocalShipping,
            "green",
            [
                CommunityActivityBoardKeys.TransportRequest,
                CommunityActivityBoardKeys.DispatchDecision,
                CommunityActivityBoardKeys.LoadingJourney,
                CommunityActivityBoardKeys.DeliveryHandover,
                CommunityActivityBoardKeys.FoodDeliveryHandoff
            ]),
        new(
            "warehouse",
            "01A.11",
            "창고 · 재고",
            "보관",
            "입고·검수·적재·피킹·출고의 실제 작업 경험을 살핍니다.",
            "수량·온도·파손·담당자와 원장 사건을 함께 남기되 개인정보는 공개하지 않습니다.",
            Icons.Material.Filled.Warehouse,
            "amber",
            [
                CommunityActivityBoardKeys.SellerWarehouseReceipt,
                CommunityActivityBoardKeys.WarehouseInbound,
                CommunityActivityBoardKeys.PickingHandover,
                CommunityActivityBoardKeys.MartFulfillment
            ]),
        new(
            "ledger",
            "01A.12",
            "통관 · 원장 · 다이어그램",
            "근거",
            "전문가 검토와 업무 흐름, 가원장 전환 근거를 한곳에 모읍니다.",
            "별도 업무 상태를 만들지 않고 각 업무 게시판과 공동 원장의 관계를 횡단해 봅니다.",
            Icons.Material.Filled.AccountTree,
            "violet",
            [],
            IsCrossCutting: true)
    ];

    public static CommunityMobileWorkGroupPresentation? FindWorkGroup(string? key)
        => string.IsNullOrWhiteSpace(key)
            ? null
            : WorkGroups.FirstOrDefault(group =>
                string.Equals(group.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));
}
