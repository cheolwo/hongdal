using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Common;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 판매채널ControllerTests
{
    [Fact]
    public void Controller는_운영사용자정책으로보호된다()
    {
        var type = typeof(판매채널Controller);

        Assert.Equal("운영사용자전용", type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal("api/v1/sales-channels", type.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void 계정상세는_정확한AccountIdGet경로와판매채널기능경계를사용한다()
    {
        var method = typeof(판매채널Controller).GetMethod(nameof(판매채널Controller.계정상세));

        Assert.NotNull(method);
        Assert.Equal("accounts/{accountId:long}", method.GetCustomAttribute<HttpGetAttribute>()?.Template);
        var feature = Assert.Single(method.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(method.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V2_5, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow, version.FeatureKey);
    }

    [Fact]
    public void 주문상세는_정확한OrderIdGet경로와판매채널기능경계를사용한다()
    {
        var method = typeof(판매채널Controller).GetMethod(nameof(판매채널Controller.주문출고후보상세));

        Assert.NotNull(method);
        Assert.Equal("orders/{orderId:long}", method.GetCustomAttribute<HttpGetAttribute>()?.Template);
        var feature = Assert.Single(method.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(method.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V2_5, version.Version);
    }
}
