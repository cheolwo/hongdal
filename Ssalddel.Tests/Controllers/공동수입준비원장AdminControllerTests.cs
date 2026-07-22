using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Admin.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 공동수입준비원장AdminControllerTests
{
    [Fact]
    public void Controller는_관리자와1_5기능플래그로보호된다()
    {
        var type = typeof(공동수입준비원장AdminController);
        var version = type.GetCustomAttribute<SsalddelApiVersionAttribute>();
        var feature = Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>());

        Assert.Equal("서버관리자전용", type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(SsalddelProductVersion.V1_5, version?.Version);
        Assert.Equal(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, version?.FeatureKey);
        Assert.Equal(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, Assert.Single(feature.Arguments!));
        Assert.Equal(
            SsalddelWorkflow.GroupPurchaseImport,
            Assert.Single(type.GetCustomAttributes<SsalddelApiWorkflowAttribute>()).Workflow);
        Assert.Equal(
            "api/v1/admin/orderer/group-purchase-demand-os/groups/{autoGroupId}/trade-readiness",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void 계약_Api_Application은_같은1_5기능메타데이터를가진다()
    {
        var metadata = new[]
        {
            typeof(공동수입준비원장저장요청),
            typeof(공동수입준비원장AdminController),
            typeof(공동수입준비원장Service)
        }
            .SelectMany(SsalddelCodeMetadataReader.Read)
            .Where(item => item.FeatureKey == SsalddelCodeFeatureKeys.GroupImportTradeReadiness)
            .ToArray();

        Assert.Equal(3, metadata.Length);
        Assert.Equal(
            [SsalddelCodeLayer.Contract, SsalddelCodeLayer.Api, SsalddelCodeLayer.Application],
            metadata.OrderBy(item => item.FlowOrder).Select(item => item.Layer).ToArray());
        Assert.All(metadata, item => Assert.Contains("계약", item.Boundary + item.Responsibility, StringComparison.OrdinalIgnoreCase));
    }
}
