using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace FDriverApp.Services;

public sealed class FDriverPlatformCommunityNodeNavigationResolver
    : IPlatformCommunityNodeNavigationResolver
{
    private const string Workspace = "/food-delivery/open/workspace";
    private const string Dispatch = "/food-delivery/open/dispatch";
    private const string Delivery = "/food-delivery/open/delivery";
    private const string Route = "/food-delivery/open/route";
    private const string Settlement = "/food-delivery/open/settlement";

    public PlatformCommunityNodeNavigationTarget? Resolve(
        PlatformCommunityNodeNavigationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.IsForm(PlatformDiagramFormKinds.TransportPickupConfirmation)
            || request.IsForm(PlatformDiagramFormKinds.TransportDropoffConfirmation))
        {
            return Driver(Delivery, "배달 이행 화면");
        }

        if (request.IsLedgerTemplate(CommunityLedgerTemplateKeys.CargoTransport)
            || request.IsLedgerTemplate(CommunityLedgerTemplateKeys.FoodDelivery))
        {
            if (request.TitleContainsAny("배차", "기사 수락", "기사 거절"))
            {
                return Driver(Dispatch, "배차 추천 화면");
            }

            if (request.TitleContainsAny("정산"))
            {
                return Driver(Settlement, "배달 정산 화면");
            }

            return request.TitleContainsAny("운송 구간", "경로")
                ? Driver(Route, "위치·경로 화면")
                : Driver(Workspace, "배달 업무 화면");
        }

        return request.IsNodeKind("delivery")
            ? Driver(Delivery, "배달 이행 화면")
            : null;
    }

    private static PlatformCommunityNodeNavigationTarget Driver(string path, string label)
        => new(path, label, PlatformCommunityNodeNavigationArea.Driver);
}
