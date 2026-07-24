namespace Ssalddel.Tests.Architecture;

public sealed class DriverTransportHistoryPageResponsibilityTests
{
    [Fact]
    public void 운송_이력_page는_과거_기록만_조회하고_상하차_Command_route를_노출하지_않는다()
    {
        var root = FindRepositoryRoot();
        var pagePath = Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "DriverTransportHistoryPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.Contains("Where(IsHistorical)", source);
        Assert.Contains("DriverRoutes.CurrentTransport", source);
        Assert.DoesNotContain("DriverRoutes.PickupFor", source);
        Assert.DoesNotContain("DriverRoutes.DropoffFor", source);
        Assert.DoesNotContain("/pickup", source);
        Assert.DoesNotContain("/dropoff", source);
        Assert.DoesNotContain("상차지도착Async", source);
        Assert.DoesNotContain("하차지도착Async", source);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
