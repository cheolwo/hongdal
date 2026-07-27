using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Common;

namespace Ssalddel.Tests.Controllers;

public sealed class 주문원장ControllerTests
{
    [Fact]
    public void 기존Controller는_네원장종류와주문자내원장목록을제공한다()
    {
        var type = typeof(주문원장Controller);

        Assert.Equal(
            "types",
            type.GetMethod(nameof(주문원장Controller.원장종류))?
                .GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.Equal(
            "mine",
            type.GetMethod(nameof(주문원장Controller.내원장목록))?
                .GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.Equal(
            "{주문원장Id}/views/orderer",
            type.GetMethod(nameof(주문원장Controller.주문자조회))?
                .GetCustomAttribute<HttpGetAttribute>()?.Template);
    }
}
