namespace Ssalddel.Tests.Architecture;

public sealed class AdminBusinessPackageCompositionTests
{
    [Fact]
    public void 통합관리자는_세업무패키지진입점에서_기존관리자화면으로연결한다()
    {
        var landing = Read("SsalddelAdmin", "Components", "Pages", "BusinessPackageAdmin.razor");

        Assert.Contains("@page \"/admin/food-delivery\"", landing);
        Assert.Contains("@page \"/admin/freight-delivery\"", landing);
        Assert.Contains("@page \"/admin/order-warehouse\"", landing);
        Assert.Contains("BusinessPackageCatalog.GetRequired", landing);
        Assert.Contains("workflow.AdminPath", landing);
    }

    [Theory]
    [InlineData("FoodOperations.razor", "@page \"/admin/food-delivery/operations\"")]
    [InlineData("FoodOrderOperationsTrace.razor", "@page \"/admin/food-delivery/order-trace\"")]
    [InlineData("FoodDeliveryDispatchAIReview.razor", "@page \"/admin/food-delivery/dispatch-ai-review\"")]
    [InlineData("Requests.razor", "@page \"/admin/freight-delivery/requests\"")]
    [InlineData("DispatchWait.razor", "@page \"/admin/freight-delivery/dispatch-wait\"")]
    [InlineData("DomesticCargoDispatchAIReview.razor", "@page \"/admin/freight-delivery/dispatch-ai-review\"")]
    [InlineData("DriverOperatingView.razor", "@page \"/admin/freight-delivery/drivers\"")]
    [InlineData("Transports.razor", "@page \"/admin/freight-delivery/transports\"")]
    [InlineData("VehicleManagement.razor", "@page \"/admin/freight-delivery/vehicles\"")]
    [InlineData("Dashboard.razor", "@page \"/admin/order-warehouse/dashboard\"")]
    [InlineData("Requests.razor", "@page \"/admin/order-warehouse/outbound-requests\"")]
    [InlineData("Transports.razor", "@page \"/admin/order-warehouse/outbound-transports\"")]
    [InlineData("Documents.razor", "@page \"/admin/order-warehouse/documents\"")]
    public void 기존관리자페이지는_업무패키지별칭을유지한다(string fileName, string routeDirective)
    {
        var page = Read("SsalddelAdmin", "Components", "Pages", fileName);

        Assert.Contains(routeDirective, page);
    }

    [Fact]
    public void 분리실행앱의공통화면은_루트경로와단일카탈로그를사용한다()
    {
        var page = Read("Ssalddel.BusinessPackages.AdminUi", "PackageAdminApp.razor");

        Assert.Contains("@page \"/\"", page);
        Assert.Contains("BusinessPackageCatalog.GetRequired", page);
        Assert.Contains("workflow.AdminPath", page);
        Assert.DoesNotContain("LegacyAdminPath", page);
    }

    private static string Read(params string[] path)
        => File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. path]));

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
