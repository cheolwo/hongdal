using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Common;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 인사역할지원ControllerTests
{
    [Fact]
    public void Controller는_로그인과Hr기능으로보호된지원원장경계다()
    {
        var type = typeof(인사역할지원Controller);

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
        var type = typeof(인사역할지원Controller);

        Assert.NotNull(type.GetMethod(nameof(인사역할지원Controller.내지원목록조회)));
        Assert.Equal(string.Empty, type.GetMethod(nameof(인사역할지원Controller.지원))?
            .GetCustomAttribute<HttpPostAttribute>()?.Template ?? string.Empty);
        Assert.Equal("{applicationId:guid}/withdraw", type.GetMethod(nameof(인사역할지원Controller.지원철회))?
            .GetCustomAttribute<HttpPostAttribute>()?.Template);
        Assert.Null(type.GetMethod("Assign"));
        Assert.Null(type.GetMethod("Approve"));
    }
}
