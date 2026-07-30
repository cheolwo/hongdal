using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class WebExperienceCatalogTests
{
    [Fact]
    public void Roles_HaveUniqueKeysAndRequiredExperienceGroups()
    {
        var roles = WebExperienceCatalog.Roles;

        Assert.Equal(roles.Count, roles.Select(role => role.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(
        [
            "community",
            "orderer",
            "shipper",
            "driver",
            "warehouse"
        ],
            roles.Select(role => role.Key));
        Assert.Equal(
        [
            "01 · COMMUNITY",
            "02 · ORDERER",
            "03 · SHIPPER",
            "04 · DRIVER",
            "05 · WAREHOUSE"
        ],
            roles.Select(role => role.Eyebrow));
    }

    [Fact]
    public void Roles_ExposeUsableStartAndScreenRoutes()
    {
        foreach (var role in WebExperienceCatalog.Roles)
        {
            Assert.StartsWith("/", role.StartHref);
            Assert.StartsWith("/roles/", role.AppHref);
            Assert.EndsWith("/", role.AppHref);
            Assert.StartsWith("/images/role-previews/", role.ImageUrl);
            Assert.True(role.Screens.Count >= 4);
            Assert.All(role.Screens, screen => Assert.StartsWith("/", screen.Href));
            Assert.All(role.Screens, screen =>
                Assert.StartsWith(role.AppHref, role.HrefFor(screen.Href)));
        }
    }

    [Fact]
    public void Roles_UseFiveIndependentWebAppEntryPoints()
    {
        Assert.Equal(
        [
            "/roles/01/",
            "/roles/02/",
            "/roles/03/",
            "/roles/04/",
            "/roles/05/"
        ],
            WebExperienceCatalog.Roles.Select(role => role.AppHref));
    }

    [Fact]
    public void Find_ReturnsRequestedRoleOrDefault()
    {
        Assert.Equal("orderer", WebExperienceCatalog.Find("ORDERER").Key);
        Assert.Same(WebExperienceCatalog.DefaultRole, WebExperienceCatalog.Find("unknown"));
    }

    [Fact]
    public void 역할_포털은_미국_현지_사용자_전용_화면을_노출하지_않는다()
    {
        var routes = WebExperienceCatalog.Roles
            .SelectMany(role => role.Screens.Select(screen => screen.Href).Append(role.StartHref));

        Assert.DoesNotContain(routes, route =>
            route.StartsWith("/us/", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(WebExperienceCatalog.Roles, role =>
            role.Key.StartsWith("us", StringComparison.OrdinalIgnoreCase)
            || role.Eyebrow.Contains("· US ", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ShipperRole_대표_화면은_모바일홈과_조건검토_흐름을_노출한다()
    {
        var shipper = WebExperienceCatalog.Find("shipper");

        Assert.Contains(shipper.Screens, screen => screen.Href == ShipperRoutes.Home);
        Assert.Contains(shipper.Screens, screen => screen.Href == ShipperRoutes.Request);
        Assert.Contains(shipper.Screens, screen => screen.Href == ShipperRoutes.RequestReview);
    }

    [Fact]
    public void DriverAndWarehouseRoles_ExposeTheirOwnOperationalHomes()
    {
        var driver = WebExperienceCatalog.Find("driver");
        var warehouse = WebExperienceCatalog.Find("warehouse");

        Assert.Equal(DriverRoutes.Home, driver.StartHref);
        Assert.Contains(driver.Screens, screen => screen.Href == DriverRoutes.CurrentTransport);
        Assert.Equal(WarehouseManagerRoutes.Home, warehouse.StartHref);
        Assert.Contains(warehouse.Screens, screen => screen.Href == WarehouseManagerRoutes.InboundInspection);
    }
}
