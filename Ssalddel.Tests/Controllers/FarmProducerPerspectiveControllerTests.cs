using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Controllers.Shipper;

namespace Ssalddel.Tests.Controllers;

public sealed class FarmProducerPerspectiveControllerTests
{
    [Fact]
    public void 생산자농장Route는_고정되어있고_인증을요구한다()
    {
        var type = typeof(농장생산자World관점Controller);

        Assert.Equal(
            FarmProducerPerspectiveRoutes.Producer,
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(type.GetCustomAttribute<ApiControllerAttribute>());
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
        Assert.DoesNotContain(
            type.GetMethod(nameof(농장생산자World관점Controller.생산자관점조회))!
                .GetParameters(),
            parameter => parameter.ParameterType == typeof(string));
    }
}
