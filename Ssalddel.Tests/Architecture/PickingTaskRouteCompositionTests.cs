namespace Ssalddel.Tests.Architecture;

public sealed class PickingTaskRouteCompositionTests
{
    [Theory]
    [InlineData("Ssalddel.WebApp", "Pages/WarehousePickingBatchPage.razor", "/work/picking-batch", "<PickingTaskListScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/WarehousePickingBatchDetailPage.razor", "/work/picking-batch/{TaskKey}", "<PickingTaskDetailScreen")]
    [InlineData("Ssalddel.WebApp", "Pages/WarehousePickingBatchExecutePage.razor", "/work/picking-batch/{TaskKey}/execute", "<PickingTaskExecuteScreen")]
    [InlineData("WarehouseManagerApp", "Components/Pages/PickingBatch.razor", "/work/picking-batch", "<PickingTaskListScreen")]
    [InlineData("WarehouseManagerApp", "Components/Pages/PickingBatchDetail.razor", "/work/picking-batch/{TaskKey}", "<PickingTaskDetailScreen")]
    [InlineData("WarehouseManagerApp", "Components/Pages/PickingBatchExecute.razor", "/work/picking-batch/{TaskKey}/execute", "<PickingTaskExecuteScreen")]
    public void Web과창고앱은_같은책임별공용Screen을조립한다(
        string project,
        string relativePath,
        string route,
        string screenMarkup)
    {
        var source = File.ReadAllText(ProjectFile(project, relativePath));

        Assert.Contains($"@page \"{route}\"", source);
        Assert.Contains(screenMarkup, source);
        Assert.DoesNotContain("I피킹작업페이지Service", source);
        Assert.DoesNotContain("SsalddelPickingTaskWorkspace", source);
        Assert.DoesNotContain(File.ReadLines(ProjectFile(project, relativePath)), line => line.Trim().Equals("try", StringComparison.Ordinal));
    }

    [Fact]
    public void 목록과상세Screen은_Command입력을소유하지않는다()
    {
        var list = CommonScreen("PickingTaskListScreen.razor");
        var detail = CommonScreen("PickingTaskDetailScreen.razor");
        var execute = CommonScreen("PickingTaskExecuteScreen.razor");

        Assert.DoesNotContain("피킹작업처리ViewModel", list);
        Assert.DoesNotContain("시작후재조회Async", list);
        Assert.DoesNotContain("완료후재조회Async", list);
        Assert.DoesNotContain("피킹작업처리ViewModel", detail);
        Assert.DoesNotContain("시작후재조회Async", detail);
        Assert.DoesNotContain("완료후재조회Async", detail);
        Assert.Contains("피킹작업실행ViewModel", execute);
        Assert.Contains("시작후재조회Async", execute);
        Assert.Contains("완료후재조회Async", execute);
    }

    [Fact]
    public void 실행ViewModel은_목록을재조회하지않고_같은Key상세만재조회한다()
    {
        var source = File.ReadAllText(ProjectFile(
            "Ssalddel.Ui.Common",
            "Areas/App/ViewModels/피킹작업페이지ViewModels.cs"));
        var executionStart = source.IndexOf("public sealed class 피킹작업실행ViewModel", StringComparison.Ordinal);

        Assert.True(executionStart >= 0);
        var execution = source[executionStart..];
        Assert.DoesNotContain("피킹작업목록ViewModel", execution);
        Assert.DoesNotContain("목록.조회Async", execution);
        Assert.Contains("상세.조회Async", execution);
    }

    [Fact]
    public void 기존QueryKey는_stableKey상세Route로호환이동한다()
    {
        foreach (var path in new[]
                 {
                     ProjectFile("Ssalddel.WebApp", "Pages/WarehousePickingBatchPage.razor"),
                     ProjectFile("WarehouseManagerApp", "Components/Pages/PickingBatch.razor")
                 })
        {
            var source = File.ReadAllText(path);
            Assert.Contains("SupplyParameterFromQuery(Name = \"taskKey\")", source);
            Assert.Contains("PickingTaskScreenKind.Detail", source);
            Assert.Contains("replace: true", source);
        }
    }

    [Fact]
    public void 이전복합Workspace는_제거되었다()
    {
        Assert.False(File.Exists(ProjectFile(
            "Ssalddel.Ui.Common",
            "Areas/App/Components/WarehouseOperations/SsalddelPickingTaskWorkspace.razor")));
        Assert.False(File.Exists(ProjectFile(
            "WarehouseManagerApp",
            "Components/Pages/PickingBatchWorkspace.razor")));
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
