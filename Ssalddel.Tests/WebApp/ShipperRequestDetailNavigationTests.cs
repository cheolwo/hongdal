using Microsoft.AspNetCore.Components;
using Ssalddel.WebApp.Models;
using Ssalddel.WebApp.Pages;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class ShipperRequestDetailNavigationTests
{
    [Theory]
    [InlineData("HD-WEB-001", "/shipper/request/HD-WEB-001")]
    [InlineData("REQ/2026 01", "/shipper/request/REQ%2F2026%2001")]
    public void 상세_경로는_의뢰Id를_하나의_안전한_경로_구간으로_만든다(
        string requestId,
        string expected)
    {
        Assert.Equal(expected, ShipperRoutes.RequestDetailFor(requestId));
    }

    [Fact]
    public void 등록_완료_경로는_같은_Id_상세에_완료_표시를_붙인다()
    {
        Assert.Equal(
            "/shipper/request/HD-WEB-001?created=true",
            ShipperRoutes.CreatedRequestDetailFor("HD-WEB-001"));
    }

    [Fact]
    public void 상세_페이지는_Id_입력_진입점과_Id_직접_경로를_함께_제공한다()
    {
        var routes = typeof(ShipperRequestDetailPreview)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .Cast<RouteAttribute>()
            .Select(attribute => attribute.Template)
            .ToArray();

        Assert.Contains(ShipperRoutes.RequestDetailLookup, routes);
        Assert.Contains("/shipper/request/{RequestId}", routes);
    }

    [Theory]
    [InlineData("/shipper/request/detail")]
    [InlineData("/shipper/request/HD-WEB-001")]
    public void 상세_진입점은_로그인이_필요한_ReadOnly_Beta_경계다(string href)
    {
        var state = IntegratedBetaCatalog.Resolve(href);

        Assert.Equal(IntegratedBetaStage.Beta, state.Stage);
        Assert.Equal(WebInteractionBoundary.ReadOnly, state.Boundary);
        Assert.True(state.RequiresAuthentication);
    }
}
