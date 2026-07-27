namespace Ssalddel.Tests.Architecture;

public sealed class FDriverRealtimeAuthRecipientCompositionTests
{
    [Fact]
    public void 음식배달기사앱은_새추천을_지도배너로알리고_바로가기초점을_실제영역으로이동한다()
    {
        var model = Read("FDriverApp", "PageModels/MainPageModel.cs");
        var page = Read("FDriverApp", "Pages/MainPage.xaml");
        var codeBehind = Read("FDriverApp", "Pages/MainPage.xaml.cs");

        Assert.Contains("HasNewRecommendations", model);
        Assert.Contains("NewRecommendationNotice", model);
        Assert.Contains("OpenNewRecommendations", model);
        Assert.Contains("_knownRecommendedTicketIds", model);
        Assert.Contains("WorkspaceRefreshInterval = TimeSpan.FromSeconds(30)", model);
        Assert.Contains("await ReloadAsync(updateLocation: IsOnDuty)", model);
        Assert.Contains("다음 자동 갱신 30초 이내", model);
        Assert.Contains("OpenNewRecommendationsCommand", page);
        Assert.Contains("눌러서 음식점·전달지와 예상 경로를 확인하세요.", page);
        Assert.Contains("x:Name=\"WorkspaceScroll\"", page);
        Assert.Contains("x:Name=\"ActiveDeliverySection\"", page);
        Assert.Contains("x:Name=\"RecommendationSection\"", page);
        Assert.Contains("ScrollToEntryFocusAsync", codeBehind);
        Assert.Contains("\"dispatch\" or \"bundle\" => RecommendationSection", codeBehind);
        Assert.Contains("\"delivery\" => ActiveDeliverySection", codeBehind);
    }

    [Fact]
    public void 음식배달기사앱은_실시간배차를_받되_30초조회를_보조경로로유지한다()
    {
        var contract = Read("Ssalddel.Contracts", "Common/Drivers/DriverDispatchRealtimeContract.cs");
        var server = Read("Ssalddel", "Services/Dispatch/Recommendation/DispatchRecommendationService.cs");
        var realtime = Read("FDriverApp", "Services/FDriverDispatchRealtimeService.cs");
        var project = Read("FDriverApp", "FDriverApp.csproj");
        var model = Read("FDriverApp", "PageModels/MainPageModel.cs");
        var page = Read("FDriverApp", "Pages/MainPage.xaml");

        Assert.Contains("HubPath = \"/hubs/dispatch-recommendations\"", contract);
        Assert.Contains("RecommendationsEvent = \"ReceiveDispatchRecommendations\"", contract);
        Assert.Contains("DriverDispatchRealtimeContract.RecommendationsEvent", server);
        Assert.Contains("new HubConnectionBuilder()", realtime);
        Assert.Contains("WithAutomaticReconnect", realtime);
        Assert.Contains("options.AccessTokenProvider", realtime);
        Assert.Contains("RecommendationsReceived", realtime);
        Assert.Contains("Microsoft.AspNetCore.SignalR.Client", project);
        Assert.Contains("WorkspaceRefreshInterval = TimeSpan.FromSeconds(30)", model);
        Assert.Contains("OnRealtimeRecommendationsReceivedAsync", model);
        Assert.Contains("실시간 배차 연결 대기 · 30초 자동 조회 보조", model);
        Assert.Contains("RealtimeConnectionText", page);
    }

    [Fact]
    public void 음식배달기사앱은_저장된갱신토큰과_401응답으로_액세스토큰을자동갱신한다()
    {
        var session = Read("FDriverApp", "Services/FDriverAuthSession.cs");
        var authApi = Read("FDriverApp", "Services/FDriverAuthApiService.cs");
        var foodApi = Read("FDriverApp", "Services/FoodDeliveryDriverApiService.cs");

        Assert.Contains("ClientAuthSessionRestoreState.RefreshRequired", session);
        Assert.Contains("RefreshTokenExpiresAtUtc", session);
        Assert.Contains("api/v1/auth/refresh", authApi);
        Assert.Contains("EnsureAccessTokenAsync", authApi);
        Assert.Contains("SemaphoreSlim _refreshGate", authApi);
        Assert.Contains("response.StatusCode == HttpStatusCode.Unauthorized", foodApi);
        Assert.Contains("forceRefresh: true", foodApi);
        Assert.Contains("SendOnceAsync(method, path, body", foodApi);
    }

    [Fact]
    public void 실제수령자정보는_배차확정뒤_진행업무에만연결된다()
    {
        var workContract = Read("Ssalddel.Contracts", "Common/Drivers/DriverWorkDtos.cs");
        var workspaceContract = Read("Ssalddel.Contracts", "Driver/Food/FoodDeliveryDriverWorkspaceDtos.cs");
        var workService = Read("Ssalddel", "Services/Dispatch/Recommendation/FoodDeliveryDriverWorkService.cs");
        var workspaceUseCase = Read("Ssalddel", "Application/Driver/Food/FoodDeliveryDriverWorkspaceUseCase.cs");
        var page = Read("FDriverApp", "Pages/MainPage.xaml");

        Assert.Contains("DriverWorkRecipientDto", workContract);
        Assert.Contains("FoodDeliveryDriverRecipientDto Recipient", workspaceContract);
        Assert.Contains("isRecommended", workService);
        Assert.Contains("? null", workService);
        Assert.Contains("order.수령인명", workService);
        Assert.Contains("order.수령인연락처", workService);
        Assert.Contains("order.수령요청사항", workService);
        Assert.Contains("Recipient = ToRecipient(offer.Recipient)", workspaceUseCase);
        Assert.Contains("ActiveDelivery.HasRecipient", page);
        Assert.Contains("배차가 확정된 현재 업무에서만 수령자 정보를 표시합니다.", page);
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
