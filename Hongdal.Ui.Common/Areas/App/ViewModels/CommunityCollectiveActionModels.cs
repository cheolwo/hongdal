using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public static class CommunityCollectiveActionPageKeys
{
    public const string Gathering = "gathering";
    public const string Conditions = "conditions";
    public const string Party = "party";
    public const string Readiness = "readiness";
    public const string InProgress = "in-progress";
    public const string Completed = "completed";
    public const string Mine = "mine";
    public const string Stories = "stories";
    public const string Professionals = "professionals";

    public static IReadOnlyList<string> Ordered { get; } =
    [
        Gathering,
        Conditions,
        Party,
        Readiness,
        InProgress,
        Completed,
        Mine,
        Stories,
        Professionals
    ];

    public static string Normalize(string? value)
        => Ordered.Contains(value?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            ? Ordered.First(key => string.Equals(key, value?.Trim(), StringComparison.OrdinalIgnoreCase))
            : Gathering;
}

public sealed record CommunityCollectiveActionPageDefinition(
    string Key,
    int Number,
    string Title,
    string ShortTitle,
    string Description,
    string Icon,
    bool IsJourneyStage);

public static class CommunityCollectiveActionPageCatalog
{
    public static IReadOnlyList<CommunityCollectiveActionPageDefinition> All { get; } =
    [
        new(CommunityCollectiveActionPageKeys.Gathering, 1, "마음 모으는 중", "마음 모으기", "게시글에서 시작된 관심과 필요한 수량을 모읍니다.", "groups", true),
        new(CommunityCollectiveActionPageKeys.Conditions, 2, "우리 조건 맞추기", "조건 맞추기", "상품, 수량, 가격, 수령과 거래 조건을 함께 확인합니다.", "tune", true),
        new(CommunityCollectiveActionPageKeys.Party, 3, "함께할 사람", "함께할 사람", "거래 당사자와 전문·현장 역할의 빈자리를 확인합니다.", "diversity_3", true),
        new(CommunityCollectiveActionPageKeys.Readiness, 4, "실행 준비 확인", "실행 준비", "합의, 서명, 역할 수락과 업무 인계를 각각 확인합니다.", "fact_check", true),
        new(CommunityCollectiveActionPageKeys.InProgress, 5, "같이 하는 중", "같이 하는 중", "실행 배치의 진행과 추가 참여 가능 여력을 확인합니다.", "local_shipping", true),
        new(CommunityCollectiveActionPageKeys.Completed, 6, "우리 해냈어요", "완료", "함께 만든 결과와 다음 모임에 남길 기록을 확인합니다.", "celebration", true),
        new(CommunityCollectiveActionPageKeys.Mine, 7, "내가 함께한 일", "내 참여", "관심을 표시했거나 역할을 맡은 일을 한곳에서 봅니다.", "person_book", false),
        new(CommunityCollectiveActionPageKeys.Stories, 8, "성사된 이야기", "성사 이야기", "완료된 모임의 결과와 배운 점을 나눕니다.", "auto_stories", false),
        new(CommunityCollectiveActionPageKeys.Professionals, 9, "전문가 참여 요청함", "전문가 요청", "통관, 운송, 창고와 문서 역할의 참여 요청을 모아봅니다.", "support_agent", false)
    ];

    public static CommunityCollectiveActionPageDefinition Find(string? key)
    {
        var normalized = CommunityCollectiveActionPageKeys.Normalize(key);
        return All.First(page => page.Key == normalized);
    }
}

public static class CommunityCollectiveActionRoutes
{
    public const string Root = "/community/actions";

    public static string Build(string? pageKey, Guid? actionId = null)
    {
        var path = $"{Root}/{CommunityCollectiveActionPageKeys.Normalize(pageKey)}";
        return actionId.HasValue ? $"{path}?campaignId={actionId.Value:D}" : path;
    }
}

public enum CommunityCollectiveActionDataMode
{
    Live,
    Preview
}

public enum CommunityCapacityEvidenceStatus
{
    Confirmed,
    Pending,
    NotRequired,
    Closed
}

public sealed record CommunityActionConditionSnapshot(
    string Code,
    string Label,
    string Value,
    string StatusLabel,
    bool Confirmed);

public sealed record CommunityActionRoleSlotSnapshot(
    string Category,
    string RoleCode,
    string RoleLabel,
    string Responsibility,
    bool Required,
    string? ParticipantLabel,
    string StatusLabel,
    bool Accepted);

public sealed record CommunityActionReadinessCheckSnapshot(
    string Code,
    string Label,
    string Detail,
    bool Complete,
    bool BlocksExecution);

public sealed record CommunityCapacityEvidenceSnapshot(
    string Code,
    string Label,
    string ResponsibleRole,
    CommunityCapacityEvidenceStatus Status,
    decimal? MaximumTotalQuantity,
    string EvidenceLabel,
    bool Required = true);

public sealed record CommunityActionTimelineItemSnapshot(
    DateTimeOffset At,
    string Title,
    string Detail,
    bool Complete);

public sealed record CommunityActionOutcomeSnapshot(
    string Label,
    string Value,
    string Detail);

public sealed class CommunityCollectiveActionSnapshot
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string CommunityScope { get; init; } = string.Empty;
    public string CurrentPageKey { get; init; } = CommunityCollectiveActionPageKeys.Gathering;
    public string StatusLabel { get; init; } = string.Empty;
    public string ProductLabel { get; init; } = string.Empty;
    public string SourceCountryCode { get; init; } = string.Empty;
    public string DestinationCountryCode { get; init; } = string.Empty;
    public int ParticipantCount { get; init; }
    public decimal CurrentCommittedQuantity { get; init; }
    public decimal CurrentPotentialQuantity { get; init; }
    public decimal MinimumOrderQuantity { get; init; }
    public string QuantityUnit { get; init; } = "개";
    public DateTimeOffset? AdditionalParticipationClosesAt { get; init; }
    public decimal? EstimatedCurrentUnitCost { get; init; }
    public decimal? EstimatedSafeMaximumUnitCost { get; init; }
    public bool IsPreview { get; init; }
    public bool IsMine { get; init; }
    public IReadOnlyList<CommunityActionConditionSnapshot> Conditions { get; init; } = [];
    public IReadOnlyList<CommunityActionRoleSlotSnapshot> RoleSlots { get; init; } = [];
    public IReadOnlyList<CommunityActionReadinessCheckSnapshot> ReadinessChecks { get; init; } = [];
    public IReadOnlyList<CommunityCapacityEvidenceSnapshot> CapacityEvidence { get; init; } = [];
    public IReadOnlyList<CommunityActionTimelineItemSnapshot> Timeline { get; init; } = [];
    public IReadOnlyList<CommunityActionOutcomeSnapshot> Outcomes { get; init; } = [];
}

