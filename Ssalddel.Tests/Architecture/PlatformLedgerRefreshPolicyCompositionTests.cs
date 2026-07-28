namespace Ssalddel.Tests.Architecture;

public sealed class PlatformLedgerRefreshPolicyCompositionTests
{
    [Fact]
    public void 화주와관리자원장상세는_공통30초보완조회정책을사용한다()
    {
        var policy = Read(
            "Ssalddel.Client.Infrastructure",
            "Transport/TransportRequestLedgerObserver.cs");
        var shipper = Read(
            "SsalddelApp",
            "ViewModels/Shipper/ShipperRequestDetailPageViewModel.cs");
        var adminRequest = Read(
            "SsalddelAdmin",
            "Components/Pages/RequestDetail.razor");
        var adminTransport = Read(
            "SsalddelAdmin",
            "Components/Pages/TransportWorkflowDetail.razor");

        Assert.Contains(
            "FallbackPollingInterval = TimeSpan.FromSeconds(30)",
            policy);
        Assert.Contains(
            "TransportRequestLedgerRefreshPolicy.FallbackPollingInterval",
            shipper);
        Assert.Contains(
            "TransportRequestLedgerRefreshPolicy.FallbackPollingInterval",
            adminRequest);
        Assert.Contains(
            "TransportRequestLedgerRefreshPolicy.FallbackPollingInterval",
            adminTransport);
        Assert.DoesNotContain("TimeSpan.FromSeconds(15)", shipper);
        Assert.DoesNotContain("TimeSpan.FromSeconds(15)", adminRequest);
        Assert.DoesNotContain("TimeSpan.FromSeconds(15)", adminTransport);
    }

    [Fact]
    public void 운송원장실시간경로는_인증Claim그룹과_서버재조회신호만사용한다()
    {
        var contract = Read(
            "Ssalddel.Contracts",
            "Common/Transport/TransportRequestLedgerRealtimeDtos.cs");
        var hub = Read(
            "Ssalddel",
            "Hubs/TransportRequestLedgerHub.cs");
        var publisher = Read(
            "Ssalddel",
            "Services/Transport/TransportRequestLedgerRealtimeService.cs");
        var client = Read(
            "Ssalddel.Client.Infrastructure",
            "Transport/TransportRequestLedgerRealtimeClient.cs");
        var program = Read("Ssalddel", "Program.cs");

        Assert.Contains("HubPath = \"/hubs/transport-ledger\"", contract);
        Assert.Contains("[Authorize]", hub);
        Assert.Contains("FindFirstValue(ClaimTypes.NameIdentifier)", hub);
        Assert.DoesNotContain("JoinGroup", hub);
        Assert.Contains("TransportRequestLedgerHub.UserGroup", publisher);
        Assert.Contains("request.주문자UserId", publisher);
        Assert.Contains("transport?.확정기사Id", publisher);
        Assert.Contains("_observer.RequestRefresh", client);
        Assert.Contains("path.StartsWithSegments(TransportRequestLedgerRealtime.HubPath)", program);
    }

    [Fact]
    public void 배차수락Push도_인증주문자UserId를우선하고_화주Id는호환값으로유지한다()
    {
        var source = Read(
            "Ssalddel",
            "Application/Driver/DispatchAction/Handlers/배차수락사후처리EventHandler.Notification.cs");

        Assert.Contains("dispatchRequest.주문자UserId", source);
        Assert.Contains("TargetUserId = targetUserId", source);
        Assert.Contains("ShipperUserId = notification.화주Id", source);
    }

    private static string Read(string project, string relativePath)
        => File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            project,
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
