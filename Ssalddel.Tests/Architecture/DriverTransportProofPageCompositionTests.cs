namespace Ssalddel.Tests.Architecture;

public sealed class DriverTransportProofPageCompositionTests
{
    [Fact]
    public void 통합_증빙_라우트는_ViewModel과_증빙_컴포넌트만_조립한다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var pagePath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "DriverTransportProofPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.True(File.ReadLines(pagePath).Count() <= 90);
        Assert.Contains("MvvmComponentBase<DriverTransportProofPageViewModel>", source);
        Assert.Contains("<DriverTransportProofTargetPanel", source);
        Assert.Contains("<DriverPickupProofEditor", source);
        Assert.Contains("<DriverDropoffProofEditor", source);
        Assert.Contains("<DriverTransportIssueEditor", source);
        Assert.DoesNotContain("기사운송증빙Service", source);
        Assert.DoesNotContain("IBrowserFile", source);
        Assert.DoesNotContain("ReadImageAsync", source);
        Assert.DoesNotContain("기사운송문제신고요청", source);
    }

    [Theory]
    [InlineData("DriverTransportProofTargetPanel.razor")]
    [InlineData("DriverTransportProofStatePanel.razor")]
    [InlineData("DriverPickupProofEditor.razor")]
    [InlineData("DriverDropoffProofEditor.razor")]
    [InlineData("DriverTransportIssueEditor.razor")]
    [InlineData("DriverTransportProofPresentation.cs")]
    public void 증빙_표시와_입력_책임은_별도_파일로_존재한다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var componentPath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "Pages",
            "DriverTransportProof",
            fileName);

        Assert.True(File.Exists(componentPath), $"통합 증빙 하위 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(componentPath));
    }

    [Theory]
    [InlineData("DriverTransportProofPageViewModel.cs")]
    [InlineData("DriverPickupProofViewModel.cs")]
    [InlineData("DriverDropoffProofViewModel.cs")]
    [InlineData("DriverTransportIssueViewModel.cs")]
    public void 증빙_workflow는_대상_상차_하차_예외_책임으로_분리된다(string fileName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var viewModelPath = Path.Combine(
            repositoryRoot,
            "Ssalddel.WebApp",
            "ViewModels",
            fileName);

        Assert.True(File.Exists(viewModelPath), $"통합 증빙 workflow 파일이 없습니다: {fileName}");
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
