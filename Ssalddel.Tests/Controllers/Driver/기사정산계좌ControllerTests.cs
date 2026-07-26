using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Driver.Settlement06;
using 살뜰.Data;

namespace Ssalddel.Tests.Controllers.Driver;

public sealed class 기사정산계좌ControllerTests
{
    [Fact]
    public void Controller는_기사본인정산계좌경로와_기사역할을_요구한다()
    {
        var type = typeof(기사정산계좌Controller);

        Assert.Equal(
            "api/v1/driver/settlement-account",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal(
            역할명.기사,
            type.GetCustomAttribute<AuthorizeAttribute>()?.Roles);
        Assert.NotNull(type.GetMethod(nameof(기사정산계좌Controller.조회))
            ?.GetCustomAttribute<HttpGetAttribute>());
        Assert.NotNull(type.GetMethod(nameof(기사정산계좌Controller.저장))
            ?.GetCustomAttribute<HttpPutAttribute>());
        Assert.NotNull(type.GetMethod(nameof(기사정산계좌Controller.삭제))
            ?.GetCustomAttribute<HttpDeleteAttribute>());
    }
}
