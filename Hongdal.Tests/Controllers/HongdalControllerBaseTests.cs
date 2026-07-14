using System.Security.Claims;
using Hongdal.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Tests.Controllers;

public sealed class HongdalControllerBaseTests
{
    [Fact]
    public void CurrentDriverId_ReturnsNameIdentifierClaim()
    {
        var controller = CreateController(
            new Claim(ClaimTypes.NameIdentifier, "driver-1"));

        var driverId = controller.GetCurrentDriverId();

        Assert.Equal("driver-1", driverId);
    }

    [Fact]
    public void CurrentDriverId_ThrowsExpectedErrorWhenClaimIsMissing()
    {
        var controller = CreateController();

        var exception = Assert.Throws<InvalidOperationException>(
            controller.GetCurrentDriverId);

        Assert.Equal("기사 인증 정보가 없습니다.", exception.Message);
    }

    private static TestDriverController CreateController(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "test");
        return new TestDriverController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private sealed class TestDriverController : DriverControllerBase
    {
        public string GetCurrentDriverId()
            => CurrentDriverId();
    }
}
