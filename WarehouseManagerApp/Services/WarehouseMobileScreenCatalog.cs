namespace WarehouseManagerApp.Services;

public static class WarehouseMobileScreenCatalog
{
    public static readonly WarehouseMobileScreenDefinition Home = new(
        "05.01",
        "창고 운영 홈",
        "입고·재고·출고의 오늘 상태와 예외를 한눈에 확인합니다.",
        "A-01");

    public static WarehouseMobileScreenDefinition? Resolve(string? path)
    {
        var normalizedPath = Normalize(path);

        if (normalizedPath is WarehouseManagerRoutes.Home or WarehouseManagerRoutes.Warehouse)
        {
            return Home;
        }

        return normalizedPath switch
        {
            WarehouseManagerRoutes.WorkBoard => new(
                "05.02", "창고 작업 보드",
                "입고부터 출고까지 대기·진행·완료 작업을 우선순위로 봅니다.", "실시간"),
            WarehouseManagerRoutes.ExpectedInbounds => new(
                "05.03", "입고 예정 조회",
                "업체 코드와 입고예정으로 도착 예정 상품과 준비 상태를 찾습니다.", "조회"),
            WarehouseManagerRoutes.Scan => new(
                "05.04", "스캔 스테이션",
                "작업지시·팔레트·상품 코드를 스캔해 다음 업무를 엽니다.", "SCAN"),
            WarehouseManagerRoutes.InboundProductScan => new(
                "05.05", "입고상품 수령",
                "도착 상품을 스캔하고 예정 수량과 실제 수량을 대조합니다.", "IN-82"),
            WarehouseManagerRoutes.InboundInspection => new(
                "05.06", "입고 검수 목록",
                "수령 상품을 우선순위와 취급 조건에 따라 검수합니다.", "검사"),
            WarehouseManagerRoutes.PutAwayTask => new(
                "05.09", "적재 작업",
                "검수 완료 상품의 지정 위치를 확인해 재고 위치를 기록합니다.", "PUT-41"),
            WarehouseManagerRoutes.GeneralInventory => new(
                "05.10", "일반 재고 현황",
                "상품·LOT·위치별 가용·할당·보류 수량을 조회합니다.", "재고"),
            WarehouseManagerRoutes.PickingBatch => new(
                "05.11", "피킹 작업 목록",
                "출고 마감·동선·상품 수를 기준으로 피킹 작업을 봅니다.", "피킹"),
            WarehouseManagerRoutes.PackingTask => new(
                "05.14", "포장 작업",
                "피킹 결과를 주문별로 포장하고 라벨·무게·봉인을 확인합니다.", "OUT-31"),
            WarehouseManagerRoutes.OutboundPlanReview => new(
                "05.15", "출고예정 운송 전 검토",
                "출고 수량·상차 시간·차량·인계 증빙을 운송 전에 확인합니다.", "검토"),
            WarehouseManagerRoutes.GeneralTransportHandoff => new(
                "05.16", "출고 인계 준비",
                "포장 완료 상품을 기사·차량·인계 시간과 대조합니다.", "OUT-31"),
            WarehouseManagerRoutes.WarehouseExceptions => new(
                "05.17", "창고 예외 처리",
                "수량·파손·온도·주소 차이를 담당자 근거와 함께 처리합니다.", "예외"),
            WarehouseManagerRoutes.WarehouseHistory => new(
                "05.18", "창고 작업 이력",
                "작업자·시간·업무·결과 상태를 기준으로 변경 이력을 조회합니다.", "이력"),
            WarehouseManagerRoutes.WarehouseSettings => new(
                "05.19", "창고 설정",
                "창고 역할과 운영 유형별로 사용할 업무 화면을 설정합니다.", "설정"),
            WarehouseManagerRoutes.ImportCustoms => new(
                "05.20", "보세·통관 상태",
                "보세 반입부터 검사·신고·반출 가능 상태를 서류 근거와 함께 봅니다.", "수입"),
            _ => ResolveDynamic(normalizedPath)
        };
    }

    private static WarehouseMobileScreenDefinition? ResolveDynamic(string path)
    {
        if (path.StartsWith($"{WarehouseManagerRoutes.InboundInspection}/", StringComparison.OrdinalIgnoreCase))
        {
            return path.EndsWith("/record", StringComparison.OrdinalIgnoreCase)
                ? new(
                    "05.08", "입고 검수 실행",
                    "불량·수량·온도와 검수 증빙을 기록하고 완료 여부를 결정합니다.", "기록")
                : new(
                    "05.07", "입고 검수 상세",
                    "예정 정보와 실제 수령 상품을 비교해 검수 항목을 확인합니다.", "IN-82");
        }

        if (path.StartsWith($"{WarehouseManagerRoutes.PickingBatch}/", StringComparison.OrdinalIgnoreCase))
        {
            return path.EndsWith("/execute", StringComparison.OrdinalIgnoreCase)
                ? new(
                    "05.13", "피킹 작업 실행",
                    "위치와 상품을 순서대로 스캔해 피킹 수량을 기록합니다.", "진행")
                : new(
                    "05.12", "피킹 작업 상세",
                    "배치 정보와 첫 피킹 위치, LOT 우선순위를 확인합니다.", "PICK-31");
        }

        return null;
    }

    private static string Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var normalized = path.Split('?', '#')[0].TrimEnd('/');
        return normalized.Length == 0 ? "/" : normalized;
    }
}

public sealed record WarehouseMobileScreenDefinition(
    string ScreenCode,
    string Title,
    string Description,
    string Badge);
