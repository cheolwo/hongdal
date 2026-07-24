namespace Ssalddel.Tests.Ui.Common;

public sealed class WarehouseMobileFigma05PresentationTests
{
    [Theory]
    [InlineData("WarehouseWorkspace.razor", "/warehouse")]
    [InlineData("WorkBoard.razor", "/work-board")]
    [InlineData("ExpectedInbounds.razor", "/warehouse/inbounds/expected")]
    [InlineData("ScanStation.razor", "/scan")]
    [InlineData("InboundProductScan.razor", "/work/inbound/products")]
    [InlineData("InboundInspection.razor", "/work/inbound/inspection")]
    [InlineData("InboundInspectionDetail.razor", "/work/inbound/inspection/{InboundItemId:long}")]
    [InlineData("InboundInspectionRecord.razor", "/work/inbound/inspection/{InboundItemId:long}/record")]
    [InlineData("PutAwayTask.razor", "/work/inbound/put-away")]
    [InlineData("GeneralInventory.razor", "/warehouse/general/inventory")]
    [InlineData("PickingBatch.razor", "/work/picking-batch")]
    [InlineData("PickingBatchDetail.razor", "/work/picking-batch/{TaskKey}")]
    [InlineData("PickingBatchExecute.razor", "/work/picking-batch/{TaskKey}/execute")]
    [InlineData("PackingTask.razor", "/work/outbound/packing")]
    [InlineData("OutboundPlanReview.razor", "/warehouse/general/outbound-plan-review")]
    [InlineData("GeneralTransportHandoff.razor", "/warehouse/general/transport-handoff")]
    [InlineData("WarehouseExceptions.razor", "/warehouse/exceptions")]
    [InlineData("WarehouseHistory.razor", "/warehouse/history")]
    [InlineData("WarehouseSettings.razor", "/warehouse/settings")]
    [InlineData("ImportCustoms.razor", "/warehouse/import/customs")]
    public void Figma05의_스무화면은_기존창고Route와업무화면을재사용한다(
        string fileName,
        string route)
    {
        var source = Read("WarehouseManagerApp", "Components", "Pages", fileName);

        Assert.Contains($"@page \"{route}\"", source);
    }

    [Theory]
    [InlineData("05.01", "창고 운영 홈")]
    [InlineData("05.02", "창고 작업 보드")]
    [InlineData("05.03", "입고 예정 조회")]
    [InlineData("05.04", "스캔 스테이션")]
    [InlineData("05.05", "입고상품 수령")]
    [InlineData("05.06", "입고 검수 목록")]
    [InlineData("05.07", "입고 검수 상세")]
    [InlineData("05.08", "입고 검수 실행")]
    [InlineData("05.09", "적재 작업")]
    [InlineData("05.10", "일반 재고 현황")]
    [InlineData("05.11", "피킹 작업 목록")]
    [InlineData("05.12", "피킹 작업 상세")]
    [InlineData("05.13", "피킹 작업 실행")]
    [InlineData("05.14", "포장 작업")]
    [InlineData("05.15", "출고예정 운송 전 검토")]
    [InlineData("05.16", "출고 인계 준비")]
    [InlineData("05.17", "창고 예외 처리")]
    [InlineData("05.18", "창고 작업 이력")]
    [InlineData("05.19", "창고 설정")]
    [InlineData("05.20", "보세·통관 상태")]
    public void 모바일ScreenCatalog는_Figma05책임코드와제목을고정한다(
        string screenCode,
        string title)
    {
        var source = Read(
            "WarehouseManagerApp",
            "Services",
            "WarehouseMobileScreenCatalog.cs");

        Assert.Contains($"\"{screenCode}\"", source);
        Assert.Contains($"\"{title}\"", source);
    }

    [Fact]
    public void 창고MauiShell은_FigmaAppBar와네개하단Navigation을제공한다()
    {
        var source = Read(
            "WarehouseManagerApp",
            "Components",
            "Layout",
            "MainLayout.razor");
        var styles = Read(
            "WarehouseManagerApp",
            "wwwroot",
            "warehouse-mobile.css");

        Assert.Contains("warehouse-mobile-shell__appbar", source);
        Assert.Contains("warehouse-mobile-shell__bottom-nav", source);
        Assert.Contains("살뜰 창고", source);
        Assert.Contains(">홈</span>", source);
        Assert.Contains(">입고</span>", source);
        Assert.Contains(">작업</span>", source);
        Assert.Contains(">출고</span>", source);
        Assert.Contains("--mud-palette-primary: #ef6c00", styles);
        Assert.Contains("width: min(100%, 520px)", styles);
    }

    [Fact]
    public void 앱시작주소는_실제창고운영홈으로호환이동한다()
    {
        var source = Read(
            "WarehouseManagerApp",
            "Components",
            "Pages",
            "Home.razor");
        var mainPage = Read(
            "WarehouseManagerApp",
            "MainPage.xaml");

        Assert.Contains("@page \"/\"", source);
        Assert.Contains("WarehouseManagerRoutes.Warehouse", source);
        Assert.Contains("replace: true", source);
        Assert.DoesNotContain("<CommunityWorkspaceScreen", source);
        Assert.Contains("StartPath=\"/warehouse\"", mainPage);
    }

    [Fact]
    public void 기존업무화면은_ViewModel과API경계를유지한다()
    {
        var workspace = Read(
            "WarehouseManagerApp",
            "Components",
            "Pages",
            "WarehouseWorkspace.razor");
        var expectedInbounds = Read(
            "WarehouseManagerApp",
            "Components",
            "Pages",
            "ExpectedInbounds.razor");

        Assert.Contains("MvvmComponentBase<창고홈PageViewModel>", workspace);
        Assert.Contains("ViewModel.창고조회.오류메시지", workspace);
        Assert.Contains("MvvmComponentBase<창고입고예정조회PageViewModel>", expectedInbounds);
        Assert.Contains("ViewModel.페이지오류메시지", expectedInbounds);
    }

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine(new[] { FindRepositoryRoot() }.Concat(segments).ToArray()));

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
