namespace Ssalddel.Tests.Architecture;

public sealed class ShipperDriverFallbackPollingCompositionTests
{
    [Fact]
    public void 화주목록과기사추천진행화면은_공통30초보완조회정책을사용한다()
    {
        var shipperWorkspace = Read("SsalddelApp/Components/Pages/TransportWorkspace.razor");
        var driverRecommendations = Read("DriverApp/Components/Pages/Driver/02_Recommendation/추천목록Page.razor");
        var driverCurrentTransport = Read("DriverApp/Components/Pages/Driver/03_Progress/진행중운송Page.razor");

        Assert.Contains("TransportRequestLedgerRefreshPolicy.FallbackPollingInterval", shipperWorkspace);
        Assert.Contains("TransportRequestLedgerRefreshPolicy.FallbackPollingInterval", driverRecommendations);
        Assert.Contains("TransportRequestLedgerRefreshPolicy.FallbackPollingInterval", driverCurrentTransport);
        Assert.Contains("Samples.RefreshAsync(force: force)", driverRecommendations);
        Assert.Contains("데이터조회Async(force: true)", driverCurrentTransport);
    }

    private static string Read(string relativePath)
        => File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

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
