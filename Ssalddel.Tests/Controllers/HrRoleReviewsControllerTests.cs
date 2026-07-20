using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Admin.HumanResources;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class HrRoleReviewsControllerTests
{
    [Fact]
    public void Controller는_서버관리자와Hr기능으로보호된읽기경계다()
    {
        var type = typeof(HrRoleReviewsController);

        Assert.Equal("api/v1/admin/hr-role-reviews", type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal("서버관리자전용", type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.HrParticipationWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V2_5, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.HrParticipationWorkflow, version.FeatureKey);
    }

    [Fact]
    public void 상세는_정확한ReviewIdGet경로만제공한다()
    {
        var method = typeof(HrRoleReviewsController).GetMethod(nameof(HrRoleReviewsController.Detail));

        Assert.NotNull(method);
        Assert.Equal("{reviewId:guid}", method.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.Null(typeof(HrRoleReviewsController).GetMethod("Assign"));
        Assert.Null(typeof(HrRoleReviewsController).GetMethod("Revoke"));
    }
}
