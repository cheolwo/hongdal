namespace Ssalddel.Tests.Architecture;

public sealed class MartPickingRouteCompositionTests
{
    [Fact]
    public void 복합피킹Workflow는_목록과상세Screen으로분리된다()
    {
        var directory = Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "WarehouseOperations");

        Assert.False(File.Exists(Path.Combine(directory, "SsalddelMartPickingPackingWorkflow.razor")));

        var list = File.ReadAllText(Path.Combine(directory, "SsalddelMartPickingOrderList.razor"));
        var detail = File.ReadAllText(Path.Combine(directory, "SsalddelMartPickingOrderDetail.razor"));

        Assert.Contains("마트피킹주문목록ViewModel", list);
        Assert.DoesNotContain("마트피킹주문상세ViewModel", list);
        Assert.Contains("마트피킹주문상세ViewModel", detail);
        Assert.DoesNotContain("마트피킹주문목록ViewModel", detail);
    }

    [Theory]
    [InlineData("WarehouseManagerApp", "Components/Pages/MartPickingPacking.razor", "SsalddelMartPickingOrderList")]
    [InlineData("WarehouseManagerApp", "Components/Pages/MartPickingPackingDetail.razor", "SsalddelMartPickingOrderDetail")]
    [InlineData("Ssalddel.WebApp", "Pages/WarehouseMartPickingPackingPage.razor", "SsalddelMartPickingOrderList")]
    [InlineData("Ssalddel.WebApp", "Pages/WarehouseMartPickingOrderDetailPage.razor", "SsalddelMartPickingOrderDetail")]
    public void RoutePage는_한가지피킹Screen만사용한다(string project, string relativePath, string expectedScreen)
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

        Assert.Contains($"<{expectedScreen}", source);
        Assert.Equal(
            1,
            CountOccurrences(source, "<SsalddelMartPickingOrderList")
            + CountOccurrences(source, "<SsalddelMartPickingOrderDetail"));
        Assert.DoesNotContain("SupplyParameterFromQuery", source);
        Assert.DoesNotContain("?orderId=", source);
    }

    [Fact]
    public void 마트업무허브는_샘플건수나샘플작업자를운영값처럼표시하지않는다()
    {
        var root = FindRepositoryRoot();
        var commonHub = File.ReadAllText(Path.Combine(root, "Ssalddel.Ui.Common", "Areas", "App", "Components", "WarehouseOperations", "SsalddelMartWarehouseWorkflowHub.razor"));
        var appBoard = File.ReadAllText(Path.Combine(root, "WarehouseManagerApp", "Components", "Pages", "MartWorkBoard.razor"));
        var webBoard = File.ReadAllText(Path.Combine(root, "Ssalddel.WebApp", "Pages", "WarehouseMartWorkBoardPage.razor"));
        var appWorkStart = File.ReadAllText(Path.Combine(root, "WarehouseManagerApp", "Components", "Pages", "MartWorkStart.razor"));
        var webWorkStart = File.ReadAllText(Path.Combine(root, "Ssalddel.WebApp", "Pages", "WarehouseMartWorkStartPage.razor"));

        Assert.Contains("SsalddelMartWarehouseWorkflowHub", appBoard);
        Assert.Contains("SsalddelMartWarehouseWorkflowHub", webBoard);
        Assert.DoesNotContain("오늘 주문", commonHub);
        Assert.DoesNotContain("RIDER-73", commonHub);
        Assert.DoesNotContain("77771111", appWorkStart);
        Assert.DoesNotContain("77771111", webWorkStart);
        Assert.Contains("Simulation", appWorkStart);
        Assert.Contains("Simulation", webWorkStart);
    }

    private static int CountOccurrences(string source, string value)
        => source.Split(value, StringSplitOptions.None).Length - 1;

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
