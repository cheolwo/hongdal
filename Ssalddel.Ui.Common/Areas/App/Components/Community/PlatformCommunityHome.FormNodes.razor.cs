using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private static IReadOnlyList<PlatformDiagramPaletteBlock> DiagramFormPaletteBlocks
        => PlatformDiagramFormNodeCatalog.All;

    private static bool CanConnectDiagramNodes(원장블록노드 fromNode, 원장블록노드 toNode)
        => !string.Equals(fromNode.Kind, "form", StringComparison.OrdinalIgnoreCase) ||
            PlatformDiagramFormNodeCatalog.CanConnect(
                fromNode.FormKind,
                toNode.Kind,
                toNode.Title);

    private static string ResolveDiagramFormConnectionLabel(string? formKind)
        => PlatformDiagramFormNodeCatalog.GetConnectionRule(formKind).ConnectionLabel;

    private static string BuildDiagramFormConnectionFailureMessage(
        원장블록노드 fromNode,
        원장블록노드 toNode)
    {
        var rule = PlatformDiagramFormNodeCatalog.GetConnectionRule(fromNode.FormKind);
        return $"'{fromNode.Title}'은 {rule.Description} 선택한 대상은 '{toNode.Title}'입니다.";
    }

    private static IReadOnlyList<도형입력항목> 폼도형입력항목해결(string? formKind)
        => formKind switch
        {
            PlatformDiagramFormKinds.TransportRequest =>
            [
                필수입력("화물 품목", "텍스트", "운송할 화물의 이름, 종류와 수량입니다."),
                필수입력("크기/무게", "텍스트", "차량과 작업 난이도 판단에 필요한 부피와 무게입니다."),
                필수입력("상차지", "주소", "화물을 싣는 출발 장소입니다."),
                필수입력("하차지", "주소", "화물을 내리는 도착 장소입니다."),
                필수입력("희망 상차 시간", "일시/시간대", "상차를 원하는 시각 또는 가능한 시간대입니다."),
                선택입력("차량/운임 조건", "텍스트", "차종, 수작업, 운임 협의와 배차 조건입니다.")
            ],
            PlatformDiagramFormKinds.WarehouseOutbound =>
            [
                필수입력("출고 창고", "창고/텍스트", "출고 작업을 수행할 창고 또는 재고 거점입니다."),
                필수입력("출고 품목", "목록/SKU", "출고할 SKU, 상품명과 수량입니다."),
                필수입력("출고 목적지", "주소/텍스트", "다음 창고, 수령자 또는 운송 인계 장소입니다."),
                필수입력("희망 출고 시간", "일시/시간대", "피킹·포장을 마치고 출고해야 하는 시점입니다."),
                선택입력("포장/취급 조건", "텍스트", "냉장, 파손주의, 분할 포장 같은 조건입니다.")
            ],
            PlatformDiagramFormKinds.WarehouseInbound =>
            [
                필수입력("입고 창고", "창고/텍스트", "품목을 인수하고 검수할 도착 창고입니다."),
                필수입력("입고 예정 품목", "목록/SKU", "입고할 SKU, 상품명과 예정 수량입니다."),
                필수입력("입고 예정 시간", "일시/시간대", "납품 또는 운송 하차가 예정된 시점입니다."),
                선택입력("납품자/운송 원장", "텍스트/원장", "공급자, 차량, 송장 또는 연결된 운송 원장입니다."),
                선택입력("보관 조건", "텍스트", "냉장, 파손주의, 랙 또는 검수 조건입니다.")
            ],
            PlatformDiagramFormKinds.TransportPickupConfirmation =>
            [
                필수입력("운송 원장", "원장/텍스트", "상차 확인을 남길 운송진행 원장입니다."),
                필수입력("상차 완료 시각", "일시", "실제로 상차가 끝난 시각입니다."),
                필수입력("적재 수량/상태", "목록/텍스트", "실제 적재 수량, 포장과 파손 여부입니다."),
                선택입력("상차 증빙", "이미지/파일", "상차 사진, 인수 확인과 현장 메모입니다.")
            ],
            PlatformDiagramFormKinds.TransportDropoffConfirmation =>
            [
                필수입력("운송 원장", "원장/텍스트", "하차 확인을 남길 운송진행 원장입니다."),
                필수입력("하차 완료 시각", "일시", "실제로 하차가 끝난 시각입니다."),
                필수입력("인수자/인수 상태", "텍스트/선택", "수령자와 정상, 부분, 보류 같은 인수 결과입니다."),
                선택입력("하차 증빙", "이미지/파일", "하차 사진, 서명, 파손·누락과 인계 메모입니다.")
            ],
            _ =>
            [
                필수입력("폼 목적", "텍스트", "이 폼으로 어떤 업무 정보를 받을지 적습니다."),
                필수입력("필수 입력 항목", "목록/텍스트", "사용자가 반드시 채워야 하는 항목을 적습니다."),
                선택입력("선택 입력 항목", "목록/텍스트", "필요할 때만 받을 보조 항목을 적습니다."),
                선택입력("제출 후 연결", "노드/텍스트", "폼 제출 뒤 이어질 업무 노드나 원장을 정합니다.")
            ]
        };

    private 노드입력준비도 폼노드입력준비도해결(원장블록노드 node)
    {
        var requiredFields = 도형입력항목해결(node)
            .Where(field => field.IsRequired)
            .ToArray();
        var completedCount = requiredFields.Count(field =>
            !string.IsNullOrWhiteSpace(GetDiagramFormValue(node, field)));
        var percent = requiredFields.Length == 0
            ? 100
            : (int)Math.Round(completedCount * 100d / requiredFields.Length);
        var missingFields = requiredFields
            .Where(field => string.IsNullOrWhiteSpace(GetDiagramFormValue(node, field)))
            .Select(field => field.Label)
            .ToArray();

        return new 노드입력준비도(
            percent,
            completedCount,
            requiredFields.Length,
            true,
            missingFields,
            []);
    }

    private string GetDiagramFormValue(원장블록노드 node, 도형입력항목 field)
        => diagramFormValues.TryGetValue(BuildDiagramFormValueKey(node, field), out var value)
            ? value
            : string.Empty;

    private void SetDiagramFormValue(원장블록노드 node, 도형입력항목 field, string? value)
        => diagramFormValues[BuildDiagramFormValueKey(node, field)] = value ?? string.Empty;

    private static string BuildDiagramFormValueKey(원장블록노드 node, 도형입력항목 field)
        => $"{node.FormKind ?? PlatformDiagramFormKinds.Generic}:{node.Title}:{field.Label}";

    private static string ResolveDiagramFormKindLabel(string? formKind)
        => formKind switch
        {
            PlatformDiagramFormKinds.TransportRequest => "운송의뢰 양식",
            PlatformDiagramFormKinds.WarehouseOutbound => "창고 출고 양식",
            PlatformDiagramFormKinds.WarehouseInbound => "창고 입고 양식",
            PlatformDiagramFormKinds.TransportPickupConfirmation => "상차 확인 양식",
            PlatformDiagramFormKinds.TransportDropoffConfirmation => "하차 확인 양식",
            _ => "사용자 정의 양식"
        };

}