public interface ICommunityCollectiveActionSource
{
    Task<IReadOnlyList<CommunityVoteResponse>> LoadAsync(CancellationToken cancellationToken = default);
}

public sealed class PlatformCommunityCollectiveActionSource(
    PlatformCommunityService communityService) : ICommunityCollectiveActionSource
{
    public async Task<IReadOnlyList<CommunityVoteResponse>> LoadAsync(
        CancellationToken cancellationToken = default)
        => (await communityService.GetGroupPurchaseVotesAsync(cancellationToken: cancellationToken)).Items;
}

public static class CommunityCollectiveActionSnapshotFactory
{
    public static CommunityCollectiveActionSnapshot FromCampaign(CommunityVoteResponse campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        var groupPurchase = campaign.GroupPurchase;
        var totalQuantity = groupPurchase?.TotalRequestedQuantity ?? campaign.Options.Sum(option => option.RequestedQuantity);
        var minimumQuantity = groupPurchase?.MinimumTotalQuantity ?? 1;
        var isSigned = string.Equals(
            campaign.ResolutionDocument?.Status,
            CommunityVoteResolutionStatusCodes.Signed,
            StringComparison.OrdinalIgnoreCase);
        var currentPageKey = ResolveCurrentPageKey(campaign, isSigned);
        var productLabel = string.Join(", ", campaign.Options
            .Select(option => option.Text)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Take(2));
        if (string.IsNullOrWhiteSpace(productLabel))
        {
            productLabel = "상품 조건 확인 중";
        }

        var isImport = groupPurchase?.IsGroupImportCandidate == true;
        var routeReady = groupPurchase is not null
                         && groupPurchase.TradeRouteMissingFieldCodes.Count == 0
                         && groupPurchase.TradeRouteInvalidFieldCodes.Count == 0;
        var minimumReached = groupPurchase?.IsMinimumReached == true || totalQuantity >= minimumQuantity;
        var pickupCapacity = groupPurchase?.PickupPoints
            .Where(point => point.CapacityQuantity.HasValue)
            .Sum(point => point.CapacityQuantity!.Value);

        return new CommunityCollectiveActionSnapshot
        {
            Id = campaign.Id,
            Title = campaign.Title,
            Summary = campaign.Description,
            CommunityScope = campaign.CommunityScope,
            CurrentPageKey = currentPageKey,
            StatusLabel = ResolveStatusLabel(currentPageKey),
            ProductLabel = productLabel,
            SourceCountryCode = groupPurchase?.ShipFromCountryCode ?? string.Empty,
            DestinationCountryCode = groupPurchase?.DeliveryCountryCode ?? string.Empty,
            ParticipantCount = campaign.TotalVoteCount,
            CurrentCommittedQuantity = totalQuantity,
            CurrentPotentialQuantity = totalQuantity,
            MinimumOrderQuantity = minimumQuantity,
            QuantityUnit = string.IsNullOrWhiteSpace(groupPurchase?.QuantityUnit)
                ? "개"
                : groupPurchase.QuantityUnit,
            AdditionalParticipationClosesAt = campaign.Status == CommunityVoteStatusCodes.Open
                ? ToOffset(campaign.ClosesAtUtc)
                : null,
            Conditions = BuildConditions(campaign, productLabel, minimumReached, routeReady),
            RoleSlots = BuildRoleSlots(campaign, isImport),
            ReadinessChecks = BuildReadinessChecks(campaign, minimumReached, routeReady, isSigned),
            CapacityEvidence = BuildCapacityEvidence(isImport, pickupCapacity),
            Timeline = BuildTimeline(campaign, isSigned),
            Outcomes = isSigned
                ?
                [
                    new("모인 사람", $"{campaign.TotalVoteCount}명", "현재 결의문 기준 참여 인원"),
                    new("함께 정한 수량", $"{totalQuantity:N0}{groupPurchase?.QuantityUnit ?? "개"}", "서명된 수요 기준"),
                    new("다음 기록", "이행 중", "실제 출고·수령 결과는 업무 원장에서 이어집니다.")
                ]
                : []
        };
    }

