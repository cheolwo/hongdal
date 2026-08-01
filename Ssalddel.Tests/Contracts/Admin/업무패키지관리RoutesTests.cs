using Ssalddel.Contracts.Admin.BusinessPackages;

namespace Ssalddel.Tests.Contracts.Admin;

public sealed class 업무패키지관리RoutesTests
{
    [Fact]
    public void 세운영패키지는서로다른관리자진입점과기존업무별하위경로를갖는다()
    {
        var roots = new[]
        {
            업무패키지관리Routes.FoodDeliveryAdminRoot,
            업무패키지관리Routes.FreightDeliveryAdminRoot,
            업무패키지관리Routes.OrderWarehouseAdminRoot
        };

        Assert.Equal(3, roots.Distinct(StringComparer.Ordinal).Count());
        Assert.StartsWith(업무패키지관리Routes.FoodDeliveryAdminRoot, 업무패키지관리Routes.FoodDeliveryOrderTrace);
        Assert.StartsWith(업무패키지관리Routes.FreightDeliveryAdminRoot, 업무패키지관리Routes.FreightDeliveryTransports);
        Assert.StartsWith(업무패키지관리Routes.OrderWarehouseAdminRoot, 업무패키지관리Routes.OrderWarehouseDocuments);
    }

    [Fact]
    public void 업무패키지카탈로그는_세패키지와기존관리자화면별칭을단일하게관리한다()
    {
        Assert.Equal(
            [BusinessPackageCatalog.FoodDelivery, BusinessPackageCatalog.FreightDelivery, BusinessPackageCatalog.OrderWarehouse],
            BusinessPackageCatalog.All.Keys);

        var expectedRoots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [BusinessPackageCatalog.FoodDelivery] = 업무패키지관리Routes.FoodDeliveryAdminRoot,
            [BusinessPackageCatalog.FreightDelivery] = 업무패키지관리Routes.FreightDeliveryAdminRoot,
            [BusinessPackageCatalog.OrderWarehouse] = 업무패키지관리Routes.OrderWarehouseAdminRoot
        };

        foreach (var definition in BusinessPackageCatalog.All.Values)
        {
            Assert.NotEmpty(definition.Workflows);
            Assert.All(
                definition.Workflows,
                workflow => Assert.StartsWith(expectedRoots[definition.Code], workflow.AdminPath));
        }

        var allPaths = BusinessPackageCatalog.All.Values
            .SelectMany(definition => definition.Workflows)
            .Select(workflow => workflow.AdminPath)
            .ToArray();

        Assert.Equal(allPaths.Length, allPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
