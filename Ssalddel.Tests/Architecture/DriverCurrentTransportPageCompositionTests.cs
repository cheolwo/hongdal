namespace Ssalddel.Tests.Architecture;

public sealed class DriverCurrentTransportPageCompositionTests
{
    [Fact]
    public void 현재_운송_라우트_페이지는_ViewModel과_업무_컴포넌트만_조립한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var pagePath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "DriverCurrentTransportPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 100);
        Assert.Contains("MvvmComponentBase<DriverCurrentTransportPageViewModel>", source);
        Assert.Contains("<DriverCurrentTransportOverview", source);
        Assert.Contains("<DriverCurrentTransportStatusActions", source);
        Assert.Contains("<DriverCurrentTransportNavigation", source);
        Assert.DoesNotContain("기사운송증빙Service", source);
        Assert.DoesNotContain("ITransportRequestLedgerObserver", source);
        Assert.DoesNotContain("PeriodicTimer", source);
        Assert.DoesNotContain("상차지도착Async", source);
        Assert.DoesNotContain("하차지도착Async", source);
    }

    [Theory]
    [InlineData("DriverCurrentTransportEmptyState.razor")]
    [InlineData("DriverCurrentTransportOverview.razor")]
    [InlineData("DriverCurrentTransportTimeline.razor")]
    [InlineData("DriverCurrentTransportStatusActions.razor")]
    [InlineData("DriverCurrentTransportNavigation.razor")]
    [InlineData("DriverCurrentTransportPresentation.cs")]
    public void 현재_운송_하위_책임은_별도_파일로_존재한다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentPath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "DriverCurrentTransport",
            fileName);

        Assert.True(File.Exists(componentPath), $"현재 운송 하위 책임 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
    }

    [Theory]
    [InlineData("DriverCurrentTransportPageViewModel.cs")]
    [InlineData("DriverCurrentTransportActionsViewModel.cs")]
    [InlineData("DriverCurrentTransportRefreshSession.cs")]
    public void 현재_운송_workflow_책임은_별도_객체로_존재한다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var viewModelPath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "ViewModels",
            fileName);

        Assert.True(File.Exists(viewModelPath), $"현재 운송 workflow 책임 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(viewModelPath));
    }

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