    private static string ResolveCurrentPageKey(CommunityVoteResponse campaign, bool isSigned)
    {
        if (isSigned)
        {
            return CommunityCollectiveActionPageKeys.InProgress;
        }

        if (campaign.ResolutionDocument is not null)
        {
            return CommunityCollectiveActionPageKeys.Readiness;
        }

        if (!string.Equals(campaign.Status, CommunityVoteStatusCodes.Open, StringComparison.OrdinalIgnoreCase)
            || campaign.GroupPurchase?.IsMinimumReached == true)
        {
            return CommunityCollectiveActionPageKeys.Conditions;
        }

        return CommunityCollectiveActionPageKeys.Gathering;
    }

    private static string ResolveStatusLabel(string pageKey)
        => pageKey switch
        {
            CommunityCollectiveActionPageKeys.Conditions => "조건 조율 중",
            CommunityCollectiveActionPageKeys.Party => "역할 구성 중",
            CommunityCollectiveActionPageKeys.Readiness => "실행 준비 중",
            CommunityCollectiveActionPageKeys.InProgress => "같이 하는 중",
            CommunityCollectiveActionPageKeys.Completed => "완료",
            _ => "마음 모으는 중"
        };

    private static IReadOnlyList<CommunityActionConditionSnapshot> BuildConditions(
        CommunityVoteResponse campaign,
        string productLabel,
        bool minimumReached,
        bool routeReady)
    {
        var groupPurchase = campaign.GroupPurchase;
        var serviceArea = string.IsNullOrWhiteSpace(groupPurchase?.ServiceAreaLabel)
            ? campaign.CommunityScope
            : groupPurchase.ServiceAreaLabel;
        return
        [
            new("product", "함께 살 것", productLabel, "확인 중", campaign.Options.Count > 0),
            new(
                "quantity",
                "필요한 양",
                $"{groupPurchase?.TotalRequestedQuantity ?? 0:N0}/{groupPurchase?.MinimumTotalQuantity ?? 1:N0}{groupPurchase?.QuantityUnit ?? "개"}",
                minimumReached ? "기준 도달" : "더 모으는 중",
                minimumReached),
            new("area", "수령 범위", serviceArea, "모집 조건", !string.IsNullOrWhiteSpace(serviceArea)),
            new(
                "route",
                "거래 경로",
                groupPurchase?.IsGroupImportCandidate == true ? "해외 조달 검토" : "국내 공동구매",
                routeReady ? "필수값 확인" : "확인 필요",
                routeReady),
            new(
                "agreement",
                "현재 합의",
                campaign.ResolutionDocument?.DocumentTitle ?? "모집 결과를 바탕으로 작성 예정",
                campaign.ResolutionDocument is null ? "초안 전" : "문서 있음",
                campaign.ResolutionDocument is not null)
        ];
    }

