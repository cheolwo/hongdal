using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Driver.Settlement06;
using 살뜰.Data;

namespace Ssalddel.Tests.Controllers.Driver;

public sealed class 기사지급준비ControllerTests
{
    [Fact]
    public void Controller는_기사본인_지급준비조회만_노출한다()
    {
        var type = typeof(기사지급준비Controller);

        Assert.Equal(
            "api/v1/driver/payout-preparations",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal(
            역할명.기사,
            type.GetCustomAttribute<AuthorizeAttribute>()?.Roles);
        Assert.NotNull(type.GetMethod(nameof(기사지급준비Controller.월별조회))
            ?.GetCustomAttribute<HttpGetAttribute>());
        Assert.DoesNotContain(
            type.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.GetCustomAttributes<HttpPostAttribute>().Any()
                      || method.GetCustomAttributes<HttpPutAttribute>().Any()
                      || method.GetCustomAttributes<HttpDeleteAttribute>().Any());
    }
}
