using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Controllers.Orderer;

public sealed class 공동구매내원함ControllerTests
{
    [Fact]
    public async Task 목록은_인증클레임의_현재사용자만_조회한다()
    {
        var useCase = new RecordingUseCase
        {
            Response = new 공동구매내원함목록응답
            {
                전체건수 = 1,
                활성건수 = 1,
                원함목록 =
                [
                    new 공동구매내원함응답
                    {
                        개별원함원장Id = "wish-ledger-1",
                        원함상태 = 공동구매내원함상태코드.활성
                    }
                ]
            }
        };
        var controller = new 공동구매내원함Controller(useCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "authenticated-orderer")
                    ], "test"))
                }
            }
        };

        var result = await controller.목록(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(useCase.Response, ok.Value);
        Assert.Equal("authenticated-orderer", useCase.OrdererId);
    }

    [Fact]
    public void Endpoint는_인증된_내원함_전용경로다()
    {
        var controllerType = typeof(공동구매내원함Controller);
        var route = Assert.Single(controllerType.GetCustomAttributes<RouteAttribute>());

        Assert.Equal("api/v1/orderer/group-purchase-wishes/me", route.Template);
        Assert.NotNull(controllerType.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Null(
            controllerType
                .GetMethod(nameof(공동구매내원함Controller.목록))!
                .GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public async Task 인증클레임이없으면_500예외대신_401을반환한다()
    {
        var controller = new 공동구매내원함Controller(new RecordingUseCase())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.목록(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    private sealed class RecordingUseCase : I공동구매내원함조회UseCase
    {
        public 공동구매내원함목록응답 Response { get; set; } = new();
        public string OrdererId { get; private set; } = string.Empty;

        public Task<공동구매내원함목록응답> 조회Async(
            string 주문자키,
            CancellationToken cancellationToken = default)
        {
            OrdererId = 주문자키;
            return Task.FromResult(Response);
        }
    }
}