    private static IReadOnlyList<CommunityActionRoleSlotSnapshot> BuildRoleSlots(
        CommunityVoteResponse campaign,
        bool isImport)
    {
        var proposerLabel = string.IsNullOrWhiteSpace(campaign.CreatedByDisplayName)
            ? "제안자 확인 필요"
            : campaign.CreatedByDisplayName;
        var slots = new List<CommunityActionRoleSlotSnapshot>
        {
            new("거래 당사자", "buyer", "구매자·참여자", "수량과 수령 조건을 직접 확인합니다.", true, $"{campaign.TotalVoteCount}명 관심", "관심 확인", campaign.TotalVoteCount > 0),
            new("거래 당사자", "representative", "공동구매 대표", "모인 조건을 정리하고 당사자 확인을 요청합니다.", true, proposerLabel, "제안 기록", !string.IsNullOrWhiteSpace(campaign.CreatedByDisplayName)),
            new("거래 당사자", "seller", "판매자·공급자", "공급 가능량, 가격과 출하 조건을 제안합니다.", true, null, "참여 요청", false),
            new("현장 이행", "warehouse", "창고·수령소 운영", "입고, 보관, 분배와 수령 가능량을 확인합니다.", false, null, "필요 시 참여", false),
            new("실제 운송", "carrier", "운송 제공자", "적재 여력과 운송 조건을 자신의 책임으로 제안합니다.", false, null, "필요 시 참여", false)
        };

        if (isImport)
        {
            slots.InsertRange(2,
            [
                new("거래 당사자", "importer", "수입 책임 당사자", "수입 계약과 신고 책임 범위를 직접 수락합니다.", true, null, "참여 요청", false),
                new("거래 당사자", "exporter", "수출 책임 당사자", "수출 계약과 출발국 서류 책임을 확인합니다.", true, null, "참여 요청", false),
                new("통관·문서", "import-customs", "수입 통관 전문가", "도착국 신고와 통관 서류를 별도 수임으로 검토합니다.", false, null, "전문가 요청", false),
                new("통관·문서", "export-customs", "수출 통관 전문가", "출발국 수출 신고와 서류를 별도 수임으로 검토합니다.", false, null, "전문가 요청", false),
                new("운송 중개·주선", "forwarder", "허가·등록된 운송 주선업자", "운송 경로와 제공자 조건을 자신의 권한 안에서 제안합니다.", false, null, "전문가 요청", false)
            ]);
        }

        return slots;
    }

