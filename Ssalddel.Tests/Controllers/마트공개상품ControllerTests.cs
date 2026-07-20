using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 마트공개상품ControllerTests
{
    [Fact]
    public void Controller는_공개상품경로와마트기능경계를사용한다()
    {
        var type = typeof(마트공개상품Controller);

        Assert.Equal("api/v1/orderer/mart/products", type.GetCustomAttribute<RouteAttribute>()?.Template);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.SsalddelMartWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V3_5, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.SsalddelMartWorkflow, version.FeatureKey);
    }

    [Fact]
    public void 상세는_정확한ProductIdGet경로를사용한다()
    {
        var method = typeof(마트공개상품Controller).GetMethod(nameof(마트공개상품Controller.상세));

        Assert.NotNull(method);
        Assert.Equal("{productId:long}", method.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }
}
