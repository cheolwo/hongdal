using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Common;
using Ssalddel.Filters;
using Ssalddel.Security;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 마트피킹ControllerTests
{
    [Fact]
    public void Controller는_운영사용자와Hr역할및마트기능으로보호된다()
    {
        var type = typeof(마트피킹Controller);

        Assert.Equal("api/v1/warehouse-operations/mart/picking-orders", type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal("운영사용자전용", type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        var hrRole = Assert.Single(type.GetCustomAttributes<RequireHrRoleAttribute>());
        Assert.Contains("Warehouse.Manager", hrRole.RoleCodes);
        Assert.Contains("Warehouse.InventoryOperator", hrRole.RoleCodes);
        Assert.Contains("Warehouse.DispatchOperator", hrRole.RoleCodes);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.SsalddelMartWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V3_5, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.SsalddelMartWorkflow, version.FeatureKey);
    }

    [Fact]
    public void 상세는_정확한OrderIdGet경로를사용한다()
    {
        var method = typeof(마트피킹Controller).GetMethod(nameof(마트피킹Controller.상세));

        Assert.NotNull(method);
        Assert.Equal("{orderId:long}", method.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }
}
