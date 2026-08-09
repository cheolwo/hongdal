using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Controllers.Driver.Progress05;
using Ssalddel.Controllers.Orderer;

namespace Ssalddel.Tests.Controllers;

public sealed class ResidentialPickupPerspectiveControllerTests
{
    [Fact]
    public void 주문자와운송자Route는_역할선택파라미터없이_분리되어있다()
    {
        AssertRoute<주거공동체World관점Controller>(ResidentialPickupPerspectiveRoutes.Orderer);
        AssertRoute<기사주거공동체World관점Controller>(ResidentialPickupPerspectiveRoutes.Transporter);

        Assert.DoesNotContain(
            typeof(주거공동체World관점Controller)
                .GetMethod(nameof(주거공동체World관점Controller.주문자관점조회))!
                .GetParameters(),
            parameter => parameter.ParameterType == typeof(string));
        Assert.DoesNotContain(
            typeof(기사주거공동체World관점Controller)
                .GetMethod(nameof(기사주거공동체World관점Controller.운송자관점조회))!
                .GetParameters(),
            parameter => parameter.ParameterType == typeof(string));
    }

    [Fact]
    public void 주문자와운송자Route는_모두_인증을요구한다()
    {
        Assert.NotNull(
            typeof(주거공동체World관점Controller)
                .GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(
            typeof(기사주거공동체World관점Controller)
                .GetCustomAttribute<AuthorizeAttribute>());
    }

    private static void AssertRoute<TController>(string expectedRoute)
    {
        var type = typeof(TController);
        Assert.Equal(expectedRoute, type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(type.GetCustomAttribute<ApiControllerAttribute>());
    }
}
