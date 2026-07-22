using Ssalddel.Contracts.Shipper;

namespace Ssalddel.Tests.Contracts.Shipper;

public sealed class ShipperHomePageRoutesTests
{
    [Fact]
    public void 화주허브와대표업무route는_Web과모바일공용계약이다()
    {
        Assert.Equal("/shipper", ShipperHomePageRoutes.Root);
        Assert.Equal("/shipper/request", ShipperHomePageRoutes.DefaultTransportEntry);
        Assert.Equal("/shipper/transport", ShipperHomePageRoutes.TransportWorkspace);
        Assert.Equal("/shipper/warehouse/workspace", ShipperHomePageRoutes.WarehouseWorkspace);
        Assert.Equal("/shipper/sales/orders", ShipperHomePageRoutes.DefaultSalesEntry);
        Assert.Equal("/shipper/international/fcl-lcl", ShipperHomePageRoutes.InternationalPlanner);
    }
}