    private static IReadOnlyList<CommunityActionReadinessCheckSnapshot> BuildReadinessChecks(
        CommunityVoteResponse campaign,
        bool minimumReached,
        bool routeReady,
        bool isSigned)
        =>
        [
            new("demand", "최소 수요", minimumReached ? "필요한 수량과 인원이 모였습니다." : "최소 수량 또는 인원이 더 필요합니다.", minimumReached, true),
            new("route", "거래 경로", routeReady ? "출발·도착과 거래 방향을 확인했습니다." : "거래 경로 필수값을 확인해야 합니다.", routeReady, true),
            new("roles", "필수 역할 수락", "관심 표시는 역할 수락이나 자격 확인을 대신하지 않습니다.", false, true),
            new("resolution", "결의문", campaign.ResolutionDocument is null ? "조건 조율 후 결의문을 작성합니다." : campaign.ResolutionDocument.DocumentTitle, campaign.ResolutionDocument is not null, true),
            new("signature", "현재 리비전 동의", isSigned ? "현재 결의문 서명이 완료됐습니다." : "주요 조건이 확정되면 당사자 동의를 받습니다.", isSigned, true),
            new("capacity", "이행 여력", "공급·창고·운송·서류 담당자가 각자 가능한 범위를 확인합니다.", false, true)
        ];

    private static IReadOnlyList<CommunityCapacityEvidenceSnapshot> BuildCapacityEvidence(
        bool isImport,
        int? pickupCapacity)
        =>
        [
            new("supply", "공급 가능량", "판매자·공급자", CommunityCapacityEvidenceStatus.Pending, null, "공급자 확인 전"),
            new("warehouse", "창고·수령소 처리량", "창고·수령소 운영자", CommunityCapacityEvidenceStatus.Pending, pickupCapacity, pickupCapacity.HasValue ? "수령소 등록값 · 운영자 재확인 필요" : "운영자 확인 전"),
            new("packing", "포장 가능량", "포장·현장 담당자", CommunityCapacityEvidenceStatus.Pending, null, "작업 담당자 확인 전"),
            new("transport", "운송 적재 여력", "운송사·주선업자", CommunityCapacityEvidenceStatus.Pending, null, "운송 제공자 확인 전"),
            new(
                "documents",
                "통관·서류 허용량",
                isImport ? "수출입 책임 당사자·통관 전문가" : "거래 당사자",
                isImport ? CommunityCapacityEvidenceStatus.Pending : CommunityCapacityEvidenceStatus.NotRequired,
                null,
                isImport ? "관할·서류 확인 전" : "국내 거래 기준 해당 없음",
                Required: isImport)
        ];

    private static IReadOnlyList<CommunityActionTimelineItemSnapshot> BuildTimeline(
        CommunityVoteResponse campaign,
        bool isSigned)
    {
        var items = new List<CommunityActionTimelineItemSnapshot>
        {
            new(ToOffset(campaign.CreatedAtUtc) ?? DateTimeOffset.UtcNow, "제안이 열렸어요", campaign.CreatedByDisplayName, true)
        };
        if (campaign.ClosedAtUtc.HasValue)
        {
            items.Add(new(ToOffset(campaign.ClosedAtUtc)!.Value, "수요 모집을 마쳤어요", $"{campaign.TotalVoteCount}명이 뜻을 모았습니다.", true));
        }

        if (campaign.ResolutionDocument is not null)
        {
            items.Add(new(ToOffset(campaign.ResolutionDocument.CreatedAtUtc) ?? DateTimeOffset.UtcNow, "함께 정한 내용을 문서로 남겼어요", campaign.ResolutionDocument.DocumentTitle, true));
        }

        if (isSigned)
        {
            items.Add(new(DateTimeOffset.UtcNow, "현재 조건에 동의했어요", "실제 이행은 담당 업무 원장에서 이어집니다.", true));
        }

        return items;
    }

    private static DateTimeOffset? ToOffset(DateTime? value)
        => value.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
            : null;
}

public static class CommunityCollectiveActionPreviewCatalog
{
    public static IReadOnlyList<CommunityCollectiveActionSnapshot> Create()
    {
        var now = DateTimeOffset.UtcNow;
        return
        [
            CreateInProgress(now),
            CreateGathering(now),
            CreateReadiness(now),
            CreateCompleted(now)
        ];
    }

