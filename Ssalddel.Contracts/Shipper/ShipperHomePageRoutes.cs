using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Contracts.Shipper;

/// <summary>
/// Web과 모바일 화주 허브가 공유하는 canonical route입니다.
/// 실제 업무 route의 권한과 기능 플래그는 각 화면과 서버가 다시 확인합니다.
/// </summary>
public static class ShipperHomePageRoutes
{
    public const string Root = "/shipper";
    public const string TransportWorkspace = $"{Root}/transport";
    public const string WarehouseWorkspace = $"{Root}/warehouse/workspace";
    public const string InternationalPlanner = $"{Root}/international/fcl-lcl";

    public const string DefaultTransportEntry = ShipperRequestPageRoutes.Root;
    public const string DefaultSalesEntry = SalesOrderPageRoutes.Root;
}
