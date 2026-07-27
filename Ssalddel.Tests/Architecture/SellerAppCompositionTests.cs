namespace Ssalddel.Tests.Architecture;

public sealed class SellerAppCompositionTests
{
    [Fact]
    public void 판매자앱은_화주로컬메모리가아닌_공통서버판매모듈을사용한다()
    {
        var startup = Read("SellerApp", "MauiProgram.cs");
        var project = Read("SellerApp", "SellerApp.csproj");

        Assert.Contains("AddSsalddelUiCommonAppServices<SellerAuthSession>()", startup);
        Assert.Contains("AddSsalddelApiHttpClient", startup);
        Assert.DoesNotContain("ShipperSalesService", startup);
        Assert.DoesNotContain("InMemoryShipperStore", startup);
        Assert.Contains("Ssalddel.Ui.Common.csproj", project);
    }

    [Fact]
    public void 판매자앱은_판매준비부터_영속주문까지_독립동선을제공한다()
    {
        var layout = Read("SellerApp", "Components/Layout/MainLayout.razor");
        var home = Read("SellerApp", "Components/Pages/Home.razor");
        var orders = Read("SellerApp", "Components/Pages/Orders.razor");

        Assert.Contains("판매채널", layout);
        Assert.Contains("판매 페이지 초안", layout);
        Assert.Contains("판매 가능 재고", layout);
        Assert.Contains("판매상품", layout);
        Assert.Contains("채널 출품", layout);
        Assert.Contains("판매 주문", layout);
        Assert.Contains("같은 판매자 원장 흐름", home);
        Assert.Contains("ShipperSalesOrderWorkspaceMode.List", orders);
        Assert.Contains("TimeSpan.FromSeconds(30)", orders);
    }

    [Fact]
    public void 판매자앱은_국내와미국운영시장프로필을_기존계약으로구분한다()
    {
        var profile = Read("SellerApp", "Services/SellerMarketProfileService.cs");
        var home = Read("SellerApp", "Components/Pages/Home.razor");

        Assert.Contains("OperatingMarketCodes.Korea", profile);
        Assert.Contains("OperatingMarketProfileCatalog.Get", profile);
        Assert.Contains("MarketCodeKey", profile);
        Assert.Contains("PreferredCommerceChannelCodes", home);
        Assert.Contains("서버 권한이나 외부 채널 연결을 자동 변경하지 않습니다", Read(
            "SellerApp",
            "Components/Layout/MainLayout.razor"));
    }

    [Fact]
    public void 판매자앱인증은_보안저장과갱신토큰을사용하고_판매역할을확인한다()
    {
        var session = Read("SellerApp", "Services/SellerAuthSession.cs");
        var auth = Read("SellerApp", "Services/SellerAuthService.cs");
        var store = Read("SellerApp", "Services/MauiSecureTokenStore.cs");

        Assert.Contains("IClientSessionGuard", session);
        Assert.Contains("ClientAuthSessionRestoreState.RefreshRequired", session);
        Assert.Contains("\"api/v1/auth/refresh\"", auth);
        Assert.Contains("\"판매자\", \"화주\", \"서버관리자\"", auth);
        Assert.Contains("SecureStorage.Default", store);
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
