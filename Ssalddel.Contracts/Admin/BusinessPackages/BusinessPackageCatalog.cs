namespace Ssalddel.Contracts.Admin.BusinessPackages;

/// <summary>
/// Stable business ownership for administrator applications.  Identity, tenant
/// context and authorization remain shared platform concerns; this catalog only
/// assigns operational workflows and their compatibility routes.
/// </summary>
public sealed record BusinessPackageDefinition(
    string Code,
    string DisplayName,
    string Description,
    IReadOnlyList<BusinessPackageWorkflow> Workflows,
    string? CompletedOutboundHandoffPackageCode = null);

public sealed record BusinessPackageWorkflow(string Label, string Description, string AdminPath);

public static class BusinessPackageCatalog
{
    public const string FoodDelivery = "food-delivery";
    public const string FreightDelivery = "freight-delivery";
    public const string OrderWarehouse = "order-warehouse";

    public static IReadOnlyDictionary<string, BusinessPackageDefinition> All { get; } =
        new Dictionary<string, BusinessPackageDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            [FoodDelivery] = new(
                FoodDelivery,
                "음식 배달",
                "음식점 주문의 조리·픽업·라이더 배정·배달 상태를 운영합니다.",
                [
                    new("음식 운영", "음식점 주문과 배달 운영 상태를 확인합니다.", 업무패키지관리Routes.FoodDeliveryOperations),
                    new("음식 주문 운영 추적", "주문, 배차·추천, 전달 확인과 Outbox 상태를 조회합니다.", 업무패키지관리Routes.FoodDeliveryOrderTrace),
                    new("음식 배달 배차 검토", "음식 배달 기사 후보와 배차 판단을 검토합니다.", 업무패키지관리Routes.FoodDeliveryDispatchReview)
                ]),
            [FreightDelivery] = new(
                FreightDelivery,
                "화물 배송",
                "화물 의뢰, 차량·기사 배정, 배차·경로, 추적과 운송 예외를 운영합니다.",
                [
                    new("화물 의뢰", "화주 운송 의뢰와 화물 정보를 확인합니다.", 업무패키지관리Routes.FreightDeliveryRequests),
                    new("배차 대기", "배차가 필요한 운송 의뢰를 확인합니다.", 업무패키지관리Routes.FreightDeliveryDispatchWait),
                    new("화물 배차 검토", "국내 화물 차량 후보와 배차 판단을 검토합니다.", 업무패키지관리Routes.FreightDeliveryDispatchReview),
                    new("기사 운행", "운행 중인 기사와 최근 위치 및 배차 이력을 확인합니다.", 업무패키지관리Routes.FreightDeliveryDrivers),
                    new("운송 관제", "운송 상태, 이벤트, 증빙과 정산 흐름을 확인합니다.", 업무패키지관리Routes.FreightDeliveryTransports),
                    new("차량 관리", "운송 차량과 배차 가능 상태를 관리합니다.", 업무패키지관리Routes.FreightDeliveryVehicles)
                ]),
            [OrderWarehouse] = new(
                OrderWarehouse,
                "주문·창고",
                "주문과 출고 요청을 확인하고 창고에서 완료된 출고를 운송과 문서 원장으로 인계합니다.",
                [
                    new("주문·출고 운영 현황", "주문 처리와 출고 인계 상태를 운영 관점에서 확인합니다.", 업무패키지관리Routes.OrderWarehouseDashboard),
                    new("출고 요청", "창고에서 완료된 출고가 운송 의뢰로 인계되었는지 확인합니다.", 업무패키지관리Routes.OrderWarehouseOutboundRequests),
                    new("출고 운송 추적", "화물 배송 패키지로 인계된 운송 상태를 조회합니다.", 업무패키지관리Routes.OrderWarehouseOutboundTransports),
                    new("증빙과 문서", "입고·출고와 운송 인계에 연결된 문서 원장을 확인합니다.", 업무패키지관리Routes.OrderWarehouseDocuments)
                ],
                FreightDelivery)
        };

    public static BusinessPackageDefinition GetRequired(string packageCode)
        => All.TryGetValue(packageCode, out var definition)
            ? definition
            : throw new InvalidOperationException($"Unknown business package '{packageCode}'.");
}
