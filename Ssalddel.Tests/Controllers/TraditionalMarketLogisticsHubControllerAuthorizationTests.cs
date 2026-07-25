using Ssalddel.Controllers.Admin.TraditionalMarkets;
using Ssalddel.Controllers.Common;
using Microsoft.AspNetCore.Authorization;

namespace Ssalddel.Tests.Controllers;

public sealed class TraditionalMarketLogisticsHubControllerAuthorizationTests
{
    [Fact]
    public void 공개조회Controller는_익명조회를허용한다()
    {
        var attribute = Assert.Single(
            typeof(전통시장물류거점Controller)
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), true));

        Assert.IsType<AllowAnonymousAttribute>(attribute);
    }

    [Fact]
    public void 관리Controller는_서버관리자정책을요구한다()
    {
        var attribute = Assert.Single(
            typeof(전통시장물류거점AdminController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal("서버관리자전용", attribute.Policy);
    }

    [Fact]
    public void 생활권협의Controller는_로그인사용자를요구한다()
    {
        var attribute = Assert.Single(
            typeof(전통시장생활권협의Controller)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>());

        Assert.Null(attribute.Policy);
        Assert.Empty(attribute.Roles ?? string.Empty);
        Assert.Empty(
            typeof(전통시장생활권협의Controller)
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }
}
