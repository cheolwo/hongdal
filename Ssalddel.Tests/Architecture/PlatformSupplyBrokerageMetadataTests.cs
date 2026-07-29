using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Application.ContractManagement;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Controllers.Admin.ContractManagement;
using Ssalddel.Controllers.Common;
using Ssalddel.Filters;
using 살뜰.Data.Configurations.ContractManagement;
using 살뜰.도메인.공급중개;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Architecture;

public sealed class PlatformSupplyBrokerageMetadataTests
{
    [Fact]
    public void 공급중개Metadata는_계약부터Api까지세로경계를표시한다()
    {
        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
            typeof(플랫폼공급계약등록요청).Assembly,
            typeof(플랫폼공급조건계약).Assembly,
            typeof(플랫폼공급조건계약Configuration).Assembly,
            typeof(플랫폼공급계약관리UseCase).Assembly);

        Assert.Contains(metadata, item => item.ComponentType == typeof(플랫폼공급계약등록요청));
        Assert.Contains(metadata, item => item.ComponentType == typeof(플랫폼공급조건계약));
        Assert.Contains(metadata, item => item.ComponentType == typeof(조직개별공급발주));
        Assert.Contains(metadata, item => item.ComponentType == typeof(플랫폼공급조건계약Configuration));
        Assert.Contains(metadata, item => item.ComponentType == typeof(플랫폼공급계약관리UseCase));
        Assert.Contains(metadata, item => item.ComponentType == typeof(조직개별공급발주UseCase));
        Assert.Contains(metadata, item => item.ComponentType == typeof(플랫폼공급계약AdminController));
        Assert.Contains(metadata, item => item.ComponentType == typeof(조직개별공급발주Controller));
        Assert.All(metadata, item => Assert.False(string.IsNullOrWhiteSpace(item.Boundary)));
        Assert.Equal(
            metadata.OrderBy(item => item.FlowOrder).ThenBy(
                item => item.ComponentType.FullName,
                StringComparer.Ordinal),
            metadata);
    }

    [Fact]
    public void 공급중개Api는_후속기능Gate와조직별권한경계를유지한다()
    {
        var organizationController = typeof(조직개별공급발주Controller);
        var adminController = typeof(플랫폼공급계약AdminController);

        Assert.Equal(
            "api/v1/supply-brokerage",
            organizationController.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(organizationController.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(
            VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
            Assert.Single(
                    organizationController.GetCustomAttributes<RequireVersionFeatureAttribute>())
                .Arguments?[0]);

        Assert.Equal(
            "api/v1/admin/supply-brokerage",
            adminController.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal(
            "서버관리자전용",
            adminController.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(
            VersionFeatureFlagKeys.WarehouseFulfillmentWorkflow,
            Assert.Single(adminController.GetCustomAttributes<RequireVersionFeatureAttribute>())
                .Arguments?[0]);
    }
}