    private static CommunityCollectiveActionSnapshot CreateInProgress(DateTimeOffset now)
        => new()
        {
            Id = Guid.Parse("11111111-1111-4111-8111-111111111111"),
            Title = "성북 생활모임 올리브오일 공동수입",
            Summary = "한 가정이 부담 없이 쓸 수 있는 수량으로 나누고, 남은 적재 여력만큼 추가 참여를 확인하고 있습니다.",
            CommunityScope = "서울 성북 · 한국/스페인",
            CurrentPageKey = CommunityCollectiveActionPageKeys.InProgress,
            StatusLabel = "같이 하는 중",
            ProductLabel = "엑스트라 버진 올리브오일 1L",
            SourceCountryCode = "ES",
            DestinationCountryCode = "KR",
            ParticipantCount = 18,
            CurrentCommittedQuantity = 42,
            CurrentPotentialQuantity = 44,
            MinimumOrderQuantity = 36,
            QuantityUnit = "박스",
            AdditionalParticipationClosesAt = now.AddDays(2).AddHours(4),
            EstimatedCurrentUnitCost = 148_000m,
            EstimatedSafeMaximumUnitCost = 141_500m,
            IsPreview = true,
            IsMine = true,
            Conditions =
            [
                new("product", "함께 살 것", "엑스트라 버진 올리브오일 1L × 12병", "확정", true),
                new("quantity", "기본 배치", "42박스", "확정", true),
                new("price", "예상 도착원가", "현재 148,000원/박스", "추정값", true),
                new("pickup", "공동 수령", "성북 단지 물류실 · 토요일", "확정", true),
                new("revision", "현재 합의", "계획 리비전 4", "18명 동의", true)
            ],
            RoleSlots =
            [
                new("거래 당사자", "buyer", "구매자", "수량과 수령 조건 확인", true, "18명", "수락", true),
                new("거래 당사자", "seller", "판매자·수출자", "공급·출하 조건 제안", true, "스페인 협동조합", "역할 수락", true),
                new("거래 당사자", "importer", "수입 책임 당사자", "계약·수입 책임 확인", true, "수입 책임 참여자", "검토 중", false),
                new("통관·문서", "import-customs", "수입 통관 전문가", "통관 서류 별도 수임", false, "참여 의향 1명", "자격·수임 확인 중", false),
                new("운송 중개·주선", "forwarder", "해상 운송 주선업자", "운송 조건 별도 제안", false, "참여 의향 1곳", "계약 전", false),
                new("현장 이행", "warehouse", "단지 물류 관리자", "입고·분배 여력 확인", true, "성북 단지 물류실", "역할 수락", true)
            ],
            ReadinessChecks =
            [
                new("demand", "최소 수요", "36박스 기준을 넘었습니다.", true, true),
                new("route", "거래 경로", "스페인 출발·한국 도착을 확인했습니다.", true, true),
                new("roles", "필수 역할 수락", "수입 책임 범위를 최종 확인하고 있습니다.", false, true),
                new("resolution", "결의문", "계획 리비전 4를 문서로 남겼습니다.", true, true),
                new("signature", "현재 리비전 동의", "18명이 현재 조건에 동의했습니다.", true, true),
                new("capacity", "이행 여력", "현재 배치에 8박스의 확인된 여력이 있습니다.", true, true)
            ],
            CapacityEvidence =
            [
                new("supply", "공급 가능량", "판매자·공급자", CommunityCapacityEvidenceStatus.Confirmed, 60, "공급 확인서 · 리비전 2"),
                new("warehouse", "창고·수령소 처리량", "단지 물류 관리자", CommunityCapacityEvidenceStatus.Confirmed, 54, "입고 일정 확인 · 리비전 1"),
                new("packing", "포장 가능량", "현장 포장 담당", CommunityCapacityEvidenceStatus.Confirmed, 56, "포장 작업표 · 리비전 3"),
                new("transport", "운송 적재 여력", "허가·등록된 운송 제공자", CommunityCapacityEvidenceStatus.Confirmed, 52, "적재 여력 확인 · 계약 확정 전"),
                new("documents", "통관·서류 허용량", "수입 책임 당사자·통관 전문가", CommunityCapacityEvidenceStatus.Confirmed, 52, "서류 검토 범위 · 수임 별도")
            ],
            Timeline =
            [
                new(now.AddDays(-18), "게시글에서 마음을 모았어요", "생활 게시판의 올리브오일 이야기에서 시작됐습니다.", true),
                new(now.AddDays(-12), "가원장을 만들었어요", "관심 역할과 참여 의사를 비구속적으로 기록했습니다.", true),
                new(now.AddDays(-5), "현재 조건에 동의했어요", "가격과 수령 조건 리비전 4를 확인했습니다.", true),
                new(now.AddDays(-1), "추가 참여 창이 열렸어요", "확인된 여력 안에서 참여 의향을 더 받습니다.", true),
                new(now.AddDays(2), "포장 수량을 마감해요", "마감 뒤 참여는 다음 배치 대기로 이동합니다.", false)
            ],
            Outcomes =
            [
                new("지금 함께", "18명", "기본 배치 참여자"),
                new("확정 수량", "42박스", "임시 참여 2박스 별도"),
                new("남은 여력", "8박스", "모든 필수 담당자 확인 기준")
            ]
        };

