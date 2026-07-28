namespace Ssalddel.Tests.Architecture;

public sealed class AdminMobileOperationsCompositionTests
{
    [Fact]
    public void 모바일관리앱의_기본화면은_실제운영요약과_핵심관리동선을_제공한다()
    {
        var home = Read("SsalddelAdminApp", "Components/Pages/Home.razor");
        var formerHome = Read(
            "SsalddelAdminApp",
            "Components/Pages/CommunityInformationReview.razor");
        var dashboardService = Read(
            "SsalddelAdminApp",
            "Services/AdminDashboardService.cs");

        Assert.Contains("@page \"/\"", home);
        Assert.Contains("@page \"/overview\"", home);
        Assert.Contains("@inject AdminDashboardService DashboardService", home);
        Assert.Contains("RefreshInterval = TimeSpan.FromSeconds(30)", home);
        Assert.Contains("관리자확인필요수", home);
        Assert.Contains("운송예외수", home);
        Assert.Contains("배차대기수", home);
        Assert.Contains("Href=\"/operations\"", home);
        Assert.Contains("Href=\"/community-management\"", home);
        Assert.Contains("Href=\"/trade-readiness\"", home);
        Assert.DoesNotContain("@page \"/\"", formerHome);
        Assert.Contains("\"api/v1/admin/dashboard\"", dashboardService);
    }

    [Fact]
    public void 모바일운영화면은_실제운송원장과_운행기사목록을_함께조회한다()
    {
        var page = Read("SsalddelAdminApp", "Components/Pages/Operations.razor");
        var service = Read("SsalddelAdminApp", "Services/AdminOperationsService.cs");
        var layout = Read("SsalddelAdminApp", "Components/Layout/MainLayout.razor");
        var startup = Read("SsalddelAdminApp", "MauiProgram.cs");

        Assert.Contains("@page \"/operations\"", page);
        Assert.Contains("@inject AdminAuthService AuthService", page);
        Assert.Contains("@inject AdminOperationsService OperationsService", page);
        Assert.Contains("RefreshInterval = TimeSpan.FromSeconds(30)", page);
        Assert.Contains("@inject TransportRequestLedgerRealtimeClient RealtimeClient", page);
        Assert.Contains("LedgerObserver.RefreshRequested += OnLedgerRefreshRequested", page);
        Assert.Contains("RealtimeClient.StartAsync", page);
        Assert.Contains("RetryAuthenticationAsync", page);
        Assert.Contains("transport.관리자확인필요 || transport.예외신고됨", page);
        Assert.Contains("snapshot.OperatingDrivers", page);
        Assert.Contains("\"api/v1/admin/transports\"", service);
        Assert.Contains("\"api/v1/admin/drivers/operating\"", service);
        Assert.Contains("Task.WhenAll", service);
        Assert.Contains("Href=\"/operations\"", layout);
        Assert.Contains("AddScoped<AdminOperationsService>()", startup);
        Assert.Contains("AddScoped<AdminDashboardService>()", startup);
        Assert.Contains("AddSingleton<ITransportRequestLedgerObserver, TransportRequestLedgerObserver>()", startup);
        Assert.Contains("new TransportRequestLedgerRealtimeClient", startup);
    }

    [Fact]
    public void 모바일관리인증은_갱신토큰을보존하고_401에서_한번만재시도한다()
    {
        var session = Read("SsalddelAdminApp", "Services/AdminAuthSession.cs");
        var auth = Read("SsalddelAdminApp", "Services/AdminAuthService.cs");
        var apiClient = Read(
            "SsalddelAdminApp",
            "Services/AdminAuthenticatedApiClient.cs");
        var startup = Read("SsalddelAdminApp", "MauiProgram.cs");

        Assert.Contains("RefreshTokenExpiresAtUtc", session);
        Assert.Contains("ClientAuthSessionRestoreState.RefreshRequired", session);
        Assert.Contains("IClientSessionGuard", session);
        Assert.Contains("\"api/v1/auth/refresh\"", auth);
        Assert.Contains("EnsureAccessTokenAsync", auth);
        Assert.Contains("SemaphoreSlim refreshGate", auth);
        Assert.Contains("response.StatusCode == HttpStatusCode.Unauthorized", apiClient);
        Assert.Contains("forceRefresh: true", apiClient);
        Assert.Contains("SendOnceAsync(method, path", apiClient);
        Assert.Contains("AddSingleton<IClientSessionGuard, ClientSessionGuard>()", startup);
        Assert.Contains("AddScoped<AdminAuthenticatedApiClient>()", startup);
    }

    private static string Read(string project, string relativePath)
        => File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

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
