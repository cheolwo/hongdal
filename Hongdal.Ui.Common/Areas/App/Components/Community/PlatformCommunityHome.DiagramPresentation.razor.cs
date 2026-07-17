using System.Globalization;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private bool 원장블록노드선택됨(원장블록노드 node)
        => string.Equals(선택원장블록노드제목, node.Title, StringComparison.OrdinalIgnoreCase);

    private string 원장블록노드클래스생성(
        원장블록노드 node,
        int nodeIndex,
        IReadOnlyList<원장블록노드> nodes)
    {
        var selected = 원장블록노드선택됨(node)
            ? " platform-ledger-flow-node--selected"
            : string.Empty;
        var processingState = IsDiagramLayerVisible(DiagramLayerState)
            ? $" {원장블록처리상태클래스생성(원장블록처리상태해결(nodeIndex, nodes))}"
            : string.Empty;
        var prioritySignal = 최우선도형레이어신호해결(node, nodeIndex, nodes);
        var prioritySignalClass = prioritySignal is null ? string.Empty : $" {prioritySignal.CssClass}";

        return $"platform-ledger-flow-node platform-ledger-flow-node--{node.Kind}{processingState}{prioritySignalClass}{selected}";
    }

    private string BuildMobileDiagramNodeClass(
        원장블록노드 node,
        int nodeIndex,
        IReadOnlyList<원장블록노드> nodes)
    {
        var selected = 원장블록노드선택됨(node)
            ? " platform-ledger-mobile-node--selected"
            : string.Empty;
        var state = $" {원장블록처리상태클래스생성(원장블록처리상태해결(nodeIndex, nodes))}";
        var connection = string.IsNullOrWhiteSpace(connectionStartNodeTitle)
            ? string.Empty
            : string.Equals(connectionStartNodeTitle, node.Title, StringComparison.OrdinalIgnoreCase)
                ? " platform-ledger-mobile-node--connection-source"
                : " platform-ledger-mobile-node--connection-target";

        return $"platform-ledger-mobile-node{state}{selected}{connection}";
    }

    private string BuildMobileDiagramConnectorClass(원장블록연결선? edge)
    {
        var selected = edge is not null &&
                       string.Equals(selectedDiagramEdgeId, edge.Id, StringComparison.OrdinalIgnoreCase)
            ? " platform-ledger-mobile-connector--selected"
            : string.Empty;
        var empty = edge is null ? " platform-ledger-mobile-connector--empty" : string.Empty;
        return $"platform-ledger-mobile-connector{selected}{empty}";
    }

    private 원장블록처리상태 원장블록처리상태해결(
        int nodeIndex,
        IReadOnlyList<원장블록노드> nodes)
    {
        var ledger = 선택현재원장;
        if (ledger is null)
        {
            return 원장블록처리상태.대기;
        }

        var activeIndex = 현재원장활성노드순서해결(ledger, nodes);
        if (activeIndex >= nodes.Count)
        {
            return 원장블록처리상태.완료;
        }

        if (activeIndex < 0)
        {
            return 원장블록처리상태.대기;
        }

        if (nodeIndex < activeIndex)
        {
            return 원장블록처리상태.완료;
        }

        return nodeIndex == activeIndex
            ? 원장블록처리상태.진행중
            : 원장블록처리상태.대기;
    }

    private int 현재원장활성노드순서해결(
        현재원장컨텍스트 ledger,
        IReadOnlyList<원장블록노드> nodes)
    {
        var normalizedState = ledger.StateLabel.Trim();
        if (string.IsNullOrWhiteSpace(normalizedState))
        {
            return nodes.Count == 0 ? -1 : 0;
        }

        if (IsLedgerStateFullyCompleted(normalizedState))
        {
            return nodes.Count;
        }

        var mappedTitle = 현재원장활성노드제목해결(ledger.TemplateKey, normalizedState);
        if (!string.IsNullOrWhiteSpace(mappedTitle))
        {
            var mappedIndex = 원장블록노드순서찾기(nodes, mappedTitle);
            if (mappedIndex >= 0)
            {
                return mappedIndex;
            }
        }

        var directIndex = FindFirstStateMatchingNodeIndex(nodes, normalizedState);
        return directIndex >= 0 ? directIndex : 0;
    }

    private static bool IsLedgerStateFullyCompleted(string stateLabel)
        => ContainsAny(stateLabel, "전체 완료", "종료", "닫힘", "정산 완료", "완료됨");

    private static string? 현재원장활성노드제목해결(string templateKey, string stateLabel)
    {
        if (templateKey is CommunityLedgerTemplateKeys.CargoTransport)
        {
            return ResolveCargoTransportActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.HongdalMart)
        {
            return ResolveHongdalMartActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.WarehouseInbound)
        {
            return ResolveWarehouseInboundActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.WarehouseOutbound)
        {
            return ResolveWarehouseOutboundActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.FoodOrder)
        {
            return ResolveFoodOrderActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.FoodDelivery)
        {
            return ResolveFoodDeliveryActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.GroupPurchase)
        {
            return ResolveGroupPurchaseActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.GroupImport)
        {
            return ResolveGroupImportActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.LocalSale)
        {
            return ResolveLocalSaleActiveNodeTitle(stateLabel);
        }

        if (templateKey is CommunityLedgerTemplateKeys.Errand)
        {
            return ResolveErrandActiveNodeTitle(stateLabel);
        }

        return null;
    }

    private static string? ResolveCargoTransportActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(
                stateLabel,
                "정산",
                "결제",
                "후불",
                "환불",
                "보류",
                "분쟁",
                "완료됨",
                "전체 완료",
                "종료",
                "닫힘"))
        {
            return "결제/정산";
        }

        if (ContainsAny(stateLabel, "하차", "수령", "인수", "POD"))
        {
            return "하차";
        }

        if (ContainsAny(stateLabel, "상차", "운송 중", "이동 중", "출발"))
        {
            return "상차";
        }

        if (ContainsAny(stateLabel, "기사 확인", "기사 수락", "배차 확인", "배차 대기", "배차 추천", "추천", "운임확정"))
        {
            return "운송 의뢰";
        }

        return "운송 의뢰";
    }

    private static string? ResolveHongdalMartActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "배송", "픽업", "전달", "기사", "수령", "완료"))
        {
            return "픽업/전달";
        }

        if (ContainsAny(stateLabel, "피킹", "포장"))
        {
            return "피킹/포장";
        }

        return ContainsAny(stateLabel, "재고", "창고")
            ? "도심 재고"
            : "마트 주문";
    }

    private static string? ResolveWarehouseInboundActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "보관", "재고", "마감", "입고 완료", "완료"))
        {
            return "보관/재고화";
        }

        if (ContainsAny(stateLabel, "검수", "이상", "파손", "누락"))
        {
            return "검수/이상";
        }

        if (ContainsAny(stateLabel, "하차", "납품", "도착", "배송"))
        {
            return "운송 하차";
        }

        return ContainsAny(stateLabel, "입고 시작", "입고 대기", "인수")
            ? "창고 입고"
            : "입고 요청";
    }

    private static string? ResolveWarehouseOutboundActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "창고 입고", "입고 완료"))
        {
            return "창고 입고";
        }

        if (ContainsAny(stateLabel, "하차", "도착", "수령", "인수"))
        {
            return "운송 하차";
        }

        if (ContainsAny(stateLabel, "상차", "배송", "운송", "인계", "재위탁"))
        {
            return "운송 상차";
        }

        if (ContainsAny(stateLabel, "출고 확정", "출고 완료", "완료"))
        {
            return "창고 출고";
        }

        if (ContainsAny(stateLabel, "포장"))
        {
            return "포장";
        }

        return ContainsAny(stateLabel, "피킹", "검수", "출고")
            ? "피킹/검수"
            : "출고 요청";
    }

    private static string? ResolveFoodOrderActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "픽업대기", "준비 완료", "조리 완료", "완료", "직접 수령"))
        {
            return "준비 완료";
        }

        if (ContainsAny(stateLabel, "조리", "주방"))
        {
            return "조리";
        }

        return ContainsAny(stateLabel, "수락", "접수")
            ? "주문 수락"
            : "음식 주문";
    }

    private static string? ResolveFoodDeliveryActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "전달", "수령", "정산", "완료", "부재", "실패"))
        {
            return "전달/수령";
        }

        if (ContainsAny(stateLabel, "픽업", "이동", "배송 중", "배달 중"))
        {
            return "픽업/이동";
        }

        return ContainsAny(stateLabel, "배차", "기사", "추천", "재배차")
            ? "배차"
            : "배달 회차";
    }

    private static string? ResolveGroupPurchaseActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "정산", "수령", "마감", "완료"))
        {
            return "정산/수령";
        }

        if (ContainsAny(stateLabel, "분배", "전달", "인계", "배송"))
        {
            return "분배";
        }

        if (ContainsAny(stateLabel, "거점", "창고", "입고", "보관"))
        {
            return "수령 거점";
        }

        if (ContainsAny(stateLabel, "구매", "결정", "확정", "가격", "수량"))
        {
            return "구매 확정";
        }

        return "수요 모집";
    }

    private static string? ResolveGroupImportActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "정산", "수령", "마감", "완료"))
        {
            return "정산/수령";
        }

        if (ContainsAny(stateLabel, "분배", "국내 운송", "세대 배송"))
        {
            return "세대 분배";
        }

        if (ContainsAny(stateLabel, "3PL", "창고", "입고"))
        {
            return "3PL 입고";
        }

        if (ContainsAny(stateLabel, "통관", "반출", "HS", "관세", "검역", "보세", "검토"))
        {
            return "통관/반출";
        }

        if (ContainsAny(stateLabel, "해외", "발주", "선적", "인보이스", "패킹", "ImportInProgress"))
        {
            return "해외 선적";
        }

        if (ContainsAny(stateLabel, "수입", "결정", "확정", "FCL", "LCL", "가격", "수량", "ImportDecision", "Cancelled"))
        {
            return "수입 결정";
        }

        return "원천 공동구매";
    }

    private static string? ResolveLocalSaleActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "정산", "결제", "입금", "확인", "완료", "마감"))
        {
            return "정산/확인";
        }

        if (ContainsAny(stateLabel, "전달", "배송", "직거래", "택배", "수령"))
        {
            return "전달";
        }

        return ContainsAny(stateLabel, "보관", "준비", "예약")
            ? "보관/준비"
            : "판매/예약";
    }

    private static string? ResolveErrandActiveNodeTitle(string stateLabel)
    {
        if (ContainsAny(stateLabel, "확인", "완료", "보류", "증빙", "마감"))
        {
            return "확인/마감";
        }

        return ContainsAny(stateLabel, "진행", "수행", "작업", "참여")
            ? "수행"
            : "요청";
    }

    private static int FindFirstStateMatchingNodeIndex(
        IReadOnlyList<원장블록노드> nodes,
        string stateLabel)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (ContainsAny(stateLabel, node.Title, node.GroupLabel) ||
                ContainsAny($"{node.Title} {node.GroupLabel} {node.Description}", stateLabel))
            {
                return i;
            }
        }

        return -1;
    }

    private static int 원장블록노드순서찾기(
        IReadOnlyList<원장블록노드> nodes,
        string nodeTitle)
        => nodes
            .Select((node, index) => new { node, index })
            .FirstOrDefault(item => string.Equals(item.node.Title, nodeTitle, StringComparison.OrdinalIgnoreCase))
            ?.index ?? -1;

    private static string 원장블록처리상태클래스생성(원장블록처리상태 state)
        => state switch
        {
            원장블록처리상태.완료 => "platform-ledger-flow-node--state-completed",
            원장블록처리상태.진행중 => "platform-ledger-flow-node--state-active",
            _ => "platform-ledger-flow-node--state-pending"
        };

    private IReadOnlyList<도형레이어배지> 도형레이어배지목록생성(
        원장블록노드 node,
        int nodeIndex,
        IReadOnlyList<원장블록노드> nodes)
    {
        var badges = new List<도형레이어배지>();

        if (IsDiagramLayerVisible(DiagramLayerRisk))
        {
            var riskSignal = 도형리스크신호해결(node, nodeIndex, nodes);
            if (riskSignal is not null)
            {
                badges.Add(riskSignal.Badge);
            }
        }

        if (IsDiagramLayerVisible(DiagramLayerEvidence))
        {
            var evidenceSignal = 도형증빙신호해결(node, nodeIndex, nodes);
            if (evidenceSignal is not null)
            {
                badges.Add(evidenceSignal.Badge);
            }
        }

        if (IsDiagramLayerVisible(DiagramLayerState))
        {
            var state = 원장블록처리상태해결(nodeIndex, nodes);
            if (state is 원장블록처리상태.진행중)
            {
                badges.Add(레이어배지생성(
                    DiagramLayerState,
                    "현재",
                    "현재 처리 중인 단계입니다.",
                    Icons.Material.Filled.Schedule,
                    "platform-ledger-flow-node-layer-badge--state"));
            }
        }

        if (IsDiagramLayerVisible(DiagramLayerApi))
        {
            var surfaceCount = 흐름노드처리표면해결(node).Count;
            if (surfaceCount > 0)
            {
                badges.Add(레이어배지생성(
                    DiagramLayerApi,
                    $"API {surfaceCount}",
                    "이 노드와 연결되는 기존 API 처리 표면이 있습니다.",
                    Icons.Material.Filled.OpenInNew,
                    "platform-ledger-flow-node-layer-badge--api"));
            }
        }

        return badges
            .OrderByDescending(badge => badge.ConflictPriority)
            .ThenBy(badge => badge.DisplayOrder)
            .ToList();
    }

    private static string 도형입력요약라벨생성(IReadOnlyList<도형입력항목> fields)
        => fields.Count == 0 ? "입력 양식 없음" : $"입력 양식 {fields.Count}";

    private static string 도형입력요약제목생성(IReadOnlyList<도형입력항목> fields)
        => fields.Count == 0
            ? "이 노드에 제안된 입력 양식이 아직 없습니다."
            : $"입력 양식: {string.Join(", ", fields.Select(field => field.Label))}";

    private IReadOnlyList<CommunityLedgerBlockResponse> 흐름노드관련블록해결(원장블록노드 node)
        => SelectedLedgerTemplate.LedgerBlocks
            .Where(block => 흐름노드가블록과맞는가(node, block))
            .ToList();

    private IReadOnlyList<CommunityLedgerCompositionRuleResponse> 흐름노드관련규칙해결(
        원장블록노드 node,
        IReadOnlyList<CommunityLedgerBlockResponse> relatedBlocks)
    {
        var relatedRuleCodes = relatedBlocks
            .SelectMany(block => block.CompositionRuleCodes)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return SelectedLedgerTemplate.CompositionRules
            .Where(rule => relatedRuleCodes.Contains(rule.Code) || 흐름노드가규칙과맞는가(node, rule))
            .ToList();
    }

    private IReadOnlyList<CommunityLedgerProcessingSurfaceResponse> 흐름노드처리표면해결(원장블록노드 node)
        => SelectedLedgerTemplate.ProcessingSurfaces
            .Where(surface => 흐름노드가처리표면과맞는가(node, surface))
            .ToList();

    private string 흐름노드블록값해결(CommunityLedgerBlockResponse block)
    {
        var value = Get원장블록입력값(block.Code);
        return string.IsNullOrWhiteSpace(value) ? "아직 입력 없음" : value.Trim();
    }

    private static bool 흐름노드가블록과맞는가(원장블록노드 node, CommunityLedgerBlockResponse block)
    {
        var text = $"{block.DisplayName} {block.UiSectionHint} {block.BlockType} {block.Purpose}";
        return node.Kind switch
        {
            "sales-channel" => block.BlockType is CommunityLedgerBlockTypes.Order
                or CommunityLedgerBlockTypes.Item
                || 판매채널성격섹션인가(text)
                || 상품성격섹션인가(text),
            "product" => block.BlockType is CommunityLedgerBlockTypes.Item
                or CommunityLedgerBlockTypes.Order
                or CommunityLedgerBlockTypes.Quantity
                || 상품성격섹션인가(text),
            "warehouse" => block.BlockType is CommunityLedgerBlockTypes.Inventory
                or CommunityLedgerBlockTypes.Place
                || 창고성격섹션인가(text),
            "work" => block.BlockType is CommunityLedgerBlockTypes.State
                or CommunityLedgerBlockTypes.Inventory
                || 작업성격섹션인가(text),
            "delivery" => block.BlockType is CommunityLedgerBlockTypes.Handoff
                or CommunityLedgerBlockTypes.Place
                || 배송성격섹션인가(text),
            "confirm" => block.BlockType is CommunityLedgerBlockTypes.Evidence
                or CommunityLedgerBlockTypes.Settlement
                or CommunityLedgerBlockTypes.State
                || 확인성격섹션인가(text),
            _ => text.Contains(node.Title, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static bool 흐름노드가규칙과맞는가(원장블록노드 node, CommunityLedgerCompositionRuleResponse rule)
    {
        var text = $"{rule.Title} {rule.Description} {string.Join(' ', rule.RequiredUiSectionHints)} {string.Join(' ', rule.GatedActionHints)}";
        return node.Kind switch
        {
            "sales-channel" => 판매채널성격섹션인가(text) || 상품성격섹션인가(text),
            "product" => 상품성격섹션인가(text),
            "warehouse" => 창고성격섹션인가(text),
            "work" => 작업성격섹션인가(text),
            "delivery" => 배송성격섹션인가(text),
            "confirm" => 확인성격섹션인가(text),
            _ => text.Contains(node.Title, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static bool 흐름노드가처리표면과맞는가(원장블록노드 node, CommunityLedgerProcessingSurfaceResponse surface)
    {
        var text = $"{surface.ApiEndpointKey} {surface.Method} {surface.RoutePattern} {surface.Purpose} {surface.ServiceHint}";
        return node.Kind switch
        {
            "sales-channel" => ContainsAny(text, "판매", "채널", "스마트스토어", "쿠팡", "스토어", "주문", "sales", "channel", "commerce", "order"),
            "product" => ContainsAny(text, "상품", "주문", "수요", "의뢰", "inventory"),
            "warehouse" => ContainsAny(text, "창고", "재고", "입고", "출고", "warehouse", "inventory"),
            "work" => ContainsAny(text, "피킹", "검수", "포장", "picking", "pack"),
            "delivery" => ContainsAny(text, "배송", "운송", "인계", "배차", "재위탁", "transport", "dispatch", "reconsignment"),
            "confirm" => ContainsAny(text, "완료", "확인", "증빙", "수령", "complete", "proof"),
            _ => text.Contains(node.Title, StringComparison.OrdinalIgnoreCase)
        };
    }

    private bool 원함입력됨 => !string.IsNullOrWhiteSpace(원함입력);

    private bool HasAny원장블록입력
        => 원장블록입력값.Values.Any(value => !string.IsNullOrWhiteSpace(value));

    private string 원함전체문장
        => string.Join(" ", new[] { 원함입력, 원함조건입력 }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim()));

    private CommunityLedgerFlowCandidateResponse 원함추천후보
        => 원함분석결과?.PrimaryCandidate ?? new CommunityLedgerFlowCandidateResponse();

    private CommunityLedgerTemplateResponse 원함추천템플릿
        => CommunityLedgerTemplateCatalog.Find(string.IsNullOrWhiteSpace(원함추천후보.TemplateKey)
            ? selectedLedgerTemplateKey
            : 원함추천후보.TemplateKey);

    private string 원장화판정
    {
        get
        {
            if (원함분석결과 is null)
            {
                return "원함 입력 전";
            }

            if (홍달처리범위밖신호있음())
            {
                return "홍달 처리 범위 밖";
            }

            if (원함추천후보.RelationCode == CommunityLedgerFlowRelationCodes.StrongFlowMatch &&
                !원함분석결과.RequiresHumanReview)
            {
                return "원장 생성 가능";
            }

            if (원함추천후보.RelationCode == CommunityLedgerFlowRelationCodes.LooseCommunityRequest)
            {
                return "커뮤니티 대화 유지";
            }

            return "추가 정보 필요";
        }
    }

    private Color 원장화판정Color
        => 원장화판정 switch
        {
            "원장 생성 가능" => Color.Success,
            "추가 정보 필요" => Color.Warning,
            "홍달 처리 범위 밖" => Color.Error,
            _ => Color.Info
        };

    private Severity 원장화판정Severity
        => 원장화판정 switch
        {
            "원장 생성 가능" => Severity.Success,
            "추가 정보 필요" => Severity.Warning,
            "홍달 처리 범위 밖" => Severity.Error,
            _ => Severity.Info
        };

    private string 원함판정설명
        => 원장화판정 switch
        {
            "원장 생성 가능" => "참여자와 조건을 조금만 더 확인하면 원장 초안으로 정리할 수 있습니다.",
            "추가 정보 필요" => "홍달이 원장 형태를 제안할 수 있지만, 진행 전에 부족한 블록을 더 채워야 합니다.",
            "홍달 처리 범위 밖" => "홍달은 이 내용을 기록하거나 대화로 정리할 수는 있지만, 보증·강제 이행·법적 판단까지 대신하지는 않습니다.",
            _ => "아직 실행 원장보다 커뮤니티 대화나 추가 질문으로 두는 편이 좋습니다."
        };

    private IReadOnlyList<string> 원함보완안내목록
    {
        get
        {
            if (홍달처리범위밖신호있음())
            {
                return
                [
                    "플랫폼 보증, 법적 판단, 강제 이행, 자동 결제 확정으로 읽히는 부분을 사람 확인 문구로 낮춰야 합니다.",
                    "실제 약속과 책임은 참여자가 직접 확인해야 합니다.",
                    "필요하면 원장보다 커뮤니티 대화나 신고/분쟁 검토로 먼저 남깁니다."
                ];
            }

            var 보완 = new List<string>();

            if (원함분석결과 is not null)
            {
                보완.AddRange(원함추천후보.MissingRequiredSignals.Select(signal => $"{signal} 정보를 더 적어주세요."));
            }

            if (보완.Count == 0)
            {
                보완.AddRange(원함추천템플릿.사용자확인책임안내목록.Take(3));
            }

            return 보완;
        }
    }

    protected override Task OnInitializedAsync()
    {
        ViewModel.Configure(AppKey, ResolveRoleTag(RoleLabel));
        HomeModeState.Changed += HandleModeChanged;
        DiagramPalette.Changed += HandleDiagramPaletteChanged;
        DecorationState.Changed += HandleDecorationStateChanged;
        DiagramPalette.BlockRequested += HandlePaletteBlockRequested;
        DiagramPalette.WorkflowPresetRequested += HandleWorkflowPresetRequested;
        isWorkMode = HomeModeState.IsWorkMode;
        isCompactHomeSummary = UseCompactHomeSummary && !isWorkMode && !DiagramPalette.IsDiagramMode;
        HandlePaletteBlockRequested();
        HandleWorkflowPresetRequested();
        _ = LoadCommunityDataInStagesAsync();
        return Task.CompletedTask;
    }

}
