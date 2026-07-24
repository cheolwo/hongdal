using System.Reflection;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 공동구매체험ControllerTests
{
    [Fact]
    public void 체험API는_익명접근과독립기능플래그를명시한다()
    {
        var type = typeof(공동구매체험Controller);
        var version = type.GetCustomAttribute<SsalddelApiVersionAttribute>();
        var feature = type.GetCustomAttribute<RequireVersionFeatureAttribute>();
        var route = type.GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(type.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(version);
        Assert.Equal(SsalddelProductVersion.V1_0, version.Version);
        Assert.Equal(VersionFeatureFlagKeys.GroupPurchasePracticeWorkflow, version.FeatureKey);
        Assert.Equal(VersionFeatureFlagKeys.GroupPurchasePracticeWorkflow, version.WorkflowKey);
        Assert.NotNull(feature);
        Assert.Equal(
            VersionFeatureFlagKeys.GroupPurchasePracticeWorkflow,
            Assert.IsType<string>(Assert.Single(feature.Arguments!)));
        Assert.Equal("api/v1/orderer/group-purchase-practice", route?.Template);
    }

    [Fact]
    public void 잘못된연습요청은_400문제로변환한다()
    {
        var controller = new 공동구매체험Controller(new ThrowingPracticeService());

        var result = controller.시뮬레이션(new 공동구매체험요청());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(400, problem.Status);
        Assert.Contains("연습 조건", problem.Title, StringComparison.Ordinal);
    }

    private sealed class ThrowingPracticeService : I공동구매체험Service
    {
        public IReadOnlyList<공동구매체험시나리오응답> 시나리오목록() => [];

        public 공동구매체험응답 시뮬레이션(공동구매체험요청 request)
            => throw new InvalidOperationException("잘못된 연습 요청");
    }
}
