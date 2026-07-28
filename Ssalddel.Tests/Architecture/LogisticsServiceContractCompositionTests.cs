using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Controllers.Common;

namespace Ssalddel.Tests.Architecture;

public sealed class LogisticsServiceContractCompositionTests
{
    [Fact]
    public void CostPreview_RequiresLoginButNotAPreassignedShipperRole()
    {
        var controllerType = typeof(물류대행계약Controller);
        var authorize = Assert.Single(
            controllerType.GetCustomAttributes<AuthorizeAttribute>());
        var route = Assert.Single(controllerType.GetCustomAttributes<RouteAttribute>());

        Assert.Null(authorize.Roles);
        Assert.Null(authorize.Policy);
        Assert.Equal("api/v1/logistics-service-contracts", route.Template);
        Assert.NotNull(controllerType.GetMethod("비용미리보기"));
    }

    [Fact]
    public void ShipperApp_ExposesContractPreviewFromWarehouseWorkspace()
    {
        var root = FindRepositoryRoot();
        var page = File.ReadAllText(Path.Combine(
            root,
            "SsalddelApp",
            "Components",
            "Pages",
            "LogisticsServiceContract.razor"));
        var workspace = File.ReadAllText(Path.Combine(
            root,
            "SsalddelApp",
            "Components",
            "Pages",
            "WarehouseWorkspace.razor"));
        var client = File.ReadAllText(Path.Combine(
            root,
            "SsalddelApp",
            "Services",
            "LogisticsServiceContractClient.cs"));

        Assert.Contains("@page \"/shipper/warehouse/logistics-contract\"", page);
        Assert.Contains("누구든 물건을 맡기는 계약에서는 화주가 될 수 있습니다", page);
        Assert.Contains("SsalddelWarehouseBillingPreview", page);
        Assert.Contains("ShipperRoutes.LogisticsServiceContract", workspace);
        Assert.Contains("api/v1/logistics-service-contracts/cost-preview", client);
        Assert.Contains("EnsureAccessTokenAsync", client);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx"))
                || File.Exists(Path.Combine(directory.FullName, "Ssalddel.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("저장소 루트를 찾을 수 없습니다.");
    }
}
