using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.WebApp.Services;

public sealed class WebPlatformCommunityNodeNavigationResolver
    : IPlatformCommunityNodeNavigationResolver
{
    private const string DriverRecommendations = DriverRoutes.Recommendations;
    private const string DriverCurrentTransport = DriverRoutes.CurrentTransport;

    public PlatformCommunityNodeNavigationTarget? Resolve(
        PlatformCommunityNodeNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (ResolveForm(request) is { } formTarget)
        {
            return formTarget;
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.CargoTransport))
        {
            return ResolveCargoTransport(request);
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.SsalddelMart))
        {
            return request.TitleContainsAny("피킹", "포장", "픽업")
                ? Warehouse(WarehouseManagerRoutes.MartPickingPacking, "마트 피킹·포장 화면")
                : request.TitleContainsAny("재고", "창고")
                    ? Warehouse(WarehouseManagerRoutes.MartWorkBoard, "마트 작업 보드")
                    : Warehouse(WarehouseManagerRoutes.MartHome, "마트 업무 화면");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.WarehouseInbound))
        {
            if (request.TitleContainsAny("운송 하차", "하차 증빙"))
            {
                return Driver(DriverCurrentTransport, "현재 운송에서 하차 단계 확인");
            }

            if (request.TitleContainsAny("검수"))
            {
                return Warehouse(WarehouseManagerRoutes.InboundInspection, "입고 검수 화면");
            }

            return request.TitleContainsAny("상품", "바코드", "입고")
                ? Warehouse(WarehouseManagerRoutes.InboundProductScan, "입고 상품 확인 화면")
                : Shipper(ShipperRoutes.InboundRequests, "입고 요청 목록");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.WarehouseOutbound))
        {
            if (request.TitleContainsAny("운송 상차", "운송 하차", "상차 증빙", "하차 증빙"))
            {
                return Driver(DriverCurrentTransport, "현재 운송에서 증빙 단계 확인");
            }

            if (request.TitleContainsAny("창고 입고"))
            {
                return Warehouse(WarehouseManagerRoutes.InboundProductScan, "입고 상품 확인 화면");
            }

            if (request.TitleContainsAny("피킹"))
            {
                return Warehouse(WarehouseManagerRoutes.PickingBatch, "피킹 작업 목록");
            }

            if (request.TitleContainsAny("출고", "포장", "작업"))
            {
                return Warehouse(WarehouseManagerRoutes.WorkBoard, "창고 작업 보드");
            }

            return request.TitleContainsAny("배송", "운송")
                ? Driver(DriverRecommendations, "기사 배차·추천 화면")
                : Warehouse(WarehouseManagerRoutes.Home, "창고 업무 화면");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.FoodDelivery))
        {
            return Driver(DriverRecommendations, "기사 배차·추천 화면");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.GroupPurchase))
        {
            return Community(CommunityPageRoutes.GroupPurchase, "공동구매 합의 화면");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.GroupImport))
        {
            return Community(CommunityPageRoutes.GroupImport, "같이 수입 원장 화면");
        }

        return request.IsNodeKind("warehouse")
            ? Warehouse(WarehouseManagerRoutes.Home, "창고 업무 화면")
            : request.IsNodeKind("delivery")
                ? Driver(DriverRecommendations, "기사 배차·추천 화면")
                : Community(CommunityPageRoutes.Home, "커뮤니티 원장 화면");
    }

    private static PlatformCommunityNodeNavigationTarget? ResolveForm(
        PlatformCommunityNodeNavigationRequest request)
    {
        if (request.IsForm(PlatformDiagramFormKinds.TransportRequest))
        {
            return Shipper(ShipperRoutes.Request, "운송 의뢰 작성 화면");
        }

        if (request.IsForm(PlatformDiagramFormKinds.WarehouseOutbound))
        {
            return Warehouse(WarehouseManagerRoutes.WorkBoard, "창고 작업 보드");
        }

        if (request.IsForm(PlatformDiagramFormKinds.WarehouseInbound))
        {
            return Shipper(ShipperRoutes.InboundRequestCreate, "입고 요청 작성 화면");
        }

        return request.IsForm(PlatformDiagramFormKinds.TransportPickupConfirmation)
               || request.IsForm(PlatformDiagramFormKinds.TransportDropoffConfirmation)
            ? Driver(DriverCurrentTransport, "현재 운송에서 증빙 단계 확인")
            : null;
    }

    private static PlatformCommunityNodeNavigationTarget ResolveCargoTransport(
        PlatformCommunityNodeNavigationRequest request)
    {
        if (request.TitleContainsAny("운송 의뢰"))
        {
            return Shipper(ShipperRoutes.Request, "운송 의뢰 작성 화면");
        }

        if (request.TitleContainsAny("배차", "기사 수락", "기사 거절"))
        {
            return Driver(DriverRecommendations, "기사 배차·추천 화면");
        }

        if (request.TitleContainsAny("운송 구간"))
        {
            return Driver(DriverCurrentTransport, "진행 중 운송 화면");
        }

        if (request.TitleContainsAny("상차", "하차", "수령", "인수", "증빙"))
        {
            return Driver(DriverCurrentTransport, "현재 운송에서 증빙 단계 확인");
        }

        return request.TitleContainsAny("정산")
            ? Shipper(ShipperRoutes.PaymentStatus, "결제·정산 상태 화면")
            : Shipper(ShipperRoutes.Request, "운송 의뢰 목록");
    }

    private static PlatformCommunityNodeNavigationTarget Community(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Community);

    private static PlatformCommunityNodeNavigationTarget Shipper(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Shipper);

    private static PlatformCommunityNodeNavigationTarget Driver(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Driver);

    private static PlatformCommunityNodeNavigationTarget Warehouse(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Warehouse);
}
