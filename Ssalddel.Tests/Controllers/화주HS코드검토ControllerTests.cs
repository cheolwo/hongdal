using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Shipper.Customs;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 화주HS코드검토ControllerTests
{
    [Fact]
    public void Controller는_화주판매자정책과통관기능플래그로보호된다()
    {
        var type = typeof(화주HS코드검토Controller);

        Assert.Equal(
            "화주또는판매자전용",
            type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(
            "api/v1/shipper/customs/hs-reviews",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());
        Assert.Equal(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, Assert.Single(feature.Arguments!));
        Assert.Equal(
            SsalddelWorkflow.CustomsAndTradeData,
            Assert.Single(type.GetCustomAttributes<SsalddelApiWorkflowAttribute>()).Workflow);
    }

    [Fact]
    public void 상세는_목록과분리된ReviewIdGet경로를사용한다()
    {
        var method = typeof(화주HS코드검토Controller).GetMethod(nameof(화주HS코드검토Controller.상세));

        Assert.NotNull(method);
        Assert.Equal("{reviewId:long}", method.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }
}
