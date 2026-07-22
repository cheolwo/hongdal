namespace Ssalddel.Tests.Architecture;

public sealed class InboundRequestRouteCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperInboundRequestsPage.razor", "/shipper/inbound/requests", "<InboundRequestListScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperInboundRequestCreatePage.razor", "/shipper/inbound/requests/new", "<InboundRequestCreateScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperInboundRequestDetailPage.razor", "/shipper/inbound/requests/{InboundId:long}", "<InboundRequestDetailScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperInboundRequestCompletePage.razor", "/shipper/inbound/requests/{InboundId:long}/complete", "<InboundRequestCompleteScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/ShipperWarehouseRegistrationPage.razor", "/shipper/warehouses/new", "<WarehouseRegistrationScreen")]
    [InlineData("SsalddelApp", "Components/Pages/InboundRequests.razor", "/shipper/inbound/requests", "<InboundRequestListScreen")]
    [InlineData("SsalddelApp", "Components/Pages/InboundRequestCreatePage.razor", "/shipper/inbound/requests/new", "<InboundRequestCreateScreen")]
    [InlineData("SsalddelApp", "Components/Pages/InboundRequestDetailPage.razor", "/shipper/inbound/requests/{InboundId:long}", "<InboundRequestDetailScreen")]
    [InlineData("SsalddelApp", "Components/Pages/InboundRequestCompletePage.razor", "/shipper/inbound/requests/{InboundId:long}/complete", "<InboundRequestCompleteScreen")]
    [InlineData("SsalddelApp", "Components/Pages/WarehouseRegistrationPage.razor", "/shipper/warehouses/new", "<WarehouseRegistrationScreen")]
    public void Web과모바일Route는_같은책임별공용Screen을조립한다(
        string project,
        string relativePath,
        string route,
        string screenMarkup)
    {
        var source = File.ReadAllText(ProjectFile(project, relativePath));

        Assert.Contains($"@page \"{route}\"", source);
        Assert.Contains(screenMarkup, source);
        Assert.Contains("InboundRequestPageViewModel", source);
        Assert.DoesNotContain("IWarehouseWorkspaceService", source);
        Assert.DoesNotContain("SsalddelInboundRequestManager", source);
        Assert.DoesNotContain(File.ReadLines(ProjectFile(project, relativePath)), line => line.Trim().Equals("try", StringComparison.Ordinal));
    }

    [Fact]
    public void 입고Command는_신청과완료전용Route에서만호출한다()
    {
        foreach (var project in new[] { "Ssalddel.WebApp", "SsalddelApp" })
        {
            var basePath = project == "Ssalddel.WebApp" ? "Pages" : "Components/Pages";
            var list = File.ReadAllText(ProjectFile(project, $"{basePath}/InboundRequests.razor".Replace("InboundRequests", project == "Ssalddel.WebApp" ? "ShipperInboundRequestsPage" : "InboundRequests")));
            var detailName = project == "Ssalddel.WebApp" ? "ShipperInboundRequestDetailPage.razor" : "InboundRequestDetailPage.razor";
            var createName = project == "Ssalddel.WebApp" ? "ShipperInboundRequestCreatePage.razor" : "InboundRequestCreatePage.razor";
            var completeName = project == "Ssalddel.WebApp" ? "ShipperInboundRequestCompletePage.razor" : "InboundRequestCompletePage.razor";
            var detail = File.ReadAllText(ProjectFile(project, $"{basePath}/{detailName}"));
            var create = File.ReadAllText(ProjectFile(project, $"{basePath}/{createName}"));
            var complete = File.ReadAllText(ProjectFile(project, $"{basePath}/{completeName}"));

            Assert.DoesNotContain("CreateInboundAsync", list);
            Assert.DoesNotContain("CompleteInboundAsync", list);
            Assert.DoesNotContain("CreateInboundAsync", detail);
            Assert.DoesNotContain("CompleteInboundAsync", detail);
            Assert.Contains("CreateInboundAsync", create);
            Assert.Contains("CompleteInboundAsync", complete);
        }
    }

    [Fact]
    public void 다이어그램창고Panel은_Command대신신청서Route를연다()
    {
        var panel = File.ReadAllText(ProjectFile(
            "Ssalddel.Ui.Common",
            "Areas/App/Components/Community/PlatformCommunityWarehouseProxyPanel.razor"));
        var viewModel = File.ReadAllText(ProjectFile(
            "Ssalddel.Ui.Common",
            "Areas/App/ViewModels/PlatformCommunityWarehouseProxyViewModel.cs"));

        Assert.DoesNotContain("SubmitAsync", panel);
        Assert.DoesNotContain("CreateInboundAsync", viewModel);
        Assert.Contains("InboundRequestScreenKind.Create", viewModel);
    }

    [Fact]
    public void 모바일Adapter는_입고Id단건Endpoint를사용한다()
    {
        var service = File.ReadAllText(ProjectFile("SsalddelApp", "Services/ShipperWarehouseService.cs"));

        Assert.Contains("api/v1/warehouse-operations/inbounds/{inboundId.ToString", service);
        Assert.Contains("GetInboundAsync", service);
    }

    [Fact]
    public void 이전복합Manager는_제거되었다()
    {
        Assert.False(File.Exists(ProjectFile(
            "Ssalddel.Ui.Common",
            "Areas/App/Components/WarehouseOperations/SsalddelInboundRequestManager.razor")));
    }

    private static string ProjectFile(string project, string relativePath)
        => Path.Combine(
            FindRepositoryRoot(),
            project,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

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
