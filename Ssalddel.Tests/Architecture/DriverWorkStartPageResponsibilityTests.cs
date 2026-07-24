namespace Ssalddel.Tests.Architecture;

public sealed class DriverWorkStartPageResponsibilityTests
{
    [Fact]
    public void 운행_시작_page는_운행_세션과_명시적_위치_갱신만_담당한다()
    {
        var root = FindRepositoryRoot();
        var pagePath = Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "DriverWorkStartPage.razor");
        var source = File.ReadAllText(pagePath);

        Assert.Contains("@inject 기사운행Service", source);
        Assert.Contains("HasValidCoordinates", source);
        Assert.Contains("CanStartWork", source);
        Assert.Contains("DriverRoutes.Recommendations", source);
        Assert.DoesNotContain("I기사추천수신Service", source);
        Assert.DoesNotContain("추천Service.", source);
        Assert.DoesNotContain("ConnectRecommendationHubAsync", source);
        Assert.DoesNotContain("SendDrivingStatusToHubAsync", source);
        Assert.DoesNotContain("37.526", source);
        Assert.DoesNotContain("126.875", source);
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
