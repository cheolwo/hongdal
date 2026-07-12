using Hongdal.Contracts.Common.Community;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private 도형레이어신호? 최우선도형레이어신호해결(
        원장블록노드 node,
        int nodeIndex,
        IReadOnlyList<원장블록노드> nodes)
    {
        var signals = new List<도형레이어신호>();

        if (IsDiagramLayerVisible(DiagramLayerRisk))
        {
            var riskSignal = 도형리스크신호해결(node, nodeIndex, nodes);
            if (riskSignal is not null)
            {
                signals.Add(riskSignal);
            }
        }

        if (IsDiagramLayerVisible(DiagramLayerEvidence))
        {
            var evidenceSignal = 도형증빙신호해결(node, nodeIndex, nodes);
            if (evidenceSignal is not null && evidenceSignal.ShouldEmphasizeNode)
            {
                signals.Add(evidenceSignal);
            }
        }

        return signals
            .OrderByDescending(signal => signal.ConflictPriority)
            .FirstOrDefault();
    }

    private 도형레이어신호? 도형증빙신호해결(
        원장블록노드 node,
        int nodeIndex,
        IReadOnlyList<원장블록노드> nodes)
    {
        if (!도형증빙필요한가(node))
        {
            return null;
        }

        var state = 원장블록처리상태해결(nodeIndex, nodes);
        var layer = FindDiagramLayer(DiagramLayerEvidence)!;
        var description = "이 단계는 확인, 사진, 수령 또는 정산 근거를 남기는 단계입니다.";

        if (state is 원장블록처리상태.완료)
        {
            return new 도형레이어신호(
                DiagramLayerEvidence,
                layer.DisplayOrder,
                layer.ConflictPriority,
                "platform-ledger-flow-node--priority-evidence",
                레이어배지생성(DiagramLayerEvidence, "증빙됨", description, Icons.Material.Filled.FactCheck, "platform-ledger-flow-node-layer-badge--evidence"),
                false);
        }

        if (state is 원장블록처리상태.진행중)
        {
            return new 도형레이어신호(
                DiagramLayerEvidence,
                layer.DisplayOrder,
                layer.ConflictPriority,
                "platform-ledger-flow-node--priority-evidence",
                레이어배지생성(DiagramLayerEvidence, "증빙 필요", description, Icons.Material.Filled.FactCheck, "platform-ledger-flow-node-layer-badge--evidence"),
                true);
        }

        return new 도형레이어신호(
            DiagramLayerEvidence,
            layer.DisplayOrder,
            layer.ConflictPriority,
            "platform-ledger-flow-node--priority-evidence",
            레이어배지생성(DiagramLayerEvidence, "증빙 대기", description, Icons.Material.Filled.FactCheck, "platform-ledger-flow-node-layer-badge--evidence"),
            false);
    }

    private 도형레이어신호? 도형리스크신호해결(
        원장블록노드 node,
        int nodeIndex,
        IReadOnlyList<원장블록노드> nodes)
    {
        var ledger = 선택현재원장;
        if (ledger is null)
        {
            return null;
        }

        var ledgerText = 현재원장검색문장생성(ledger);
        var nodeText = $"{node.Title} {node.GroupLabel} {node.Description}";
        var state = 원장블록처리상태해결(nodeIndex, nodes);

        if (ContainsAny(ledgerText, "신고", "분쟁", "오류", "실패", "거절", "취소"))
        {
            return 리스크신호생성("리스크", "신고, 분쟁, 오류, 거절 또는 취소 신호가 있어 다른 레이어보다 우선합니다.");
        }

        if (ContainsAny(ledgerText, "지연", "보류", "승인 필요", "점검", "누락") &&
            (state is 원장블록처리상태.진행중 ||
             ContainsAny(nodeText, "관리자", "점검", "정산", "증빙", "확인", "완료")))
        {
            return 리스크신호생성("점검", "지연, 보류, 승인 필요 또는 누락 신호가 있어 운영 점검이 먼저입니다.");
        }

        return null;
    }

    private 도형레이어신호 리스크신호생성(string label, string description)
    {
        var layer = FindDiagramLayer(DiagramLayerRisk)!;
        return new 도형레이어신호(
            DiagramLayerRisk,
            layer.DisplayOrder,
            layer.ConflictPriority,
            "platform-ledger-flow-node--priority-risk",
            레이어배지생성(DiagramLayerRisk, label, description, Icons.Material.Filled.HelpOutline, "platform-ledger-flow-node-layer-badge--risk"),
            true);
    }

    private static bool 도형증빙필요한가(원장블록노드 node)
        => node.Kind is "confirm" ||
           ContainsAny($"{node.Title} {node.GroupLabel} {node.Description}", "증빙", "확인", "완료", "수령", "정산", "도착", "인수");

    private static 도형레이어배지 레이어배지생성(
        string layerKey,
        string label,
        string description,
        string icon,
        string modifierClass)
    {
        var layer = FindDiagramLayer(layerKey)!;
        return new 도형레이어배지(
            layerKey,
            label,
            description,
            icon,
            layer.DisplayOrder,
            layer.ConflictPriority,
            $"platform-ledger-flow-node-layer-badge {modifierClass}");
    }

    private static string 흐름노드역할라벨해결(원장블록노드 node)
    {
        var text = $"{node.Title} {node.GroupLabel} {node.Description}";

        if (ContainsAny(text, "관리자", "운영 점검"))
        {
            return "관리자";
        }

        if (ContainsAny(text, "배차 추천", "정산 표시", "결제/정산"))
        {
            return "플랫폼";
        }

        if (ContainsAny(text, "기사", "상차", "운송 구간", "하차"))
        {
            return "기사";
        }

        if (ContainsAny(text, "수령", "인수", "하차지"))
        {
            return "수령자";
        }

        if (ContainsAny(text, "운송 의뢰", "상품 요청", "음식 요청", "상품 수요"))
        {
            return "요청자";
        }

        return node.Kind switch
        {
            "sales-channel" => "판매자",
            "product" => "요청자",
            "warehouse" => "창고",
            "work" => "작업자",
            "delivery" => "운송자",
            "confirm" => "확인자",
            _ => "참여자"
        };
    }

    private static string 현재원장검색문장생성(현재원장컨텍스트 ledger)
        => string.Join(
            ' ',
            new[]
            {
                ledger.Title,
                ledger.StateLabel,
                ledger.Wish,
                ledger.ConditionSummary,
                ledger.Summary
            }.Concat(ledger.ContextValues.SelectMany(pair => new[] { pair.Key, pair.Value })));

    private static string 원장블록노드아이콘해결(원장블록노드 node)
        => node.Kind switch
        {
            "product" => Icons.Material.Filled.Inventory2,
            "sales-channel" => Icons.Material.Filled.Storefront,
            "place" => Icons.Material.Filled.Flag,
            "warehouse" => Icons.Material.Filled.Warehouse,
            "work" => Icons.Material.Filled.Inventory,
            "delivery" => Icons.Material.Filled.LocalShipping,
            "confirm" => Icons.Material.Filled.TaskAlt,
            _ => Icons.Material.Filled.Hub
        };

    private 노드스티커이미지Response? 원장블록노드스티커해결(원장블록노드 node)
    {
        var pinnedSticker = 노드스티커Catalog.이미지찾기(node.스티커이미지Key);
        if (pinnedSticker is not null)
        {
            return pinnedSticker;
        }

        return 노드스티커Catalog.노드기본이미지찾기(new()
        {
            원장템플릿Key = SelectedLedgerTemplate.Key,
            노드종류 = node.Kind,
            노드제목 = node.Title,
            역할라벨 = 흐름노드역할라벨해결(node),
            상태라벨 = BuildNodeProcessingStateLabel(ResolveNodeDetailProcessingState(node))
        });
    }

    private static Color 원장블록노드색상해결(string kind)
        => 원장블록종류정규화(kind) switch
        {
            "product" => Color.Primary,
            "sales-channel" => Color.Secondary,
            "place" => Color.Tertiary,
            "warehouse" => Color.Success,
            "work" => Color.Warning,
            "delivery" => Color.Secondary,
            "confirm" => Color.Info,
            _ => Color.Default
        };

    private static string 원장블록종류정규화(string kind)
        => kind switch
        {
            "product" or "sales-channel" or "place" or "warehouse" or "work" or "delivery" or "confirm" => kind,
            _ => "work"
        };
}