    private static CommunityCollectiveActionSnapshot CreateGathering(DateTimeOffset now)
        => new()
        {
            Id = Guid.Parse("22222222-2222-4222-8222-222222222222"),
            Title = "동네 제철 사과를 같이 받아볼까요",
            Summary = "산지 출하일에 맞춰 한 번에 받고 가까운 수령소에서 나누려는 가벼운 제안입니다.",
            CommunityScope = "경기 성남",
            CurrentPageKey = CommunityCollectiveActionPageKeys.Gathering,
            StatusLabel = "마음 모으는 중",
            ProductLabel = "제철 사과 5kg",
            SourceCountryCode = "KR",
            DestinationCountryCode = "KR",
            ParticipantCount = 14,
            CurrentCommittedQuantity = 23,
            CurrentPotentialQuantity = 27,
            MinimumOrderQuantity = 30,
            QuantityUnit = "상자",
            AdditionalParticipationClosesAt = now.AddDays(5),
            IsPreview = true,
            Conditions =
            [
                new("product", "함께 살 것", "제철 사과 5kg", "제안", true),
                new("quantity", "필요한 양", "23/30상자", "7상자 더 필요", false),
                new("pickup", "수령 후보", "단지 관리동 앞", "의견 받는 중", false)
            ],
            RoleSlots =
            [
                new("거래 당사자", "buyer", "구매자", "희망 수량 표시", true, "14명 관심", "관심", true),
                new("거래 당사자", "seller", "생산자·판매자", "공급 조건 제안", true, null, "참여 요청", false),
                new("현장 이행", "pickup", "수령소 운영", "수령 시간과 보관 확인", false, null, "참여 요청", false)
            ],
            ReadinessChecks =
            [
                new("demand", "최소 수요", "7상자가 더 필요합니다.", false, true),
                new("roles", "공급자 참여", "생산자 또는 판매자의 제안이 필요합니다.", false, true)
            ],
            Timeline =
            [
                new(now.AddDays(-2), "게시글이 올라왔어요", "사과를 함께 받고 싶다는 이야기가 시작됐습니다.", true),
                new(now.AddDays(-1), "14명이 관심을 보였어요", "희망 수량은 아직 바꿀 수 있습니다.", true)
            ]
        };

