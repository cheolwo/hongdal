using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Common;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class HrRoleApplicationsControllerTests
{
    [Fact]
    public void Controller는_로그인과Hr기능으로보호된지원원장경계다()
    {
        var type = typeof(HrRoleApplicationsController);

        Assert.Equal("api/v1/hr/role-applications", type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.HrParticipationWorkflow, Assert.Single(feature.Arguments!));
        var version = Assert.Single(type.GetCustomAttributes<SsalddelApiVersionAttribute>());
        Assert.Equal(SsalddelProductVersion.V2_5, version.Version);
    }

    [Fact]
    public void Controller는_조회제출철회를분리하고배정승인경계를제공하지않는다()
    {
        var type = typeof(HrRoleApplicationsController);

        Assert.NotNull(type.GetMethod(nameof(HrRoleApplicationsController.MyApplications)));
        Assert.Equal(string.Empty, type.GetMethod(nameof(HrRoleApplicationsController.Submit))?
            .GetCustomAttribute<HttpPostAttribute>()?.Template ?? string.Empty);
        Assert.Equal("{applicationId:guid}/withdraw", type.GetMethod(nameof(HrRoleApplicationsController.Withdraw))?
            .GetCustomAttribute<HttpPostAttribute>()?.Template);
        Assert.Null(type.GetMethod("Assign"));
        Assert.Null(type.GetMethod("Approve"));
    }
}
