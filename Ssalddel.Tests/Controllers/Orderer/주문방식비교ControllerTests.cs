using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers.Orderer;

public sealed class 주문방식비교ControllerTests
{
    [Fact]
    public void Controller는_0점5비교경로와공동구매기능경계를사용한다()
    {
        var type = typeof(주문방식비교Controller);

        Assert.Equal(
            "api/v1/orderer/order-mode-comparisons",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(
            VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
            Assert.Single(feature.Arguments!));
        var method = type.GetMethod(nameof(주문방식비교Controller.미리보기));
        Assert.Equal("preview", method?.GetCustomAttribute<HttpPostAttribute>()?.Template);
    }

    [Fact]
    public void 미리보기는_저장없이UseCase비교결과를반환한다()
    {
        var response = new 주문방식비교응답 { 상품키 = "apple-5kg" };
        var fake = new FakeUseCase(response);
        var controller = new 주문방식비교Controller(fake);

        var action = controller.미리보기(new 주문방식비교요청());

        Assert.Same(response, Assert.IsType<OkObjectResult>(action).Value);
        Assert.Equal(1, fake.CallCount);
    }

    [Fact]
    public void 잘못된비교조건은_400으로응답한다()
    {
        var controller = new 주문방식비교Controller(
            new ThrowingUseCase());

        var action = controller.미리보기(new 주문방식비교요청());

        var problem = Assert.IsType<ObjectResult>(action);
        Assert.Equal(400, problem.StatusCode);
    }

    private sealed class FakeUseCase(주문방식비교응답 response) : I주문방식비교UseCase
    {
        public int CallCount { get; private set; }

        public 주문방식비교응답 비교(주문방식비교요청 request)
        {
            CallCount++;
            return response;
        }
    }

    private sealed class ThrowingUseCase : I주문방식비교UseCase
    {
        public 주문방식비교응답 비교(주문방식비교요청 request)
            => throw new ArgumentException("invalid");
    }
}
