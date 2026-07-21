namespace Ssalddel.Tests.Architecture;

public sealed class DriverTransportPickupPageCompositionTests
{
    [Fact]
    public void 상차_라우트는_상차_ViewModel과_재사용_증빙_컴포넌트만_조립한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var pagePath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "DriverTransportPickupPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 100);
        Assert.Contains("MvvmComponentBase<DriverTransportPickupPageViewModel>", source);
        Assert.Contains("<DriverTransportPickupSummaryPanel", source);
        Assert.Contains("<DriverPickupProofPhotoEditor", source);
        Assert.Contains("<DriverPickupReceiptEditor", source);
        Assert.Contains("<DriverTransportIssueEditor", source);
        Assert.DoesNotContain("기사운송증빙Service", source);
        Assert.DoesNotContain("IBrowserFile", source);
        Assert.DoesNotContain("ReadImageAsync", source);
        Assert.DoesNotContain("기사상차인수증입력", source);
    }

    [Theory]
    [InlineData("DriverTransportPickupSummaryPanel.razor")]
    [InlineData("DriverTransportPickupNavigation.razor")]
    public void 상차_전용_표시_책임은_별도_컴포넌트로_존재한다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentPath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "DriverTransportPickup",
            fileName);

        Assert.True(File.Exists(componentPath), $"상차 전용 컴포넌트가 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
    }

    [Theory]
    [InlineData("DriverPickupProofPhotoEditor.razor")]
    [InlineData("DriverPickupReceiptEditor.razor")]
    [InlineData("DriverTransportIssueEditor.razor")]
    public void 상차_증빙_입력은_통합_증빙과_같은_컴포넌트를_재사용한다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentPath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "DriverTransportProof",
            fileName);

        Assert.True(File.Exists(componentPath), $"재사용 증빙 컴포넌트가 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
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
