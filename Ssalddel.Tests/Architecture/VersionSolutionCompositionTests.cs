namespace Ssalddel.Tests.Architecture;

public sealed class VersionSolutionCompositionTests
{
    [Fact]
    public void V1_0_솔루션은_주문자공동구매를포함하고_운송앱을제외한다()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Ssalddel.v1.0.slnx"));

        Assert.Contains("OrdererApp/OrdererApp.csproj", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DriverApp/DriverApp.csproj", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DriverApp.NaverMaps.Android/DriverApp.NaverMaps.Android.csproj", source, StringComparison.Ordinal);
    }

    [Fact]
    public void V1_5_솔루션은_공동구매와무역준비운영을포함하고_이행앱을제외한다()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Ssalddel.v1.5.slnx"));

        Assert.Contains("OrdererApp/OrdererApp.csproj", source, StringComparison.Ordinal);
        Assert.Contains("SsalddelAdmin/SsalddelAdmin.csproj", source, StringComparison.Ordinal);
        Assert.Contains("Ssalddel.Tests/Ssalddel.Tests.csproj", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DriverApp/DriverApp.csproj", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DriverApp.NaverMaps.Android/DriverApp.NaverMaps.Android.csproj", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WarehouseManagerApp/WarehouseManagerApp.csproj", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Restaurant", source, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root could not be found.");
    }
}
