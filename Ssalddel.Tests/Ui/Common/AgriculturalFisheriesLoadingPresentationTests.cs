namespace Ssalddel.Tests.Ui.Common;

public sealed class AgriculturalFisheriesLoadingPresentationTests
{
    [Fact]
    public void 농수산가격조회Button은_요청완료까지_로딩상태를표시한다()
    {
        var publicData = Read("농수산공공데이터Workspace.razor");
        var comparison = Read("농수산가격ComparisonWorkspace.razor");
        var button = Read("데이터조회Button.razor");

        Assert.Equal(3, publicData.Split("<데이터조회Button", StringSplitOptions.None).Length - 1);
        Assert.Contains("<데이터조회Button", comparison);
        Assert.Contains("IsLoading=\"ViewModel.IsLoading\"", publicData);
        Assert.Contains("IsLoading=\"ViewModel.IsLoading\"", comparison);
        Assert.Contains("Disabled=\"@(Disabled || IsLoading)\"", button);
        Assert.Contains("aria-busy=\"@(IsLoading ? \"true\" : \"false\")\"", button);
        Assert.Contains("<MudProgressCircular", button);
        Assert.Contains("<span role=\"status\">@LoadingText</span>", button);
    }

    private static string Read(string fileName)
        => File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Information",
            fileName));

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
