namespace Ssalddel.Tests.Architecture;

public sealed class InboundInspectionRouteCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/WarehouseInboundInspectionPage.razor", "/work/inbound/inspection", "<InboundInspectionListScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/WarehouseInboundInspectionDetailPage.razor", "/work/inbound/inspection/{InboundItemId:long}", "<InboundInspectionDetailScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/WarehouseInboundInspectionRecordPage.razor", "/work/inbound/inspection/{InboundItemId:long}/record", "<InboundInspectionRecordScreen")]
    [InlineData("WarehouseManagerApp", "Components/Pages/InboundInspection.razor", "/work/inbound/inspection", "<InboundInspectionListScreen")]
    [InlineData("WarehouseManagerApp", "Components/Pages/InboundInspectionDetail.razor", "/work/inbound/inspection/{InboundItemId:long}", "<InboundInspectionDetailScreen")]
    [InlineData("WarehouseManagerApp", "Components/Pages/InboundInspectionRecord.razor", "/work/inbound/inspection/{InboundItemId:long}/record", "<InboundInspectionRecordScreen")]
    public void Web과창고앱은_같은책임별공용Screen을조립한다(
        string project,
        string relativePath,
        string route,
        string screenMarkup)
    {
        var source = File.ReadAllText(ProjectFile(project, relativePath));

        Assert.Contains($"@page \"{route}\"", source);
        Assert.Contains(screenMarkup, source);
        Assert.DoesNotContain("I입고검수페이지Service", source);
        Assert.DoesNotContain("SsalddelInboundInspectionWorkspace", source);
        Assert.DoesNotContain(File.ReadLines(ProjectFile(project, relativePath)), line => line.Trim().Equals("try", StringComparison.Ordinal));
    }

    [Fact]
    public void 목록과상세Screen은_Command입력을소유하지않는다()
    {
        var list = CommonScreen("InboundInspectionListScreen.razor");
        var detail = CommonScreen("InboundInspectionDetailScreen.razor");
        var record = CommonScreen("InboundInspectionRecordScreen.razor");

        Assert.DoesNotContain("입고검수작성ViewModel", list);
        Assert.DoesNotContain("검수후재조회Async", list);
        Assert.DoesNotContain("입고검수작성ViewModel", detail);
        Assert.DoesNotContain("검수후재조회Async", detail);
        Assert.Contains("입고검수실행ViewModel", record);
        Assert.Contains("검수후재조회Async", record);
    }

    [Fact]
    public void 기존QueryId는_stableId상세Route로호환이동한다()
    {
        foreach (var path in new[]
                 {
                     ProjectFile("Ssalddel.WebApp", "Pages/WarehouseInboundInspectionPage.razor"),
                     ProjectFile("WarehouseManagerApp", "Components/Pages/InboundInspection.razor")
                 })
        {
            var source = File.ReadAllText(path);
            Assert.Contains("SupplyParameterFromQuery(Name = \"inboundItemId\")", source);
            Assert.Contains("InboundInspectionScreenKind.Detail", source);
            Assert.Contains("replace: true", source);
        }
    }

    [Fact]
    public void 이전복합Workspace는_제거되었다()
    {
        Assert.False(File.Exists(ProjectFile(
            "Ssalddel.Ui.Common",
            "Areas/App/Components/WarehouseOperations/SsalddelInboundInspectionWorkspace.razor")));
    }

    private static string CommonScreen(string fileName)
        => File.ReadAllText(ProjectFile(
            "Ssalddel.Ui.Common",
            $"Areas/App/Components/WarehouseOperations/{fileName}"));

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
