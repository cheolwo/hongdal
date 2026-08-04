using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Common;

namespace Ssalddel.Tests.Controllers;

public sealed class 지도신청가원장조회ControllerTests
{
    [Fact]
    public void 마커별내원장조회는_인증된기존지도신청경계에만열린다()
    {
        var controller = typeof(지도신청가원장Controller);
        var method = controller.GetMethod(nameof(지도신청가원장Controller.내마커원장조회));

        Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(
            "api/v1/community/map-applications/provisional-ledger",
            controller.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal("by-map-marker", method?.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.True(method?.GetCustomAttribute<ResponseCacheAttribute>()?.NoStore);
        Assert.Null(method?.GetCustomAttribute<AllowAnonymousAttribute>());
    }
}
