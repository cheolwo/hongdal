using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 마트주문요청ControllerTests
{
    [Fact]
    public void Controller는_로그인과마트기능으로보호된요청원장경계다()
    {
        var type = typeof(마트주문요청Controller);

        Assert.Equal("api/v1/orderer/mart/order-requests", type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.SsalddelMartWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V3_5, version.Version);
    }

    [Fact]
    public void Controller는_본인목록조회와비구속요청수정철회를제공하고_결제나출고Command는제공하지않는다()
    {
        var type = typeof(마트주문요청Controller);

        Assert.Equal(string.Empty, type.GetMethod(nameof(마트주문요청Controller.목록))?
            .GetCustomAttribute<HttpGetAttribute>()?.Template ?? string.Empty);
        Assert.Equal("{orderRequestId:guid}", type.GetMethod(nameof(마트주문요청Controller.상세))?
            .GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.Equal(string.Empty, type.GetMethod(nameof(마트주문요청Controller.등록))?
            .GetCustomAttribute<HttpPostAttribute>()?.Template ?? string.Empty);
        Assert.Equal("{orderRequestId:guid}/quantity", type.GetMethod(nameof(마트주문요청Controller.수량변경))?
            .GetCustomAttribute<HttpPutAttribute>()?.Template);
        Assert.Equal("{orderRequestId:guid}/withdrawal", type.GetMethod(nameof(마트주문요청Controller.철회))?
            .GetCustomAttribute<HttpPostAttribute>()?.Template);
        Assert.Null(type.GetMethod("결제"));
        Assert.Null(type.GetMethod("재고예약"));
        Assert.Null(type.GetMethod("출고"));
    }
}
