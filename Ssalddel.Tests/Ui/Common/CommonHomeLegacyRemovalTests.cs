namespace Ssalddel.Tests.Ui.Common;

public sealed class CommonHomeLegacyRemovalTests
{
    [Fact]
    public void 공통홈은_레거시Platform요약대신_현재탐색과역할선택을제공한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            "Components",
            "Pages",
            "RoleNeutralHome.razor"));

        Assert.DoesNotContain("<PlatformCommunityHome", source);
        Assert.DoesNotContain("홍달", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CommunityPageRoutes.Home", source);
        Assert.Contains("CommunityPageRoutes.Regions", source);
        Assert.Contains("ShipperRoutes.PublicDataInformation", source);
        Assert.Contains("SsalddelClientRole.Shipper", source);
        Assert.Contains("SsalddelClientRole.WarehouseManager", source);
    }

    [Fact]
    public void Maui프로젝트실행이름과표시이름은_살뜰기준이다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            "SsalddelApp.csproj"));

        Assert.Contains("<AssemblyName>SsalddelApp</AssemblyName>", source);
        Assert.Contains("<ApplicationTitle>살뜰</ApplicationTitle>", source);
        Assert.DoesNotContain("HongdalApp", source, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Resources/AppIcon/appicon.svg")]
    [InlineData("Resources/AppIcon/appiconfg.svg")]
    [InlineData("Resources/Splash/splash.svg")]
    public void Maui브랜드자산은_기본DotNet로고대신_살뜰표식을사용한다(string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "SsalddelApp",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

        Assert.DoesNotContain("#512BD4", source, StringComparison.OrdinalIgnoreCase);
        if (!relativePath.EndsWith("appicon.svg", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Contains("ssalddel-mark", source);
        }
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
