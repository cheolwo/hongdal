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
    public void 세업무패키지는_별도실행프로젝트없이_통합관리자에서제공한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var solution = Read("Ssalddel.v3.5.slnx");
        var adminProject = Read("SsalddelAdmin", "SsalddelAdmin.csproj");

        Assert.False(File.Exists(Path.Combine(repositoryRoot, "Ssalddel.FoodDelivery.Admin", "Ssalddel.FoodDelivery.Admin.csproj")));
        Assert.False(File.Exists(Path.Combine(repositoryRoot, "Ssalddel.FreightDelivery.Admin", "Ssalddel.FreightDelivery.Admin.csproj")));
        Assert.False(File.Exists(Path.Combine(repositoryRoot, "Ssalddel.OrderWarehouse.Admin", "Ssalddel.OrderWarehouse.Admin.csproj")));
        Assert.False(File.Exists(Path.Combine(repositoryRoot, "Ssalddel.BusinessPackages.AdminUi", "Ssalddel.BusinessPackages.AdminUi.csproj")));
        Assert.DoesNotContain("Ssalddel.FoodDelivery.Admin", solution);
        Assert.DoesNotContain("Ssalddel.FreightDelivery.Admin", solution);
        Assert.DoesNotContain("Ssalddel.OrderWarehouse.Admin", solution);
        Assert.DoesNotContain("Ssalddel.BusinessPackages.AdminUi", adminProject);
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