    private static CommunityCollectiveActionSnapshot CreateReadiness(DateTimeOffset now)
        => new()
        {
            Id = Guid.Parse("33333333-3333-4333-8333-333333333333"),
            Title = "공동주택 공용 세제 함께 구매",
            Summary = "관리동 수령과 가구별 분배 조건을 맞추고 현재 리비전의 동의를 확인하고 있습니다.",
            CommunityScope = "인천 송도",
            CurrentPageKey = CommunityCollectiveActionPageKeys.Readiness,
            StatusLabel = "실행 준비 중",
            ProductLabel = "친환경 세탁세제 3L",
            SourceCountryCode = "KR",
            DestinationCountryCode = "KR",
            ParticipantCount = 32,
            CurrentCommittedQuantity = 64,
            CurrentPotentialQuantity = 64,
            MinimumOrderQuantity = 60,
            QuantityUnit = "통",
            IsPreview = true,
            IsMine = true,
            Conditions =
            [
                new("product", "함께 살 것", "친환경 세탁세제 3L", "확정", true),
                new("quantity", "함께 살 양", "64통", "기준 도달", true),
                new("pickup", "공동 수령", "관리동 지하 물류실", "확정", true),
                new("agreement", "현재 합의", "가구별 최대 3통 · 리비전 2", "동의 중", false)
            ],
            RoleSlots =
            [
                new("거래 당사자", "buyer", "구매자", "수량·수령 조건 확인", true, "32가구", "수락 중", false),
                new("거래 당사자", "seller", "판매자", "공급·출하 조건 확인", true, "국내 공급사", "역할 수락", true),
                new("현장 이행", "warehouse", "단지 물류 관리자", "입고·분배 일정 확인", true, "단지 물류 관리자", "역할 수락", true)
            ],
            ReadinessChecks =
            [
                new("demand", "최소 수요", "60통 기준을 넘었습니다.", true, true),
                new("route", "국내 거래 경로", "공급처와 수령소를 확인했습니다.", true, true),
                new("roles", "필수 역할 수락", "판매자와 물류 관리자가 역할을 수락했습니다.", true, true),
                new("resolution", "결의문", "가구별 수량과 수령 시간 리비전 2", true, true),
                new("signature", "현재 리비전 동의", "32가구 중 27가구가 확인했습니다.", false, true),
                new("capacity", "이행 여력", "동의 완료 후 최종 입고 수량을 확인합니다.", false, true)
            ],
            Timeline =
            [
                new(now.AddDays(-10), "수요 모집을 시작했어요", "관리동 게시판과 생활 게시판에서 함께 모았습니다.", true),
                new(now.AddDays(-4), "최소 수량을 넘었어요", "공급 조건 조율을 시작했습니다.", true),
                new(now.AddDays(-1), "리비전 2 동의를 받고 있어요", "주요 조건 변경 시 다시 확인합니다.", true)
            ]
        };

    private static CommunityCollectiveActionSnapshot CreateCompleted(DateTimeOffset now)
        => new()
        {
            Id = Guid.Parse("44444444-4444-4444-8444-444444444444"),
            Title = "마을 축제 다회용 식기 공동 대여",
            Summary = "세 동네가 수량을 합쳐 필요한 식기를 빌리고 반납까지 마쳤습니다.",
            CommunityScope = "전북 전주",
            CurrentPageKey = CommunityCollectiveActionPageKeys.Completed,
            StatusLabel = "우리 해냈어요",
            ProductLabel = "다회용 식기 세트",
            SourceCountryCode = "KR",
            DestinationCountryCode = "KR",
            ParticipantCount = 41,
            CurrentCommittedQuantity = 300,
            CurrentPotentialQuantity = 300,
            MinimumOrderQuantity = 250,
            QuantityUnit = "세트",
            IsPreview = true,
            Conditions =
            [
                new("quantity", "함께 쓴 수량", "300세트", "완료", true),
                new("return", "반납", "300세트 확인", "완료", true)
            ],
            RoleSlots =
            [
                new("거래 당사자", "participants", "행사 참여자", "수량·반납 약속 이행", true, "41명", "완료", true),
                new("현장 이행", "operator", "현장 운영", "배부·회수·검수", true, "3개 동네 운영팀", "완료", true)
            ],
            ReadinessChecks =
            [
                new("completed", "결과 확인", "배부와 반납 수량을 모두 확인했습니다.", true, false)
            ],
            Timeline =
            [
                new(now.AddDays(-21), "세 동네가 마음을 모았어요", "각 행사에서 필요한 수량을 합쳤습니다.", true),
                new(now.AddDays(-7), "식기를 함께 사용했어요", "현장별 담당자가 배부 수량을 확인했습니다.", true),
                new(now.AddDays(-5), "반납을 마쳤어요", "파손 2세트를 기록하고 정산했습니다.", true)
            ],
            Outcomes =
            [
                new("함께한 사람", "41명", "세 동네 행사 참여자"),
                new("사용한 식기", "300세트", "일회용품을 대신했습니다."),
                new("반납 확인", "298+2세트", "정상 298 · 파손 기록 2")
            ]
        };
}
