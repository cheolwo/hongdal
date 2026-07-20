using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Food;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 음식주문ControllerTests
{
    [Fact]
    public void Controller는_기존음식주문경로와음식배달기능경계를유지한다()
    {
        var type = typeof(음식주문Controller);

        Assert.Equal("api/v1/food-orders", type.GetCustomAttribute<RouteAttribute>()?.Template);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.FoodDeliveryWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V3_0, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.FoodDeliveryWorkflow, version.FeatureKey);
    }

    [Theory]
    [InlineData(nameof(음식주문Controller.목록조회), null)]
    [InlineData(nameof(음식주문Controller.상세조회), "{orderNo}")]
    public void 주문자조회는_로그인과정확한기존Get경로를요구한다(string methodName, string? route)
    {
        var method = typeof(음식주문Controller).GetMethod(methodName);

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(route, method.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }
}
