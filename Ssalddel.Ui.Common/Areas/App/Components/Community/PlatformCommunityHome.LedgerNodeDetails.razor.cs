using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App;
using Ssalddel.Ui.Common.Areas.App.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    [Inject]
    private IPlatformCommunityNodeNavigationResolver NodeNavigationResolver { get; set; } = default!;

    private PlatformCommunityDiagramNodeDetailPresentation BuildNodeDetailPresentation(원장블록노드 node)
    {
        var state = ResolveNodeDetailProcessingState(node);
        var readiness = 노드입력준비도해결(node, state);
        var currentLedger = 선택현재원장;
        return new(
            node,
            BuildNodeProcessingStateLabel(state),
            BuildNodeProcessingStateColor(state),
            BuildNodeKindLabel(node.Kind),
            IsDiagramMode ? BuildDiagramNodeStackLabel(node) : null,
            node.Kind.Equals("form", StringComparison.OrdinalIgnoreCase)
                ? ResolveDiagramFormKindLabel(node.FormKind)
                : null,
            readiness,
            readiness.Percent >= 100
                ? "platform-diagram-node-readiness-card platform-diagram-node-readiness-card--complete"
                : "platform-diagram-node-readiness-card",
            BuildNodeReadinessStyle(readiness),
            ResolveNodeDetailContextValues(node),
            도형입력항목해결(node),
            currentLedger is null ? null : $"{currentLedger.Title} · {currentLedger.StateLabel}",
            BuildNodeDetailAction(node),
            IsDiagramMode,
            CanBringSelectedDiagramNodeToFront(),
            CanSendSelectedDiagramNodeToBack(),
            창고대행신청노드인가(node));
    }

    private string ResolveNodeDetailFormValue(도형입력항목 field)
        => nodeDetailPanelNode is null ? string.Empty : GetDiagramFormValue(nodeDetailPanelNode, field);

    private void HandleNodeDetailLedgerBlockValueChanged(PlatformCommunityLedgerBlockValueChange change)
        => Set원장블록입력값(change.BlockCode, change.Value);

    private void HandleNodeDetailFormValueChanged(PlatformCommunityDiagramFormValueChange change)
    {
        if (nodeDetailPanelNode is not null)
        {
            SetDiagramFormValue(nodeDetailPanelNode, change.Field, change.Value);
        }
    }

    private 도형상세동작 BuildNodeDetailAction(원장블록노드 node)
    {
        var target = NodeNavigationResolver.Resolve(new(
            selectedLedgerTemplateKey,
            node.Title,
            node.Kind,
            node.FormKind));
        var basePath = PageNavigationContext.NormalizeReturnPath(target?.Path);
        if (target is null || basePath is null)
        {
            return new(
                null,
                $"현재 앱에는 {node.Title} 노드와 연결된 전용 화면이 없습니다. 원장 맥락은 이 패널에서 계속 확인할 수 있습니다.",
                Icons.Material.Filled.LinkOff,
                Color.Default);
        }

        var values = new Dictionary<string, string?>
        {
            ["source"] = "diagram-node",
            ["ledgerTemplateKey"] = selectedLedgerTemplateKey,
            ["ledgerId"] = 선택현재원장?.Id,
            ["nodeTitle"] = node.Title,
            ["nodeKind"] = node.Kind,
            ["formKind"] = node.FormKind,
            [PageNavigationQueryNames.ReturnPath] = PageNavigationContext.NormalizeReturnPath(DiagramReturnHref)
        };

        return new(
            PlatformCommunityNavigationQuery.Build(basePath, values),
            $"{node.Title} 노드를 {target.DestinationLabel}에서 확인합니다.",
            ResolveNodeDetailActionIcon(target.Area),
            ResolveNodeDetailActionColor(target.Area));
    }

    private static string ResolveNodeDetailActionIcon(PlatformCommunityNodeNavigationArea area)
        => area switch
        {
            PlatformCommunityNodeNavigationArea.Warehouse => Icons.Material.Filled.Warehouse,
            PlatformCommunityNodeNavigationArea.Driver => Icons.Material.Filled.LocalShipping,
            PlatformCommunityNodeNavigationArea.Shipper => Icons.Material.Filled.Assignment,
            PlatformCommunityNodeNavigationArea.Food => Icons.Material.Filled.Restaurant,
            _ => Icons.Material.Filled.OpenInNew
        };

    private static Color ResolveNodeDetailActionColor(PlatformCommunityNodeNavigationArea area)
        => area switch
        {
            PlatformCommunityNodeNavigationArea.Warehouse => Color.Success,
            PlatformCommunityNodeNavigationArea.Driver => Color.Secondary,
            PlatformCommunityNodeNavigationArea.Shipper => Color.Primary,
            PlatformCommunityNodeNavigationArea.Food => Color.Warning,
            _ => Color.Default
        };

    private 원장블록처리상태 ResolveNodeDetailProcessingState(원장블록노드 node)
    {
        var nodes = 정렬된원장블록노드목록가져오기(선택원장블록흐름도);
        var nodeIndex = 원장블록노드순서찾기(nodes, node.Title);
        return nodeIndex >= 0
            ? 원장블록처리상태해결(nodeIndex, nodes)
            : 원장블록처리상태.대기;
    }

    private IReadOnlyList<KeyValuePair<string, string>> ResolveNodeDetailContextValues(원장블록노드 node)
    {
        var ledger = 선택현재원장;
        if (ledger is null)
        {
            return [];
        }

        return ledger.ContextValues
            .Where(pair => IsNodeDetailContextMatch(node, pair.Key))
            .Take(4)
            .ToList();
    }

    private static bool IsNodeDetailContextMatch(원장블록노드 node, string contextKey)
    {
        var nodeText = $"{node.Title} {node.GroupLabel} {node.Description}";
        if (nodeText.Contains(contextKey, StringComparison.OrdinalIgnoreCase) ||
            contextKey.Contains(node.Title, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ContainsAny(node.Title, "상차") && contextKey.Equals("상차", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ContainsAny(node.Title, "하차") && contextKey.Equals("하차", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ContainsAny(node.Title, "운송", "배차", "기사", "의뢰") &&
            ContainsAny(contextKey, "참여자", "화물"))
        {
            return true;
        }

        if (ContainsAny(node.Title, "증빙", "확인") && ContainsAny(contextKey, "증빙", "참여자"))
        {
            return true;
        }

        if (ContainsAny(node.Title, "정산") && contextKey.Equals("정산", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ContainsAny(node.Title, "재고", "창고") && ContainsAny(contextKey, "재고", "보관", "입고"))
        {
            return true;
        }

        if (ContainsAny(node.Title, "피킹", "포장") && ContainsAny(contextKey, "피킹", "포장"))
        {
            return true;
        }

        return false;
    }

    private static string BuildNodeProcessingStateLabel(원장블록처리상태 state)
        => state switch
        {
            원장블록처리상태.완료 => "처리 완료",
            원장블록처리상태.진행중 => "처리 중",
            _ => "처리 대기"
        };

    private static Color BuildNodeProcessingStateColor(원장블록처리상태 state)
        => state switch
        {
            원장블록처리상태.완료 => Color.Success,
            원장블록처리상태.진행중 => Color.Primary,
            _ => Color.Default
        };

    private static string BuildNodeKindLabel(string kind)
        => 원장블록종류정규화(kind) switch
        {
            "product" => "요청/상품",
            "sales-channel" => "판매채널",
            "place" => "장소",
            "warehouse" => "창고/재고",
            "work" => "작업",
            "delivery" => "운송/전달",
            "confirm" => "확인/증빙",
            "form" => "입력 폼",
            _ => "업무 노드"
        };

    private static bool 창고대행신청노드인가(원장블록노드 node)
        => node.Kind.Equals("warehouse", StringComparison.OrdinalIgnoreCase);

}
