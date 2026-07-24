using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class WebExperienceCatalogTests
{
    [Fact]
    public void Roles_HaveUniqueKeysAndRequiredExperienceGroups()
    {
        var roles = WebExperienceCatalog.Roles;

        Assert.Equal(roles.Count, roles.Select(role => role.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(roles, role => role.Key == "global-supplier");
        Assert.Contains(roles, role => role.Key == "shipper-seller");
        Assert.Contains(roles, role => role.Key == "driver");
        Assert.Contains(roles, role => role.Key == "warehouse");
        Assert.Contains(roles, role => role.Key == "community-orderer");
    }

    [Fact]
    public void Roles_ExposeUsableStartAndScreenRoutes()
    {
        foreach (var role in WebExperienceCatalog.Roles)
        {
            Assert.StartsWith("/", role.StartHref);
            Assert.StartsWith("/images/role-previews/", role.ImageUrl);
            Assert.True(role.Screens.Count >= 4);
            Assert.All(role.Screens, screen => Assert.StartsWith("/", screen.Href));
        }
    }

    [Fact]
    public void Find_ReturnsRequestedRoleOrDefault()
    {
        Assert.Equal("driver", WebExperienceCatalog.Find("DRIVER").Key);
        Assert.Same(WebExperienceCatalog.DefaultRole, WebExperienceCatalog.Find("unknown"));
    }

    [Fact]
    public void DriverRole_대표_화면은_읽기_허브와_전용_업무_진입점만_노출한다()
    {
        var driver = WebExperienceCatalog.Find("driver");

        Assert.Contains(driver.Screens, screen => screen.Href == DriverRoutes.CurrentTransport);
        Assert.Contains(driver.Screens, screen => screen.Href == DriverRoutes.TransportHistory);
        Assert.DoesNotContain(driver.Screens, screen => screen.Href == DriverRoutes.ProofStageSelector);
        Assert.DoesNotContain(driver.Screens, screen => screen.Href.StartsWith(DriverRoutes.DispatchDecisions, StringComparison.Ordinal));
    }
}
