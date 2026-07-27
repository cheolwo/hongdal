using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SsalddelApp.Services;

public sealed class SsalddelAppPlatformCommunityNodeNavigationResolver
    : IPlatformCommunityNodeNavigationResolver
{
    public PlatformCommunityNodeNavigationTarget? Resolve(
        PlatformCommunityNodeNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.IsForm(PlatformDiagramFormKinds.TransportRequest))
        {
            return Shipper(ShipperRoutes.Request, "운송 의뢰 작성 화면");
        }

        if (request.IsForm(PlatformDiagramFormKinds.WarehouseInbound))
        {
            return Shipper(ShipperRoutes.InboundRequestCreate, "입고 요청 작성 화면");
        }

        if (request.IsForm(PlatformDiagramFormKinds.WarehouseOutbound))
        {
            return Warehouse(ShipperRoutes.WarehouseWorkspace, "창고 작업대");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.CargoTransport)
            || request.IsForm(PlatformDiagramFormKinds.TransportPickupConfirmation)
            || request.IsForm(PlatformDiagramFormKinds.TransportDropoffConfirmation))
        {
            return Shipper(ShipperRoutes.Request, "운송 의뢰 목록");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.WarehouseInbound))
        {
            return Shipper(ShipperRoutes.InboundRequests, "입고 요청 목록");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.SsalddelMart)
            || request.IsLedgerTemplate(CommunityLedgerTemplateKeys.WarehouseOutbound)
            || request.IsNodeKind("warehouse"))
        {
            return Warehouse(ShipperRoutes.WarehouseWorkspace, "창고 작업대");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.GroupPurchase))
        {
            return Community(CommunityPageRoutes.GroupPurchase, "공동구매 합의 화면");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.GroupImport))
        {
            return Community(CommunityPageRoutes.GroupImport, "같이 수입 원장 화면");
        }

        return Community(CommunityPageRoutes.Home, "커뮤니티 원장 화면");
    }

    private static PlatformCommunityNodeNavigationTarget Community(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Community);

    private static PlatformCommunityNodeNavigationTarget Shipper(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Shipper);

    private static PlatformCommunityNodeNavigationTarget Warehouse(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Warehouse);
}
