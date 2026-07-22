using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace WarehouseManagerApp.Services;

public sealed class WarehousePlatformCommunityNodeNavigationResolver
    : IPlatformCommunityNodeNavigationResolver
{
    public PlatformCommunityNodeNavigationTarget? Resolve(
        PlatformCommunityNodeNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.IsForm(PlatformDiagramFormKinds.TransportRequest))
        {
            return Warehouse(WarehouseManagerRoutes.TransportRequestDraft, "운송 의뢰 초안 화면");
        }

        if (request.IsForm(PlatformDiagramFormKinds.WarehouseOutbound))
        {
            return Warehouse(WarehouseManagerRoutes.WorkBoard, "창고 작업 보드");
        }

        if (request.IsForm(PlatformDiagramFormKinds.WarehouseInbound))
        {
            return Warehouse(WarehouseManagerRoutes.InboundWorkStart, "입고 작업 화면");
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
            if (request.TitleContainsAny("검수"))
            {
                return Warehouse(WarehouseManagerRoutes.InboundInspection, "입고 검수 화면");
            }

            return request.TitleContainsAny("상품", "바코드", "입고")
                ? Warehouse(WarehouseManagerRoutes.InboundProductScan, "입고 상품 확인 화면")
                : Warehouse(WarehouseManagerRoutes.InboundWorkStart, "입고 작업 화면");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.WarehouseOutbound))
        {
            if (request.TitleContainsAny("피킹"))
            {
                return Warehouse(WarehouseManagerRoutes.PickingBatch, "피킹 작업 목록");
            }

            return request.TitleContainsAny("출고", "포장", "작업")
                ? Warehouse(WarehouseManagerRoutes.WorkBoard, "창고 작업 보드")
                : Warehouse(WarehouseManagerRoutes.Warehouse, "창고 업무 화면");
        }

        return request.IsNodeKind("warehouse")
            ? Warehouse(WarehouseManagerRoutes.Warehouse, "창고 업무 화면")
            : null;
    }

    private static PlatformCommunityNodeNavigationTarget Warehouse(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Warehouse);
}
