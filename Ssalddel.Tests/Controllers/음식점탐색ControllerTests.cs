using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 음식점탐색ControllerTests
{
    [Fact]
    public void Controller는_공개음식점경로와음식배달기능경계를사용한다()
    {
        var type = typeof(음식점탐색Controller);

        Assert.Equal("api/v1/orderer/restaurants", type.GetCustomAttribute<RouteAttribute>()?.Template);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.FoodDeliveryWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V3_0, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.FoodDeliveryWorkflow, version.FeatureKey);
    }

    [Fact]
    public void 상세는_정확한RestaurantIdGet경로를사용한다()
    {
        var method = typeof(음식점탐색Controller).GetMethod(nameof(음식점탐색Controller.상세));

        Assert.NotNull(method);
        Assert.Equal("{restaurantId:long}", method.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }
}
