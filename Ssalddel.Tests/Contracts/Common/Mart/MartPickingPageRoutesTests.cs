using Ssalddel.Contracts.Common.Mart;

namespace Ssalddel.Tests.Contracts.Common.Mart;

public sealed class MartPickingPageRoutesTests
{
    [Fact]
    public void 앱과Web상세Route는_같은주문Id의독립화면을가리킨다()
    {
        Assert.Equal("/mart/picking/orders/73", MartPickingPageRoutes.AppDetailFor(73));
        Assert.Equal("/warehouse/mart/picking/orders/73", MartPickingPageRoutes.WebDetailFor(73));
        Assert.Throws<ArgumentOutOfRangeException>(() => MartPickingPageRoutes.AppDetailFor(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => MartPickingPageRoutes.WebDetailFor(-1));
    }
}
